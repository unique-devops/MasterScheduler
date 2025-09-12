using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared
{
    public class JobStatusDto
    {
        public int JobId { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public DateTime? LastRun { get; set; }
        public DateTime? NextRun { get; set; }
        public bool IsEnabled { get; set; }
        public int Progress { get; set; }
    }
}
