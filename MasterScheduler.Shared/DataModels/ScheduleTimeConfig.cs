using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class ScheduleTimeConfig
    {
        public string Frequency { get; set; }
        public string ExecutionTime { get; set; }
        public List<string> DaysOfWeek { get; set; } = new List<string>();
    }
}
