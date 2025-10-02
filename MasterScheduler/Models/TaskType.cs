using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Models
{
    public partial class TaskType : ObservableObject
    {
        [ObservableProperty]
        private int id;
        [ObservableProperty]
        private string name;
        [ObservableProperty]
        private string description;
    }
}
