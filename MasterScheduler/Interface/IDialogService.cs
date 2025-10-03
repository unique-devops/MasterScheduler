using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Interface
{
    public interface IDialogService
    {
        bool? ShowDialog<TView>(TView view) where TView : class;
    }

}
