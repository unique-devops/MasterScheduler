using MasterScheduler.Shared;
using MasterScheduler.Worker.Hubs;
using MasterScheduler.Worker.Jobs;
using Microsoft.AspNetCore.SignalR;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Worker.Services
{
    public class SchedulerService
    {
        private readonly IScheduler _scheduler;
        private readonly JobRepository _repo;
        private readonly IHubContext<JobStatusHub> _hub;

        public SchedulerService(JobRepository repo, IHubContext<JobStatusHub> hub)
        {
            _repo = repo;
            _hub = hub;
            _scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
        }
        public async Task StartAsync()
        {
            await _scheduler.Start();
            await LoadJobsFromDb();
        }

        private async Task LoadJobsFromDb()
        {
            var jobs = await _repo.GetAllJobsAsync();
            foreach (var job in jobs.Where(j => j.IsEnabled==1))
            {
                var jobDetail = JobBuilder.Create<SqlBackupJob>()
                    .WithIdentity(job.JobId.ToString(), "SQLBackups")
                    .Build();

                ITrigger trigger = BuildTrigger(job);
                if (trigger != null)
                    await _scheduler.ScheduleJob(jobDetail, trigger);
            }
        }

        private ITrigger BuildTrigger(JobModel job)
        {
            return job.ScheduleType switch
            {
                "Daily" => TriggerBuilder.Create().WithCronSchedule(job.CronExpression).Build(),
                "Weekly" => TriggerBuilder.Create().WithCronSchedule(job.CronExpression).Build(),
                "One-Time" => TriggerBuilder.Create().StartAt(DateTime.Parse(job.OneTimeDateTime)).Build(),
                "Interval" => TriggerBuilder.Create().WithSimpleSchedule(x => x.WithInterval(GetInterval(job)).RepeatForever()).Build(),
                "Custom" => TriggerBuilder.Create().WithCronSchedule(job.CronExpression).Build(),
                _ => null
            };
        }

        private TimeSpan GetInterval(JobModel job) =>
         job.IntervalUnit switch
         {
             "Seconds" => TimeSpan.FromSeconds(job.IntervalValue ?? 1),
             "Minutes" => TimeSpan.FromMinutes(job.IntervalValue ?? 1),
             "Hours" => TimeSpan.FromHours(job.IntervalValue ?? 1),
             _ => TimeSpan.FromMinutes(1)
         };

        public async Task SetJobEnabled(Guid jobId, bool isEnabled)
        {
            var jobKey = new JobKey(jobId.ToString(), "SQLBackups");

            if (!isEnabled)
                await _scheduler.PauseJob(jobKey);
            else
                await _scheduler.ResumeJob(jobKey);

            var job = (await _repo.GetAllJobsAsync()).FirstOrDefault(j => j.JobId == jobId);
            if (job != null)
            {
                job.IsEnabled = isEnabled ? 1 : 0;
                await _repo.AddOrUpdateJobAsync(job);
            }
        }
    }
}
