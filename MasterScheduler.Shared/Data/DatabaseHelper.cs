using Microsoft.Data.Sqlite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Data
{
    public static class DatabaseHelper
    {
        private static readonly string _dbPath = Path.Combine(
           Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
           "rosh", "masterScheduler", "data.db"
        );

        public static string ConnectionString => $"Data Source={_dbPath};";

        public static void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);
            
            using var con = new SqliteConnection(ConnectionString);
            con.Open();
            var sql = @"CREATE TABLE IF NOT EXISTS Jobs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        JobName TEXT NOT NULL,
                        JobType TEXT NOT NULL, 
                        CronExpression TEXT NOT NULL,
                        IsActive INTEGER NOT NULL DEFAULT 1,
                        Parameters TEXT NULL,
                        LastRunTime TEXT NULL,
                        NextRunTime TEXT NULL
                    );";
            using var cmd2 = new SqliteCommand(sql, con);
            cmd2.ExecuteNonQuery();
        }
    }
}
