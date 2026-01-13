using Google.Apis.Util.Store;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
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
        public Task StoreAsync<T>(string key, T value)
        {
            var json = JsonConvert.SerializeObject(value);

            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = @"
                INSERT INTO GoogleSettings (Key, TokenJson) 
                VALUES ($key, $json)
                ON CONFLICT(Key) DO UPDATE SET TokenJson = $json;";
                command.Parameters.AddWithValue("$key", key);
                command.Parameters.AddWithValue("$json", json);
                command.ExecuteNonQuery();
            }
            return Task.CompletedTask;
        }

        public Task<T> GetAsync<T>(string key)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "SELECT TokenJson FROM GoogleSettings WHERE Key = $key";
                command.Parameters.AddWithValue("$key", key);

                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        var json = reader.GetString(0);
                        return Task.FromResult(JsonConvert.DeserializeObject<T>(json));
                    }
                }
            }
            return Task.FromResult(default(T));
        }

        public Task DeleteAsync<T>(string key)
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM GoogleSettings WHERE Key = $key";
                command.Parameters.AddWithValue("$key", key);
                command.ExecuteNonQuery();
            }
            return Task.CompletedTask;
        }

        public Task ClearAsync()
        {
            using (var connection = new SqliteConnection(_connectionString))
            {
                connection.Open();
                var command = connection.CreateCommand();
                command.CommandText = "DELETE FROM GoogleSettings";
                command.ExecuteNonQuery();
            }
            return Task.CompletedTask;
        }
    }
}
