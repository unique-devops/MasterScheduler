using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Service;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Views;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MasterScheduler.ViewModels
{
    public partial class SQLBackupScheduleViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;

        [ObservableProperty]
        private string serverName;

        [ObservableProperty]
        private bool isServerConnected;

        private string ConnectionString;
        public ObservableCollection<string> SelectedDatabases { get; set; } = new();
        public ObservableCollection<BackupDestination> BackupDestinations { get; set; } = new();

        private readonly IDialogService _dialogService;
        public SQLBackupScheduleViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
        }


        [RelayCommand]
        public void ConnectServer()
        {
            var dialog = new MSSQLConnectView();
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            var result = dialog.ShowDialog();            
            if (result == true)
            {
                var vm =(MSSQLConnectViewModel)dialog.DataContext;
                if (vm.IsConnectedServer)
                {
                    ServerName = vm.SelectedServer;
                    ConnectionString = $"Server={ServerName};Database=master;Trusted_Connection=True;TrustServerCertificate=True"; ;
                    IsServerConnected = true;
                }
                else
                {
                    IsServerConnected = false;
                }
                
            }
            
        }
        [RelayCommand]
        public async Task OpenDatabaseSelection()
        {            
            if (IsServerConnected == false)
            {
                await App.ToastService.ShowAsync("Server not connected!", ToastType.Error);
                return;
            }
            var dialog = new DatabaseSelectionDialog();
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x=>x.IsActive);
            // Simulate loading available databases
            var databases = LoadDatabases();
            dialog.AvailableDatabases = new ObservableCollection<DatabaseItem>(databases);
            if (dialog.ShowDialog() == true)
            {
                SelectedDatabases.Clear();
                foreach (var db in dialog.AvailableDatabases.Where(d => d.IsChecked))
                {
                    SelectedDatabases.Add(db.Name);
                }
            }
        }

        private IEnumerable<DatabaseItem> LoadDatabases()
        {
            var list = new List<DatabaseItem>();
            using SqlConnection connection = new SqlConnection(ConnectionString);
            connection.Open();

            var cmd = new SqlCommand("SELECT Name FROM sys.databases WHERE database_id > 4", connection); // exclude system DBs
            var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                list.Add(new DatabaseItem { Name = reader.GetString(0) });
            }
            return list;
        }

        [RelayCommand]
        public void BackupDestination()
        {           
            BackupDestinations.Add(new BackupDestination { Name = "DB1", Icon = "Server" });                        
        }

        

        [RelayCommand]
        public void SchedulerSettings()
        {
            _navigationService.NavigateTo<SchedulerSettingsViewModel>();
        }
    }
}
