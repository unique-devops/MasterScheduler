using Dapper;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using Quartz;
using Quartz.Impl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Worker.Services
{
    public class JobSchedulerService
    {
        private readonly IScheduler _scheduler;
        private readonly JobRepository _repo = new JobRepository();

        public JobSchedulerService()
        {
            _scheduler = StdSchedulerFactory.GetDefaultScheduler().Result;
        }

        public async Task StartAsync()
        {
            await _scheduler.Start();
            var jobs = _repo.GetAll().FindAll(j => j.IsActive);
            foreach (var job in jobs)
                await ScheduleJob(job);
        }

        private async Task ScheduleJob(JobModel job)
        {
            var jobDetail = JobBuilder.Create<JobExecutor>()
                .WithIdentity(job.JobName)
                .UsingJobData("JobId", job.Id)
                .UsingJobData("JobType", job.JobType)
                .Build();

            jobDetail.JobDataMap.Put("JobModel", job);

            var trigger = TriggerBuilder.Create()
                .WithIdentity($"{job.JobName}_trigger")
                .WithCronSchedule(job.CronExpression)
                .Build();

            await _scheduler.ScheduleJob(jobDetail, trigger);
        }

        public async Task StopAsync()
        {
            await _scheduler.Shutdown();
        }

        public async Task StopJobAsync(string jobName)
        {
            await _scheduler.DeleteJob(new JobKey(jobName));
        }

        public async Task StartJobAsync(JobModel job)
        {
            await ScheduleJob(job);
        }
    }
}
