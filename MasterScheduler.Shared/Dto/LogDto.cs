using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Dto
{
    public class LogDto
    {
        public int Id { get; set; }
        public int? JobId { get; set; }
        public string Type { get; set; }
        public string Message { get; set; }
        public string Level { get; set; } // Info, Warning, Error
        public DateTime Timestamp { get; set; }
        
    }
}
