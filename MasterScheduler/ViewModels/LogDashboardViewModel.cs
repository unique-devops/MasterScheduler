using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MasterScheduler.ViewModels
{
    public partial class LogDashboardViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        private readonly JobRepository _repo = new JobRepository();

        private ObservableCollection<LogDto> AllLogs;

        [ObservableProperty]
        private ObservableCollection<LogDto> logs;

        [ObservableProperty]
        private string searchText;

        [ObservableProperty]
        private string selectedLevel;
        public ObservableCollection<string> Levels { get; } =
       new() { "All", "Info", "Warning", "Error" };
        public LogDashboardViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            LoadLogs();
        }
        [RelayCommand]
        private void GoBack()
        {
            //_navigationService.GoBack();
            _navigationService.NavigateTo<DashboardViewModel>();
        }

        [RelayCommand]
        private void LoadLogs()
        {
            var data = _repo.GetAllLogs();
            AllLogs = new ObservableCollection<LogDto>(data);
            Logs = new ObservableCollection<LogDto>(AllLogs);
        }

        private void FilterLogs()
        {
            if (AllLogs == null) return;

            var filtered = AllLogs.Where(log =>
                (SelectedLevel == "All" || string.IsNullOrEmpty(SelectedLevel) || log.Level == SelectedLevel) &&
                (string.IsNullOrEmpty(SearchText) ||
                 log.Message.Contains(SearchText, StringComparison.OrdinalIgnoreCase))
            );

            Logs = new ObservableCollection<LogDto>(filtered);
        }

        [RelayCommand]
        private void ClearLogs()
        {
            if (MessageBox.Show("Delete all log history?",
                "Confirm", MessageBoxButton.YesNo) != MessageBoxResult.Yes)
                return;

            _repo.DeleteLogs();
            LoadLogs();
        }

    }
}
