using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Models;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public partial class LocalPathBackupConfigViewModel : ObservableObject, IClosableDialog
    {
        public event Action<bool?> RequestClose;

        [ObservableProperty] private string _targetPath;
        [ObservableProperty] private bool _isNetwork;
        [ObservableProperty] private string _username;
        [ObservableProperty] private string _password;
        [ObservableProperty] private int _retentionDays;

        public LocalPathBackupConfigViewModel(LocalFolderConfig config = null)
        {
            if (config != null)
            {
                // Map the data from your DB model to the Observable properties
                TargetPath = config.TargetPath;
                IsNetwork = config.IsNetwork;
                Username = config.Username;
                Password = config.Password;
                RetentionDays = config.RetentionDays;
            }
            else
            {
                // Set defaults for a brand new config
                RetentionDays = 0;
            }
        }
        public LocalFolderConfig GetModel()
        {
            return new LocalFolderConfig
            {
                TargetPath = this.TargetPath,
                IsNetwork = this.IsNetwork,
                Username = this.Username,
                Password = this.Password,
                RetentionDays = this.RetentionDays
            };
        }

        [RelayCommand]
        private void Browse()
        {
            var dialog = new Microsoft.WindowsAPICodePack.Dialogs.CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == Microsoft.WindowsAPICodePack.Dialogs.CommonFileDialogResult.Ok)
            {
                TargetPath = dialog.FileName;
            }
        }

        [RelayCommand]
        private void Save()
        {
            if (System.IO.Directory.Exists(TargetPath))
                RequestClose?.Invoke(true);
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);
    }
}
