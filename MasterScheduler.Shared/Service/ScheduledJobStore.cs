using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Shared.Interface;
using MasterScheduler.Shared.JobHelper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Collections.Concurrent;
using System.Linq;
using System.Numerics;

namespace MasterScheduler.Shared.Service
{
    public class ScheduledJobStore : IScheduledJobStore
    {
        private IJobRepository _jobRepository;
        private ILogger<ScheduledJobStore> _logger;
        private IEmailService _emailService;
        LicenseService licenseService = new LicenseService();
        public ScheduledJobStore(IJobRepository jobRepository, ILogger<ScheduledJobStore> logger,IEmailService emailService)
        {
            _jobRepository = jobRepository;
            _logger = logger;
            _emailService = emailService;
        }
        public async Task RunSqlBackupAsync(JobModel job, CancellationToken token)
        {
            var sqlBackupconfig = _jobRepository.GetJobConfiguration<SqlBackupDetails>(job.Id);
            if (sqlBackupconfig == null)
            {
                _logger.LogWarning("SQL Backup configuration missing for Job {Id}", job.Id);
                return;
            }                                    
            foreach (var db in sqlBackupconfig.Databases)
            {                
                var tempFileName = $"{db}_{DateTime.Now:yyyyMMddHHmm}.bak";
                var TempBackupPath = string.IsNullOrWhiteSpace(sqlBackupconfig.TempBackupPath) ? Path.Combine(GetDefaultSQLBackupPath(sqlBackupconfig.ConnectionString), tempFileName) : Path.Combine(sqlBackupconfig.TempBackupPath, tempFileName);
               
                try
                {
                    _logger.LogInformation("Starting SQL Backup for {db}...", db);
                    if (sqlBackupconfig.Compression.ToLower() == "zip" || sqlBackupconfig.Compression.ToLower() == "none")
                    {
                        await PerformSqlBackupAsync(sqlBackupconfig.ConnectionString, db, TempBackupPath, false, token);
                    }
                    else {
                        await PerformSqlBackupAsync(sqlBackupconfig.ConnectionString, db, sqlBackupconfig.TempBackupPath, true, token);
                    }
                    _logger.LogInformation("SQL Backup to Temp successful: {path}", sqlBackupconfig.TempBackupPath);
                    await Task.Delay(1000, token);

                    if (sqlBackupconfig.Compression.ToLower() == "zip")
                    {
                        await FileCompressionHelper.ZipCompressAsync(TempBackupPath.Replace(".bak",".zip"), TempBackupPath, token);
                        TempBackupPath = TempBackupPath.Replace(".bak", ".zip");
                    }


                    bool allFinished = true;
                    foreach (var dest in sqlBackupconfig.Destinations)
                    {
                        //if (dest.Status == "Success") continue;
                        try
                        {
                            await SendToDestinationAsync(db, TempBackupPath, dest, job.Id, token);
                            dest.Status = "Success";
                            _jobRepository.UpdateJobConfiguration(job.Id, sqlBackupconfig); // Save progress
                        }
                        catch (OperationCanceledException)
                        {
                            dest.Status = "Cancelled";
                            _jobRepository.UpdateJobConfiguration(job.Id, sqlBackupconfig);
                            allFinished = false;
                            throw; // Stop the loop
                        }
                        catch (Exception)
                        {
                            dest.Status = "Error";
                            _jobRepository.UpdateJobConfiguration(job.Id, sqlBackupconfig);
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
                    if (File.Exists(TempBackupPath))
                    {
                        await DeleteFileWithRetryAsync(TempBackupPath, 3);
                        _logger.LogInformation("Deleted temp file for Job {Id}", job.Id);
                    }
                }               
            }

            if (sqlBackupconfig.Notifications.ActiveAlert && !string.IsNullOrWhiteSpace(sqlBackupconfig?.Notifications?.EmailOnSuccess))
            {
                
                await _emailService.SendEmailAsync(sqlBackupconfig?.Notifications?.EmailOnSuccess, sqlBackupconfig?.JobName, $"The scheduler finished successfully at {DateTime.Now}", token);
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
        private async Task PerformSqlBackupAsync(string connectionString, string dbName, string path, bool IsCompressed, CancellationToken ct)
        {

            string safePath = path.Replace("'", "''");
            string safeDb = dbName.Replace("]", "]]");

            string sql = $"BACKUP DATABASE [{safeDb}] TO DISK = N'{safePath}' WITH INIT, FORMAT, MEDIANAME = 'SQLBackup', NAME = N'Full Backup of {safeDb}'";
            using var conn = new SqlConnection(connectionString);
            await conn.OpenAsync(ct);
            if (IsCompressed)
            {
                sql += ", COMPRESSION";
            }            
            using var cmd = new SqlCommand(sql, conn);
            //cmd.Parameters.AddWithValue("@db", dbName);
            //cmd.Parameters.AddWithValue("@name", "Full Backup of " + dbName);
            //cmd.Parameters.AddWithValue("@path", path);

            // CommandTimeout needs to be high for large backups
            cmd.CommandTimeout = 0;

            await cmd.ExecuteNonQueryAsync(ct);
        }

        private async Task SendToDestinationAsync(string dbName, string tempBackupFile, BackupDestination destination, int jobId, CancellationToken token)
        {
            try
            {
                var backFileName = Path.GetFileName(tempBackupFile);                
                
                if (destination.Type == DestinationType.LocalFolder)
                {
                    var config = (LocalFolderConfig)destination.Config;

                    if (config.RetentionDays > 0)
                    {
                        _logger.LogInformation("Cleaning up old backups for {db}...", dbName);
                        var directory = new DirectoryInfo(config.TargetPath);
                        DateTime cutoffDate = DateTime.Now.AddDays(config.RetentionDays * -1);
                        var oldFiles = directory.GetFiles($"{dbName}_*.bak")
                                .Where(f => f.LastWriteTime < cutoffDate);

                        var zipFiles = directory.GetFiles($"{dbName}_*.zip")
                                .Where(f => f.LastWriteTime < cutoffDate);

                        oldFiles.ToList().AddRange(zipFiles);

                        foreach (var file in oldFiles)
                        {
                            try
                            {
                                file.Delete();
                                _logger.LogInformation("Deleted old backup file: {name}", file.Name);
                            }
                            catch (Exception ex)
                            {
                                _logger.LogWarning("Failed to delete {file}: {msg}", file.Name, ex.Message);
                            }
                        }
                    }
                    if (config.TargetPath =="")
                    { 

                    }
                    string targetFile = Path.Combine(config.TargetPath, backFileName);

                    // Use Async copy for better performance
                    using var sourceStream = File.OpenRead(tempBackupFile);
                    using var destStream = File.Create(targetFile);
                    await sourceStream.CopyToAsync(destStream, token);
                    
                    _logger.LogInformation("Backup to local path: {path} (Job {id})", targetFile, jobId);
                }
                else if (destination.Type == DestinationType.GoogleDrive && IsEditionLicense("PRO"))
                {
                    
                    var driveConfig = (GoogleDriveConfig)destination.Config;                   

                    GoogleDriveHelper googleDriveHelper = new GoogleDriveHelper();
                    var cred = await googleDriveHelper.GetAccountCredentialsAsync(driveConfig.UserEmail);
                    await googleDriveHelper.UploadBackup(cred, Path.GetFileNameWithoutExtension(tempBackupFile), tempBackupFile, driveConfig, token);
                    _logger.LogInformation("Uploaded to Google Drive (Job {id})", jobId);

                    if (driveConfig.RetentionDays > 0)
                    {
                       // _logger.LogInformation("Cleaning up Google Drive backups for {db}...", dbName);
                       //var res =  await googleDriveHelper.CleanOldBackupsAsync(cred, driveConfig.TargetFolderId,driveConfig.RetentionDays, token);
                       //_logger.LogInformation("Deleted old Google Drive backup file: {name} : Status:" + res, dbName);                        
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send backup to {Type} for Job {Id}", destination.Type, jobId);                
            }
        }

       

        // Use a dictionary or DB to store URIs for jobs in progress
        private static readonly ConcurrentDictionary<int, Uri> _resumeUris = new();

        private bool IsEditionLicense(string editionName)
        {
            try
            {
                var lic = licenseService.GetLicByName(editionName);
                var exist = lic.Any(c => c.IsExpired == false);                
                return exist;
            }
            catch 
            {
                return false;
            }
           
        }
       
    }
}
