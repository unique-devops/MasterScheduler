using Google.Apis.Auth.OAuth2;
using Google.Apis.Drive.v3;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.JobHelper;
using System;
using System.Collections.Generic;
using System.Linq;
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
    public partial class GoogleDriveConfigDialog : Window
    {
        private CancellationTokenSource _cts;
        public GoogleDriveConfig ResultConfig { get; private set; }
        public GoogleDriveConfigDialog()
        {
            InitializeComponent();
        }

        private async Task AuthorizeDrive()
        {
            string clientId = TxtClientId.Text;
            string clientSecret = TxtClientSecret.Password;

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                MessageBox.Show("Please enter both Client ID and Client Secret.");
                return;
            }

            BtnAuthorize.IsEnabled = false;
            StatusMsg.Text = "Opening browser for authorization...";
            _cts = new CancellationTokenSource();
            try
            {
                // Trigger Google OAuth 2.0
                UserCredential credential = await GoogleWebAuthorizationBroker.AuthorizeAsync(
                    new ClientSecrets { ClientId = clientId, ClientSecret = clientSecret },
                    new[] { DriveService.Scope.DriveFile },
                    "user",
                    CancellationToken.None
                );

                // Create the config object to return
                ResultConfig = new GoogleDriveConfig
                {
                    ClientId = clientId,
                    ClientSecret = clientSecret,
                    // Encrypt the RefreshToken immediately
                    RefreshToken = Cipher.Protect(credential.Token.RefreshToken),
                    TargetFolderId = TxtFolderId.Text
                };

                this.DialogResult = true; // Closes the window and signals success
            }
            catch (OperationCanceledException)
            {
                StatusMsg.Text = "User cancelled or timed out.";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Authorization Failed: {ex.Message}");
                BtnAuthorize.IsEnabled = true;
                StatusMsg.Text = "Failed.";
            }
        }

        private async void BtnAuthorize_Click(object sender, RoutedEventArgs e)
        {
            await AuthorizeDrive();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            _cts?.Cancel();
            Close();
        }
    }
}
