using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.Dto
{
    public class LicenseResponseDto
    {
        public string PCID { get; set; }
        public string Email { get; set; }
        public string LicenseType { get; set; }
        public string ExpiryDate { get; set; }
        public string Status { get; set; }
        public string LicenseKey { get; set; }
    }
}
