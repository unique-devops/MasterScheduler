using MasterScheduler.Shared;
using MasterScheduler.ViewModels;
using MasterScheduler.Views;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Service
{
    public class SignalRService
    {
        private HubConnection _connection;
        private DashboardViewModel _vm;

        public SignalRService(DashboardViewModel vm)
        {
            _vm = vm;
            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/jobHub")
                .WithAutomaticReconnect()
                .Build();
        }

        public async Task StartAsync()
        {
            _connection.On<JobStatusDto>("ReceiveJobUpdate", job =>
            {
                var existing = _vm.Jobs.FirstOrDefault(x => x.Id == job.JobId);
                if (existing != null)
                {
                    existing.Status = job.Status;
                    existing.Progress = job.Status;
                    existing.LastRun = job.LastRun;
                    existing.NextRun = job.NextRun;
                }
            });

            await _connection.StartAsync();
        }
    }

}
