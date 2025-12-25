using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
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
            // 1. Decrypt the Refresh Token (using the Cipher helper we created)
            string decryptedRefreshToken = Cipher.Unprotect(driveConfig.RefreshToken);

            // 2. Setup the Authorization Flow
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = driveConfig.ClientId,
                    ClientSecret = driveConfig.ClientSecret
                }
            });

            // 3. Create the Credential using the Refresh Token
            var tokenResponse = new TokenResponse { RefreshToken = decryptedRefreshToken };
            var credential = new UserCredential(flow, "user", tokenResponse);


            // 4. Initialize the Drive Service
            using var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "MasterScheduler"
            });

            // 5. Prepare File Metadata
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = string.IsNullOrEmpty(driveConfig.TargetFolderId) ? null : new List<string> { driveConfig.TargetFolderId }
            };

            // 6. Execute the Upload
            using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            var request = driveService.Files.Create(fileMetadata, stream, "application/octet-stream");

            // Optional: Add a progress tracker for large SQL files
            //request.ProgressChanged += (progress) =>
            //{
            //    Console.WriteLine($"Upload Status: {progress.Status} {progress.BytesSent} bytes sent.");
            //};

            await request.UploadAsync(ct);

            if (request.ResponseBody == null)
            {
                throw new Exception("Upload failed: No response from Google Drive.");
            }

        }
    }
}
