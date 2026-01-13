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
        public static DateTime getTimeZone()
        {
            string zoneId = OperatingSystem.IsWindows() ? "India Standard Time" : "Asia/Kolkata";
            var indiaTimeZone = TimeZoneInfo.FindSystemTimeZoneById(zoneId);            
            var nowInIndia = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, indiaTimeZone);
            return nowInIndia;
        }
        public static string GetNextRunAt(string crons)
        {
            //crons = $"0 {crons}";
            var schedule = CrontabSchedule.Parse(crons);
            var next = schedule.GetNextOccurrence(getTimeZone());

            return next.ToString("F");
        }
        public static DateTime? GetNextRunTime(string crons)
        {
            //crons = $"0 {crons}";
            var schedule = CrontabSchedule.Parse(crons);
            var next = schedule.GetNextOccurrence(getTimeZone());

            var nextTime = next.ToString("F");
            var isDateTimeValid = DateTime.TryParse(nextTime, out DateTime res);
            if (isDateTimeValid)
            {
                return res;
            }
            else
            {
                return null;
            }                       
        }

        public static string GetHumanReadableDescription(string crons)
        {
            //crons = $"0 {crons}";
            return ExpressionDescriptor.GetDescription(crons, new Options { Verbose = true });
        }

    }
}
