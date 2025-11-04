using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Models
{
    public partial class ScheduledJobDto : ObservableObject
    {
        [ObservableProperty]
        private int id;
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private string jobType;
        [ObservableProperty]
        private string status;
        [ObservableProperty]
        private string lastRunAt;
        [ObservableProperty]
        private string nextRunAt;               
    }
}
