using MasterScheduler.ViewModels;
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
    /// Interaction logic for SQLBackupScheduleView.xaml
    /// </summary>
    public partial class SQLBackupScheduleView : UserControl
    {
        public SQLBackupScheduleView()
        {
            InitializeComponent();
        }

        private void btnBackupDestiny_Click(object sender, RoutedEventArgs e)
        {
            btnBackupDestiny.ContextMenu.PlacementTarget = btnBackupDestiny;
            btnBackupDestiny.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
            btnBackupDestiny.ContextMenu.IsOpen = true;
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            var viewmodel = (SQLBackupScheduleViewModel)this.DataContext;
            viewmodel.BackupDestinationCommand.Execute(this);
        }
        private void ContextMenuButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.ContextMenu != null)
            {
                button.ContextMenu.PlacementTarget = button;
                button.ContextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom;
                button.ContextMenu.IsOpen = true;
            }
        }

    }
}
