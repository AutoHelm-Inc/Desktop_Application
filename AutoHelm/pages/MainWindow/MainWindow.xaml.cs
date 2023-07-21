using AutoHelm.user_controls;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Runtime.Caching;
using System.Collections.Generic;
using System.Text;
using System.Windows.Shapes;
using System.Windows.Controls;

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
            getPathsFromFile();

            /// Dev functions for how, except for maybe home page, that should be kept and changed to a home icon
            TopBar.HomeButton_Click_Page += TopBar_HomeButton_Click_Page;
            TopBar.CreateButton_Click_Page += CreateButton_Click_Page;
            TopBar.ExecuteButton_Click_Page += TopBar_ExecuteButton_Click_Page;


            TopBar.SaveAs_Click += SaveAs_Click;
            TopBar.Save_Click += Save_Click;

            HomePage.NewAHILPage += CreateButton_Click_Page;
            HomePage.OpenAHILPage += OpenButton_Click_Page;
            HomePage.Load_Saved_Page += Load_Saved_Page;
        }

        private void TopBar_HomeButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new HomePage();
        }
        private void CreateButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new CreatePage();
        }
        private void OpenButton_Click_Page(object source, EventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "AHIL file (*.ahil)|*.ahil|Text file (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                saveToCache(filePath);
                SavedEventArgs savedArgs = new SavedEventArgs(filePath);
                Load_Saved_Page(this, savedArgs);
            }
        }
        private void TopBar_ExecuteButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new ExecutePage();
        }
        private void Load_Saved_Page(object source, EventArgs e)
        {
            SavedEventArgs savedArgs = e as SavedEventArgs;
            string filePath = savedArgs.getfilePath;
            CreatePage createPage = new CreatePage();
            mainFrame.Content = createPage;
            Grid grid = (Grid)createPage.Content;
            TextBlock textBlock = (TextBlock)grid.FindName("createTitle");
            textBlock.Text = filePath;

        }
        private void SaveAs_Click(object source, EventArgs e)
        {
            NavigationService navigationService = mainFrame.NavigationService;
            if (navigationService.Content is CreatePage)
            {
                SaveFileDialog saveAsDialog = new SaveFileDialog();
                saveAsDialog.Filter = "AHIL file (*.ahil)|*.ahil|Text file (*.txt)|*.txt";
                saveAsDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if ((bool)saveAsDialog.ShowDialog())
                {
                    string filePath = saveAsDialog.FileName;
                    CreatePage createPage = (CreatePage)mainFrame.Content;
                    File.WriteAllText(filePath, createPage.TempTxt.Text);
                    saveToCache(filePath);
                }
            }
        }
        private void Save_Click(object source, EventArgs e)
        {
            NavigationService navigationService = mainFrame.NavigationService;
            if (navigationService.Content is CreatePage)
            {
                CreatePage createPage = (CreatePage)mainFrame.Content;
                Grid grid = (Grid)createPage.Content;
                TextBlock textBlock = (TextBlock)grid.FindName("createTitle");
                string filePath = textBlock.Text;

                if (File.Exists(filePath))
                {
                    File.WriteAllText(filePath, createPage.TempTxt.Text);
                    saveToCache(filePath);
                }
                else
                {
                    SaveAs_Click(this, null);
                }
            }
        }
        private void saveToCache(string filePath)
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;

            if (filePaths == null) { 
                filePaths = new List<string>();
                CacheItemPolicy policy = new CacheItemPolicy();
                cache.Set("path", filePaths, policy);
            }
            if (!filePaths.Contains(filePath)){
                filePaths.Add(filePath);
            }
            else
            {
                int indexOfRecentFile = filePaths.IndexOf(filePath);
                filePaths.RemoveAt(indexOfRecentFile);
                filePaths.Add(filePath);
            }

        }
        private void getPathsFromFile()
        {

            bool exists = Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm"));

            if (!exists)
                Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm"));

            const Int32 BufferSize = 128;
            var fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm/cachedPaths.xml");

            if (File.Exists(fileName))
            {
                using (var fileStream = File.OpenRead(fileName))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    String line;
                    while ((line = streamReader.ReadLine()) != null)
                    {
                        saveToCache(line);
                    }
                }
            }
            else
            {
                using (StreamWriter sw = File.CreateText(fileName));
            }

        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;

            var fileName = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm/cachedPaths.xml");

            using (StreamWriter writetext = new StreamWriter(fileName))
            {
                foreach(string path in filePaths)
                {
                    writetext.WriteLine(path);
                }
            }
        }

    }
}
