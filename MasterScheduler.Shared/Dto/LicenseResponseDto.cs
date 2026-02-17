using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Dto
{
    public class LicenseResponseDto
    {             
        public Guid id { get; set; }
       
        public string appName { get; set; }
       
        public string licenseKey { get; set; }
       
        public string ownerEmail { get; set; }
        
        public string status { get; set; }
       
        public int maxActivations { get; set; }
        
        public DateTime? expiresAt { get; set; }
        public List<LicenseActivationDto> activations { get; set; }
    }
}
