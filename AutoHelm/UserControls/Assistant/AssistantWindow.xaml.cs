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

namespace AutoHelm.UserControls.Assistant
{
    /// <summary>
    /// Interaction logic for AssistantWindow.xaml
    /// </summary>
    public partial class AssistantWindow : Window
    {
        public string text
        {
            get { return assistantTextBox.Text; }
        }

        public AssistantWindow()
        {
            InitializeComponent();
        }

        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            assistantTextBox.Text = string.Empty;
            Close();
        }

        private void saveButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
