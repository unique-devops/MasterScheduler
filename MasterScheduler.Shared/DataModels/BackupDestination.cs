using MasterScheduler.Shared.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MasterScheduler.Shared.DataModels
{
    public class BackupDestination
    {
        public Guid Id { get; set; }
        public DestinationType Type { get; set; }
        public string DisplayText { get; set; }
        public DestinationConfig Config { get; set; }
    }
}
