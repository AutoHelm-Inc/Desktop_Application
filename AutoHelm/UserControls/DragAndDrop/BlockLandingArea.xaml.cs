using Automation_Project.src.ast;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using static AutoHelm.UserControls.DragAndDrop.DraggingStatementBlock;

namespace AutoHelm.UserControls.DragAndDrop
{
    public partial class BlockLandingArea : UserControl
    {
        private Functions? function;
        private Keywords? keyword;
        private Boolean dropabble;
        private BlockLandingArea? parentBlock;
        public BlockLandingArea()
        {
            this.function = null; 
            this.keyword = null;
            this.AllowDrop = true;
            this.parentBlock = null;
            InitializeComponent();
        }

        public BlockLandingArea(BlockLandingArea parentBlock)
        {
            this.function = null;
            this.keyword = null;
            this.AllowDrop = true;
            this.parentBlock = parentBlock;
            InitializeComponent();
        }

        public Boolean Dropabble
        {
            get { return this.dropabble;}
            set { this.dropabble = value; }
        }

        private void UserControl_Drop(object sender, DragEventArgs dragEventData)
        {
            if (this.AllowDrop)
            {
                string oldDropZoneLabel = dropZoneLabel.Content.ToString();

                BlockDataToTransfer blockDataFromDrag = ((BlockDataToTransfer)(dragEventData.Data.GetData("DRAG_BLOCK_DATA")));

                borderRect.Fill = (Brush)blockDataFromDrag.backgroundColor;
                if(blockDataFromDrag.function != null)
                {
                    dropZoneLabel.Content = blockDataFromDrag.function.ToString();
                    this.function = blockDataFromDrag.function;
                    this.keyword = null;
                }
                else
                {
                    dropZoneLabel.Content = blockDataFromDrag.keyword.ToString();
                    this.keyword = blockDataFromDrag.keyword;
                    this.function = null;
                }
                dropZoneLabel.Foreground = blockDataFromDrag.labelColor;

                borderRect.StrokeDashArray = null;
                borderRect.Stroke = Brushes.Black;

                //Make delete button and set styling
                Button deleteButton = new Button();
                deleteButton.Background = new SolidColorBrush(Colors.Transparent);
                deleteButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                deleteButton.Width = 34;
                deleteButton.Height = 34;
                deleteButton.VerticalAlignment = VerticalAlignment.Top;
                deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
                deleteButton.Margin = new Thickness(10);
                deleteButton.Content = "X";
                deleteButton.FontSize = 18;
                deleteButton.FontWeight = FontWeights.Bold;
                deleteButton.Foreground = new SolidColorBrush(Colors.PaleVioletRed);
                deleteButton.Click += new RoutedEventHandler(DeleteStatementButton);

                //Make edit button and set styling
                Button editButton = new Button();
                editButton.Background = new SolidColorBrush(Colors.Transparent);
                editButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                editButton.Width = 34;
                editButton.Height = 34;
                editButton.VerticalAlignment = VerticalAlignment.Bottom;
                editButton.HorizontalAlignment = HorizontalAlignment.Right;
                editButton.Margin = new Thickness(10);
                Image editButtonImage = new Image();
                editButtonImage.Source = new BitmapImage(new Uri("C:\\Users\\zaidl\\Documents\\School\\Year 4\\ECE 498A\\AutoHelm\\Desktop_Application\\AutoHelm\\Assets\\gear.png"));
                //editButtonImage.Source = new BitmapImage(new Uri(@"../../Assets/gear.png", UriKind.Relative));
                editButtonImage.Width = 18;
                editButtonImage.Height = 18;
                editButton.Content = editButtonImage;
                editButton.Click += new RoutedEventHandler(EditStatementButton);

                landingAreaGrid.Children.Add(editButton);
                landingAreaGrid.Children.Add(deleteButton);

                StackPanel parentStackPanel = this.Parent as StackPanel;

                if (((oldDropZoneLabel.Equals("Drag Block Here!")) && (this.keyword == Keywords.For)) || (parentStackPanel.Name.Equals("NestedStatemetnsPanel")))
                {
                    if (this.keyword == Keywords.For)
                    {
                        landingAreaGrid.Width = landingAreaGrid.Width + 35;
                        borderRect.Height = borderRect.Height + 150;
                        borderRect.Width = borderRect.Width + 35;
                        NestedStatemetnsPanel.Children.Add(new BlockLandingArea(this));
                    }
                    changeParentDimensions(1);
                        
                    
                    parentStackPanel.Children.Add(new BlockLandingArea(parentBlock));

                }
                else if (oldDropZoneLabel.Equals("Drag Block Here!"))
                {                
                    parentStackPanel.Children.Add(new BlockLandingArea());
                }

            }
            this.AllowDrop = false;

        }

        private void changeParentDimensions(int factor)
        {
            if(this.parentBlock != null)
            {
                BlockLandingArea? tempParentBlock = this.parentBlock;
                Rectangle? tempRect = tempParentBlock.borderRect;
                Grid? tempGrid = tempParentBlock.landingAreaGrid;


                do
                {
                    if (this.keyword == Keywords.For)
                    {

                        tempGrid.Width = tempGrid.Width + 35 * factor;
                        tempRect.Width = tempRect.Width + 35 * factor;
                        
                        
                    }
                    tempRect.Height = tempRect.Height + (borderRect.Height+25)*factor;


                    tempParentBlock = tempParentBlock.parentBlock;
                    if(tempParentBlock != null)
                    {
                        tempRect = tempParentBlock.borderRect;
                        tempGrid = tempParentBlock.landingAreaGrid;
                    }

                } while (tempParentBlock != null);
            }
        }

        private void DragEnterLandingArea(object sender, DragEventArgs e)
        {
            if (this.AllowDrop)
            {
                borderRect.StrokeDashArray = new DoubleCollection() { 4, 2 };
                borderRect.Stroke = Brushes.LightBlue;
            }
        }

        private void DragLeaveLandingArea(object sender, DragEventArgs e)
        {
            borderRect.StrokeDashArray = null;
            borderRect.Stroke = Brushes.Black;
        }

        private void DeleteStatementButton(object sender, RoutedEventArgs routedEventArgs)
        {
            StackPanel parentStackPanel = this.Parent as StackPanel;
            parentStackPanel.Children.Remove(this);
            changeParentDimensions(-1);
        }

        private void EditStatementButton(object sender, RoutedEventArgs routedEventArgs)
        {
            if (function != null)
            {
                ParameterInputWindow parameterInputWindow = new ParameterInputWindow(function);
                parameterInputWindow.ShowDialog();
            }
            else
            {
                ParameterInputWindow parameterInputWindow = new ParameterInputWindow(keyword);
                parameterInputWindow.ShowDialog();
            }
        }
    }
}
