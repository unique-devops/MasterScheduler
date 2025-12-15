using Cronos;
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
            var cronExp = CronExpression.Parse(crons);

            DateTimeOffset nowUtc = DateTimeOffset.UtcNow;

            var next = cronExp.GetNextOccurrence(DateTime.UtcNow);
            return next;
        }
    }
}
