using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Drive.v3;
using Google.Apis.Services;
using Google.Apis.Upload;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Shared.Interface;
using MasterScheduler.Shared.JobHelper;
using Microsoft.Data.SqlClient;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Service
{
    public class ScheduledJobStore : IScheduledJobStore
    {
        private IJobRepository _jobRepository;
        private ILogger<ScheduledJobStore> _logger;
        public ScheduledJobStore(IJobRepository jobRepository, ILogger<ScheduledJobStore> logger)
        {
            _jobRepository = jobRepository;
            _logger = logger;
        }
        public async Task RunSqlBackupAsync(JobModel job, CancellationToken token)
        {
            var sqlBackupDetails = _jobRepository.GetJobConfiguration<SqlBackupDetails>(job.Id);
            if (sqlBackupDetails == null)
            {
                _logger.LogWarning("SQL Backup configuration missing for Job {Id}", job.Id);
                return;
            }                                    
            foreach (var db in sqlBackupDetails.Databases)
            {
                string localPath = Path.Combine(GetDefaultSQLBackupPath(sqlBackupDetails.ConnectionString), $"{db}_{DateTime.Now:yyyyMMddHHmm}.bak");
               
                try
                {
                    _logger.LogInformation("Starting SQL Backup for {db}...", db);
                    await PerformSqlBackupAsync(sqlBackupDetails.ConnectionString, db, localPath, token);
                    _logger.LogInformation("SQL Backup to Temp successful: {path}", localPath);

                    bool allFinished = true;
                    foreach (var dest in sqlBackupDetails.Destinations)
                    {
                        if (dest.Status == "Success") continue;
                        try
                        {
                            await SendToDestinationAsync(localPath, dest, job.Id, token);
                            dest.Status = "Success";
                            _jobRepository.UpdateJobConfiguration(job.Id, sqlBackupDetails); // Save progress
                        }
                        catch (OperationCanceledException)
                        {
                            dest.Status = "Paused";
                            _jobRepository.UpdateJobConfiguration(job.Id, sqlBackupDetails);
                            allFinished = false;
                            throw; // Stop the loop
                        }
                        catch (Exception)
                        {
                            dest.Status = "Error";
                            _jobRepository.UpdateJobConfiguration(job.Id, sqlBackupDetails);
                            allFinished = false;
                            // Continue to next destination or stop based on your preference
                        }                       
                    }
                    // 4. Always cleanup the temp file, even if an upload failed
                    if (File.Exists(localPath) && allFinished)
                    {
                        File.Delete(localPath);
                        _logger.LogInformation("Deleted temp file for Job {Id}", job.Id);
                    }
                }
                catch (OperationCanceledException)
                {
                    _logger.LogWarning("Backup for {db} was cancelled by user.", db);
                    throw; // Re-throw so the Worker knows it was cancelled
                }
                catch (SqlException ex)
                {
                    _logger.LogError(ex, "SQL Server Error during backup of {db}", db);
                    throw; // Let the Worker handle the DB status update
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process backup for {db}", db);
                    throw;
                }
                finally
                {
                    
                }               
            }
        }

        public string GetDefaultSQLBackupPath(string connectionString)
        {
            string defaultSqlPath = "";
            using var con = new SqlConnection(connectionString);
            con.Open();
            using var cmd = new SqlCommand("SELECT SERVERPROPERTY('InstanceDefaultBackupPath') AS DefaultPath", con);
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                defaultSqlPath = reader["DefaultPath"].ToString()!;
            }
            return defaultSqlPath;
        }
        private async Task PerformSqlBackupAsync(string connectionString, string dbName, string path, CancellationToken ct)
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

        private async Task SendToDestinationAsync(string filePath, BackupDestination destination, int jobId, CancellationToken token)
        {
            try
            {
                if (destination.Type == DestinationType.LocalFolder)
                {
                    var config = (LocalFolderConfig)destination.Config;
                    string targetFile = Path.Combine(config.TargetPath, Path.GetFileName(filePath));

                    // Use Async copy for better performance
                    using var sourceStream = File.OpenRead(filePath);
                    using var destStream = File.Create(targetFile);
                    await sourceStream.CopyToAsync(destStream, token);

                    _logger.LogInformation("Backup to local path: {path} (Job {id})", targetFile, jobId);
                }
                else if (destination.Type == DestinationType.GoogleDrive)
                {
                    await UploadToGoogleDriveAsync(filePath, (GoogleDriveConfig)destination.Config, token);
                    _logger.LogInformation("Uploaded to Google Drive (Job {id})", jobId);

                    //if (sqlBackupDetails.RetentionDays > 0)
                    //{
                    //    await CleanOldGoogleDriveBackupsAsync(gdriveDest, sqlBackupDetails.RetentionDays, token);
                    //}
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send backup to {Type} for Job {Id}", destination.Type, jobId);
                // We don't throw here if you want other destinations to still try even if one fails
            }
        }
        private async Task UploadToGoogleDriveAsyncWithoutResume(string filePath, GoogleDriveConfig driveConfig, CancellationToken ct)
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

        // Use a dictionary or DB to store URIs for jobs in progress
        private static readonly ConcurrentDictionary<int, Uri> _resumeUris = new();
        private async Task UploadToGoogleDriveAsync(string filePath, GoogleDriveConfig driveConfig, CancellationToken ct)
        {
            // 1. Decrypt Token
            string decryptedRefreshToken = Cipher.Unprotect(driveConfig.RefreshToken);

            // 2. Setup Flow
            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets
                {
                    ClientId = driveConfig.ClientId,
                    ClientSecret = driveConfig.ClientSecret
                }
            });

            // 3. Create Credential and FORCE REFRESH
            var tokenResponse = new TokenResponse { RefreshToken = decryptedRefreshToken };
            var credential = new UserCredential(flow, "user", tokenResponse);

            // CRITICAL: Ensure the access token is fresh before starting a long backup upload
            if (credential.Token.IsStale)
            {
                await credential.RefreshTokenAsync(ct);
            }

            // 4. Initialize Service
            using var driveService = new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "MasterScheduler"
            });

            // 5. Prepare Metadata
            var fileMetadata = new Google.Apis.Drive.v3.Data.File()
            {
                Name = Path.GetFileName(filePath),
                Parents = string.IsNullOrEmpty(driveConfig.TargetFolderId)
                          ? null : new List<string> { driveConfig.TargetFolderId }
            };

            // 6. Execute Resumable Upload
            await using var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 4096, useAsync: true);

            var request = driveService.Files.Create(fileMetadata, stream, "application/octet-stream");

            // Set chunk size (e.g., 1MB chunks) for stability on large files
            request.ChunkSize = ResumableUpload.MinimumChunkSize * 4;
            // --- RESUME LOGIC START ---
            var jobId = 1;
            if (_resumeUris.TryGetValue(jobId, out Uri sessionUri))
            {
                _logger.LogInformation("Found existing upload session for Job {Id}. Attempting to resume...", jobId);
                // This tells the SDK to try and pick up where it left off
                await request.ResumeAsync(sessionUri, ct);
            }
            else
            {
                // First time starting: Subscribe to find out the Session URI Google gives us
                request.ResponseReceived += (file) => _resumeUris.TryRemove(jobId, out _); // Cleanup on finish

                // This event captures the URI so we can save it if it fails later
                request.UploadSessionData += (uploadProgress) =>
                {
                    if (uploadProgress.UploadUri != null)
                    {
                        _resumeUris[jobId] = uploadProgress.UploadUri;
                    }
                };

                await request.UploadAsync(ct);
            }

            // --- RESUME LOGIC END ---

            if (request.GetProgress().Status == UploadStatus.Failed)
            {
                // If it failed, we DON'T remove the URI from _resumeUris. 
                // The next time the worker tries this job, it will hit the 'if' block above.
                throw new Exception($"Upload failed: {request.GetProgress().Exception.Message}");
            }
            //// Progress Tracking (logged to Serilog)
            //request.ProgressChanged += (progress) =>
            //{
            //    if (progress.Status == UploadStatus.Uploading)
            //        _logger.LogDebug("Job {Id}: Uploading to GDrive... {Bytes} bytes sent", driveConfig.JobId, progress.BytesSent);
            //    else if (progress.Status == UploadStatus.Failed)
            //        _logger.LogError(progress.Exception, "Job {Id}: GDrive Upload failed", driveConfig.JobId);
            //};

            //var finalStatus = await request.UploadAsync(ct);

            //if (finalStatus.Status == UploadStatus.Failed)
            //{
            //    throw new Exception($"Google Drive upload failed: {finalStatus.Exception?.Message}", finalStatus.Exception);
            //}
        }

        private DriveService GetDriveService(GoogleDriveConfig config)
        {
            string decryptedRefreshToken = Cipher.Unprotect(config.RefreshToken);

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer
            {
                ClientSecrets = new ClientSecrets { ClientId = config.ClientId, ClientSecret = config.ClientSecret }
            });

            var credential = new UserCredential(flow, "user", new TokenResponse { RefreshToken = decryptedRefreshToken });

            return new DriveService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "MasterScheduler"
            });
        }
        public async Task CleanOldGoogleDriveBackupsAsync(GoogleDriveConfig driveConfig, int retentionDays, CancellationToken ct)
        {
            // 1. Initialize Service (Reuse the same logic from Upload)
            var driveService = GetDriveService(driveConfig); // Helper to encapsulate the Flow/Credential logic

            // 2. Calculate the threshold date (RFC 3339 format required by Google)
            string dateThreshold = DateTime.UtcNow.AddDays(-retentionDays).ToString("yyyy-MM-ddTHH:mm:ssZ");

            // 3. Prepare the Query
            // 'modifiedTime < ...' finds old files
            // 'trashed = false' ensures we only look at active files
            // 'parents in ...' limits the search to your specific backup folder
            var request = driveService.Files.List();
            request.Q = $"modifiedTime < '{dateThreshold}' and trashed = false and '{driveConfig.TargetFolderId}' in parents";
            request.Fields = "files(id, name, modifiedTime)";

            var result = await request.ExecuteAsync(ct);

            if (result.Files != null && result.Files.Any())
            {
                _logger.LogInformation("Found {count} old backups to delete in Google Drive.", result.Files.Count);

                foreach (var file in result.Files)
                {
                    try
                    {
                        // 4. Delete the file
                        await driveService.Files.Delete(file.Id).ExecuteAsync(ct);
                        _logger.LogInformation("Deleted old GDrive backup: {name} (ID: {id})", file.Name, file.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Failed to delete old GDrive file {id}", file.Id);
                    }
                }
            }
        }
    }
}
