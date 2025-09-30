using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Worker.Services
{
    public class JobRepository
    {
        private readonly string _connectionString = "Data Source=jobs.db";

        //public async Task<IEnumerable<JobModel>> GetAllJobsAsync()
        //{
        //    using var conn = new SQLiteConnection(_connectionString);
        //    return await conn.QueryAsync<JobModel>("SELECT * FROM ScheduledJobs");
        //}

        //public async Task AddOrUpdateJobAsync(JobModel job)
        //{
        //    using var conn = new SQLiteConnection(_connectionString);
        //    await conn.ExecuteAsync(@"
        //    INSERT OR REPLACE INTO ScheduledJobs 
        //    (JobId, Name, ScheduleType, CronExpression, IntervalValue, IntervalUnit, OneTimeDateTime, IsEnabled)
        //    VALUES (@JobId, @Name, @ScheduleType, @CronExpression, @IntervalValue, @IntervalUnit, @OneTimeDateTime, @IsEnabled)",
        //        job);
        //}

        //public async Task UpdateJobStatusAsync(int jobId, string status, DateTime? lastRun, DateTime? nextRun)
        //{
        //    using var conn = new SQLiteConnection(_connectionString);
        //    await conn.ExecuteAsync(@"
        //    UPDATE ScheduledJobs 
        //    SET LastStatus=@Status, LastRun=@LastRun, NextRun=@NextRun
        //    WHERE JobId=@JobId",
        //        new { JobId = jobId.ToString(), Status = status, LastRun = lastRun?.ToString("s"), NextRun = nextRun?.ToString("s") });
        //}

        //public async Task DeleteJobAsync(int jobId)
        //{
        //    using var conn = new SQLiteConnection(_connectionString);
        //    await conn.ExecuteAsync("DELETE FROM ScheduledJobs WHERE JobId=@JobId", new { JobId = jobId.ToString() });
        //}
    }
}
