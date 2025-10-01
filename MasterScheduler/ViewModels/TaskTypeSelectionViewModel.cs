using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public partial class TaskTypeSelectionViewModel : ObservableObject
    {
        [ObservableProperty]
        private string headerTitle;

        private readonly INavigationService _navigationService;
        public TaskTypeSelectionViewModel(INavigationService navigationService)
        {
            HeaderTitle = "Choose task";
            _navigationService = navigationService;
        }

        [RelayCommand]
        private void GoBack()
        {
            //_navigationService.GoBack();
            _navigationService.NavigateTo<DashboardViewModel>();
        }
    }
}
