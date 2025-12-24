using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class LocalFolderConfig : DestinationConfig
    {
        public string TargetPath { get; set; }       // C:\Backup or \\Server\Share
        public bool CreateDateFolder { get; set; }   // yyyy-MM-dd
    }

}
