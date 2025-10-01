using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Interface
{
    public interface INavigationService
    {
        ObservableObject CurrentViewModel { get; }       
        void NavigateTo<TViewModel>(bool IsPersistHistory = false) where TViewModel : class;
        void NavigateTo<TViewModel>(object parameter, bool IsPersistHistory = false) where TViewModel : class;

        void GoBack();
        bool CanGoBack { get; }
    }

}
