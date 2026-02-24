using CommunityToolkit.Mvvm.ComponentModel;
using MasterScheduler.Models;
using MasterScheduler.Shared.DataModels;
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
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Window;

namespace MasterScheduler.Views
{
    /// <summary>
    /// Interaction logic for LocalPathBackupConfigDialog.xaml
    /// </summary>
    public partial class LocalPathBackupConfigDialog : Window
    {
        public LocalPathDestinationModel ViewModel => (LocalPathDestinationModel)DataContext;
        public LocalPathBackupConfigDialog(LocalPathDestinationModel LocalPathDestinationModel)
        {
            InitializeComponent();            
            this.DataContext = LocalPathDestinationModel;
        }

        private void btnBrowsePath_Click(object sender, RoutedEventArgs e)
        {
            using var dialog = new CommonOpenFileDialog { IsFolderPicker = true };
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                // Set the property on the ViewModel, UI updates automatically
                ViewModel.TargetPath = dialog.FileName;
            }
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (Directory.Exists(ViewModel.TargetPath))
            {
                DialogResult = true;
                Close();
            }
            else
            {
                txtPath.Background = Brushes.MistyRose; // Better UI than pure red
            }

        }

        private void MaskNumericInput(object sender, System.Windows.Input.TextCompositionEventArgs e)
        {
            // Check if the input text contains non-digit characters
            e.Handled = !e.Text.All(c => Char.IsDigit(c));
        }

        private void MaskNumericPaste(object sender, DataObjectPastingEventArgs e)
        {
            // Prevent pasting non-numeric text
            var text = e.DataObject.GetData(typeof(string)) as string;
            if (text == null || !text.All(Char.IsDigit))
            {
                e.CancelCommand();
            }
        }
    }
}
