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
    public partial class DashboardViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;

        public DashboardViewModel(INavigationService navigation)
        {
            _navigation = navigation;
        }

        [RelayCommand]
        private void AddJob()
        {
           _navigation.NavigateTo<TaskTypeSelectionViewModel>();
        }
    }

}
