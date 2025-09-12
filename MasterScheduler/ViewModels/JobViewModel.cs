using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public class JobViewModel : ObservableObject
    {
        public int Id { get; set; }
        public string JobName { get; set; }
        public string Status { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public int Progress { get; set; }
    }
}
