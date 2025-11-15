using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
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

        public ObservableCollection<TaskType> TaskTypeList { get; set; } 

        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private TaskType selectedTaskType;
        public TaskTypeSelectionViewModel(INavigationService navigationService)
        {
            HeaderTitle = "Choose task";
            _navigationService = navigationService;
            TaskTypeList = new ObservableCollection<TaskType>
            {
                new TaskType { Id=1, Name = "SQL Backup", Description = "SQL Database Backup." },
                new TaskType { Id=2, Name = "Software Engineer", Description = "Develops software systems." },
                new TaskType { Id=3, Name = "Data Analyst", Description = "Analyzes data and trends." },
                new TaskType { Id=4, Name = "Project Manager", Description = "Oversees project lifecycle." },
                new TaskType { Id=4, Name = "UI/UX Designer", Description = "Designs user interfaces." }
            };

           
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
            switch (SelectedTaskType.Id)
            {
                case 1:
                    _navigationService.NavigateTo<SQLBackupScheduleViewModel>();
                    break;
                case 2:
                    break;
            }
            
        }
    }
}
