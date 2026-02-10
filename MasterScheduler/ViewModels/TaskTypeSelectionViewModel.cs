using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Service;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public partial class TaskTypeSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string headerTitle;

        public ObservableCollection<JobTypeModel> TaskTypeList { get; set; } 

        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private JobTypeModel? selectedTaskType;

        LicenseService licenseService = new LicenseService();
        public TaskTypeSelectionViewModel(INavigationService navigationService)
        {
            HeaderTitle = "Choose task";
            _navigationService = navigationService;
            TaskTypeList = new ObservableCollection<JobTypeModel>
            {               
                new JobTypeModel { Type = "SQLBACKUP", Name = "SQL Backup", Description = "Backup databases to local or cloud." },
                new JobTypeModel { Type = "FILESYNC", Name = "File Sync", Description = "Sync folders between locations." },
                new JobTypeModel { Type = "CLEANUP", Name = "Folder Cleanup", Description = "Delete old logs and temp files." },
                new JobTypeModel { Type = "Report", Name = "Report", Description = "Generate and email status reports." }
            };

            var current = licenseService.GetLocalLicense();
            foreach (var job in TaskTypeList)
            {
                job.IsLocked = !licenseService.HasModule(job.Type, current);
            }
            SelectedTaskType =  TaskTypeList.FirstOrDefault();
        }

        [RelayCommand]
        private void GoBack()
        {
            //_navigationService.GoBack();
            _navigationService.NavigateTo<DashboardViewModel>();
        }

        [RelayCommand]
        private void Next()
        {
            if (SelectedTaskType == null) return;
            switch (SelectedTaskType.Type)
            {
                case "SQLBACKUP":
                    _navigationService.NavigateTo<SQLBackupScheduleViewModel>();
                    break;
                case "":
                    break;
            }
            
        }
    }
}
