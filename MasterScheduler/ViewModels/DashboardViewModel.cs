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
        private ObservableCollection<ScheduledJob> jobs;

        [ObservableProperty] private ScheduledJob? _selectedJob;
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
                Jobs.Add(new ScheduledJob { Id = j.Id, Name = j.JobName, JobType = j.JobType, NextRunAt = j.NextRunTime.ToString(), LastRunAt = j.LastRunTime.ToString() });
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
