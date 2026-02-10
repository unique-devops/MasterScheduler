using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public class AddOnsViewModel
    {
        public string LicType { get; set; }      // Free / Trial / Pro Core
        public string LicStatus { get; set; }    // Active / Expired
        public ObservableCollection<string> EnabledModules { get; }
        public ObservableCollection<string> LockedModules { get; }
    }
}
