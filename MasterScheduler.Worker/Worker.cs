using MasterScheduler.Shared;
using MasterScheduler.Worker.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace MasterScheduler.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IHubContext<JobStatusHub> _hub;
        public Worker(ILogger<Worker> logger,IHubContext<JobStatusHub> hub)
        {
            _logger = logger;
            _hub = hub;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var random = new Random();

            while (!stoppingToken.IsCancellationRequested)
            {
                // simulate job every 30 seconds
                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

                var job = new JobStatusDto
                {
                    JobId = 1,
                    Name = "SQL Backup - CustomersDB",
                    Status = "Running",
                    Progress = 0,
                    LastRun = DateTime.Now
                };

                // update progress in loop
                for (int i = 0; i <= 100; i += 10)
                {
                    job.Progress = i;
                    await _hub.Clients.All.SendAsync("ReceiveJobUpdate", job, stoppingToken);
                    await Task.Delay(500, stoppingToken);
                }

                job.Status = "Success";
                job.NextRun = DateTime.Now.AddMinutes(10);

                await _hub.Clients.All.SendAsync("ReceiveJobUpdate", job, stoppingToken);
            }
            //while (!stoppingToken.IsCancellationRequested)
            //{
            //    if (_logger.IsEnabled(LogLevel.Information))
            //    {
            //        _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            //    }
            //    await Task.Delay(1000, stoppingToken);
            //}
        }
    }
}
