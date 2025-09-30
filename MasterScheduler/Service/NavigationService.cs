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
    public class NavigationService : INavigationService
    {
        private readonly IServiceProvider _provider;
        private readonly NavigationHost _host;

        public NavigationService(IServiceProvider provider, NavigationHost host)
        {
            _provider = provider;
            _host = host;
        }

        public void NavigateTo<TViewModel>() where TViewModel : class
        {
            var vm = _provider.GetRequiredService<TViewModel>();
            _host.CurrentViewModel = vm;
        }

        public void NavigateTo<TViewModel>(object parameter) where TViewModel : class
        {
            var vm = _provider.GetRequiredService<TViewModel>();
            if (vm is INavigationAware aware)
                aware.OnNavigatedTo(parameter);

            _host.CurrentViewModel = vm;
        }
    }


}
