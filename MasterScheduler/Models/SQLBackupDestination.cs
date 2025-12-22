using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Models
{
    public partial class SQLBackupDestination : ObservableObject
    {
        [ObservableProperty]
        private string type;

        [ObservableProperty]
        private string icon;

        [ObservableProperty]
        private string detail;
    }
}
