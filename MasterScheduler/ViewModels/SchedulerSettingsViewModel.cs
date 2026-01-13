using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Models;
using MasterScheduler.Shared;
using MasterScheduler.Shared.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MasterScheduler.ViewModels
{
    public partial class SchedulerSettingsViewModel : ObservableObject
    {
        [ObservableProperty] private string minute = "00";
        [ObservableProperty] private string hour = "12";
        [ObservableProperty] private string day = "*";
        [ObservableProperty] private string month = "*";
        [ObservableProperty] private string weekday = "*";

        [ObservableProperty] private string cronExpression;
        [ObservableProperty] private string humanText;
        [ObservableProperty] private string nextRun;
        [ObservableProperty] private string isValidCron = "Valid";

        // This method is called by the CommunityToolkit whenever any property changes
        partial void OnMinuteChanged(string value) => UpdateCronDetails();
        partial void OnHourChanged(string value) => UpdateCronDetails();
        partial void OnDayChanged(string value) => UpdateCronDetails();
        partial void OnMonthChanged(string value) => UpdateCronDetails();
        partial void OnWeekdayChanged(string value) => UpdateCronDetails();

        private readonly JobRepository _repo = new JobRepository();
        public async Task InitializeAsync(int jobId)
        {
            // 1. Fetch from Database (Example using a repository or service)
            var job = await _repo.GeByIdAsync(jobId);

            if (job != null)
            {
                // 2. Map the DB values to your ObservableProperties
                // Assuming your DB stores the full cron or individual parts
                var crons = job.CronExpression.Split();
                Minute = crons[0];
                Hour = crons[1];
                Day = crons[2];
                Month = crons[3];
                Weekday = crons[4];               
            }
            UpdateCronDetails();
        }
        private void UpdateCronDetails()
        {
            // 1. Construct the string
            CronExpression = $"{Minute} {Hour} {Day} {Month} {Weekday}";

            try
            {
                // 2. Validate and Get Next Run                
                var next = CronosHelper.GetNextRunAt(CronExpression);

                NextRun = next ?? "No future occurrence";
                IsValidCron = "Valid";

                // 3. Get Human Readable Text
                HumanText = CronosHelper.GetHumanReadableDescription(CronExpression);
            }
            catch (Exception)
            {
                IsValidCron = "Error";
                HumanText = "Invalid Cron Format";
                NextRun = "N/A";
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (IsValidCron == "Valid")
            {                
                CloseAction?.Invoke(true);
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            CloseAction?.Invoke(false);
        }

        public Action<bool?> CloseAction { get; set; }
    }
}
