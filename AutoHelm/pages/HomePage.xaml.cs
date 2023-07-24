using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Diagnostics;
using System.Windows.Media;
using AutoHelm.UserControls;
    using System.IO;
using Firebase.Auth;
using static System.Net.Mime.MediaTypeNames;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace AutoHelm.pages
{
    public class SavedEventArgs : EventArgs
    {
        private readonly string filePath;

        public SavedEventArgs(string filePath)
        {
            this.filePath = filePath;
        }

        public string getfilePath
        {
            get { return this.filePath; }
        }
    }

    public partial class HomePage : Page
    {

        public delegate void MyEventHandler(object source, EventArgs e);
        public static event MyEventHandler NewAHILPage;
        public static event MyEventHandler OpenAHILPage;
        public static event MyEventHandler Load_Saved_Page;
        public static event MyEventHandler SyncAHIL;

        public HomePage()
        {
            InitializeComponent();
            getCachedPath();
        }
        private void NewAHILPage_Click(object sender, RoutedEventArgs e)
        {
            NewAHILPage(this, null);
        }
        private void OpenAHILPage_Click(object sender, RoutedEventArgs e)
        {
            OpenAHILPage(this, null);
        }
        private void SyncAHIL_Click(object sender, RoutedEventArgs e)
        {
            if (Interlocked.Exchange(ref AutoHelm.Firebase.FirebaseFunctions.isSaving, 1) == 0)
            {
                new Thread(() =>
                {
                    Interlocked.Exchange(ref AutoHelm.Firebase.FirebaseFunctions.isSaving, 1);
                    AutoHelm.Firebase.FirebaseFunctions.CloudUpload("", "");
                    MessageBox.Show("All applicable files have been saved to the cloud.",
                                     "Cloud Saving Complete");
                    Interlocked.Exchange(ref AutoHelm.Firebase.FirebaseFunctions.isSaving, 0);

                }).Start();
            }
            
        }
        private void NewButton_Click(object sender, RoutedEventArgs e)
        {
            Button clickedButton = (Button)sender;
            string filePath = (string)clickedButton.Tag;
            SavedEventArgs savedArgs = new SavedEventArgs(filePath);
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
