using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using Google.Apis.Util.Store;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.JobHelper;
using Microsoft.Identity.Client.Platforms.Features.DesktopOs.Kerberos;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for GoogleDriveConfigDialog.xaml
    /// </summary>
    public partial class GoogleDriveConfigDialog : Window, INotifyPropertyChanged
    {
        private CancellationTokenSource _cts;

        public GoogleDriveConfig ResultConfig = new GoogleDriveConfig();

        private bool _isConnected;
        public bool IsConnected
        {
            get => _isConnected;
            set { _isConnected = value; OnPropertyChanged("IsConnected"); }
        }
        GoogleDriveHelper GoogleDriveHelper = new GoogleDriveHelper();
        public GoogleDriveConfigDialog(GoogleDriveConfig googleDriveConfig)
        {
            InitializeComponent();
            ResultConfig = googleDriveConfig ?? new GoogleDriveConfig();
            this.DataContext = this;
            CheckExistingConnection();
        }
        private async void CheckExistingConnection()
        {
            try
            {
                UserCredential credential = await GoogleDriveHelper.GetSilentCredentialsAsync();
                var testResult = await GoogleDriveHelper.TestAuthOnlyAsync(credential);               
                IsConnected = testResult.success;
                BtnAuthorize.Content = "Connected";
            }
            catch
            {
                BtnAuthorize.Content = "Link Accoun";
                IsConnected = false;
            }

        }
        private async Task AuthenticateGoogleDrive()
        {
            try
            {
                // This opens the Gmail login page in the browser
                UserCredential credential = await GoogleDriveHelper.GetCredentialsAsync();

                if (credential != null)
                {
                    IsConnected = true;
                    var details = await GoogleDriveHelper.GetAccountDetailsAndFolders(credential);

                    // Update your WPF UI
                    ResultConfig = details;
                    IsConnected = true;
                    BtnAuthorize.Content = "Connected";
                    MessageBox.Show("Drive Connected Successfully!");
                }
            }
            catch (Exception ex)
            {
                BtnAuthorize.Content = "Link Accoun";
                IsConnected = false;
                MessageBox.Show("Login Failed: " + ex.Message);
            }

        }

        private async void BtnAuthorize_Click(object sender, RoutedEventArgs e)
        {
            await AuthenticateGoogleDrive();
        }

        private async void BtnCreateDriveFolder_Click(object sender, RoutedEventArgs e)
        {
            string userInputName = TxtFolderPath.Text; // e.g., "My SQL Backups"
            if (string.IsNullOrWhiteSpace(userInputName)) return;
            // Get the ID (either existing or newly created)
            UserCredential credential = await GoogleDriveHelper.GetSilentCredentialsAsync();
            string finalFolderId = await GoogleDriveHelper.GetOrCreateFolderAsync(credential, userInputName);

            // Save this ID to your ResultConfig and SQLite
            ResultConfig?.TargetFolderId = finalFolderId;
            ResultConfig?.FolderName = userInputName;           

            MessageBox.Show("Folder linked successfully!");
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            Close();
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (BtnAuthorize.Content.ToString() == "Connected")
            {
                DialogResult = true;
                Close();
            }
            else
            {
                MessageBox.Show("Drive not connected?");               
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));       

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UserCredential credential = await GoogleDriveHelper.GetSilentCredentialsAsync();
                var testResult = await GoogleDriveHelper.TestConnectionAsync(credential, ResultConfig?.TargetFolderId);
                MessageBox.Show(testResult.message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            
        }
    }
}
