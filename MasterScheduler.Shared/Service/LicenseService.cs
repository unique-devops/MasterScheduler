using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Dto;
using MasterScheduler.Shared.Enums;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Service
{
    public class LicenseService
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
                string defaultType = "Free";
                string secureKey = GenerateSecureKey(pcId, defaultType);
                var lic = new LicenseInfoModel{ Edition = defaultType,LicenseKey = secureKey,PCID = pcId, Modules = new HashSet<string> { "SQLBACKUP" }, Connectors = new HashSet<string> { "LOCAL" } };
                SaveLocalLicense(lic);

                // Try to notify server (Async - don't block startup)
                _ = RegisterWithServer(lic);
            }
            else
            {
                // 2. Anti-Tamper Check
                bool isTampered = !VerifyIntegrity(pcId, localLicense.Edition, localLicense.LicenseKey);

                if (isTampered)
                {
                    // Someone changed 'lite' to 'pro' in the database manually!
                    // Revert them to Lite.
                    string secureKey = GenerateSecureKey(pcId, "Free");
                    var lic = new LicenseInfoModel { Edition = "Free", LicenseKey = secureKey, PCID = pcId };
                    UpdateLocalLicense(lic);                    
                }

                // 3. Online Sync (If internet is available)
                var serverUpdate = await ValidateWithServer(pcId);
                if (serverUpdate != null)
                {
                    // Update local DB with new status (e.g. if they bought Pro)
                    string newKey = GenerateSecureKey(pcId, serverUpdate.Edition);
                    serverUpdate.LicenseKey = newKey;
                    UpdateLocalLicense(serverUpdate);
                }
            }            
            return localLicense?.ExpiryDate ==null ? true : localLicense.ExpiryDate > DateTime.Now;
        }

        public async Task ActivateTrialLicense(string userEmail)
        {
            string pcId = fingerprintGenerator.GetId(); // Use the HWID code from earlier
            var localLicense = GetLocalLicense();
            string defaultType = "Pro";
            string secureKey = GenerateSecureKey(pcId, defaultType);
            var lic = new LicenseInfoModel { Edition = defaultType, LicenseKey = secureKey, PCID = pcId, Email = userEmail, ExpiryDate = DateTime.Now.AddMonths(1) };
            if (localLicense == null)
            {                                               
                SaveLocalLicense(lic);                                
            }
            else
            {
                UpdateLocalLicense(lic);                
            }
            await RegisterWithServer(lic);
        }
        private async Task<LicenseInfoModel> RegisterWithServer(LicenseInfoModel lic)
        {
            using var client = new HttpClient();
            var payload = new { pc_id = lic.PCID, customer_email = lic.Email, version = "1.0.0" };
            var content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json");

            var response = await client.PostAsync(_apiUrl, content);
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<LicenseInfoModel>(json);
            }
            return null;
        }

        private void SaveLocalLicense(LicenseInfoModel license)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var cmd = new SqliteCommand("INSERT INTO LicenseInfo (PCID, Edition, Status, LicenseKey, Modules, Connectors) VALUES (@pcid, @edition, @status, @key, @modules, @connectors)", con);           
            cmd.Parameters.AddWithValue("@pcid", license.PCID);
            cmd.Parameters.AddWithValue("@edition", license.Edition);
            cmd.Parameters.AddWithValue("@status", license.IsExpired ? "Expired" : "Active");           
            cmd.Parameters.AddWithValue("@key", license.LicenseKey);

            cmd.Parameters.AddWithValue("@modules",
                JsonConvert.SerializeObject(license.Modules));

            cmd.Parameters.AddWithValue("@connectors",
                JsonConvert.SerializeObject(license.Connectors));

            cmd.ExecuteNonQuery();
        }
        public void UpdateLocalLicense(LicenseInfoModel lic)
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            var sql = "UPDATE LicenseInfo SET Email = @email ,PCID =@pc , LicenseType =@type, ExpiryDate =@expiry, LicenseKey =@licenseKey";
            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@pc", lic.PCID);
            cmd.Parameters.AddWithValue("@email", lic.Email);
            cmd.Parameters.AddWithValue("@edition", lic.Edition);
            cmd.Parameters.AddWithValue("@status", lic.IsExpired ? "Expired" : "Active");
            cmd.Parameters.AddWithValue("@expiry", lic.ExpiryDate  == null ? "" :lic.ExpiryDate?.ToString("yyyy-MM-dd"));
            cmd.Parameters.AddWithValue("@licenseKey", lic.LicenseKey);
            cmd.ExecuteNonQuery();

            // Tip: Call your Vercel API here too to sync the email to Supabase
        }
        public LicenseInfoModel GetLocalLicense()
        {
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();

            // We only ever expect one row in this table
            var sql = "SELECT * FROM LicenseInfo LIMIT 1";

            using var cmd = new SqliteCommand(sql, con);
            using var reader = cmd.ExecuteReader();           

            if (reader.Read())
            {
                return new LicenseInfoModel
                {                    
                    Edition = reader["Edition"].ToString(),
                    ExpiryDate = reader["ExpiryDate"] == DBNull.Value
                        ? null
                        : DateTime.Parse(reader["ExpiryDate"].ToString()),
                    IsExpired = reader["Status"].ToString() == "Expired",
                    Status = reader["Status"].ToString(),
                     LicenseKey = reader["LicenseKey"].ToString(),
                    Modules = JsonConvert.DeserializeObject<HashSet<string>>(
                    reader["Modules"]?.ToString() ?? "[]"),

                            Connectors = JsonConvert.DeserializeObject<HashSet<string>>(
                    reader["Connectors"]?.ToString() ?? "[]")
                };
            }
           return null;
        }
        public async Task<LicenseInfoModel> ValidateWithServer(string pcId, string serial = "")
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
                    var data = JsonConvert.DeserializeObject<LicenseInfoModel>(json);
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

        public bool HasModule(string moduleCode, LicenseInfoModel Current)
        {
            if (Current == null)
                return false;

            // Trial: allow everything
            if (Current.Edition == "Trial" && !Current.IsExpired)
                return true;

            // Free edition: no paid modules
            if (Current.Edition == "Free")
                return false;

            // Lite edition: allow core modules only
            if (Current.Edition == "Lite")
            {
                return Current.Modules.Contains(moduleCode);
            }

            // Pro edition: check purchased modules
            if (Current.Edition == "Pro")
            {
                return Current.Modules.Contains(moduleCode);
            }

            return false;
        }

    }
}
