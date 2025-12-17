using MasterScheduler.Shared;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using Microsoft.AspNetCore.SignalR;
using System.Collections.Concurrent;
using static Quartz.Logging.OperationName;

namespace MasterScheduler.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ConcurrentDictionary<int, CancellationTokenSource> _activeJobs = new();
        private readonly ILogger<Worker> _logger;
        private readonly JobRepository _repo = new JobRepository();
        private readonly PipeServer _pipeServer;
        static SemaphoreSlim _parallelLimit = new SemaphoreSlim(15);

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _pipeServer = new PipeServer();

        }        
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = _pipeServer.StartAsync(stoppingToken);

            // Use a short delay for responsiveness, but the core logic handles the wait
            // We check every 500ms for new jobs, but only execute them at their target time.
            const int checkIntervalMs = 500;

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // 1. Get all pending jobs, including their NextRunTime property
                    // (Assuming your job object now has a NextRunTime property populated by your scheduler logic)
                    var pendingJobs = _repo.GetPendingTask();

                    if (pendingJobs == null || pendingJobs.Count == 0)
                    {
                        // No jobs, just wait and check again
                        await Task.Delay(checkIntervalMs, stoppingToken);
                        continue;
                    }

                    // 2. Group jobs by their precise next run time
                    // Example: group all jobs scheduled for 2025-12-17 13:27:00
                    var jobGroups = pendingJobs
                        .GroupBy(job => job.NextRunTime)
                        .OrderBy(group => group.Key); // Execute the earliest group first

                    // 3. Process the next immediate group
                    var nextJobGroup = jobGroups.FirstOrDefault();

                    if (nextJobGroup != null)
                    {
                        DateTime targetTime = nextJobGroup.Key ?? DateTime.Now;

                        // A. Calculate remaining wait time
                        TimeSpan timeUntilTarget = targetTime - DateTime.Now;

                        // B. Wait if the target time is in the future
                        if (timeUntilTarget.TotalMilliseconds > 50) // Use a small buffer (50ms) to avoid over-waiting
                        {
                            _logger.LogInformation("Waiting {ms}ms until target execution time: {time}",
                                (int)timeUntilTarget.TotalMilliseconds, targetTime);

                            // Wait until just before the target time
                            await Task.Delay(timeUntilTarget, stoppingToken);
                            // Add a tiny extra check here to avoid over-waiting if Task.Delay returns late.
                        }

                        // C. Dispatch all jobs in the group concurrently
                        _logger.LogInformation("Starting {count} jobs exactly at: {time}",
                            nextJobGroup.Count(), DateTimeOffset.Now);

                        // Collect the tasks into a list
                        var concurrentTasks = new List<Task>();

                        foreach (var job in nextJobGroup)
                        {
                            // Crucial: Use 'TryMarkRunning' and 'Task.Run' inside the Task list construction
                            // to ensure all checks and starts happen immediately after the wait.
                            if (TryMarkRunning(job))
                            {
                                // Add the execution task to the list
                                concurrentTasks.Add(Task.Run(() => ExecuteJob(job, stoppingToken)));
                            }
                        }

                        // D. Wait for all dispatched tasks to complete (optional, but necessary if you need
                        // to know when the group is done before moving to the next group or loop iteration)
                        await Task.WhenAll(concurrentTasks);

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
        private bool TryMarkRunning(JobModel job)
        {
            try
            {
                job.Status = "running";
                job.LastRunTime = DateTime.Now;
                job.NextRunTime = CronosHelper.GetNextRunTime(job.CronExpression);
                _repo.Update(job);
                return true;
            }
            catch
            {
                return false;
            }

        }

        async Task ExecuteJob(JobModel job, CancellationToken serviceToken)
        {
            var jobCts = new CancellationTokenSource();

            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(serviceToken, jobCts.Token);

            // 3. Register the job so the UI/PipeServer can find it
            _activeJobs[job.Id] = jobCts;

            await _parallelLimit.WaitAsync(linkedCts.Token);

            try
            {
                _logger.LogInformation("Starting Job {id}", job.Id);
                await Task.Delay(5000, linkedCts.Token); // simulate work               
                job.Status = "completed";
                _repo.Update(job);
            }
            catch (OperationCanceledException)
            {
                _logger.LogWarning("Job {id} was cancelled.", job.Id);
                job.Status = "cancelled";
                _repo.Update(job);
            }
            catch (Exception ex)
            {
                job.Status = "error: " + ex.Message;
                _repo.Update(job);
            }
            finally
            {
                _parallelLimit.Release();
                _activeJobs.TryRemove(job.Id, out _); // Clean up
                jobCts.Dispose();
            }
        }

        public void CancelJob(int jobId)
        {
            if (_activeJobs.TryGetValue(jobId, out var cts))
            {
                cts.Cancel();
                _logger.LogInformation("Cancellation requested for job {id}", jobId);
            }
        }
    }
}
