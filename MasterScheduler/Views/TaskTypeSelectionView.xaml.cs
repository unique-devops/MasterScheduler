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
    /// Interaction logic for TaskSelectionView.xaml
    /// </summary>
    public partial class TaskTypeSelectionView : UserControl
    {
        public TaskTypeSelectionView()
        {
            InitializeComponent();
            this.Loaded += (s, e) => {
                var item = JobTypeList.ItemContainerGenerator.ContainerFromIndex(0) as ListBoxItem;
                item?.Focus();
            };
        }
        private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            //PlaceholderText.Visibility = string.IsNullOrWhiteSpace(SearchBox.Text)
            //    ? Visibility.Visible
            //    : Visibility.Collapsed;
        }
    }
}
