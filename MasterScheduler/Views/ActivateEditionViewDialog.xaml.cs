using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Shared.DataModels;
using MasterScheduler.Shared.Enums;
using MasterScheduler.Shared.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// Interaction logic for ActivateEditionViewDialog.xaml
    /// </summary>
    public partial class ActivateEditionViewDialog : Window, INotifyPropertyChanged
    {
        public ActivateEditionViewDialog()
        {
            InitializeComponent();
            DataContext = this;
        }
        LicenseService licenseService = new LicenseService();
        FingerprintGenerator fingerPrint = new FingerprintGenerator();

        private string key;
        public string Key
        {
            get => key;
            set
            {
                if (key != value)
                {
                    key = value;
                    OnPropertyChanged(nameof(Key));
                }
            }
        }

        private string pCID;
        public string PCID {
            get => pCID; 
            set {
                if (pCID != value)
                {
                    pCID = value;
                    OnPropertyChanged(nameof(PCID));
                }
            }
        }

        private bool isCopied;
        public bool IsCopied
        {
            get => isCopied;
            set
            {
                if (isCopied != value)
                {
                    isCopied = value;
                    OnPropertyChanged(nameof(IsCopied));
                }
            }
        }

        private string errorMessage;
        public string ErrorMessage
        {
            get => errorMessage;
            set
            {
                if (errorMessage != value)
                {
                    errorMessage = value;
                    OnPropertyChanged(nameof(ErrorMessage));
                }
            }
        }

        [RelayCommand]
        private void Cancel()
        {
            this.Close();
        }

        [RelayCommand]
        private void ActivateKey()
        {
            ErrorMessage = "";
            if(string.IsNullOrWhiteSpace(Key)) return;
            var success = LicenseService.VerifyLicense(Key,out LicenseDataModel license);
            if (!success)
            {
                ErrorMessage = "Invalid License !";
                return;
            }
            if (license.DeviceId != PCID || license.LicenseName != "PRO")
            {
                ErrorMessage = "Invalid License !";
                return;
            }
            success = LicenseService.IsLicenseExpired(license.ExpiryDate);
            if (success)
            {
                ErrorMessage = "License Expired !";
                return;
            }
            license.LicenseKey = Key;
            success = licenseService.SaveLicense(license,out string message);
            if (!success)
            {
                ErrorMessage = "Failed !";
                return;
            }
            this.Close();
        }

        [RelayCommand]
        private async Task Copy()
        {
            try
            {
                Clipboard.SetText(PCID);
                IsCopied = true;
                await Task.Delay(1000);
            }
            catch 
            {                
            }
            finally
            {
                IsCopied = false;
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            PCID = fingerPrint.GetId();
            txtLicKey.Focus();
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        
    }
}
