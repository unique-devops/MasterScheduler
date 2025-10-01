using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using MasterScheduler.Service;
using MasterScheduler.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        public INavigationService NavigationService { get; }

        [ObservableProperty]
        private ObservableObject _currentViewModel;
        public MainViewModel(INavigationService navigationService)
        {
            NavigationService = navigationService;

            // Default page
            NavigationService.NavigateTo<DashboardViewModel>();
        }

        //[RelayCommand]
        //private void Navigate(string destination)
        //{
        //    switch (destination)
        //    {
        //        case "Home": _navigationService.NavigateTo<HomeViewModel>(); break;
        //        case "Customer": _navigationService.NavigateTo<CustomerViewModel>(); break;
        //        case "Settings": _navigationService.NavigateTo<SettingsViewModel>(); break;
        //    }
        //}
    }

}
