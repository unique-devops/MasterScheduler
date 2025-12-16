using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared
{
    public static class CronosHelper
    {
        public static DateTime? GetNextRunTime(string crons)
        {
            
            var schedule = CrontabSchedule.Parse(crons);
            DateTime startOfToday = DateTime.Now.Date;
            DateTime scheduledTime = schedule.GetNextOccurrence(startOfToday);
            DateTime currentTime = DateTime.Now;
            if (scheduledTime < currentTime)
            {               
                return schedule.GetNextOccurrence(scheduledTime.AddSeconds(1));
            }
            else
            {               
                return scheduledTime;
            }            
        }
       
    }
}
