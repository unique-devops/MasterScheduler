using Dapper;
using Google.Apis.Util.Store;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Data
{    
    public class MySqliteDataStore : IDataStore
    {
        private readonly string _connectionString;
        public MySqliteDataStore()
        {
            _connectionString = DatabaseHelper.ConnectionString;
        }

        // 🔐 Encrypt
        private string Protect(string plainText)
        {
            var bytes = Encoding.UTF8.GetBytes(plainText);
            var encrypted = ProtectedData.Protect(bytes, null, DataProtectionScope.CurrentUser);

            return Convert.ToBase64String(encrypted);
        }

        // 🔓 Decrypt
        private string Unprotect(string cipherText)
        {
            var bytes = Convert.FromBase64String(cipherText);
            var decrypted = ProtectedData.Unprotect(bytes, null, DataProtectionScope.CurrentUser);

            return Encoding.UTF8.GetString(decrypted);
        }
        public async Task StoreAsync<T>(string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);
            var encrypted = Protect(json);

            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var sql = @"
                INSERT INTO GoogleAccounts (Email, TokenJson, LastUsedOn)
                VALUES (@email, @json, DATETIME('now'))
                ON CONFLICT(Email)
                DO UPDATE SET
                    TokenJson = excluded.TokenJson,
                    LastUsedOn = DATETIME('now');";

            await conn.ExecuteAsync(sql, new
            {
                email = key,
                json = encrypted
            });
        }


        public async Task<T> GetAsync<T>(string key)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            var encrypted = await conn.QueryFirstOrDefaultAsync<string>(
                "SELECT TokenJson FROM GoogleAccounts WHERE Email = @key",
                new { key });

            if (string.IsNullOrEmpty(encrypted))
                return default;

            var json = Unprotect(encrypted);
            return JsonConvert.DeserializeObject<T>(json);
        }

        public async Task DeleteAsync<T>(string key)
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync(
                "DELETE FROM GoogleAccounts WHERE Email = @key",
                new { key });
        }

        public async Task ClearAsync()
        {
            using var conn = new SqliteConnection(_connectionString);
            await conn.OpenAsync();

            await conn.ExecuteAsync("DELETE FROM GoogleAccounts");
        }
        
    }
}
