using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Models
{
    public partial class DayItem : ObservableObject
    {
        public DayItem(string code)
        {
            DayCode = code;
        }

        public string DayCode { get; }

        [ObservableProperty]
        private bool isSelected;
    }
}
