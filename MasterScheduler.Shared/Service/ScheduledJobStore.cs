using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Shared.Interface;
using MasterScheduler.Shared.JobHelper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Numerics;

namespace MasterScheduler.Shared.Service
{
    public class ScheduledJobStore : IScheduledJobStore
    {
        private IJobRepository _jobRepository;
        private ILogger<ScheduledJobStore> _logger;
        private IEmailService _emailService;
        public ScheduledJobStore(IJobRepository jobRepository, ILogger<ScheduledJobStore> logger,IEmailService emailService)
        {
            _jobRepository = jobRepository;
            _logger = logger;
            _emailService = emailService;
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
                    await Task.Delay(1000, token);
                    bool allFinished = true;
                    foreach (var dest in sqlBackupDetails.Destinations)
                    {
                        //if (dest.Status == "Success") continue;
                        try
                        {
                            await SendToDestinationAsync(localPath, dest, job.Id, token);
                            dest.Status = "Success";
                            _jobRepository.UpdateJobConfiguration(job.Id, sqlBackupDetails); // Save progress
                        }
                        catch (OperationCanceledException)
                        {
                            dest.Status = "Cancelled";
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
                    if (File.Exists(localPath))
                    {
                        await DeleteFileWithRetryAsync(localPath, 3);
                        _logger.LogInformation("Deleted temp file for Job {Id}", job.Id);
                    }
                }               
            }

            if (!string.IsNullOrWhiteSpace(sqlBackupDetails?.Notifications?.EmailOnSuccess))
            {
                
                await _emailService.SendEmailAsync(sqlBackupDetails?.Notifications?.EmailOnSuccess, sqlBackupDetails?.JobName, $"The scheduler finished successfully at {DateTime.Now}", token);
                _logger.LogWarning("SQL Backup Notification succes for Job {Id}", job.Id);
            }
            
        }
        async Task DeleteFileWithRetryAsync(string path, int retries)
        {
            for (int i = 0; i < retries; i++)
            {
                try
                {
                    File.Delete(path);
                    _logger.LogInformation("Deleted temp file: {path}", path);
                    return;
                }
                catch (IOException)
                {
                    if (i == retries - 1) throw;
                    await Task.Delay(2000); // Wait 2 seconds and try again
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
                    
                    var driveConfig = (GoogleDriveConfig)destination.Config;
                    GoogleDriveHelper googleDriveHelper = new GoogleDriveHelper();
                    var cred = await googleDriveHelper.GetAccountCredentialsAsync(driveConfig.UserEmail);
                    await googleDriveHelper.UploadBackup(cred,filePath, driveConfig, token);
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
       
        // Use a dictionary or DB to store URIs for jobs in progress
        private static readonly ConcurrentDictionary<int, Uri> _resumeUris = new();
       
    }
}
