using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Models;
using MasterScheduler.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public partial class SQLBackupScheduleViewModel : ObservableObject
    {
        public ObservableCollection<string> SelectedDatabases { get; set; } = new();
        public SQLBackupScheduleViewModel()
        {
            
        }

        [RelayCommand]
        public void OpenDatabaseSelection()
        {
            var dialog = new DatabaseSelectionDialog();
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
    }
}
