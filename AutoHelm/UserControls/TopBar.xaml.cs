using AutoHelm.pages;
using AutoHelm.pages.MainWindow;
using Microsoft.Win32;
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
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace AutoHelm.user_controls
{
    /// <summary>
    /// Interaction logic for TopBar.xaml
    /// </summary>
    public partial class TopBar : UserControl
    { 
        public delegate void MyEventHandler(object source, EventArgs e);

        public static event MyEventHandler HomeButton_Click_Page;
        public static event MyEventHandler CreateButton_Click_Page;
        public static event MyEventHandler ExecuteButton_Click_Page;

        public static event MyEventHandler SaveAs_Click;
        public static event MyEventHandler Save_Click;

        public static TopBar self;

        public TopBar()
        {
            InitializeComponent();
            self = this;
        }
        private void HomeButton_Click(object sender, RoutedEventArgs e)
        {
            HomeButton_Click_Page(this, null);
        }
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            CreateButton_Click_Page(this, null);
        }
        private void ExecuteButton_Click(object sender, RoutedEventArgs e)
        {
            ExecuteButton_Click_Page(this, null);
        }
        private void SaveFileAs_Click(object sender, RoutedEventArgs e)
        {
            SaveAs_Click(this, null);
        }
        private void SaveFile_Click(object sender, RoutedEventArgs e)
        {
            Save_Click(this, null);
        }
    }
}
