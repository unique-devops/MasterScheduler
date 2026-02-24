using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Models
{
    public partial class LocalPathDestinationModel : ObservableObject
    {        
        [ObservableProperty]
        public string targetPath= "";
        [ObservableProperty]
        public bool isNetwork;
        [ObservableProperty]
        public string? username;
        [ObservableProperty]
        public string? password;
        [ObservableProperty]
        public int retentionDays = 0;
        [ObservableProperty]
        public bool createDateFolder;
    }
}
