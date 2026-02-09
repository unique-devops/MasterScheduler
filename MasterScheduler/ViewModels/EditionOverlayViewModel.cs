using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Shared.Service;
using MasterScheduler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Management;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MasterScheduler.ViewModels
{
    public partial class EditionOverlayViewModel : ObservableObject
    {
        private readonly INavigationService _navigationService;
        LicenseChecker licenseChecker = new LicenseChecker();

        [ObservableProperty]
        private string liteVersion = "Current version";

        [ObservableProperty]
        private string trialVersion = "Start Trial";

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
            var lic = licenseChecker.GetLocalLicense();
            TrialVersion = lic.LicenseType == "Trial" ? "🎉 Trial Activated!" : "Start Trial";
            IsActiveTrial = lic.LicenseType == "Trial";
            LiteVersion = lic.LicenseType == "lite" ? "current version" : "Activate Lite";
        }

        [RelayCommand]
        private void ShowEmailForm()
        {
            IsEmailInputVisible = true;
        }

        [RelayCommand]
        private async Task ActivateTrial()
        {
            EnterEmailDialog enterEmail = new EnterEmailDialog();
            enterEmail.Owner = App.Current.MainWindow;
            bool? result = enterEmail.ShowDialog();            
            if (IsActiveTrial || result is null || result == false) return;
            UserEmail = enterEmail.InputValue;
            if (string.IsNullOrEmpty(UserEmail) || !UserEmail.Contains("@")) return;
            TrialVersion = "🎉 Trial Activated!";
            await licenseChecker.ActivateTrialLicense("trial@gmail.com");
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
