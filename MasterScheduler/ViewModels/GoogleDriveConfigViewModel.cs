using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Google.Apis.Auth.OAuth2;
using MasterScheduler.Interface;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.JobHelper;
using System.Windows;

namespace MasterScheduler.ViewModels
{
    public partial class GoogleDriveConfigViewModel : ObservableObject, IClosableDialog
    {
        public event Action<bool?> RequestClose;

        [ObservableProperty] private string _folderName = "MSDataBackups";
        [ObservableProperty] private string _userEmail;
        [ObservableProperty] private string _refreshToken;
        [ObservableProperty] private string _targetFolderId;
        [ObservableProperty] private object _folderList;
        [ObservableProperty] private int _retentionDays;

        [ObservableProperty] private bool _isConnected;
        [ObservableProperty] private string _buttonText;


        GoogleDriveHelper GoogleDriveHelper = new GoogleDriveHelper();
        public GoogleDriveConfigViewModel(GoogleDriveConfig config = null)
        {
            if (config != null)
            {
                // Map the data from your DB model to the Observable properties
                this.FolderName = config.FolderName;
                this.UserEmail = config.UserEmail;
                this.RefreshToken = config.RefreshToken;
                this.TargetFolderId = config.TargetFolderId;
                this.FolderList = config.FolderList;
                this.RetentionDays = config.RetentionDays;
            }
            else
            {
                // Set defaults for a brand new config
                this.RetentionDays = 0;
            }
            CheckExistingConnection();
        }

        private async void CheckExistingConnection()
        {
            try
            {
                IsConnected = await GoogleDriveHelper.IsAuthorizedAsync(UserEmail);
                ButtonText = IsConnected ? "Connected" : "Link Account";
            }
            catch
            {
                ButtonText = "Link Account";
                IsConnected = false;
            }

        }

        private async Task AuthenticateGoogleDrive()
        {
            try
            {
                // This opens the Gmail login page in the browser
                UserCredential credential = await GoogleDriveHelper.AuthorizeTempAsync();

                if (credential != null)
                {
                    IsConnected = true;
                    var loginInfo = await GoogleDriveHelper.GetLoginInfoAsync(credential);
                    await GoogleDriveHelper.SaveAuthAsync(loginInfo.Email, credential);
                    UserEmail = loginInfo.Email;
                    CheckExistingConnection();
                    System.Windows.MessageBox.Show("Drive Connected Successfully!");
                }
            }
            catch (Exception ex)
            {
                ButtonText = "Link Accoun";
                IsConnected = false;
                System.Windows.MessageBox.Show("Login Failed: " + ex.Message);
            }

        }

        [RelayCommand]
        private async Task Authorize()
        {
            await AuthenticateGoogleDrive();
        }

        [RelayCommand]
        private async Task CreateDriveFolder()
        {
            try
            {               
                if (string.IsNullOrWhiteSpace(FolderName)) return;
                // Get the ID (either existing or newly created)
                UserCredential credential = await GoogleDriveHelper.GetAccountCredentialsAsync(UserEmail);
                string finalFolderId = await GoogleDriveHelper.GetOrCreateFolderAsync(credential, FolderName);
                
                TargetFolderId = finalFolderId;               
                MessageBox.Show("Folder linked successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
                      

       

        [RelayCommand]
        private async Task Test()
        {
            try
            {
                UserCredential credential = await GoogleDriveHelper.GetAccountCredentialsAsync(UserEmail);
                var testResult = await GoogleDriveHelper.TestConnectionAsync(credential, TargetFolderId);
                MessageBox.Show(testResult.message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        [RelayCommand]
        private void Save()
        {           

            if (ButtonText == "Connected")
            {
                    RequestClose?.Invoke(true);
            }
            else
            {
                MessageBox.Show("Drive not connected?");
            }
        }

        [RelayCommand]
        private void Cancel() => RequestClose?.Invoke(false);

        public GoogleDriveConfig GetModel()
        {
            return new GoogleDriveConfig
            {
                FolderName = this.FolderName,
                UserEmail = this.UserEmail,
                RefreshToken = this.RefreshToken,
                TargetFolderId = this.TargetFolderId,
                FolderList = this.FolderList,
                RetentionDays = this.RetentionDays,
            };
        }
    }
}
