using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.ViewModels
{
    public partial class ScheduleTimeViewModel : ObservableObject
    {
        [ObservableProperty]
        private int hour;

        [ObservableProperty]
        private int minute;

        [ObservableProperty]
        private bool isDaily;
        

        public ObservableCollection<DayItem> Days { get; } =
                new ObservableCollection<DayItem>
                {
                    new DayItem("Sun" ),
                    new DayItem( "Mon" ),
                    new DayItem( "Tue" ),
                    new DayItem( "Wed" ),
                    new DayItem( "Thu" ),
                    new DayItem( "Fri" ),
                    new DayItem( "Sat" )
                };
        public ScheduleTimeViewModel()
        {
            Hour = DateTime.Now.Hour;
            Minute = DateTime.Now.Minute;
        }

        

    }
}
