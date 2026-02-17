using Azure;
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
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MasterScheduler.Shared.Service
{
    public class LicenseService
    {
        private static readonly string SecretSalt = "RoshMasterScheduler_2026_!@#";
        FingerprintGenerator fingerprintGenerator = new FingerprintGenerator();
        private readonly string _apiUrl = "http://uniquetest.somee.com/licman/api/license";
        private string _licFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");
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
       
        public async Task ActivateTrialLicense(string userEmail)
        {
            string pcId = fingerprintGenerator.GetId(); // Use the HWID code from earlier            
            var trialLicense = new TrialRequest
            {
                AppName = "MasterScheduler",
                DeviceId = pcId,
                Email = userEmail
            };
            HttpClient httpClient = new HttpClient();
            var response = await httpClient.PostAsJsonAsync($"{_apiUrl}/start-trial", trialLicense);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LicenseResponseDto>();
                string rawData = $"{result?.licenseKey}|{result?.ownerEmail}|{result?.expiresAt}|{pcId}";
                string encrypted = EncryptString(rawData, pcId + SecretSalt);
                File.WriteAllText(_licFilePath, encrypted);
            }
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


        public string[] LoadAndVerifyLicense()
        {
            if (!File.Exists(_licFilePath)) return null;

            try
            {
                string encrypted = File.ReadAllText(_licFilePath);
                string decrypted = DecryptString(encrypted, fingerprintGenerator.GetId() + SecretSalt);
                string[] parts = decrypted.Split('|');

                // Verification: Does the Hardware ID in the file match THIS PC?
                if (parts.Length == 4 && parts[3] == fingerprintGenerator.GetId())
                {
                    return parts; // Returns [Key, Email, Expiry, DeviceId]
                }
            }
            catch { /* Tampered or wrong PC */ }
            return null;
        }
        private string EncryptString(string text, string key)
        {
            var bKey = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            using (var aes = Aes.Create())
            {
                aes.Key = bKey;
                aes.GenerateIV();
                using (var ms = new MemoryStream())
                {
                    ms.Write(aes.IV, 0, aes.IV.Length);
                    using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        byte[] data = Encoding.UTF8.GetBytes(text);
                        cs.Write(data, 0, data.Length);
                    }
                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private string DecryptString(string encrypted, string key)
        {
            var bKey = Encoding.UTF8.GetBytes(key.PadRight(32).Substring(0, 32));
            byte[] fullData = Convert.FromBase64String(encrypted);
            using (var aes = Aes.Create())
            {
                aes.Key = bKey;
                byte[] iv = new byte[aes.BlockSize / 8];
                Array.Copy(fullData, 0, iv, 0, iv.Length);
                aes.IV = iv;
                using (var ms = new MemoryStream())
                {
                    using (var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read))
                    {
                        ms.Write(fullData, iv.Length, fullData.Length - iv.Length);
                        ms.Position = 0;
                        using (var reader = new StreamReader(cs)) return reader.ReadToEnd();
                    }
                }
            }
        }
    }
}
