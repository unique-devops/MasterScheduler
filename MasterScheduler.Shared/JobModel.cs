using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared
{
    public class JobModel
    {
        public Guid JobId { get; set; }
        public string Name { get; set; }
        public string ScheduleType { get; set; }
        public string CronExpression { get; set; }
        public int? IntervalValue { get; set; }
        public string IntervalUnit { get; set; }
        public string OneTimeDateTime { get; set; }
        public string LastStatus { get; set; }
        public string NextRun { get; set; }
        public string LastRun { get; set; }
        public int IsEnabled { get; set; } = 1;
    }

}
