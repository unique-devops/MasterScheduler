using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class LicenseDataModel
    {
        public string DeviceId { get; set; }
        public string LicenseName { get; set; }
        public string LicenseKey { get; set; }
        public string ExpiryDate { get; set; }
        public bool IsExpired { get; set; }
    }
}
