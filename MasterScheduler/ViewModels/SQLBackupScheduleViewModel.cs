using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MasterScheduler.ViewModels
{
    public partial class SQLBackupScheduleViewModel : ObservableObject
    {
        public ObservableCollection<string> SelectedDatabases { get; set; } = new();
        public ObservableCollection<BackupDestination> BackupDestinations { get; set; } = new();

        private readonly IDialogService _dialogService;
        public SQLBackupScheduleViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        [RelayCommand]
        public void OpenDatabaseSelection()
        {
            var dialog = new DatabaseSelectionDialog();
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x=>x.IsActive);
            // Simulate loading available databases
            dialog.AvailableDatabases = new ObservableCollection<DatabaseItem>
            {
                new DatabaseItem { Name = "DB1" },
                new DatabaseItem { Name = "DB2" },
                new DatabaseItem { Name = "DB3" }
            };
            if (dialog.ShowDialog() == true)
            {
                SelectedDatabases.Clear();
                foreach (var db in dialog.AvailableDatabases.Where(d => d.IsChecked))
                {
                    SelectedDatabases.Add(db.Name);
                }
            }
        }

        [RelayCommand]
        public void BackupDestination()
        {           
            BackupDestinations.Add(new BackupDestination { Name = "DB1", Icon = "Server" });                        
        }

        [RelayCommand]
        public void ConnectServer()
        {
            var dialog = new MSSQLConnectView();
            dialog.Owner = Application.Current.Windows.OfType<Window>().SingleOrDefault(x => x.IsActive);
            var result = dialog.ShowDialog();

            if (result == true)
            {
                // Access dbSelectorVM.SelectedDatabases here
            }
        }
    }
}
