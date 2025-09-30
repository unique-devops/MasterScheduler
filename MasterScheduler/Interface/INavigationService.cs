using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Interface
{
    public interface INavigationService
    {
        void NavigateTo<TViewModel>() where TViewModel : class;
        void NavigateTo<TViewModel>(object parameter) where TViewModel : class;
    }

}
