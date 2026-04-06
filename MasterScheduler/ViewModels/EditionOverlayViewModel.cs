using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Shared.Service;
using MasterScheduler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MasterScheduler.ViewModels
{
    public partial class EditionOverlayViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        LicenseService licenseChecker = new LicenseService();

        [ObservableProperty]
        private string liteVersion = "Current version";

        [ObservableProperty]
        private string trialVersion = "Start 14-Day Free Trial";

        [ObservableProperty]
        private bool isActiveTrial;       

        [ObservableProperty]
        private bool isEmailInputVisible;

        [ObservableProperty]
        private string userEmail;
        public EditionOverlayViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            CheckLicense();
        }

       

        private void CheckLicense()
        {
            var type = "";
            TrialVersion = "Start 14-Day Free Trial";
            LiteVersion = type == "free" ? "current version" : "Activate Lite";
            var lic = licenseChecker.LoadAndVerifyLicense();
            if (lic != null && lic.Length == 4)
            {
                type = lic[0].Split("-")[0].ToLower();
                DateTime expiry = DateTime.Parse(lic[2]);
                if (type == "trial" && expiry > DateTime.Now)
                {
                    TrialVersion = "🎉 Trial Activated!";
                    IsActiveTrial = true;
                    return;
                }
                else if (type == "trial" && expiry < DateTime.Now)
                {
                    TrialVersion = "Trial Expired";
                    IsActiveTrial = true;
                }
                else {
                    TrialVersion = type;
                    IsActiveTrial = false;
                }
            }
               
        }

        [RelayCommand]
        private void ShowEmailForm()
        {
            IsEmailInputVisible = true;
        }

        [RelayCommand]
        private async Task StartTrial()
        {
            if (IsActiveTrial) return;
            EnterEmailDialog enterEmail = new EnterEmailDialog();
            enterEmail.Owner = App.Current.MainWindow;
            bool? result = enterEmail.ShowDialog();            
            if (IsActiveTrial || result is null || result == false) return;
            UserEmail = enterEmail.InputValue;
            if (string.IsNullOrEmpty(UserEmail) || !UserEmail.Contains("@")) return;
            TrialVersion = "🎉 Trial Activated!";
            await licenseChecker.ActivateTrialLicense(UserEmail);
            _navigationService.NavigateTo<DashboardViewModel>();
        }        

        [RelayCommand]
        private void GoBack()
        {
            //_navigationService.GoBack();
            _navigationService.NavigateTo<DashboardViewModel>();
        }
    }
}
