using CronExpressionDescriptor;
using NCrontab;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared
{
    public static class CronosHelper
    {
        public static string GetNextRunAt(string Crons)
        {
            var schedule = CrontabSchedule.Parse(Crons);
            var next = schedule.GetNextOccurrence(DateTime.Now);

            return next.ToString("F");
        }
        public static DateTime? GetNextRunTime(string crons)
        {


            var schedule = CrontabSchedule.Parse(crons);
            var next = schedule.GetNextOccurrence(DateTime.Now);

            var nextTime =  next.ToString("F");
            var isDateTimeValid = DateTime.TryParse(nextTime, out DateTime res);
            if (isDateTimeValid)
            {
                return res;
            }
            else
            {
                return null;
            }
            //var schedule = CrontabSchedule.Parse(crons);
            //DateTime startOfToday = DateTime.Now.Date;
            //DateTime scheduledTime = schedule.GetNextOccurrence(startOfToday);
            //DateTime currentTime = DateTime.Now;
            //if (scheduledTime < currentTime)
            //{               
            //    return schedule.GetNextOccurrence(scheduledTime.AddSeconds(1));
            //}
            //else
            //{               
            //    return scheduledTime;
            //}            
        }

        public static string GetHumanReadableDescription(string crons)
        {

            return ExpressionDescriptor.GetDescription(crons, new Options { Verbose = true });
        }

    }
}
