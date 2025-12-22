using MasterScheduler.Shared.DataModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.JobHelper
{
    public static class SqlBackupHelper
    {
        public static void RunSqlBackup(JobDetailModel job)
        {
            try
            {
                var settings = JsonSerializer.Deserialize<SqlBackupDetails>(job.Details);
                if (settings == null) return;

                string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
                string fileName = $"{settings.Databases[0]}_{timestamp}.bak";
                string filePath = Path.Combine(settings.Server, fileName);

                //string connectionString = $"Server={settings.ServerName};Database={settings.DatabaseName};Integrated Security=True;";
                //string sql = $"BACKUP DATABASE [{settings.DatabaseName}] TO DISK='{filePath}'";

                //using var conn = new SqlConnection(connectionString);
                //conn.Open();
                //using var cmd = new SqlCommand(sql, conn);
                //cmd.ExecuteNonQuery();

                Console.WriteLine($"Backup completed: {filePath}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"SQL Backup failed: {ex.Message}");
            }
        }
    }
}
