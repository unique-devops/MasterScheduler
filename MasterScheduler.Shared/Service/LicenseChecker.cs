using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.Dto;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Service
{
    public class LicenseChecker
    {
        private static readonly string SecretSalt = "RoshMasterScheduler_2026_!@#";
        FingerprintGenerator fingerprintGenerator = new FingerprintGenerator();
        private readonly string _apiUrl = "https://license-manager-mauve.vercel.app/api/validate";

        public static string GenerateSecureKey(string pcId, string type)
        {
            // Creates a hash of: PCID + Type + Secret
            using var sha = SHA256.Create();
            byte[] bytes = Encoding.UTF8.GetBytes(pcId + type + SecretSalt);
            byte[] hash = sha.ComputeHash(bytes);
            return Convert.ToBase64String(hash);
        }

        public static bool VerifyIntegrity(string pcId, string type, string storedKey)
        {
            // Re-generate the key and compare
            string validKey = GenerateSecureKey(pcId, type);
            return validKey == storedKey;
        }
        public async Task<bool> CheckAndInitializeLicense(string userEmail = null)
        {
            string pcId = fingerprintGenerator.GetId(); // Use the HWID code from earlier
            var localLicense = GetLocalLicense();

            // SCENARIO 1: First Run (No local DB record)
            if (localLicense == null)
            {
                string defaultType = "lite";
                string secureKey = GenerateSecureKey(pcId, defaultType);
                var lic = new LicenseResponseDto { LicenseType = defaultType,LicenseKey = secureKey,PCID = pcId };
                SaveLocalLicense(lic);

                // Try to notify server (Async - don't block startup)
                _ = RegisterWithServer(lic);
            }
            else
            {
                // 2. Anti-Tamper Check
                bool isTampered = !VerifyIntegrity(pcId, localLicense.LicenseType, localLicense.LicenseKey);

                if (isTampered)
                {
                    // Someone changed 'lite' to 'pro' in the database manually!
                    // Revert them to Lite.
                    string secureKey = GenerateSecureKey(pcId, "lite");
                    var lic = new LicenseResponseDto { LicenseType = "lite", LicenseKey = secureKey, PCID = pcId };
                    UpdateLocalLicense(lic);                    
                }

                // 3. Online Sync (If internet is available)
                var serverUpdate = await ValidateWithServer(pcId);
                if (serverUpdate != null)
                {
                    // Update local DB with new status (e.g. if they bought Pro)
                    string newKey = GenerateSecureKey(pcId, serverUpdate.LicenseType);
                    serverUpdate.LicenseKey = newKey;
                    UpdateLocalLicense(serverUpdate);
                }
            }
            if (string.IsNullOrWhiteSpace(localLicense?.ExpiryDate)) return false;
            return DateTime.Parse(localLicense?.ExpiryDate) > DateTime.Now;
        }
       
        private async Task<LicenseResponseDto> RegisterWithServer(LicenseResponseDto lic)
        {
            using var client = new HttpClient();
            var payload = new { pc_id = lic.PCID, customer_email = lic.Email, version = "1.0.0" };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LicenseResponseDto>(json);
            }
            return null;
        }

        private void SaveLocalLicense(LicenseResponseDto res)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand("INSERT INTO LicenseInfo (PCID, Email, LicenseType, ExpiryDate, LicenseKey) VALUES (@pc, @email, @type, @expiry, @licenseKey)", con);
            cmd.Parameters.AddWithValue("@pc", res.PCID);
            cmd.Parameters.AddWithValue("@email", res.Email ?? "lite@gmail.com");
            cmd.Parameters.AddWithValue("@type", res.LicenseType);
            cmd.Parameters.AddWithValue("@expiry", res.ExpiryDate ?? "");
            cmd.Parameters.AddWithValue("@licenseKey", res.LicenseKey);
            cmd.ExecuteNonQuery();
        }
        public void UpdateLocalLicense(LicenseResponseDto lic)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var sql = "UPDATE LicenseInfo SET Email = @email ,PCID =@pc , LicenseType =@type, ExpiryDate =@expiry, LicenseKey =@licenseKey";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@pc", lic.PCID);
            cmd.Parameters.AddWithValue("@email", lic.Email ?? "lite@gmail.com");
            cmd.Parameters.AddWithValue("@type", lic.LicenseType);
            cmd.Parameters.AddWithValue("@expiry", lic.ExpiryDate ?? "");
            cmd.Parameters.AddWithValue("@licenseKey", lic.LicenseKey);
            cmd.ExecuteNonQuery();

            // Tip: Call your Vercel API here too to sync the email to Supabase
        }
        public LicenseResponseDto GetLocalLicense()
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();

            // We only ever expect one row in this table
            var sql = "SELECT PCID, Email, LicenseType, ExpiryDate, Status FROM LicenseInfo LIMIT 1";

            using var cmd = new SqliteCommand(sql, con);
            using var reader = cmd.ExecuteReader();

            if (reader.Read())
            {
                return new LicenseResponseDto
                {
                    PCID = reader.GetString(0),
                    Email = reader.IsDBNull(1) ? "" : reader.GetString(1),
                    LicenseType = reader.GetString(2),
                    ExpiryDate = reader.GetString(3),
                    Status = reader.GetString(4)
                };
            }

            return null; // No license found (First Run)
        }
        public async Task<LicenseResponseDto> ValidateWithServer(string pcId, string serial = "")
        {
            using var client = new HttpClient();
            var payload = new
            {
                pc_id = pcId,
                serial_key = serial,
                version = "1.0.0"
            };

            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            try
            {
                var response = await client.PostAsync($"{_apiUrl}/api/check", content);
                var json = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var data = JsonConvert.DeserializeObject<LicenseResponseDto>(json);
                    // ENCRYPT and SAVE data.expiry locally for offline checks
                    return data;
                }
                return null;
            }
            catch
            {
                // OFFLINE LOGIC: Read the encrypted local file and compare with current PC time
                return null;
            }
        }
    }
}
