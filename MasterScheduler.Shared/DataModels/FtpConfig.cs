using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class FtpConfig : DestinationConfig
    {
        public string Host { get; set; }
        public int Port { get; set; } = 21;
        public string Username { get; set; }
        public string Password { get; set; }         // store encrypted
        public string RemotePath { get; set; }
        public bool UseSsl { get; set; }
        public bool PassiveMode { get; set; }
    }

}
