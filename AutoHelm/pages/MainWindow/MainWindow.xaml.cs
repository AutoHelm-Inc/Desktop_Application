using AutoHelm.user_controls;
using Microsoft.Win32;
using System;
using System.IO;
using System.Windows;
using System.Windows.Navigation;
using System.Runtime.Caching;
using System.Collections.Generic;
using System.Text;
using System.Windows.Media.Animation;
using System.Windows.Controls;
using Automation_Project.src.parser;
using Automation_Project.src.ast;
using System.Windows.Media;
using System.Linq;
using AutoHelm.Shortcuts;

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

            Window virtualWindow = new Window();
            //create a test window to see screen resolution
            virtualWindow.Show();
            virtualWindow.Opacity = 0;
            virtualWindow.WindowState = WindowState.Maximized;
            double returnHeight = virtualWindow.Height;
            double returnWidth = virtualWindow.Width;
            virtualWindow.Close();
            //Change the UI window to the above-fetched size to prevent covering of the taskbar
            this.WindowState = WindowState.Maximized;
            this.MaxHeight = returnHeight;
            this.MaxWidth = returnWidth;
            this.ResizeMode = ResizeMode.CanMinimize;


            getPathsFromFile();

            //LoadingPageAnimation();

            /// Dev functions for how, except for maybe home page, that should be kept and changed to a home icon
            TopBar.HomeButton_Click_Page += TopBar_HomeButton_Click_Page;
            TopBar.CreateButton_Click_Page += CreateButton_Click_Page;
            TopBar.ExecuteButton_Click_Page += TopBar_ExecuteButton_Click_Page;


            TopBar.SaveAs_Click += SaveAs_Click;
            TopBar.Save_Click += Save_Click;

            HomePage.NewAHILPage += CreateButton_Click_Page;
            HomePage.OpenAHILPage += OpenButton_Click_Page;
            HomePage.Load_Saved_Page += Load_Saved_Page;

            CreatePage.OpenNewCreatePageEvent += OpenNewCreatePage;

            //Setup global shortcut manager
            ShortcutManager.systemHookSetup();
        }

        private void LoadingPageAnimation()
        {
            LoadingPage loadingPage = new LoadingPage();
            mainFrame.Content = loadingPage;
            topBar.Visibility = Visibility.Collapsed;

            Grid grid = (Grid)loadingPage.Content;
            Image logo = (Image)grid.FindName("loadingLogo");
            TextBlock title = (TextBlock)grid.FindName("loadingTitle");

            ScaleTransform scaleTransform = new ScaleTransform(0, 0);
            logo.RenderTransform = scaleTransform;
            logo.RenderTransformOrigin = new Point(0.5, 0.5);

            DoubleAnimation scaleXAnimation1 = new DoubleAnimation(0, 0.4, TimeSpan.FromSeconds(2.5));
            DoubleAnimation scaleYAnimation1 = new DoubleAnimation(0, 0.4, TimeSpan.FromSeconds(2.5));
            DoubleAnimation fadeInAnimation = new DoubleAnimation(0, 0.6, TimeSpan.FromSeconds(3.5));

            fadeInAnimation.Completed += (sender, e) =>
            {

                DoubleAnimation fadeInTitleAnimation = new DoubleAnimation(0, 0.7, TimeSpan.FromSeconds(2));
                fadeInTitleAnimation.Completed += (sender, e) =>
                {
                    DoubleAnimation fadeOutAnimation = new DoubleAnimation(0.6, 0, TimeSpan.FromSeconds(1.5));
                    fadeOutAnimation.Completed += (sender, e) =>
                    {
                        topBar.Visibility = Visibility.Visible;
                        mainFrame.Content = new LoginPopUp();
                        TopBar_HomeButton_Click_Page(this, null);
                    };
                        logo.BeginAnimation(Image.OpacityProperty, fadeOutAnimation);
                    title.BeginAnimation(Image.OpacityProperty, fadeOutAnimation);
                };

                title.BeginAnimation(Image.OpacityProperty, fadeInTitleAnimation);
            };

            logo.BeginAnimation(Image.OpacityProperty, fadeInAnimation);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, scaleXAnimation1);
            scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, scaleYAnimation1);
        }
        private void TopBar_HomeButton_Click_Page(object source, EventArgs e)
        {
            getPathsFromFile();
            mainFrame.Content = new HomePage();
        }
        private void CreateButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new CreatePage(null);
        }

        private void OpenNewCreatePage(object source, EventArgs e, CreatePage p)
        {
            mainFrame.Content = p;
        }
        private void OpenButton_Click_Page(object source, EventArgs e)
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;

            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "AHIL file (*.ahil)|*.ahil|Text file (*.txt)|*.txt";
            if (openFileDialog.ShowDialog() == true)
            {
                string filePath = openFileDialog.FileName;
                if (filePaths != null && filePaths.Contains(filePath))
                {
                    int index = filePaths.IndexOf(filePath);
                    List<string> displayNames = cache["displayName"] as List<string>;
                    List<string> descriptions = cache["description"] as List<string>;
                    SavedEventArgs savedArgs = new SavedEventArgs(filePath, displayNames[index], descriptions[index]);
                    Load_Saved_Page(this, savedArgs);
                }
                else
                {
                    // This all should change to reading from something
                    string displayName = System.IO.Path.GetFileNameWithoutExtension(filePath);
                    string description = "";

                    saveToCache(filePath, displayName, description);
                    SavedEventArgs savedArgs = new SavedEventArgs(filePath, displayName, description);
                    Load_Saved_Page(this, savedArgs);
                }

            }
            TopBar_HomeButton_Click_Page(this,null);
        }
        private void TopBar_ExecuteButton_Click_Page(object source, EventArgs e)
        {
            mainFrame.Content = new ExecutePage();
        }
        private void Load_Saved_Page(object source, EventArgs e)
        {
            SavedEventArgs savedArgs = e as SavedEventArgs;
            string filePath = savedArgs.getfilePath;
            string displayName = savedArgs.getDisplayName;
            string description = savedArgs.getDescription;

            if (!File.Exists(filePath)) {
                MessageBox.Show("File was either Deleted or Corrupted", "Opening Error", MessageBoxButton.OK, MessageBoxImage.Error);
                using (File.Create(filePath));
            }
            //Run the parser when we open a file so we get access to the AST
            Parser p = new Parser(filePath);
            //obtain the ast through parser
            AHILProgram program = p.parse();
            //pass in the new program to our create page
            CreatePage createPage = new CreatePage(program);

            mainFrame.Content = createPage;
            Grid grid = (Grid)createPage.Content;
            StackPanel stackPanel = (StackPanel)grid.FindName("createFieldPanel");
            TextBox textBox = (TextBox)grid.FindName("createTitle");
            TextBlock textBlock = (TextBlock)grid.FindName("createPath");
            TextBox textBox1 = (TextBox)grid.FindName("createDescription");
            textBlock.Text = filePath;
            textBox.Text = displayName;
            textBox1.Text = description;
            
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

                    // Writing to file as text
                    CreatePage createPage = (CreatePage)mainFrame.Content;

                    Grid grid = (Grid)createPage.Content;
                    TextBlock textBlock = (TextBlock)grid.FindName("createPath");
                    TextBox textBox = (TextBox)grid.FindName("createTitle");
                    TextBox textBox1 = (TextBox)grid.FindName("createDescription");
                    textBlock.Text = filePath;
                    string displayName = textBox.Text;
                    string description = textBox1.Text;

                    File.WriteAllText(filePath, createPage.GetProgram().generateProgramAHILCode());

                    saveToCache(filePath, displayName, description);
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
                TextBlock textBlock = (TextBlock)grid.FindName("createPath");
                TextBox textBox = (TextBox)grid.FindName("createTitle");
                TextBox textBox1 = (TextBox)grid.FindName("createDescription");
                string filePath = textBlock.Text;
                string displayName = textBox.Text;
                string description = textBox1.Text;

                if (File.Exists(filePath))
                {

                    File.WriteAllText(filePath, createPage.GetProgram().generateProgramAHILCode());
                    saveToCache(filePath, displayName, description);
                }
                else
                {
                    SaveAs_Click(this, null);
                }
            }
        }
        private void saveToCache(string filePath, string displayName, string description)
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;
            List<string> displayNames = cache["displayName"] as List<string>;
            List<string> descriptions = cache["description"] as List<string>;

            if (filePaths == null) { 
                filePaths = new List<string>();
                displayNames = new List<string>();
                descriptions = new List<string>();
                CacheItemPolicy policy = new CacheItemPolicy();
                cache.Set("path", filePaths, policy);
                cache.Set("displayName", displayNames, policy);
                cache.Set("description", descriptions, policy);
            }
            if (!filePaths.Contains(filePath)){
                filePaths.Add(filePath);
                displayNames.Add(displayName);
                descriptions.Add(description);
                Console.WriteLine(descriptions[0]);
            }
            else
            {
                int indexOfRecentFile = filePaths.IndexOf(filePath);
                filePaths.RemoveAt(indexOfRecentFile);
                displayNames.RemoveAt(indexOfRecentFile);
                descriptions.RemoveAt(indexOfRecentFile);
                filePaths.Add(filePath);
                displayNames.Add(displayName);
                descriptions.Add(description);
            }

        }
        private void getPathsFromFile()
        {
            List<string> filePaths = new List<string>();
            List<string> displayNames = new List<string>();
            List<string> descriptions = new List<string>();
            bool exists = Directory.Exists(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm"));

            if (!exists)
                Directory.CreateDirectory(System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm"));

            const Int32 BufferSize = 128;
            var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm/cachedPaths.xml");

            if (File.Exists(filePath))
            {
                // Get File
                using (var fileStream = File.OpenRead(filePath))
                using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                {
                    string path;
                    while ((path = streamReader.ReadLine()) != null)
                    {
                        if (File.Exists(path))
                        {
                            Console.WriteLine(path);
                            filePaths.Add(path);
                        }
                    }
                }

                Console.WriteLine(filePaths.Count);


                // Get Metadata
                for (int i = 0; i < filePaths.Count; i++)
                {
                    string fileName;
                    fileName = "cachedValues - " + i + ".xml";
                    filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm", fileName);

                    if (File.Exists(filePath))
                    {
                        using (var fileStream = File.OpenRead(filePath))
                        using (var streamReader = new StreamReader(fileStream, Encoding.UTF8, true, BufferSize))
                        {
                            string line;
                            string des = "";
                            bool dis = true;
                            while ((line = streamReader.ReadLine()) != null)
                            {
                                if (dis)
                                {
                                    displayNames.Add(line);
                                }
                                else
                                {
                                    des += line + "\n";
                                }
                                dis = false;
                            }
                            Console.WriteLine(des);
                            descriptions.Add(des);
                        }

                        saveToCache(filePaths[i], displayNames[i], descriptions[i]);
                    }
                }

                foreach (object item in displayNames)
                {
                    Console.WriteLine(item);
                }
            }
            else
            {
                using (StreamWriter sw = File.CreateText(filePath));
            }

        }
        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;
            List<string> displayNames = cache["displayName"] as List<string>;
            List<string> descriptions = cache["description"] as List<string>;

            var filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm/cachedPaths.xml");

            using (StreamWriter writetext = new StreamWriter(filePath))
            {
                foreach(string path in filePaths)
                {
                    writetext.WriteLine(path);
                }
            }

            string fileName;
            for (int i = 0; i < filePaths.Count; i++)
            {
                fileName = "cachedValues - " + i + ".xml";
                filePath = System.IO.Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "AutoHelm", fileName);

                using (StreamWriter writetext = new StreamWriter(filePath))
                {
                    writetext.WriteLine(displayNames[i]);
                    writetext.WriteLine(descriptions[i]);
                }
            }

            ShortcutManager.turnOffSystemHook();
        }

    }
}
