using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static MasterScheduler.Models.Enums;

namespace MasterScheduler.Models
{
    public partial class DestinationModel : ObservableObject
    {
        [ObservableProperty]
        private int id;

        [ObservableProperty]
        private DestinationType type;

        [ObservableProperty]
        private string displayName;

        [ObservableProperty]
        private string pathOrEndpoint;

    }    
}
