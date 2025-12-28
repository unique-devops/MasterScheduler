using Dapper;
using MasterScheduler.Shared.Data;
using Microsoft.Data.Sqlite;
using Serilog.Core;
using Serilog.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Logging
{
    public class MySqliteSink : ILogEventSink
    {
        private readonly string _connectionString;

        public MySqliteSink()
        {
            _connectionString = DatabaseHelper.ConnectionString;
            InitializeDatabase();
        }
        public void InitializeDatabase()
        {
            using var conn = new SqliteConnection(_connectionString);
            // This adds the logging table to your existing Job database file
            conn.Execute(@"CREATE TABLE IF NOT EXISTS BackupLogs (
                    Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Timestamp DATETIME,
                    JobId INTEGER,
                    Level TEXT,
                    Message TEXT,
                    Exception TEXT);
                    
                    CREATE INDEX IF NOT EXISTS idx_logs_jobid ON BackupLogs(JobId);");
        }
        public void Emit(LogEvent logEvent)
        {
            using var conn = new SqliteConnection(_connectionString);

            // Extract the JobId from the LogContext
            int? jobId = null;
            if (logEvent.Properties.TryGetValue("JobId", out var value) && value is ScalarValue scalar)
            {
                if (int.TryParse(scalar.Value?.ToString(), out int id)) jobId = id;
            }

            conn.Execute(@"INSERT INTO BackupLogs (Timestamp, JobId, Level, Message, Exception) 
                           VALUES (@Timestamp, @JobId, @Level, @Message, @Exception)", new
            {
                Timestamp = logEvent.Timestamp.DateTime,
                JobId = jobId,
                Level = logEvent.Level.ToString(),
                Message = logEvent.RenderMessage(),
                Exception = logEvent.Exception?.ToString()
            });
        }
    }
}
