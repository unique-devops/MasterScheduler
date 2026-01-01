using CommunityToolkit.Mvvm.ComponentModel;
using MasterScheduler.Models;
using Microsoft.VisualBasic.ApplicationServices;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.IO;
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
    /// Interaction logic for LocalPathBackupConfigDialog.xaml
    /// </summary>
    public partial class LocalPathBackupConfigDialog : Window
    {
        private readonly LocalPathDestinationModel pathDestination = new();
        public LocalPathBackupConfigDialog()
        {
            InitializeComponent();
            DataContext = pathDestination;
        }

        private void btnBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog
            {
                IsFolderPicker = true,
                Title = "Select Folder"
            };

            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathDestination.Path = dialog.FileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(pathDestination.Path))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                txtPath.Background = Brushes.Red;
            }
            
        }
    }
}
