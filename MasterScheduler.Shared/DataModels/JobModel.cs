using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class JobModel
    {
        public int Id { get; set; }
        public string JobName { get; set; } = "";
        public string JobType { get; set; } = ""; // e.g. SQLBackup      
        public string CronExpression { get; set; } = "";
        public bool IsActive { get; set; }
        public string? Status { get; set; }
        public string? Message { get; set; }
        public string? Parameters { get; set; } // JSON config
        public DateTime? LastRunTime { get; set; }
        public DateTime? NextRunTime { get; set; }
    }
}
