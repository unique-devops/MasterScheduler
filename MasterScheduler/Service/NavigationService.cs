using CommunityToolkit.Mvvm.ComponentModel;
using MasterScheduler.Interface;
using MasterScheduler.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Service
{
    public class NavigationService : ObservableObject, INavigationService
    {
        private readonly IServiceProvider _provider;

        private readonly Stack<ObservableObject> _history = new();

        private ObservableObject _currentViewModel;
        public ObservableObject CurrentViewModel
        {
            get => _currentViewModel;
            private set => SetProperty(ref _currentViewModel, value);
        }

        public NavigationService(IServiceProvider provider)
        {
            _provider = provider;          
        }

        public void NavigateTo<TViewModel>(bool IsPersistHistory = false) where TViewModel : class
        {
            if (_currentViewModel != null && IsPersistHistory)
                _history.Push(_currentViewModel);

            var vm = _provider.GetRequiredService<TViewModel>();
            CurrentViewModel = vm as ObservableObject;
        }

        public void NavigateTo<TViewModel>(object parameter, bool IsPersistHistory = false) where TViewModel : class
        {
            if (_currentViewModel != null && IsPersistHistory)
                _history.Push(_currentViewModel);

            var vm = _provider.GetRequiredService<TViewModel>();
            if (vm is INavigationAware aware)
                aware.OnNavigatedTo(parameter);

            CurrentViewModel = vm as ObservableObject;
        }

        public void GoBack()
        {
            if (_history.Count > 0)
                CurrentViewModel = _history.Pop();
        }       

        public bool CanGoBack => _history.Count > 0;
    }


}
