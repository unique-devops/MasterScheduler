using MasterScheduler.Shared;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Worker.Hubs
{
    public class JobStatusHub : Hub
    {
        private static CancellationTokenSource _cts = new();

        public async Task StartJob()
        {
            _cts = new();
            _ = RunManualBackup(_cts.Token);
            await Clients.All.SendAsync("ReceiveJobCommand", "Started manually");
        }

        public async Task StopJob()
        {
            _cts.Cancel();
            await Clients.All.SendAsync("ReceiveJobCommand", "Stopped manually");
        }

        private async Task RunManualBackup(CancellationToken token)
        {
            var job = new JobStatusDto
            {
                JobId = 2,
                Name = "Manual SQL Backup",
                Status = "Running",
                Progress = 0,
                LastRun = DateTime.Now
            };

            for (int i = 0; i <= 100 && !token.IsCancellationRequested; i += 10)
            {
                job.Progress = i;
                await Clients.All.SendAsync("ReceiveJobUpdate", job);
                await Task.Delay(500, token);
            }

            if (!token.IsCancellationRequested)
            {
                job.Status = "Success";
                job.NextRun = DateTime.Now.AddMinutes(10);
                await Clients.All.SendAsync("ReceiveJobUpdate", job);
            }
        }

        public async Task SendJobUpdate(JobStatusDto job)
        {
            await Clients.All.SendAsync("ReceiveJobUpdate", job);
        }


    }
}
