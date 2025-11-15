using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.JobHelper;
using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MasterScheduler.Worker
{
    public class JobExecutor : IJob
    {
        public Task Execute(IJobExecutionContext context)
        {
            var jobData = context.MergedJobDataMap["JobModel"] as JobModel;
            if (jobData == null) return Task.CompletedTask;

            Console.WriteLine($"Running {jobData.JobType} job: {jobData.JobName}");

            switch (jobData.JobType.ToUpper())
            {
                case "SQLBACKUP":
                    SqlBackupHelper.RunSqlBackup(jobData);
                    break;
                // future: add more job types here
                case "FileCleanup":
                    FileCleanupHelper.RunFileCleanup(jobData);
                    break;
                default:
                    Console.WriteLine("Unknown job type");
                    break;
            }

            return Task.CompletedTask;
        }

       

       
        
    }
}
