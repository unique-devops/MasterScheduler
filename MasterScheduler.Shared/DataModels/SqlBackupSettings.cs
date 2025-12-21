using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class SqlBackupSettings
    {
        public string ServerName { get; set; } = "";
        public List<string> DatabaseName { get; set; } = new List<string>();
        public List<BackupDestination> Destinations { get; set; } = new List<BackupDestination>();
    }
}
