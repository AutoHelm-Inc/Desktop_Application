using AutoHelm.user_controls;
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

namespace AutoHelm.pages.MainWindow
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            TopBar.HomeButton_Click_Page += TopBar_HomeButton_Click_Page;
            TopBar.CreateButton_Click_Page += TopBar_CreateButton_Click_Page;
            TopBar.ExecuteButton_Click_Page += TopBar_ExecuteButton_Click_Page;
        }
        private void TopBar_HomeButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new HomePage();
        }
        private void TopBar_CreateButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new CreatePage();
        }
        private void TopBar_ExecuteButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new ExecutePage();
        }

    }
}
