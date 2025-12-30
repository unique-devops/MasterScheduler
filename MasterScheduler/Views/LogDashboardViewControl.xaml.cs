using MasterScheduler.Shared.Data;
using MasterScheduler.Shared.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for LogDashboardViewControl.xaml
    /// </summary>
    public partial class LogDashboardViewControl : UserControl
    {
        private readonly JobRepository _repo = new JobRepository();
        private List<LogDto> _allLogs = new();
        public LogDashboardViewControl()
        {
            InitializeComponent();
            
        }

        private void LoadLogs()
        {
            // Fetch logs from your SQLite DB
            _allLogs = _repo.GetAllLogs();
            LogGrid.ItemsSource = _allLogs;
        }

        private void FilterLogs(object sender, EventArgs e)
        {
            var searchText = SearchBox.Text.ToLower();
            var selectedLevel = (LevelFilter.SelectedItem as ComboBoxItem)?.Content.ToString();

            var filtered = _allLogs.Where(log =>
                (selectedLevel == "All" || log.Level == selectedLevel) &&
                (string.IsNullOrEmpty(searchText) || log.Message.ToLower().Contains(searchText))
            ).ToList();

            LogGrid.ItemsSource = filtered;
        }

        private void ClearLogs(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Delete all log history?", "Confirm", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                _repo.DeleteLogs();
                LoadLogs();
            }
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }
    }
}
