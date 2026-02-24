using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Interface
{
    public interface IClosableDialog
    {
        event Action<bool?> RequestClose;
    }
}
