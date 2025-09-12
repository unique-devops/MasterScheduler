using MasterScheduler.Shared;
using MasterScheduler.Worker.Hubs;
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
        private readonly IHubContext<JobStatusHub> _hub;

        public SqlBackupJob(JobRepository repo, IHubContext<JobStatusHub> hub)
        {
            _repo = repo;
            _hub = hub;
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
                var job = new JobStatusDto
                {
                    JobId = jobId,
                    Name = jobName,
                    Status = "Running",
                    Progress = 0,
                    LastRun = DateTime.Now
                };

                // simulate backup for now
                for (int i = 0; i <= 100; i += 20)
                {
                    job.Progress = i;
                    await _hub.Clients.All.SendAsync("ReceiveJobUpdate", job);
                    await Task.Delay(500);
                }

                job.Status = "Success";
                job.NextRun = context.Trigger.GetNextFireTimeUtc()?.LocalDateTime;
                // 🔹 Add SQL backup logic here
                
                await _repo.UpdateJobStatusAsync(jobId, "Success", DateTime.Now, job.NextRun);
                await _hub.Clients.All.SendAsync("ReceiveJobUpdate", job);
            }
            catch (Exception ex)
            {
                await _repo.UpdateJobStatusAsync(jobId, "Failed: " + ex.Message, DateTime.Now, context.Trigger.GetNextFireTimeUtc()?.LocalDateTime);
                await _hub.Clients.All.SendAsync("JobUpdated", new { JobId = jobId, Status = "Failed: " + ex.Message });
            }
        }
    }
}
