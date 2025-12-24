using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class SftpConfig : DestinationConfig
    {
        public string Host { get; set; }
        public int Port { get; set; } = 22;
        public string Username { get; set; }

        public string Password { get; set; }         // OR
        public string PrivateKeyPath { get; set; }

        public string RemotePath { get; set; }
    }

}
