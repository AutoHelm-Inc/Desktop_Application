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
    public struct ReusableDialogButton
    {
        public string text;
        public Action action;
    }

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

        public ReusableDialog(string body, ReusableDialogButton[] buttons)
        {
            InitializeComponent();
            this.dialogBody.Text = body;
            foreach (var button in buttons)
            {
                Grid grid = new Grid();
                Button b = new Button();
                TextBlock tb = new TextBlock();
                b.Margin = new Thickness(30, 0, 30, 0);
                b.HorizontalAlignment = HorizontalAlignment.Center;
                b.VerticalAlignment = VerticalAlignment.Center;
                b.Background = Brushes.Transparent;
                b.BorderThickness = new Thickness(0);
                b.Cursor = Cursors.Hand;
                b.Content = tb;
                Style buttonStyle = new Style();
                ControlTemplate templateButton = new ControlTemplate(typeof(Button));
                FrameworkElementFactory elemFactory = new FrameworkElementFactory(typeof(Border));
                elemFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
                templateButton.VisualTree = elemFactory;
                elemFactory.AppendChild(new FrameworkElementFactory(typeof(ContentPresenter)));
                buttonStyle.Setters.Add(new Setter { Property = Button.TemplateProperty, Value = templateButton });
                b.Style = buttonStyle;

                tb.Text = button.text;
                tb.FontSize = 20;
                tb.Foreground = (Brush)FindResource("BlueAccent");
                tb.FontWeight = FontWeights.DemiBold;

                void buttonClick(object sender, RoutedEventArgs e)
                {
                    Close();
                    button.action.Invoke();
                }
                b.Click += new RoutedEventHandler(buttonClick);
                grid.Children.Add(b);
                this.buttonsStackPanel.Children.Add(grid);
            }
        }
        private void closeButtonClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
