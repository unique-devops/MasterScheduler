using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Interface
{
    public interface INavigationAware
    {
        void OnNavigatedTo(object parameter);
    }

}
