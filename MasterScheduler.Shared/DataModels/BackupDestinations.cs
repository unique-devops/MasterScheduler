using MasterScheduler.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class BackupDestinations
    {
        public DestinationType Type { get; set; }
        public string Name { get; set; }
        public string IconPath { get; set; }
        public bool IsActive { get; set; }
    }
}
