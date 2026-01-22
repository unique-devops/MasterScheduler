using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Service;
using MasterScheduler.Views;
using System.Collections.ObjectModel;
using System.IO;
using System.IO.Pipes;
using System.Text.Json;
using System.Windows;
using System.Windows.Media.TextFormatting;

namespace MasterScheduler.ViewModels
{
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly JobRepository _repo = new JobRepository();        
        [ObservableProperty]
        private ObservableCollection<ScheduledJobDto> jobs;

        [ObservableProperty]
        private string licType;

        [ObservableProperty]
        private string licExp;

        private const string PipeName = "JobControlPipe";

        LicenseChecker licenseChecker = new LicenseChecker();
        [ObservableProperty] private ScheduledJobDto? _selectedJob;       
        public DashboardViewModel(INavigationService navigation)
        {
            _navigation = navigation;            
            Jobs = new();
            License();
            _ =LoadNewJobs();
            _=StartAsync(new CancellationToken());
        }

        private async void License()
        {
            await licenseChecker.CheckAndInitializeLicense();
            var lic = licenseChecker.GetLocalLicense();
            LicType = lic.LicenseType;
            LicExp = lic.LicenseType =="lite" ? "" : lic.ExpiryDate;
        }

        [RelayCommand]
        private async Task LoadNewJobs()
        {
            while (true)
            {
                var dbJobs = _repo.GetAll().ToList();

                Application.Current.Dispatcher.Invoke(() =>
                {
                    // 1️⃣ REMOVE jobs that no longer exist in DB
                    var toRemove = Jobs
                        .Where(ui => !dbJobs.Any(db => db.Id == ui.Id))
                        .ToList();

                    foreach (var r in toRemove)
                        Jobs.Remove(r);

                    // 2️⃣ ADD or UPDATE jobs
                    foreach (var dbJob in dbJobs)
                    {
                        var uiJob = Jobs.FirstOrDefault(x => x.Id == dbJob.Id);                       

                        if (uiJob == null)
                        {
                            // ➕ ADD
                            Jobs.Add(new ScheduledJobDto
                            {
                                Id = dbJob.Id,
                                Name = dbJob.JobName,
                                JobType = dbJob.JobType,
                                NextRunAt = dbJob.NextRunTime.ToString(),
                                LastRunAt = dbJob.LastRunTime.ToString(),
                                Status = dbJob.Status,
                                Message = dbJob.Message,
                                IsActive = dbJob.IsActive
                            });
                        }
                        else
                        {
                            // 🔄 UPDATE (this was missing in your code)
                            uiJob.NextRunAt = dbJob.NextRunTime.ToString();
                            uiJob.LastRunAt = dbJob.LastRunTime.ToString();
                            uiJob.Status = dbJob.Status;
                            uiJob.Message = dbJob.Message;
                            uiJob.IsActive = dbJob.IsActive;
                        }
                    }
                });

                await Task.Delay(500); // refresh every 1 sec
            }
        }

        
        [RelayCommand]
        private void LoadJobs()
        {
            Jobs.Clear();
            foreach (var j in _repo.GetAll())
            {
                string lastRun = j.LastRunTime.ToString() ?? "";
                string nextRun = j.NextRunTime.ToString() ?? "";
                Jobs.Add(new ScheduledJobDto { Id = j.Id, Name = j.JobName, JobType = j.JobType, NextRunAt = string.IsNullOrWhiteSpace(nextRun) ? "N/A" : nextRun, LastRunAt = string.IsNullOrWhiteSpace(lastRun) ? "N/A" : lastRun, Status = j.Status , Message = j.Message });
            }

        }

        [RelayCommand]
        private void AddJob()
        {
           _navigation.NavigateTo<TaskTypeSelectionViewModel>();
        }

        //private bool CanDelete() => SelectedJob != null;
        [RelayCommand]
        //[RelayCommand(CanExecute = nameof(CanDelete))]
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
        private void OnToggleJob(ScheduledJobDto job)
        {
            if (job == null) return;
            
            _repo.ActiveInActive(job.Id,job.IsActive);                       
        }

        [RelayCommand]
        private async Task RunNowJob()
        {
            if (SelectedJob == null) return;
            //var job = _repo.GetById(SelectedJob.Id);
            //if (job != null)
            //{
            //    _repo.Update(job);
            //}            
            //LoadJobs();
           
            Jobs.First(c => c.Id == SelectedJob.Id).Status = "Running";
            //await PipeClient.SendAsync(SelectedJob.Id.ToString());            
        }

        [RelayCommand]
        private async Task StopJob()
        {
            if (SelectedJob == null) return;
            //var job = _repo.GetById(SelectedJob.Id);
            //if (job != null)
            //{
            //    _repo.Update(job);
            //}            
            //LoadJobs();
            bool sent = await SendCancelRequestAsync(SelectedJob.Id);
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

        [RelayCommand]      
        public void Logs()
        {           
            _navigation.NavigateTo<LogDashboardViewModel>();
        }

        [RelayCommand]
        public void OpenLog()
        {
            if (SelectedJob == null) return;
            if (SelectedJob.Status == "Running") return;
            if (SelectedJob != null && !string.IsNullOrEmpty(SelectedJob.Message))
            {
                try
                {
                    // 1. Create a temporary file path
                    string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"Log_{DateTime.Now:yyyyMMdd_HHmmss}.txt");

                    // 2. Write the log content to the file
                    System.IO.File.WriteAllText(tempPath, SelectedJob.Message);

                    // 3. Open the file with Notepad
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "notepad.exe",
                        Arguments = tempPath,
                        UseShellExecute = true
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Could not open log: {ex.Message}");
                }
            }
        }

        
        //------------------Server----------------
        public async Task StartAsync(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                var server = new NamedPipeServerStream(
                    "SchedulerUI",
                    PipeDirection.InOut,
                    NamedPipeServerStream.MaxAllowedServerInstances,
                    PipeTransmissionMode.Byte,
                    PipeOptions.Asynchronous
                );

                await server.WaitForConnectionAsync(token);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        using var reader = new StreamReader(server);                        

                        string? line = await reader.ReadLineAsync();

                        if (!string.IsNullOrEmpty(line))
                        {
                            var scheduleJob = JsonSerializer.Deserialize<ScheduledJobDto>(line);
                            if (scheduleJob != null)
                            {
                                var exit = Jobs?.First(c => c.Id == scheduleJob.Id);
                                if (exit != null)
                                {
                                    exit.Status = scheduleJob.Status.ToString();
                                }                               
                            }                            
                            //Jobs.First(c => c.Id == Convert.ToInt32(line)).Status = "completed";
                        }
                    }
                    catch (Exception ex)
                    {
                        // log if needed
                    }
                    finally
                    {
                        server.Dispose();   // IMPORTANT: do NOT use Disconnect() only
                    }

                }, CancellationToken.None);   // do NOT pass the main cancellation token
            }
        }

        public static async Task<bool> SendCancelRequestAsync(int jobId)
        {
            try
            {
                // 1. Connect to the pipe with a 2-second timeout
                using var client = new NamedPipeClientStream(".", PipeName, PipeDirection.Out);
                await client.ConnectAsync(2000);

                // 2. Write the command
                using var writer = new StreamWriter(client);
                await writer.WriteLineAsync($"CANCEL:{jobId}");
                await writer.FlushAsync();

                return true;
            }
            catch
            {
                // Service might be down or pipe is busy
                return false;
            }
        }
    }

}
