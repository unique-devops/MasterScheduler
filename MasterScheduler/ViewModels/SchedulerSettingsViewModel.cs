using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MasterScheduler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace MasterScheduler.ViewModels
{
    public partial class SchedulerSettingsViewModel : ObservableObject
    {
        [ObservableProperty]
        private int hour;

        [ObservableProperty]
        private int minute;

        [ObservableProperty]
        private bool isDaily;

        public ObservableCollection<DayItem> DaysOfWeek { get; }

        [ObservableProperty]
        private bool shouldClose;

        public SchedulerSettingsViewModel()
        {
            DaysOfWeek = new ObservableCollection<DayItem>
            {
                new("MON"), new("TUE"), new("WED"), new("THU"),
                new("FRI"), new("SAT"), new("SUN")
            };
        }

        [RelayCommand]
        private void ToggleDaily()
        {
            foreach (var day in DaysOfWeek)
                day.IsSelected = IsDaily;
        }

        [RelayCommand]
        private void Save()
        {
            ShouldClose = true;
        }

        [RelayCommand]
        private void Cancel()
        {
            ShouldClose = true;
        }

    }
}
