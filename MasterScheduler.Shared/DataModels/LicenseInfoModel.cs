using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class LicenseInfoModel
    {
        public string PCID { get; set; }
        public string Email { get; set; } = "";
        public string Edition { get; set; }   // Free, Trial, Lite, Pro
        public bool IsExpired { get; set; }
        public DateTime? ExpiryDate { get; set; }
        public HashSet<string> Modules { get; set; } = new();
        public HashSet<string> Connectors { get; set; } = new();       
        public string Status { get; set; }
        public string LicenseKey { get; set; }
    }
}
