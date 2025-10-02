using MasterScheduler.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
using System.Windows.Shapes;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for DatabaseSelectionDialog.xaml
    /// </summary>
    public partial class DatabaseSelectionDialog : Window
    {
        private ObservableCollection<DatabaseItem> _availableDatabases;
        public ObservableCollection<DatabaseItem> AvailableDatabases
        {
            get => _availableDatabases;
            set
            {
                _availableDatabases = value;
                OnPropertyChanged(nameof(AvailableDatabases));
            }
        }

        public DatabaseSelectionDialog()
        {
            InitializeComponent();
            DataContext = this;
        }

        private void Ok_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}
