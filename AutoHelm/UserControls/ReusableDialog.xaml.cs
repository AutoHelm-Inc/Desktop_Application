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
using System.Windows.Shapes;

namespace AutoHelm.UserControls
{
    /// <summary>
    /// Interaction logic for ReusableDialog.xaml
    /// </summary>
    public partial class ReusableDialog : Window
    {
        public ReusableDialog()
        {
            InitializeComponent();
        }

        public ReusableDialog(string body)
        {
            InitializeComponent();
            this.dialogBody.Text = body;
        }
        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
