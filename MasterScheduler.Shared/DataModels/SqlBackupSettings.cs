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
        public string DatabaseName { get; set; } = "";
        public string BackupFolder { get; set; } = "";
    }
}
