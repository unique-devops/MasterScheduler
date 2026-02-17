using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Dto
{
    public class LicenseActivationDto
    {        
        public Guid id { get; set; }       
        public Guid licenseId { get; set; }        
        public string deviceId { get; set; }        
        public DateTime activatedAt { get; set; }
    }
}
