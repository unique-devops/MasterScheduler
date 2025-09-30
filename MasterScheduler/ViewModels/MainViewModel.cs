using CommunityToolkit.Mvvm.ComponentModel;
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
        private readonly NavigationHost _host;

        public object CurrentView => _host.CurrentViewModel;
        public MainViewModel(NavigationHost host)
        {
            _host = host;
           
        }

        
    }

}
