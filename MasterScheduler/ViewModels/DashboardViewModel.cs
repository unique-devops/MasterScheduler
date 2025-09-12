using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Shared;
using Microsoft.AspNetCore.SignalR.Client;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace MasterScheduler.ViewModels
{
    public class DashboardViewModel : ObservableObject
    {
        private readonly HubConnection _connection;
        public ObservableCollection<JobViewModel> Jobs { get; set; } = new();

        public ICommand StartCommand { get; }
        public ICommand StopCommand { get; }
        public ICommand ConfigCommand { get; }

        public DashboardViewModel()
        {
            ConfigCommand = new RelayCommand<JobViewModel>(ConfigJob);

            StartCommand = new RelayCommand<JobViewModel>(async job =>
            {
                await _connection.InvokeAsync("StartJob");
            });

            StopCommand = new RelayCommand<JobViewModel>(async job =>
            {
                await _connection.InvokeAsync("StopJob");
            });

            _connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:5000/jobHub")
                .WithAutomaticReconnect()
                .Build();

            _connection.On<JobStatusDto>("ReceiveJobUpdate", UpdateJob);
            _connection.On<string>("ReceiveJobCommand", msg =>
            {
                MessageBox.Show(msg);
            });

            _connection.StartAsync();
            
        }
        private void StartJob(JobViewModel job) => job.Status = "Starting...";
        private void StopJob(JobViewModel job) => job.Status = "Stopped";
        private void ConfigJob(JobViewModel job) => MessageBox.Show($"Config {job.JobName}");

        private void UpdateJob(JobStatusDto dto)
        {
            var job = Jobs.FirstOrDefault(x => x.Id == dto.Id);
            if (job == null)
            {
                job = new JobViewModel { Id = dto.Id, JobName = dto.JobName };
                Application.Current.Dispatcher.Invoke(() => Jobs.Add(job));
            }

            job.Status = dto.Status;
            job.Progress = dto.Progress;
            job.LastRun = dto.LastRun;
            job.NextRun = dto.NextRun;
        }
    }
}
