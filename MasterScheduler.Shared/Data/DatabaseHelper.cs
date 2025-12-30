using MasterScheduler.Shared.DataModels;
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
        public static string ConnectionString => $"Data Source={_dbPath};Pooling=True;";

        public static void Initialize()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(_dbPath)!);

            CreateJobsTable();
            CreateJobSettingsTable();
            UpdateCrashedStatusJOb();
        }

        private static void CreateJobsTable()
        {
            using var con = new SqliteConnection(ConnectionString);
            con.Open();
            var sql = @"CREATE TABLE IF NOT EXISTS Jobs (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        JobName TEXT NOT NULL,
                        JobType TEXT NOT NULL, 
                        CronExpression TEXT NOT NULL,
                        IsActive INTEGER NOT NULL DEFAULT 1,
                        RetryCount INTEGER DEFAULT 0,
                        MaxRetry INTEGER DEFAULT 3,
                        LastRunTime TEXT NULL,
                        NextRunTime TEXT NULL,
                        Status TEXT NULL,
                        Message TEXT NULL,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
            using var cmd2 = new SqliteCommand(sql, con);
            cmd2.ExecuteNonQuery();
        }

        private static void CreateJobSettingsTable()
        {
            using var con = new SqliteConnection(ConnectionString);
            con.Open();
            var sql = @"CREATE TABLE IF NOT EXISTS JobDetails (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        JobId INTEGER NOT NULL,                                                
                        Details TEXT NULL    ,
                        CreatedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
                        UpdatedAt DATETIME DEFAULT CURRENT_TIMESTAMP
                    );";
            using var cmd2 = new SqliteCommand(sql, con);
            cmd2.ExecuteNonQuery();
        }
        
        private static void UpdateCrashedStatusJOb()
        {
            using var con = new SqliteConnection(ConnectionString);
            con.Open();
            var cmd = new SqliteCommand(@"UPDATE Jobs SET Status =@status,NextRunTime =@nextRun
                       WHERE IsActive=@active and Status ='running'", con);           
            cmd.Parameters.AddWithValue("@status", "");
            cmd.Parameters.AddWithValue("@nextRun", DateTime.Now);
            cmd.Parameters.AddWithValue("@active", 1);           
            cmd.ExecuteNonQuery();
        }


    }
}
