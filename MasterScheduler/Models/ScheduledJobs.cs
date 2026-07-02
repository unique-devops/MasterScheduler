using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Models
{
    public partial class ScheduledJobs : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private string jobName;

        [ObservableProperty]
        private string databaseName;

        [ObservableProperty]
        private string jobType;

        [ObservableProperty]
        private string scheduledAt;

        [ObservableProperty]
        private string nextRunAt;

        [ObservableProperty]
        private string lastRunAt;

        [ObservableProperty]
        private string scheduledMessage;

        [ObservableProperty]
        private int percent;

        [ObservableProperty]
        private string progress;

        [ObservableProperty]
        private string speed;

        [ObservableProperty]
        private string eta;

        [ObservableProperty]
        private string status;

        [ObservableProperty]
        private string statusColor ="#179C38";

        [ObservableProperty]
        private string statusMessage;

        [ObservableProperty]
        private bool isActive;
    }
}
