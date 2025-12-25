using MasterScheduler.Shared.DataModels;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.JobHelper
{
    public class JobStore
    {
        public async Task PerformSqlBackupAsync(string connectionString, string dbName, string path, CancellationToken ct)
        {
            try
            {
                using var conn = new SqlConnection(connectionString);
                await conn.OpenAsync(ct);

                var sql = $"BACKUP DATABASE @db TO DISK = @path WITH FORMAT, MEDIANAME = 'SQLBackup', NAME = @name";
                using var cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@db", dbName);
                cmd.Parameters.AddWithValue("@name", "Full Backup of " + dbName);
                cmd.Parameters.AddWithValue("@path", path);

                // CommandTimeout needs to be high for large backups
                cmd.CommandTimeout = 0;

                await cmd.ExecuteNonQueryAsync(ct);
            }
            catch (Exception ex)
            {
                var ss = "Error:" + ex.Message;
            }
        }

        public async Task UploadToGoogleDriveAsync(string filePath, GoogleDriveConfig driveConfig, CancellationToken ct)
        {
            // Assume _driveService is injected/initialized
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = new List<string> { driveConfig.TargetFolderId }
            };

            using var stream = new FileStream(filePath, FileMode.Open);
            //var request = _driveService.Files.Create(fileMetadata, stream, "application/octet-stream");

            //// This allows the upload to be cancelled mid-stream
            //var progress = await request.UploadAsync(ct);

            //if (progress.Status == Google.Apis.Upload.UploadStatus.Failed)
            //{
            //    throw progress.Exception;
            //}
        }
    }
}
