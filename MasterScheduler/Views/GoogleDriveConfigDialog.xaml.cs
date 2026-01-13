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
                IsConnected = await GoogleDriveHelper.IsAuthorizedAsync(ResultConfig.UserEmail);               
                BtnAuthorize.Content = IsConnected ? "Connected" : "Link Account";
            }
            catch
            {
                BtnAuthorize.Content = "Link Account";
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
                    await GoogleDriveHelper.SaveAuthAsync(loginInfo.Email,credential);
                    ResultConfig.UserEmail = loginInfo.Email;
                    CheckExistingConnection();
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
            try
            {
                string folderName = TxtFolderPath.Text; // e.g., "My SQL Backups"
                if (string.IsNullOrWhiteSpace(folderName)) return;
                // Get the ID (either existing or newly created)
                UserCredential credential = await GoogleDriveHelper.GetAccountCredentialsAsync(ResultConfig.UserEmail);
                string finalFolderId = await GoogleDriveHelper.GetOrCreateFolderAsync(credential, folderName);

                // Save this ID to your ResultConfig and SQLite
                ResultConfig?.TargetFolderId = finalFolderId;
                ResultConfig?.FolderName = folderName;

                MessageBox.Show("Folder linked successfully!");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {           
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

        private async void btnTest_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UserCredential credential = await GoogleDriveHelper.GetAccountCredentialsAsync(ResultConfig?.UserEmail);
                var testResult = await GoogleDriveHelper.TestConnectionAsync(credential, ResultConfig?.TargetFolderId);
                MessageBox.Show(testResult.message);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));       

        
    }
}
