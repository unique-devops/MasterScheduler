using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace MasterScheduler.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly JobRepository _repo = new JobRepository();
        private readonly System.Timers.Timer _refreshTimer;
        [ObservableProperty]
        private ObservableCollection<ScheduledJobDto> jobs;

        [ObservableProperty] private ScheduledJobDto? _selectedJob;       
        public DashboardViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            Jobs = new();           
            LoadNewJobs();
        }

        [RelayCommand]
        private async Task LoadNewJobs()
        {           
            var jobData = _repo.GetAll();
            Application.Current.Dispatcher.Invoke(() =>
            {
                foreach (var jobRow in Jobs)
                {
                    var isAvailable = _repo.GetOrderById(jobRow.Id);
                    if (isAvailable == null)
                    {
                        Jobs.Remove(jobRow);
                    }                   
                }
                //Jobs.Clear();
                foreach (var j in jobData)
                {                    
                    string lastRun = j.LastRunTime.ToString() ?? "";
                    string nextRun = j.NextRunTime.ToString() ?? "";
                    var exit = Jobs.Where(c => c.Id == j.Id).ToList();
                    if (exit.Count == 0)
                    {
                        Jobs?.Add(new ScheduledJobDto { Id = j.Id, Name = j.JobName, JobType = j.JobType, NextRunAt = string.IsNullOrWhiteSpace(nextRun) ? "N/A" : nextRun, LastRunAt = string.IsNullOrWhiteSpace(lastRun) ? "N/A" : lastRun, Status = j.IsActive ? "Active" : "Inactive" });
                    }
                    
                }
            });
            await Task.Delay(1000);
            await LoadNewJobs();
        }
        [RelayCommand]
        private void LoadJobs()
        {
            Jobs.Clear();
            foreach (var j in _repo.GetAll())
            {
                string lastRun = j.LastRunTime.ToString() ?? "";
                string nextRun = j.NextRunTime.ToString() ?? "";
                Jobs.Add(new ScheduledJobDto { Id = j.Id, Name = j.JobName, JobType = j.JobType, NextRunAt = string.IsNullOrWhiteSpace(nextRun) ? "N/A" : nextRun, LastRunAt = string.IsNullOrWhiteSpace(lastRun) ? "N/A" : lastRun, Status = j.IsActive ? "Active" : "Inactive" });
            }

        }

        [RelayCommand]
        private void AddJob()
        {
           _navigation.NavigateTo<TaskTypeSelectionViewModel>();
        }

        private bool CanDelete() => SelectedJob != null;
        //[RelayCommand]
        [RelayCommand(CanExecute = nameof(CanDelete))]
        public void DeleteJob()
        {
            if (SelectedJob == null) return ;
            if (SelectedJob.Status == "Running") return;
            int index = Jobs.IndexOf(SelectedJob);
            _repo.Delete(SelectedJob.Id);
            Jobs.Remove(SelectedJob);         
            if (Jobs.Count > 0)
            {
                if (index >= Jobs.Count)
                    index = Jobs.Count - 1;                
                SelectedJob = Jobs[index];                
            }           
        }

        [RelayCommand]
        private void RunNowJob()
        {
            if (SelectedJob == null) return;
            //var job = _repo.GetById(SelectedJob.Id);
            //if (job != null)
            //{
            //    _repo.Update(job);
            //}            
            //LoadJobs();
            Jobs.First(c => c.Id == SelectedJob.Id).Status = "Running";
        }

        [RelayCommand]
        private void StopJob()
        {
            if (SelectedJob == null) return;
            //var job = _repo.GetById(SelectedJob.Id);
            //if (job != null)
            //{
            //    _repo.Update(job);
            //}            
            //LoadJobs();
            Jobs.First(c => c.Id == SelectedJob.Id).Status = "Active";
        }

        [RelayCommand]
        private void ConfigJob()
        {
            if (SelectedJob == null) return;
            switch (SelectedJob.JobType.ToUpper())
            {
                case "SQLBACKUP":
                    _navigation.NavigateTo<SQLBackupScheduleViewModel>(SelectedJob.Id);
                    break;
                case "SQLLITE":
                    break;
            }
            
        }
    }

}
