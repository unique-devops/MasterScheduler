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
        LicenseService licenseService = new LicenseService();

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

        [ObservableProperty]
        private bool isPro;

        [ObservableProperty]
        private string proMessage;
        public EditionOverlayViewModel(INavigationService navigationService)
        {
            _navigationService = navigationService;
            _= LoadLicense();
        }

        private async Task LoadLicense()
        {
            try
            {
                var lic = licenseService.GetLicenses();               
                if (lic.Count > 0)
                {
                    var pro = lic.Find(c => c.LicenseName.Equals("PRO"));
                    if (pro != null)
                    {                        
                        IsPro = true;
                        var exp = $"{pro.ExpiryDate.Substring(0,2)}-{pro.ExpiryDate.Substring(2,2)}-{pro.ExpiryDate.Substring(4,4)}";
                        ProMessage = $"Activated (Expired on {exp})";
                    }                    
                }
            }
            catch (Exception ex)
            {
                await App.ToastService.ShowAsync(ex.Message,Shared.Enums.ToastType.Error);
            }
        }
        

        [RelayCommand]
        private async Task ActivateEdition()
        {
            ActivateEditionViewDialog _view = new ActivateEditionViewDialog();
            _view.Owner = App.Current.MainWindow;
            _view.ShowDialog();
            await LoadLicense();
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
            //await licenseChecker.ActivateTrialLicense(UserEmail);
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
