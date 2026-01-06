using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Models
{
    public class ScheduleTimeModel
    {
        public int Hour;

        public int Minute;

        public string EveryType = "";

        public int EveryTime;

        public List<int> Weeks = new List<int>();
        public string? Crons;
    }
}
