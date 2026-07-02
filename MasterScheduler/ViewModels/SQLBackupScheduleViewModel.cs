using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Service;
using MasterScheduler.Shared;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Shared.Service;
using MasterScheduler.Views;
using Microsoft.Data.SqlClient;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Management;
using System.Net.NetworkInformation;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;


namespace MasterScheduler.ViewModels
{
    public partial class SQLBackupScheduleViewModel : ObservableValidator, INavigationAware
    {
        private readonly INavigationService _navigationService;
        private readonly JobRepository _repo = new ();
        SQLServerService sqlService = new SQLServerService();
        SqlBackupDetails sqlBackupDetails = new SqlBackupDetails();
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
        private bool activeAlert;        

        private string _selectedCompression = "None"; // Default selected

        public string SelectedCompression
        {
            get => _selectedCompression;
            set
            {
                if (value.ToString() != "Zip" && value.ToString() != "None" && !IsServerSupportCompression())
                {
                    MessageBox.Show("Server does not support default compression.");
                    return; 
                }
                _selectedCompression = value;
                OnPropertyChanged(nameof(SelectedCompression));
            }
        }

        [ObservableProperty]
        [RegularExpression(EmailRegexPattern)]
        [NotifyDataErrorInfo]
        private string? sendConfirmationMail;

        private string? ConnectionString;
        public ObservableCollection<string> SelectedDatabases { get; set; } = new();    
        
        public ObservableCollection<BackupDestination> Destinations { get; set; } = new();

        public ObservableCollection<BackupDestinations> AvailableDestinations { get; set; }

        private readonly IDialogService _dialogService;

        private int editJobId =0;
        public ScheduleTimeModel scheduleTimeModel { get; set; } = new();
       

        LicenseService license = new LicenseService();
        public SQLBackupScheduleViewModel(IDialogService dialogService, INavigationService navigationService)
        {
            BindDestinations();
            _dialogService = dialogService;
            _navigationService = navigationService;
            JobAliasName = "Sql Backup";
            SelectedCompression = "None";
        }

        private void BindDestinations()
        {
            var lics = license.GetLicenses();
            var lic = lics.Find(c => c.LicenseName == "PRO");

            var allOptions = new List<BackupDestinations>
            {
                new() { IsActive =true, Type = DestinationType.LocalFolder, Name = "Local Folder", IconPath = "/Assets/folder.png" },
                new() { IsActive =false, Type = DestinationType.GoogleDrive, Name = "Google Drive", IconPath = "/Assets/google-drive.png" },
                new() { IsActive =true, Type = DestinationType.FTP, Name = "FTP", IconPath = "/Assets/ftp.png" },
                new() { IsActive =true, Type = DestinationType.SFTP, Name = "SFTP", IconPath = "/Assets/sftp.png" },
                new() { IsActive =false, Type = DestinationType.OneDrive, Name = "OneDrive", IconPath = "/Assets/onedrive.png" },
                new() { IsActive =false, Type = DestinationType.AmazonS3, Name = "Amazon S3", IconPath = "/Assets/s3.png" },
                new() { IsActive =false, Type = DestinationType.AzureBlob, Name = "Azure Blob", IconPath = "/Assets/azure.png" },
                new() { IsActive =true, Type = DestinationType.NetworkShare, Name = "NAS / Network Share", IconPath = "/Assets/local-area-network.png" }
            };
            if (lic !=null)
            {
                var isExpired = LicenseService.IsLicenseExpired(lic.ExpiryDate);
                
                allOptions.Find(c => c.Type == DestinationType.GoogleDrive).IsActive = !isExpired;
                allOptions.Find(c => c.Type == DestinationType.OneDrive).IsActive = !isExpired;
                allOptions.Find(c => c.Type == DestinationType.AzureBlob).IsActive = !isExpired;
                allOptions.Find(c => c.Type == DestinationType.AmazonS3).IsActive = !isExpired;                
            }
            
            AvailableDestinations = new ObservableCollection<BackupDestinations>(allOptions);
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
                SelectedCompression = sqlBackupDetails?.Compression ?? "None";                
                IsServerConnected = true;
                SelectedDatabases.Clear();
                foreach (var db in sqlBackupDetails?.Databases) SelectedDatabases.Add(db);
                
                Destinations.Clear();
                foreach (var dest in sqlBackupDetails.Destinations) Destinations.Add(dest);

                SendConfirmationMail = sqlBackupDetails?.Notifications?.EmailOnSuccess ?? "";
                ActiveAlert = sqlBackupDetails.Notifications.ActiveAlert;
            }
        }

        public bool IsServerSupportCompression()
        {
            return sqlService.IsSupportNativeCompression(sqlBackupDetails.ConnectionString);            
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

                // Querying sys.master_files to sum the size of all files (Data and Log) for each DB
                // Size is stored in 8KB pages, so (size * 8) / 1024 converts to MB
                string sql = @"
                SELECT 
                    d.name, 
                    SUM(CAST(f.size AS BIGINT) * 8 / 1024) AS SizeMB
                FROM sys.databases d
                JOIN sys.master_files f ON d.database_id = f.database_id
                WHERE d.database_id > 4
                GROUP BY d.name";

                using var cmd = new SqlCommand(sql, connection);
                using var reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    var dbName = reader.GetString(0);
                    var dbSize = reader.GetInt64(1); // The calculated SizeMB

                    list.Add(new DatabaseItem
                    {
                        Name = dbName,
                        Size = dbSize > 1024 ? $"{dbSize/1024} GB" : $"{dbSize} MB", // Assuming you add a 'Size' property to DatabaseItem
                        IsChecked = SelectedDatabases.Contains(dbName)
                    });
                }
                return list;
            });
            //return await Task.Run(() =>
            //{
            //    var list = new List<DatabaseItem>();
            //    using SqlConnection connection = new SqlConnection(ConnectionString);
            //    connection.Open();
            //    using var cmd = new SqlCommand("SELECT Name FROM sys.databases WHERE database_id > 4", connection);
            //    using var reader = cmd.ExecuteReader();
            //    while (reader.Read())
            //    {
            //        list.Add(new DatabaseItem
            //        {
            //            Name = reader.GetString(0),
            //            IsChecked = SelectedDatabases.Contains(reader.GetString(0))
            //        });
            //    }
            //    return list;
            //});
        }

        [RelayCommand]
        public void SelectDestination(BackupDestinations selectedItem)
        {
            //MenuItem? menuItem = sender as MenuItem;
            //if (selectedItem == null) return;
            if (!selectedItem.IsActive)
            {
                MessageBox.Show($"{selectedItem.Name} is a Pro Feature Would you like to visit our store to unlock all features?", "Upgrade Required");

                _navigationService.NavigateTo<EditionOverlayViewModel>();
            }
            else {
                AddUpdateDestination(selectedItem.Type.ToString(), new BackupDestination());
            }
                
        }
        private void AddUpdateDestination(string destinationType,BackupDestination destination)
        {
            var existingItem = Destinations.FirstOrDefault(d => d.Id == destination.Id);
            switch (destinationType)
            {
                case "LocalFolder":
                    var existingConfig = destination.Config as LocalFolderConfig;
                    var vm = new LocalPathBackupConfigViewModel(existingConfig);
                    if (_dialogService.ShowDialog(vm) == true)
                    {
                        // 4. Get the updated model back from the VM
                        var updatedModel = vm.GetModel();
                        UpdateOrAddDestination(existingItem, DestinationType.LocalFolder, updatedModel.TargetPath, updatedModel);
                    }

                    break;
                case "GoogleDrive":
                    var gDConfig = destination.Config as GoogleDriveConfig;
                    var gdvm = new GoogleDriveConfigViewModel(gDConfig);                   
                    if (_dialogService.ShowDialog(gdvm) == true)
                    {                        
                        var gdConfig = gdvm.GetModel();
                        UpdateOrAddDestination(existingItem, DestinationType.GoogleDrive, gdConfig.FolderName, gdConfig);
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
                sqlBackupDetails.Notifications.EmailOnSuccess = SendConfirmationMail ?? "";
                sqlBackupDetails.Compression = SelectedCompression ?? "None";
                sqlBackupDetails.Notifications.ActiveAlert = ActiveAlert;
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
                //_navigationService.NavigateTo<DashboardViewModel>();
                _navigationService.NavigateTo<HomeViewModel>();
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
            //_navigationService.NavigateTo<DashboardViewModel>();
            _navigationService.NavigateTo<HomeViewModel>();
        }
    }
}
