using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Service;
using MasterScheduler.Shared;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Views;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using static MasterScheduler.Models.Enums;

namespace MasterScheduler.ViewModels
{
    public partial class SQLBackupScheduleViewModel : ObservableObject, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly JobRepository _repo = new JobRepository();
        [ObservableProperty]
        private string _jobAliasName;

        [ObservableProperty]
        private string serverName;

        [ObservableProperty]
        private bool isServerConnected;

        [ObservableProperty]
        private string scheduledTime;

        private string ConnectionString;
        public ObservableCollection<string> SelectedDatabases { get; set; } = new();
        public ObservableCollection<DestinationModel> Destinations { get; set; } = new();

        private readonly IDialogService _dialogService;

        private int editJobId =0;        
        public ScheduleTimeModel scheduleTimeModel { get; set; } = new();

        SqlBackupDetails sqlBackupDetails = new SqlBackupDetails();       

        public SQLBackupScheduleViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            JobAliasName = "Sql Backup";
            ScheduledTime = "not schedule";            
        }

        private void GetJobDetail()
        {
            if (editJobId == 0) return;
            JobDetailModel? jobDetail =  _repo.GetDetailById(editJobId);
            if (jobDetail != null && jobDetail?.Details != null)
            {
                sqlBackupDetails = JsonSerializer.Deserialize<SqlBackupDetails>(jobDetail.Details);
                SelectedDatabases = new ObservableCollection<string>(sqlBackupDetails.Databases);
                ServerName = sqlBackupDetails.Server;               
                ConnectionString = sqlBackupDetails.ConnectionString;
                var backupDestinations = sqlBackupDetails.Destinations;
                foreach (var backupDestination in backupDestinations) 
                {
                    Destinations.Add(new DestinationModel { Type = DestinationType.LocalFolder, PathOrEndpoint = "C:Path" });
                }
            }            
        }

        public void OnNavigatedTo(object parameter)
        {
            if (parameter is int id)
            {
                editJobId = id;
                var job = _repo.GetById(id);
                if (job != null)
                {
                    JobAliasName = job.JobName;
                }
                GetJobDetail();
            }
            else
            {
               
            }
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
                sqlBackupDetails.Server = ServerName;
                sqlBackupDetails.AuthType = vm.SelectedAuthentication;
                sqlBackupDetails.ConnectionString = ConnectionString;
            }
            
        }
        [RelayCommand]
        public async Task OpenDatabaseSelection()
        {
            try
            {
                LoaderService.ShowLoader();
                if (IsServerConnected == false)
                {
                    await App.ToastService.ShowAsync("Server not connected!", ToastType.Error);
                    return;
                }
                var dialog = new DatabaseSelectionDialog();
                dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
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
                sqlBackupDetails.Databases = SelectedDatabases.ToList();
            }
            catch (Exception ex)
            {
                await App.ToastService.ShowAsync(ex.Message, ToastType.Error);

            }
            finally {
                LoaderService.HideLoader();
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
                list.Add(new DatabaseItem { Name = reader.GetString(0), IsChecked = SelectedDatabases.Contains(reader.GetString(0)) });
            }
            return list;
        }

        [RelayCommand]
        public void BackupDestination(object sender)
        {
            MenuItem menuItem = sender as MenuItem;
            if (menuItem == null) return;
            switch (menuItem.Name)
            {
                case "LocalFolder":
                    Destinations.Add(new DestinationModel { Type = DestinationType.LocalFolder, PathOrEndpoint = "DB" });
                    break;
                case "GoogleDrive":
                    Destinations.Add(new DestinationModel { Type = DestinationType.GoogleDrive, PathOrEndpoint = "DB" });
                    break;
                case "FTP":
                    Destinations.Add(new DestinationModel { Type = DestinationType.FTP, PathOrEndpoint = "DB" });
                    break;
                case "SFTP":
                    Destinations.Add(new DestinationModel { Type = DestinationType.SFTP, PathOrEndpoint = "DB" });
                    break;
                case "OneDrive":
                    Destinations.Add(new DestinationModel { Type = DestinationType.OneDrive, PathOrEndpoint = "DB" });
                    break;
                case "AmazonS3":
                    Destinations.Add(new DestinationModel { Type = DestinationType.AmazonS3, PathOrEndpoint = "DB" });
                    break;
                case "AzureBlob":
                    Destinations.Add(new DestinationModel { Type = DestinationType.AzureBlob, PathOrEndpoint = "DB" });
                    break;
                case "NetworkShare":
                    Destinations.Add(new DestinationModel { Type = DestinationType.NetworkShare, PathOrEndpoint = "DB" });
                    break;
                default:
                    break;
            }            
           
        }

        [RelayCommand]
        public void SchedulerSettings()
        {
            //_navigationService.NavigateTo<SchedulerSettingsViewModel>();
            var dialog = new ScheduleTimeView();
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            // Simulate loading available databases
            
            if (dialog.ShowDialog() == true)
            {
                scheduleTimeModel = dialog.ScheduleTime;
                ScheduledTime = $"Daily at {scheduleTimeModel.Hour}:{scheduleTimeModel.Minute}";
            }
        }

        [RelayCommand]
        private void ConfigureDestination(DestinationModel item)
        {
            MessageBox.Show($"Edit: {item.PathOrEndpoint}");
        }

        [RelayCommand]
        private void Delete(DestinationModel item)
        {
            Destinations.Remove(item);
        }


        [RelayCommand]
        private void Save()
        {
            ////int hour = DateTime.Now.Hour;
            ////int min = DateTime.Now.Minute + 1;
            int hour = scheduleTimeModel.Hour;
            int min = scheduleTimeModel.Minute;
            string cron = $"{min} {hour} * * *";
            var job = new JobModel
            {
                JobName = JobAliasName,
                JobType = "SqlBackup",                
                CronExpression = cron,
                NextRunTime = CronosHelper.GetNextRunTime(cron),
                IsActive = true,                
                Status = "pending",                
                Message = "not run yet" 
            };
            if (editJobId == 0)
            {                
                var insertedId = _repo.Add(job);
                _repo.AddUpdateJobDetail(new JobDetailModel { JobId = insertedId, Details = JsonSerializer.Serialize(sqlBackupDetails) });
            }               
            else
            {
                var existJob = _repo.GetById(editJobId);
                if (existJob != null)
                {
                    existJob.JobName = JobAliasName;
                    _repo.Update(existJob);
                    _repo.AddUpdateJobDetail(new JobDetailModel { JobId = editJobId, Details = JsonSerializer.Serialize(sqlBackupDetails) });
                }
                
            }            
            MessageBox.Show($"Job {(editJobId == 0 ? "saved" : "updated")} successfully!");
            _navigationService.NavigateTo<DashboardViewModel>();
        }

        [RelayCommand]
        private void GoBack()
        {
            _navigationService.NavigateTo<TaskTypeSelectionViewModel>();
        }

        [RelayCommand]
        private void Cancel()
        {
            _navigationService.NavigateTo<DashboardViewModel>();
        }
    }
}
