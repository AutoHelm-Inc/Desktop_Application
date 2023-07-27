using System;
using System.Collections.Generic;
using System.Runtime.Caching;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using AutoHelm.UserControls;
using System.Threading;
using System.Windows.Media.Animation;
using System.Windows.Input;

namespace AutoHelm.pages
{
    public class SavedEventArgs : EventArgs
    {
        private readonly string filePath;
        private readonly string displayName;
        private readonly string description;

        public SavedEventArgs(string filePath, string fileName, string description)
        {
            this.filePath = filePath;
            this.displayName = fileName;
            this.description = description;
        }

        public string getfilePath
        {
            get { return this.filePath; }
        }
        public string getDisplayName
        {
            get { return this.displayName; }
        }
        public string getDescription
        {
            get { return this.description; }
        }
    }

    public partial class HomePage : Page
    {

        public delegate void MyEventHandler(object source, EventArgs e);
        public static event MyEventHandler NewAHILPage;
        public static event MyEventHandler OpenAHILPage;
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
        private void SavedFile_ButtonClick(object sender, RoutedEventArgs e)
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> filePaths = cache["path"] as List<string>;
            List<string> displayNames = cache["displayName"] as List<string>;
            List<string> descriptions = cache["description"] as List<string>;

            Button clickedButton = (Button)sender;
            int index = (int)clickedButton.Tag;

            string filePath = filePaths[index];
            string displayName = displayNames[index];
            string description = descriptions[index];

            SavedEventArgs savedArgs = new SavedEventArgs(filePath, displayName, description);
            Load_Saved_Page?.Invoke(this, savedArgs);
        }

        private void getCachedPath()
        {
            ObjectCache cache = MemoryCache.Default;
            List<string> displayNames = cache["displayName"] as List<string>;
            if (displayNames != null)
            {
                int rowCount = 0;
                int columnCount = 1;
                for(int i = displayNames.Count - 1; i >= 0; i--)
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

                    Style style = new Style(typeof(Button));
                    ControlTemplate controlTemplate = new ControlTemplate(typeof(Button));
                    FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
                    borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
                    FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                    borderFactory.AppendChild(contentPresenterFactory);
                    controlTemplate.VisualTree = borderFactory;
                    Setter setter = new Setter(Control.TemplateProperty, controlTemplate);
                    style.Setters.Add(setter);

                    newButton.Style = style;

                    newButton.Background = Brushes.Transparent;
                    newButton.Foreground = Brushes.Transparent;
                    newButton.BorderBrush = Brushes.Transparent;
                    newButton.Cursor = Cursors.Hand;
                    newButton.Tag = i;
                    newButton.Name = "bruh";
                    newButton.Click += SavedFile_ButtonClick;

                    ///// Hovering
                    
                    ScaleTransform scaleTransform = new ScaleTransform(1, 1);
                    newButton.RenderTransformOrigin = new Point(0.5, 0.5);
                    newButton.RenderTransform = scaleTransform;

                    newButton.MouseEnter += (sender, e) =>
                    {
                        DoubleAnimation animation = new DoubleAnimation(1.1, TimeSpan.FromSeconds(0.1));
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };

                    newButton.MouseLeave += (sender, e) =>
                    {
                        DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };

                    /////

                    Grid.SetColumn(newButton, columnCount);
                    Grid.SetRow(newButton, rowCount);

                    RecentFiles recentFiles = new RecentFiles();

                    // Change this to display name
                    string displayName = displayNames[i];
                    recentFiles.recTempBox.Text = displayName;
                    newButton.Content = recentFiles;
                    HomePageGrid.Children.Add(newButton);
                    columnCount++;
                }
            }
        }
    }
}
