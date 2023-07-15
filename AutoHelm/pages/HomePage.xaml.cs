using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using AutoHelm.UserControls;
using System.IO;

namespace AutoHelm.pages
{
    public class SavedEventArgs : EventArgs
    {
        private readonly string fileName;

        public SavedEventArgs(string fileName)
        {
            this.fileName = fileName;
        }

        public string getFileName
        {
            get { return this.fileName; }
        }
    }

    public partial class HomePage : Page
    {

        public delegate void MyEventHandler(object source, EventArgs e);
        public static event MyEventHandler NewAHILPage;
        public static event MyEventHandler Load_Saved_Page;
        public HomePage()
        {
            InitializeComponent();
            getCachedPath();
        }
        private void NewAHILPage_Click(object sender, RoutedEventArgs e)
        {
            NewAHILPage(this, null);
        }
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string fileName = (string)clickedButton.Tag;
            SavedEventArgs savedArgs = new SavedEventArgs(fileName);
            Load_Saved_Page?.Invoke(this, savedArgs);
        }

        private void getCachedPath()
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;
            if (filePaths != null)
            {
                int rowCount = 0;
                int columnCount = 1;
                filePaths.Reverse();
                Console.WriteLine("reversed");
                foreach (string path in filePaths)
                {
                    if(columnCount == 5)
                    {
                        rowCount++;
                        columnCount = 0;
                    }

                    if(rowCount == 5) 
                    {
                        break;
                    }
                    Button newButton = new Button();
                    newButton.Background = Brushes.Transparent;
                    newButton.Foreground = Brushes.Transparent;
                    newButton.BorderBrush = Brushes.Transparent;
                    newButton.Tag = path;
                    newButton.Click += NewButton_Click;

                    Grid.SetColumn(newButton, columnCount);
                    Grid.SetRow(newButton, rowCount);

                    RecentFiles recentFiles = new RecentFiles();
                    string pathName = Path.GetFileNameWithoutExtension(path);
                    recentFiles.recTempBox.Text = pathName;
                    newButton.Content = recentFiles;
                    HomePageGrid.Children.Add(newButton);
                    columnCount++;
                }

                Console.WriteLine("reversed back");
                filePaths.Reverse();
            }
        }
    }
}
