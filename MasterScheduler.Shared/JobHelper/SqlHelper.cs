using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.JobHelper
{
    public class SqlHelper
    {
        private readonly JobRepository _repo = new JobRepository();
        private async Task SQLBackupJob(JobModel job, CancellationToken token)
        {
            var jobDetail = _repo.GetDetailById(job.Id);
            if (jobDetail == null)
                return;
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var sqlBackupDetails = JsonSerializer.Deserialize<SqlBackupDetails>(jobDetail?.Details, options);
            foreach (var db in sqlBackupDetails.Databases)
            {
                foreach (var destination in sqlBackupDetails.Destinations)
                {
                    try
                    {
                        if (destination.Type == DestinationType.LocalFolder)
                        {
                            var localDest = (LocalFolderConfig)destination.Config;
                            // 1. Generate local file path
                            string localPath = Path.Combine(localDest.TargetPath, $"{db}_{DateTime.Now:yyyyMMddHHmm}.bak");
                            // 2. Execute SQL Backup
                            //await _jobStore.PerformSqlBackupAsync(sqlBackupDetails.ConnectionString, db, localPath, token);
                            //_logger.LogInformation("SQL Backup completed for {id}", job.Id);

                        }
                        else
                        {
                            // 1. Generate local file path
                            string localPath = Path.Combine(Path.GetTempPath(), $"{db}_{DateTime.Now:yyyyMMddHHmm}.bak");
                            // 2. Execute SQL Backup
                            //await _jobStore.PerformSqlBackupAsync(sqlBackupDetails.ConnectionString, db, localPath, token);
                            //_logger.LogInformation("SQL Backup completed for {id}", job.Id);

                            //if (destination.Type == DestinationType.GoogleDrive)
                            //{
                            //    var gdriveDest = (GoogleDriveConfig)destination.Config;
                            //    await _jobStore.UploadToGoogleDriveAsync(localPath, gdriveDest, token);
                            //    _logger.LogInformation("Google Drive upload completed for {id}", job.Id);
                            //}
                            // 4. Cleanup local file
                            if (File.Exists(localPath)) File.Delete(localPath);
                        }

                    }
                    catch (Exception ex)
                    {
                        //_logger.LogError("Error:" + ex.Message);
                    }
                }

            }



        }
    }
}
