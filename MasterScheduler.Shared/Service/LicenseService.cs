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
using System.Management;
using System.Net.Http.Json;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace MasterScheduler.Shared.Service
{
    public class LicenseService
    {
        private static readonly string publicCertKey = "-----BEGIN RSA PUBLIC KEY-----\nMIIBCgKCAQEA2mEj4N2Zd4bDihw4JA9DoDVxsck61Q9ZnhVMhnHLH+F/hY9hAVZN\nx/IMMzZwVbZOKfbHcqPCgALB7xd9ELItxOnI2DEPMZX1ONLT2q8CAGZKRzuEOKKx\n+Hj9yvQyCKYCVeOmvJlBpi/xuUK60aMq8mTa8hghxpHc2O9FE6iDXsdMFhNQnuh/\nunK0+L0YFPwrIZk3TrT91yDKRgCVekfQFYsmzJmpNx+CWO03IURaTib/2qeV7pHl\noA6yaKqRwjfYCVIbOYG0+wZUx3GqXOxe1gtDarm9W86wjwY4zdK3EZtgQyPhbnso\n4moiTLuwb6ptzA9WiPaV5QaQuwy/pBK1DQIDAQAB\n-----END RSA PUBLIC KEY-----";
        FingerprintGenerator fingerprintGenerator = new FingerprintGenerator();
        private readonly string _apiUrl = "http://uniquetest.somee.com/licman/api/license";
        private string _licFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "license.dat");
                                     
       
        public bool SaveLicense(LicenseDataModel license, out string message)
        {
            SqliteTransaction? trn =null;
            
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();
            try
            {
                trn = con.BeginTransaction();
                var cmdDelete = new SqliteCommand("delete from LicenseInfo where PCID = @pcid and Edition=@edition", con, trn);
                cmdDelete.Parameters.AddWithValue("@pcid", license.DeviceId);
                cmdDelete.Parameters.AddWithValue("@edition", license.LicenseName);
                cmdDelete.ExecuteNonQuery();
                var cmd = new SqliteCommand("INSERT INTO LicenseInfo (PCID, Edition, ExpiryDate, Status, LicenseKey) VALUES (@pcid, @edition, @expiry, @status, @key)", con, trn);
                cmd.Parameters.AddWithValue("@pcid", license.DeviceId);
                cmd.Parameters.AddWithValue("@edition", license.LicenseName);
                cmd.Parameters.AddWithValue("@status", "Active");                
                cmd.Parameters.AddWithValue("@expiry", license.ExpiryDate);
                cmd.Parameters.AddWithValue("@key", license.LicenseKey);
                cmd.ExecuteNonQuery();
                trn.Commit();
                message = "success";
                return true;
            }
            catch (Exception ex)
            {

                if (trn != null)
                {
                    trn.Rollback();
                }
                message = ex.Message;
                return false;
            }                           
           
        }
        public void UpdateLicense()
        {
            try
            {
                var licenses = GetLicenses();
                if (licenses == null || licenses.Count == 0) return;
                foreach (var lic in licenses)
                {
                    using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
                    con.Open();
                    var sql = "UPDATE LicenseInfo SET PCID =@pc , Edition =@edition, Status = @status, ExpiryDate = @expiry where  LicenseKey =@licenseKey";
                    using var cmd = new SqliteCommand(sql, con);
                    cmd.Parameters.AddWithValue("@pc", lic.DeviceId);
                    cmd.Parameters.AddWithValue("@edition", lic.LicenseName);                   
                    cmd.Parameters.AddWithValue("@status", lic.IsExpired ? "Expired" : "Active");
                    cmd.Parameters.AddWithValue("@expiry", lic.ExpiryDate);
                    cmd.Parameters.AddWithValue("@licenseKey", lic.LicenseKey);
                    cmd.ExecuteNonQuery();
                }                
            }
            catch 
            {                
            }
            // Tip: Call your Vercel API here too to sync the email to Supabase
        }
        
        public List<LicenseDataModel> GetLicenses()
        {
            List<LicenseDataModel> licenses = new List<LicenseDataModel>();
            var pcID = fingerprintGenerator.GetId();
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();

            // We only ever expect one row in this table
            var sql = "SELECT * FROM LicenseInfo where PCID = @pcid";

            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@pcid", pcID);
            using var reader = cmd.ExecuteReader();           

            while (reader.Read())
            {
                if (string.IsNullOrWhiteSpace(reader["LicenseKey"].ToString())) continue;
                var valid = LicenseService.VerifyLicense(reader["LicenseKey"].ToString(), out LicenseDataModel data);
                if (valid)
                {
                    var isExpired = LicenseService.IsLicenseExpired(data.ExpiryDate);
                    licenses.Add(new LicenseDataModel
                    {
                        LicenseName = data.LicenseName,
                        ExpiryDate = data.ExpiryDate,
                        IsExpired = isExpired,
                        DeviceId = data.DeviceId,
                        LicenseKey = data.LicenseKey,
                    });
                }
                
            }
           return licenses;
        }

        public List<LicenseDataModel> GetLicByName(string Licname)
        {
            List<LicenseDataModel> licenses = new List<LicenseDataModel>();
            var pcID = fingerprintGenerator.GetId();
            using var con = new SqliteConnection(DatabaseHelper.ConnectionString);
            con.Open();

            // We only ever expect one row in this table
            var sql = "SELECT * FROM LicenseInfo where PCID = @pcid and Edition = @licname";

            using var cmd = new SqliteCommand(sql, con);
            cmd.Parameters.AddWithValue("@pcid", pcID);
            cmd.Parameters.AddWithValue("@licname", Licname);
            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                if (string.IsNullOrWhiteSpace(reader["LicenseKey"].ToString())) continue;
                var valid = LicenseService.VerifyLicense(reader["LicenseKey"].ToString(), out LicenseDataModel data);
                if (valid)
                {
                    var isExpired = LicenseService.IsLicenseExpired(data.ExpiryDate);
                    licenses.Add(new LicenseDataModel
                    {
                        LicenseName = data.LicenseName,
                        ExpiryDate = data.ExpiryDate,
                        IsExpired = isExpired,
                        DeviceId = data.DeviceId,
                        LicenseKey = data.LicenseKey,
                    });
                }

            }
            return licenses;
        }

        // =====================================
        // VERIFY LICENSE
        // =====================================
        public static bool VerifyLicense(string licenseKey,out LicenseDataModel data)
        {
            data = null;

            try
            {
                string[] parts =
                    licenseKey.Split('.');

                if (parts.Length != 2)
                    return false;

                string json =
                    Encoding.UTF8.GetString(
                        Convert.FromBase64String(parts[0]));

                string signature = parts[1];

                bool valid =
                    VerifySignature(
                        json,
                        signature,
                        publicCertKey);

                if (!valid)
                    return false;

                data = JsonConvert.DeserializeObject<LicenseDataModel>(json);

                return true;
            }
            catch
            {
                return false;
            }
        }


        // =====================================
        // VERIFY RSA SIGNATURE
        // =====================================
        private static bool VerifySignature(
            string data,
            string signature,
            string publicKey)
        {
            using RSA rsa = RSA.Create();

            rsa.ImportFromPem(publicKey);

            byte[] dataBytes =
                Encoding.UTF8.GetBytes(data);

            byte[] signBytes =
                Convert.FromBase64String(signature);

            return rsa.VerifyData(
                dataBytes,
                signBytes,
                HashAlgorithmName.SHA256,
                RSASignaturePadding.Pkcs1);
        }



        // ==============================
        // CHECK LICENSE EXPIRED
        // ==============================
        public static bool IsLicenseExpired(string expiryDate)
        {
            // Parse dynamic date format
            // Example: 112026 => 1/1/2026

            string year = expiryDate.Substring(expiryDate.Length - 4);

            string remain =
                expiryDate.Substring(0, expiryDate.Length - 4);

            int day;
            int month;

            day = int.Parse(remain.Substring(0, 2));
            month = int.Parse(remain.Substring(2, 2));

            DateTime expDate =
                new DateTime(
                    int.Parse(year),
                    month,
                    day);

            return DateTime.Now.Date > expDate.Date;
        }
    }
}
