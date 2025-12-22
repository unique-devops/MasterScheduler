using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class SqlBackupDetails
    {
        public string Server { get; set; } = "";
        public string AuthType { get; set; } = "";
        public string Username { get; set; } = "";
        public string Password { get; set; } = "";
        public string ConnectionString { get; set; } = "";
        public string BackupType { get; set; } = "";
        public bool Compression { get; set; } 
        public List<string> Databases { get; set; } = new List<string>();
        public List<BackupDestination> Destinations { get; set; } = new List<BackupDestination>();
    }
}
