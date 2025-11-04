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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MasterScheduler.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly JobRepository _repo = new JobRepository();

        [ObservableProperty]
        private ObservableCollection<ScheduledJobDto> jobs;

        [ObservableProperty] private ScheduledJobDto? _selectedJob;
        public DashboardViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            jobs = new();
            LoadJobs();
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

        [RelayCommand]
        private void DeleteJob()
        {
            if (SelectedJob == null) return;
            _repo.Delete(SelectedJob.Id);
            LoadJobs();
        }
    }

}
