using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Shared.Data;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;
using static System.Reflection.Metadata.BlobBuilder;

namespace MasterScheduler.ViewModels
{
    public partial class HomeViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly JobRepository _repo = new JobRepository();
        private DispatcherTimer? _timer;
        private readonly Random _random = new();
        public ObservableCollection<ScheduledJobs> BackupJobs { get; }
        = new ObservableCollection<ScheduledJobs>();
        private ObservableCollection<ScheduledJobs>? _jobs;
        public HomeViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            _= LoadAllJobs();
            //Start(BackupJobs);
        }

        [RelayCommand]
        private async Task LoadAllJobs()
        {
            BackupJobs.Clear();
            var jobs = _repo.GetAll();
            // 2️⃣ ADD or UPDATE jobs
            foreach (var job in jobs)
            {
                BackupJobs.Add(new ScheduledJobs
                {
                    Id = job.Id,
                    JobName = job.JobName,
                    JobType = job.JobType,
                    NextRunAt = job.NextRunTime.ToString() ?? "--",
                    LastRunAt = job.LastRunTime.ToString() ?? "--",
                    Status = job.Status ?? "--",
                    StatusMessage = job.Message ?? "",
                    IsActive = job.IsActive
                });
            }
        }

        [RelayCommand]
        private void AddJob()
        {
            _navigation.NavigateTo<TaskTypeSelectionViewModel>();
        }

        public void Start(ObservableCollection<ScheduledJobs> jobs)
        {

            _jobs = jobs;
            _timer = new DispatcherTimer();

            _timer.Interval = TimeSpan.FromSeconds(1);

            _timer.Tick += Timer_Tick;

            _timer.Start();
        }

        private void Timer_Tick(object? sender, EventArgs e)
        {
            if (_jobs == null)
                return;

            foreach (var job in _jobs)
            {
                if (job.Percent >= 100)
                {
                    job.Percent = 100;
                    job.Status = "Completed";
                    job.Speed = "0 MB/s";
                    job.Eta = "--";
                    job.StatusMessage = "Backup Completed";
                    continue;
                }

                job.Percent += _random.Next(2, 8);

                if (job.Percent > 100)
                    job.Percent = 100;

                job.Speed = $"{_random.Next(30, 180)} MB/s";

                job.Eta = $"{100 - (int)job.Percent} sec";

                job.Progress = $"{job.Percent:0}%";

                job.Status = "Running";

                job.StatusMessage = $"Started {DateTime.Now:hh:mm:ss tt}";

                //if (job.Percent < 20)
                //    job.Message = "Preparing Backup";

                //else if (job.Percent < 40)
                //    job.Message = "Reading Database Pages";

                //else if (job.Percent < 60)
                //    job.Message = "Compressing";

                //else if (job.Percent < 90)
                //    job.Message = "Writing Backup";

                //else
                //    job.Message = "Verifying Backup";
            }
        }
        public void Stop()
        {
            _timer?.Stop();
        }
        private void LoadDemoJobs()
        {
            BackupJobs.Add(new ScheduledJobs
            {
                JobName = "ERP Backup",
                DatabaseName = "ERP",
                JobType = "Full Backup",
                ScheduledMessage = "Daily 10 PM",
                ScheduledAt = "10:00 PM",
                LastRunAt = "Yesterday",
                NextRunAt = "Today",
                Percent = 0,
                Progress = "Starting...",
                Speed = "--",
                Eta = "--",
                Status = "Waiting"                
            });

            BackupJobs.Add(new ScheduledJobs
            {
                JobName = "Sales Backup",
                DatabaseName = "Sales",
                JobType = "Differential",
                ScheduledMessage = "Every Hour",
                ScheduledAt = "11:00 AM",
                LastRunAt = "10:00 AM",
                NextRunAt = "12:00 PM",
                Percent = 0,
                Progress = "Starting...",
                Speed = "--",
                Eta = "--",
                Status = "Waiting",                
            });
        }
    }
}
