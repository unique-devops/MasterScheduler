using MasterScheduler.Worker.Services;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Worker.Jobs
{
    public class SqlBackupJob : IJob
    {
        private readonly JobRepository _repo;
        
        public SqlBackupJob(JobRepository repo)
        {
            _repo = repo;
        }

        public async Task Execute(IJobExecutionContext context)
        {
            var jobData = context.MergedJobDataMap;
            var jobId = jobData.GetInt("JobId");
            var jobName = jobData.GetString("JobName");
            var dbName = jobData.GetString("DatabaseName");
            var backupPath = jobData.GetString("BackupPath");
            try
            {
               
            }
            catch (Exception ex)
            {
                
            }
        }
    }
}
