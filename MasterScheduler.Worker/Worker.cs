using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using Microsoft.AspNetCore.SignalR;

namespace MasterScheduler.Worker
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly JobRepository _repo = new JobRepository();
        private readonly PipeServer _pipeServer;
        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
            _pipeServer = new PipeServer();

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _ = _pipeServer.StartAsync(stoppingToken);  // start pipe server
            while (!stoppingToken.IsCancellationRequested)
            {
                if (_logger.IsEnabled(LogLevel.Information))
                {
                    _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                }
                AddNew();
                await Task.Delay(10000, stoppingToken);
            }
        }

        private void AddNew()
        {
            var job = new JobModel
            {
                JobName = "SQlJOb",
                JobType = "SqlBackup",
                CronExpression = "0 0/5 * * * ?",
                IsActive = true,
            };          
            _repo.Add(job);                     
        }
    }
}
