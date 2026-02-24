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
    /// Interaction logic for LocalPathBackupConfigView.xaml
    /// </summary>
    public partial class LocalPathBackupConfigView : UserControl
    {
        public LocalPathBackupConfigView()
        {
            InitializeComponent();
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
