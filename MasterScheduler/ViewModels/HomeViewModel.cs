using MasterScheduler.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public partial class HomeViewModel
    {
        private readonly INavigationService _navigation;
        public HomeViewModel(INavigationService navigation)
        {
            _navigation = navigation;
            
        }
    }
}
