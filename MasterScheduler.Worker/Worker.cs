using MasterScheduler.Shared;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Shared.Interface;
using MasterScheduler.Shared.Service;
using Microsoft.AspNetCore.SignalR;
using Quartz.Spi;
using Serilog.Context;
using System.Collections.Concurrent;
using System.Text.Json;
using static Quartz.Logging.OperationName;

namespace MasterScheduler.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeJobs = new();
        private readonly ILogger<Worker> _logger;
        private readonly JobRepository _repo ;
        private readonly PipeServer _pipeServer;
        private readonly IScheduledJobStore _jobStore;
        static SemaphoreSlim _parallelLimit = new SemaphoreSlim(15);

        // Separate limits to prevent backups from starving small tasks
        private readonly SemaphoreSlim _generalLimit = new(10);
        private readonly SemaphoreSlim _backupLimit = new(2);
        LicenseService licenseService = new LicenseService();

        public Worker(ILogger<Worker> logger, IScheduledJobStore jobStore)
        {
            _logger = logger;
            _repo = new JobRepository();
            _jobStore = jobStore;
            _pipeServer = new PipeServer(id => RequestCancellation(id),id => RequestRunNow(id));           
        }        

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = _pipeServer.StartAsync(stoppingToken);

            // Use a short delay for responsiveness, but the core logic handles the wait
            // We check every 500ms for new jobs, but only execute them at their target time.
            const int checkIntervalMs = 1000;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    licenseService.UpdateLicense();
                     var pendingJobs = _repo.GetPendingTask();

                    if (pendingJobs == null || !pendingJobs.Any())
                    {
                        // No jobs, just wait and check again
                        await Task.Delay(checkIntervalMs, stoppingToken);
                        continue;
                    }
                    
                    var nextJobGroup = pendingJobs
                        .GroupBy(job => job.NextRunTime)
                        .OrderBy(group => group.Key).FirstOrDefault(); // Execute the earliest group first
                   

                    if (nextJobGroup != null)
                    {
                        DateTime targetTime = nextJobGroup.Key ?? DateTime.Now;                      
                        TimeSpan timeUntilTarget = targetTime - DateTime.Now;

                        // B. Wait if the target time is in the future
                        if (timeUntilTarget.TotalMilliseconds > 50) // Use a small buffer (50ms) to avoid over-waiting
                        {
                            _logger.LogInformation("Waiting {ms}ms until target execution time: {time}",(int)timeUntilTarget.TotalMilliseconds, targetTime);                         
                            await Task.Delay(timeUntilTarget, stoppingToken);
                            // Add a tiny extra check here to avoid over-waiting if Task.Delay returns late.
                        }
                       
                        _logger.LogInformation("Starting {count} jobs exactly at: {time}",nextJobGroup.Count(), DateTimeOffset.Now);                       
                        //var concurrentTasks = new List<Task>();

                        foreach (var job in nextJobGroup)
                        {                                                       
                            if (TryMarkRunning(job))
                            {
                                //concurrentTasks.Add(Task.Run(() => ExecuteJob(job, stoppingToken)));
                                _ = Task.Run(() => ExecuteJobAsync(job, stoppingToken), stoppingToken);
                            }
                        }                       
                        //await Task.WhenAll(concurrentTasks);
                        _logger.LogInformation("Completed execution of {count} jobs at: {time}",
                        nextJobGroup.Count(), DateTimeOffset.Now);
                    }
                }
                catch (OperationCanceledException)
                {
                    // Expected when the service is stopping
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "An error occurred during job scheduling loop.");
                }
                // Wait for a short interval before checking the repository again
                await Task.Delay(checkIntervalMs, stoppingToken);
            }
        }
        private async Task ExecuteJobAsync(JobModel job, CancellationToken serviceToken)
        {
            var jobCts = new CancellationTokenSource();
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(serviceToken, jobCts.Token);

            // 3. Register the job so the UI/PipeServer can find it
            _activeJobs[job.Id] = jobCts;

            var semaphore = job.JobType.ToLower() == "sqlbackup" ? _backupLimit : _generalLimit;

            await _parallelLimit.WaitAsync(linkedCts.Token);
            using (LogContext.PushProperty("JobId", job.Id))
            {
                try
                {
                    _logger.LogInformation("Starting {Type} Job {Id}", job.JobType, job.Id);                   
                    switch (job.JobType.ToLower())
                    {
                        case "sqlbackup":
                            await _jobStore.RunSqlBackupAsync(job, linkedCts.Token);
                            break;
                        default:
                            _logger.LogWarning("Unknown job type: {Type}", job.JobType);
                            break;
                    }
                    await UpdateJobStatus(job, "completed", "successfully completed");
                    _logger.LogInformation("Job {id} successfully completed.", job.Id);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogError("Job {id} was cancelled.", job.Id);
                    await UpdateJobStatus(job, "cancelled", "cancelled");                    
                }
                catch (Exception ex)
                {                   
                    await UpdateJobStatus(job, "error", ex.Message);
                    _logger.LogError(exception:ex, "Job {id} Error: " + ex.Message, job.Id);
                }
                finally
                {
                    semaphore.Release();
                    _activeJobs.TryRemove(job.Id, out _); // Clean up
                    jobCts.Dispose();
                }
            }

        }

        private async Task UpdateJobStatus(JobModel job, string status, string message)
        {
            job.Status = status;
            job.Message = message;
            _repo.Update(job);
            await Task.CompletedTask;
        }
        private bool TryMarkRunning(JobModel job)
        {
            try
            {                
                job.Status = "running";
                job.Message = "running";
                job.LastRunTime = DateTime.Now;
                job.NextRunTime = CronosHelper.GetNextRunTime(job.CronExpression);
                _repo.Update(job);
                return true;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "An error occurred during job marking status.");               
                return false;
            }

        }
        private void RequestCancellation(int jobId)
        {
            if (_activeJobs.TryGetValue(jobId, out var cts))
            {
                _logger.LogWarning("UI requested cancellation for Job {id}", jobId);
                cts.Cancel();                
            }            
        }

        private async void RequestRunNow(int jobId)
        {
            _logger.LogWarning("UI requested run now for Job {id}", jobId);            
            var job = _repo.GetById(jobId);            
            if (job == null) return;
            TryMarkRunning(job);
            await ExecuteJobAsync(job, new CancellationToken());
        }

    }
}
