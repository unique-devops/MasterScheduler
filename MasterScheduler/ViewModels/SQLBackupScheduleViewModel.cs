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
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;


namespace MasterScheduler.ViewModels
{
    public partial class SQLBackupScheduleViewModel : ObservableValidator, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly JobRepository _repo = new JobRepository();

        private const string EmailRegexPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";

        [ObservableProperty]
        private string _jobAliasName;

        [ObservableProperty]
        private string? serverName;

        [ObservableProperty]
        private bool isServerConnected;

        [ObservableProperty]
        private string scheduledTime = "not schedule";

        [ObservableProperty]
        [RegularExpression(EmailRegexPattern)]
        [NotifyDataErrorInfo]
        private string? sendConfirmationMail;

        private string? ConnectionString;
        public ObservableCollection<string> SelectedDatabases { get; set; } = new();    
        
        public ObservableCollection<BackupDestination> Destinations { get; set; } = new();

        private readonly IDialogService _dialogService;

        private int editJobId =0;
        public ScheduleTimeModel scheduleTimeModel { get; set; } = new();

        SqlBackupDetails sqlBackupDetails = new SqlBackupDetails();

        public SQLBackupScheduleViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _navigationService = navigationService;
            JobAliasName = "Sql Backup";            
        }

        private void GetJobDetail()
        {
            if (editJobId == 0) return;
            JobDetailModel? jobDetail =  _repo.GetDetailById(editJobId);
            if (jobDetail?.Details != null)
            {
               

                sqlBackupDetails = JsonSerializer.Deserialize<SqlBackupDetails>(jobDetail.Details);                
                ServerName = sqlBackupDetails?.Server;
                ConnectionString = sqlBackupDetails?.ConnectionString;
                ScheduledTime = sqlBackupDetails?.Schedule.ExecutionTime ?? "00:00";
                IsServerConnected = true;
                SelectedDatabases.Clear();
                foreach (var db in sqlBackupDetails?.Databases) SelectedDatabases.Add(db);
                
                Destinations.Clear();
                foreach (var dest in sqlBackupDetails.Destinations) Destinations.Add(dest);
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
            //var dataContext = new MSSQLConnectViewModel { SelectedServer = sqlBackupDetails.Server, SelectedAuthentication = sqlBackupDetails.AuthType, LoginID = sqlBackupDetails.Username };
            var dialog = new MSSQLConnectView();
            var dataContext = (MSSQLConnectViewModel)dialog.DataContext;
            dialog.Owner = Application.Current.MainWindow;
            dataContext.SetModelData();
            var result = dialog.ShowDialog();            
            if (result == true)
            {
                var vm =(MSSQLConnectViewModel)dialog.DataContext;
                if (vm.IsConnectedServer)
                {
                    ServerName = vm.SelectedServer;
                    ConnectionString = vm.ConnectedString;
                    IsServerConnected = true;
                }
                else
                {
                    IsServerConnected = false;
                }
                sqlBackupDetails.Server = ServerName ?? "";
                sqlBackupDetails.Username = vm.LoginID;                
                sqlBackupDetails.AuthType = vm.SelectedAuthentication;
                sqlBackupDetails.ConnectionString = ConnectionString ?? "";
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
            try
            {
                LoaderService.ShowLoader();
                var databases = await LoadDatabasesAsync();

                var dialog = new DatabaseSelectionDialog();
                dialog.Owner = Application.Current.MainWindow;        
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
                await App.ToastService.ShowAsync($"Failed to fetch databases: {ex.Message}", ToastType.Error);

            }
            finally 
            {
                LoaderService.HideLoader();
            }
            
        }
        private async Task<IEnumerable<DatabaseItem>> LoadDatabasesAsync()
        {
            return await Task.Run(() =>
            {
                var list = new List<DatabaseItem>();
                using SqlConnection connection = new SqlConnection(ConnectionString);
                connection.Open();
                using var cmd = new SqlCommand("SELECT Name FROM sys.databases WHERE database_id > 4", connection);
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(new DatabaseItem
                    {
                        Name = reader.GetString(0),
                        IsChecked = SelectedDatabases.Contains(reader.GetString(0))
                    });
                }
                return list;
            });
        }

        [RelayCommand]
        public void BackupDestination(object sender)
        {
            MenuItem? menuItem = sender as MenuItem;
            if (menuItem == null) return;
            AddUpdateDestination(menuItem.Name, new BackupDestination());
        }
        private void AddUpdateDestination(string destinationType,BackupDestination destination)
        {
            var existingItem = Destinations.FirstOrDefault(d => d.Id == destination.Id);
            switch (destinationType)
            {
                case "LocalFolder":
                    var localDialog = new LocalPathBackupConfigDialog();
                    localDialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                    if (localDialog.ShowDialog() == true)
                    {
                        var data = (LocalPathDestinationModel)localDialog.DataContext;
                        var newConfig = new LocalFolderConfig { TargetPath = data.Path };
                        UpdateOrAddDestination(existingItem, DestinationType.LocalFolder, data.Path, newConfig);
                    }
                    break;
                case "GoogleDrive":
                    var googleDrive = new GoogleDriveConfigDialog();
                    googleDrive.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
                    if (googleDrive.ShowDialog() == true)
                    {                        
                        GoogleDriveConfig gdConfig = googleDrive.ResultConfig;
                        UpdateOrAddDestination(existingItem, DestinationType.GoogleDrive, gdConfig.TargetFolderId, gdConfig);
                    }                    
                    break;
                //case "FTP":
                //    Destinations.Add(new BackupDestination { Type = DestinationType.FTP });
                //    break;
                //case "SFTP":
                //    Destinations.Add(new BackupDestination { Type = DestinationType.SFTP });
                //    break;
                //case "OneDrive":
                //    Destinations.Add(new BackupDestination { Type = DestinationType.OneDrive });
                //    break;
                //case "AmazonS3":
                //    Destinations.Add(new BackupDestination { Type = DestinationType.AmazonS3 });
                //    break;
                //case "AzureBlob":
                //    Destinations.Add(new BackupDestination { Type = DestinationType.AzureBlob });
                //    break;
                //case "NetworkShare":
                //    Destinations.Add(new BackupDestination { Type = DestinationType.NetworkShare });
                //    break;
                default:
                    break;
            }
        }
        private void UpdateOrAddDestination(BackupDestination? existing, DestinationType type, string display, DestinationConfig config)
        {
            if (existing == null)
            {
                // Add new
                Destinations.Add(new BackupDestination
                {
                    Id = Guid.NewGuid(),
                    Type = type,
                    DisplayText = display,
                    Config = config
                });
            }
            else
            {
                // Edit existing
                existing.DisplayText = display;
                existing.Type = type;
                existing.Config = config;

                // FORCE UI REFRESH: Replace the item at its index
                int index = Destinations.IndexOf(existing);
                Destinations[index] = existing;
            }

            // Always sync back to the main details object for Saving
            sqlBackupDetails.Destinations = Destinations.ToList();
        }
        
        [RelayCommand]
        public async void SchedulerSettings()
        {
            var dialog = new SchedulerSettingsView();
            dialog.Owner = Application.Current.MainWindow;
            if (dialog.DataContext is SchedulerSettingsViewModel vm)
            {
                // Pass the Job ID (e.g., 101) to load the data
                await vm.InitializeAsync(editJobId);
                if (dialog.ShowDialog() == true)
                {
                    ScheduledTime = vm.HumanText;
                    sqlBackupDetails.Schedule.ExecutionTime = vm.NextRun;
                    sqlBackupDetails.Schedule.Crons = vm.CronExpression;                    
                }
            }
            
            //var dialog = new ScheduleTimeView();
            //dialog.Owner = Application.Current.MainWindow;
            //// Simulate loading available databases            
            //if (dialog.ShowDialog() == true)
            //{
            //    scheduleTimeModel = dialog.ScheduleTime;
            //    ScheduledTime = $"Daily at {scheduleTimeModel.Hour}:{scheduleTimeModel.Minute}";
            //    sqlBackupDetails.Schedule.ExecutionTime = ScheduledTime;
            //}
        }

        [RelayCommand]
        private void ConfigureDestination(BackupDestination destination)
        {
            AddUpdateDestination(destination.Type.ToString(),destination);
        }

        [RelayCommand]
        private void Delete(BackupDestination item)
        {
            Destinations.Remove(item);
        }

        [RelayCommand]
        private void Save()
        {
            try
            {
                if (string.IsNullOrWhiteSpace(ScheduledTime) || ScheduledTime == "not schedule")
                {
                    MessageBox.Show($"ScheduleTime Required!");
                    return;
                }
                //int hour = scheduleTimeModel.Hour;
                //int min = scheduleTimeModel.Minute;
                //string cron = $"{min} {hour} * * *";                
                var job = new JobModel
                {
                    JobName = JobAliasName,
                    JobType = "SqlBackup",
                    CronExpression = sqlBackupDetails.Schedule.Crons,
                    NextRunTime = CronosHelper.GetNextRunTime(sqlBackupDetails.Schedule.Crons),
                    IsActive = true,
                    Status = "pending",
                    Message = "not run yet"
                };                
                sqlBackupDetails.Destinations = Destinations.ToList();
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true
                };
                string sqlJobDetails = JsonSerializer.Serialize(sqlBackupDetails, options);
                
                if (editJobId == 0)
                {
                    var insertedId = _repo.Add(job);
                    
                    _repo.AddUpdateJobDetail(new JobDetailModel {
                        JobId = insertedId,
                        Details = sqlJobDetails
                    });
                }
                else
                {
                    var existJob = _repo.GetById(editJobId);
                    if (existJob != null)
                    {
                        existJob.JobName = JobAliasName;
                        existJob.CronExpression = job.CronExpression;
                        existJob.NextRunTime = job.NextRunTime;
                        _repo.Update(existJob);
                       
                        _repo.AddUpdateJobDetail(new JobDetailModel {
                            JobId = editJobId,
                            Details = sqlJobDetails
                        });
                    }

                }
                //MessageBox.Show($"Job {(editJobId == 0 ? "saved" : "updated")} successfully!");
                _navigationService.NavigateTo<DashboardViewModel>();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error :{ex.Message}");
            }
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
