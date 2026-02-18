using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class LocalFolderConfig : DestinationConfig
    {
        public string TargetPath { get; set; }
        public bool IsNetwork { get; set; }       // C:\Backup or \\Server\Share
        public string? Username { get; set; }       // C:\Backup or \\Server\Share
        public string? Password { get; set; }       // C:\Backup or \\Server\Share
        public int Days { get; set; }       // C:\Backup or \\Server\Share
        public bool CreateDateFolder { get; set; }   // yyyy-MM-dd
    }

}
