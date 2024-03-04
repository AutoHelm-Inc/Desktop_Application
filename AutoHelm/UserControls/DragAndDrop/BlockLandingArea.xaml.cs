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
using System.IO;
using static AutoHelm.UserControls.DragAndDrop.DraggingStatementBlock;
using System.Configuration.Internal;
using System.Xml.Linq;

namespace AutoHelm.UserControls.DragAndDrop
{
    public partial class BlockLandingArea : UserControl
    {
        private Functions? function;
        private Keywords? keyword;
        private MacroKeyword? macro;
        private Boolean dropabble;
        private BlockLandingArea? parentBlock;
        private int depth;
        private static AHILProgram? program;
        private Statement? _statement;
        private int numNestableChildBlocks;

        public BlockLandingArea(AHILProgram program) {
            this.function = null;
            this.keyword = null;
            this.macro = null;
            this.AllowDrop = true;
            this.parentBlock = null;
            depth = 0;
            BlockLandingArea.program = program;
            numNestableChildBlocks = 0;
            InitializeComponent();
        }

        public BlockLandingArea(BlockLandingArea parentBlock)
        {
            this.function = null;
            this.keyword = null;
            this.macro = null;
            this.AllowDrop = true;
            this.parentBlock = parentBlock;
            this.depth = 0;
            numNestableChildBlocks = 0;
            InitializeComponent();
        }

        public BlockLandingArea(BlockLandingArea parentBlock, int depth)
        {
            this.function = null;
            this.keyword = null;
            this.macro = null;
            this.AllowDrop = true;
            this.parentBlock = parentBlock;
            this.depth = depth;
            numNestableChildBlocks = 0;
            InitializeComponent();
        }
        public BlockLandingArea(Functions? function, Keywords? keyword, MacroKeyword? macro, BlockLandingArea? parentBlock)
        {
            this.function = function;
            this.keyword = keyword;
            this.macro = null;
            this.AllowDrop = true;
            this.parentBlock = parentBlock;
            this.depth = 0;
            numNestableChildBlocks = 0;
            InitializeComponent();
        }

        public Boolean Dropabble
        {
            get { return this.dropabble;}
            set { this.dropabble = value; }
        }

        public static String? fromEnum(Functions? @enum)
        {
            return @enum switch
            {
                Functions.Run => "Run Program",
                Functions.SwitchWindow => "Switch Window",
                Functions.Close => "Close Window",
                Functions.FileCreate => "Create File",
                Functions.DirCreate => "Create Folder",
                Functions.Save => "Save File",
                Functions.Move => "Move File/Folder",
                Functions.Del => "Delete File/Folder",
                Functions.WriteLine => "Write with New Line",
                Functions.Write => "Write",
                Functions.PressKey => "Press Key",
                //Functions.EmailsGet => _emailsGetInstance,
                //Functions.FilesGet => _filesGetInstance,
                Functions.MouseMove => "Move Mouse",
                Functions.Click => "Click Mouse",
                Functions.SaveAs => "Save As",
                Functions.Sleep => "Delay",
                Functions.MouseToWord => "Move Mouse to Word",
                _ => null,
            };
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
                    dropZoneLabel.Content = fromEnum(blockDataFromDrag.function);
                    this.function = blockDataFromDrag.function;
                    this.keyword = null;
                    this.macro = null;
                    SimpleStatement statement = new SimpleStatement(blockDataFromDrag.function);
                    if (parentBlock != null) {
                        ((NestedStructure)parentBlock._statement).addStatement(statement);
                        _statement = statement;
                    }
                    else if (program != null) {
                        program.addStatement(statement);
                        _statement = statement;
                    }
                }
                else if (blockDataFromDrag.keyword != null)
                {
                    //Set the block label for keyword
                    if (blockDataFromDrag.keyword.ToString() == "For")
                    {
                        dropZoneLabel.Content = "Loop";
                    }
                    else
                    {
                        dropZoneLabel.Content = keyword.ToString();
                    }
                    this.keyword = blockDataFromDrag.keyword;
                    this.function = null;
                    this.macro = null;
                    Statement statement = keyword switch {
                        Keywords.For => new ForLoop(),
                        _ => throw new NotImplementedException("Other keywords are not implemented"),
                    };
                    if (parentBlock != null) {
                        ((NestedStructure)parentBlock._statement).addStatement(statement);
                        _statement = statement;
                    } 
                    else if (program != null) {
                        program.addStatement(statement);
                        _statement = statement;
                    }
                }
                else
                {
                    //Set the block label for keyword
                    if (blockDataFromDrag.macro.ToString() == "GlobalDelay")
                    {
                        dropZoneLabel.Content = "Global Delay";
                    }
                    else
                    {
                        dropZoneLabel.Content = keyword.ToString();
                    }
                    this.macro = blockDataFromDrag.macro;
                    this.keyword = null;
                    this.function = null;
                    Macro statement = new Macro(blockDataFromDrag.macro);
                    if (program != null)
                    {
                        program.addMacros(statement);
                        _statement = statement;
                    }
                }
                Console.WriteLine(program.generateProgramAHILCode());

                dropZoneLabel.Foreground = blockDataFromDrag.labelColor;

                borderRect.StrokeDashArray = null;
                borderRect.Stroke = Brushes.Black;

                Style style = new Style(typeof(Button));
                ControlTemplate controlTemplate = new ControlTemplate(typeof(Button));
                FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
                borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
                FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
                borderFactory.AppendChild(contentPresenterFactory);
                controlTemplate.VisualTree = borderFactory;
                Setter setter = new Setter(Control.TemplateProperty, controlTemplate);
                style.Setters.Add(setter);

                //Make delete button and set styling
                Button deleteButton = new Button();
                deleteButton.Background = new SolidColorBrush(Colors.Transparent);
                deleteButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                deleteButton.Width = 34;
                deleteButton.Height = 34;
                deleteButton.VerticalAlignment = VerticalAlignment.Top;
                deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
                deleteButton.Margin = new Thickness(10);
                Image deleteButtonImage = new Image();
                deleteButtonImage.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/close2.png")));
                deleteButtonImage.Width = 17;
                deleteButtonImage.Height = 17;
                deleteButton.Content = deleteButtonImage;
                deleteButton.Cursor = Cursors.Hand;
                deleteButton.Style = style;
                deleteButton.Click += new RoutedEventHandler(DeleteStatementButton);

                //Make edit button and set styling
                Button editButton = new Button();
                editButton.Background = new SolidColorBrush(Colors.Transparent);
                editButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
                editButton.Width = 34;
                editButton.Height = 34;
                editButton.VerticalAlignment = VerticalAlignment.Top;
                editButton.HorizontalAlignment = HorizontalAlignment.Left;
                editButton.Margin = new Thickness(0, 10, 0, 0);
                Image editButtonImage = new Image();
                //editButtonImage.Source = new BitmapImage(new Uri("C:\\Users\\zaidl\\Documents\\School\\Year 4\\ECE 498A\\AutoHelm\\Desktop_Application\\AutoHelm\\Assets\\gear.png"));
                editButtonImage.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/gear.png")));
                //editButtonImage.Source = new BitmapImage(new Uri(@"Assets/gear.png", UriKind.Relative));
                editButtonImage.Width = 18;
                editButtonImage.Height = 18;
                editButton.Content = editButtonImage;
                editButton.Cursor = Cursors.Hand;
                editButton.Style = style;
                editButton.Click += new RoutedEventHandler(EditStatementButton);

                //Add Block Icon
                dragBlockIcon.Margin = new Thickness(0, 35, 0, 0);
                dragBlockIcon.Width = 50;
                dragBlockIcon.Height = 50;
                dropZoneLabel.Margin = new Thickness(0, 0, 0, 0);

                if (function != null)
                {
                    dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + function.ToString() + ".png")));
                }
                else if (keyword != null)
                {
                    dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + keyword.ToString() + ".png")));
                }
                else
                {
                    dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + macro.ToString() + ".png")));
                }

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
                    updateDepth(1);
                    //changeParentDimensions(1);
                        
                    
                    parentStackPanel.Children.Add(new BlockLandingArea(parentBlock));

                }
                else if (oldDropZoneLabel.Equals("Drag Block Here!"))
                {                
                    parentStackPanel.Children.Add(new BlockLandingArea(program));
                }

            }
            this.AllowDrop = false;

        }

        private void updateDepth(int factor)
        {
            BlockLandingArea? tempParentBlock = this.parentBlock;
            BlockLandingArea? tempChildBlock = this;
            Boolean widthMustIncrease = false;

            if (tempParentBlock != null && this.keyword == Keywords.For && factor < 0)
            {
                tempParentBlock.numNestableChildBlocks = tempParentBlock.numNestableChildBlocks - 1;
            }


            while (tempParentBlock != null)
            {
                changeParentHeight(Math.Abs(factor) / factor, tempChildBlock, tempParentBlock);
                if (this.keyword == Keywords.For)
                {
                    if (factor < 0)
                    {
                        double originalWidth = this.borderRect.Width;
                        if (tempParentBlock.numNestableChildBlocks == 0)
                        {
                            Console.WriteLine("depth before " + tempParentBlock.depth);
                            tempParentBlock.depth += factor;
                            Console.WriteLine("depth after " + tempParentBlock.depth);
                            changeParentWidth(Math.Abs(factor) / factor, tempParentBlock);
                        }

                        if (tempParentBlock.borderRect.Width == originalWidth + 35)
                        {
                            changeParentWidthWithCustomDepth(Math.Abs(factor) / factor, tempParentBlock, 0);
                        }


                    }
                    else if (factor > 0 && tempParentBlock.numNestableChildBlocks == 0 || widthMustIncrease)
                    {
                        Console.WriteLine("depth before " + tempParentBlock.depth);
                        tempParentBlock.depth += factor;
                        Console.WriteLine("depth after " + tempParentBlock.depth);

                        changeParentWidth(Math.Abs(factor) / factor, tempParentBlock);
                        widthMustIncrease = true;
                    }
                }
                //tempChildBlock = tempParentBlock;
                tempParentBlock = tempParentBlock.parentBlock;
            }

            tempParentBlock = this.parentBlock;
            if (tempParentBlock != null && this.keyword == Keywords.For && factor > 0)
            {
                tempParentBlock.numNestableChildBlocks = tempParentBlock.numNestableChildBlocks + 1;
            }
            //Console.WriteLine(tempParentBlock.numNestableChildBlocks);

        }

        private void changeParentWidth(int factor, BlockLandingArea? tempParentBlock)
        {
            if (tempParentBlock != null)
            {
                Rectangle? tempRect = tempParentBlock.borderRect;
                Grid? tempGrid = tempParentBlock.landingAreaGrid;

                if (this.keyword == Keywords.For)
                {
                    tempGrid.Width = tempGrid.Width + 35 * factor * (depth + 1);
                    Console.WriteLine("before W " + tempRect.Width);
                    tempRect.Width = tempRect.Width + 35 * factor * (depth + 1);
                    Console.WriteLine("after W " + tempRect.Width);
                }
            }
        }

        private void changeParentWidthWithCustomDepth(int factor, BlockLandingArea? tempParentBlock, int customDepth)
        {
            if (tempParentBlock != null)
            {
                Rectangle? tempRect = tempParentBlock.borderRect;
                Grid? tempGrid = tempParentBlock.landingAreaGrid;

                if (this.keyword == Keywords.For)
                {
                    tempGrid.Width = tempGrid.Width + 35 * factor * (customDepth + 1);
                    Console.WriteLine("before W " + tempRect.Width);
                    tempRect.Width = tempRect.Width + 35 * factor * (customDepth + 1);
                    Console.WriteLine("after W " + tempRect.Width);
                }
            }
        }

        private void changeParentHeight(int factor, BlockLandingArea? tempChildBlock, BlockLandingArea? tempParentBlock)
        {
            if (tempParentBlock != null)
            {
                Rectangle? tempRect = tempParentBlock.borderRect;
                Rectangle? tempChildRect = tempChildBlock.borderRect;
                Grid? tempGrid = tempParentBlock.landingAreaGrid;

                Console.WriteLine("before H " + tempRect.Height);
                tempRect.Height = tempRect.Height + (tempChildRect.Height + 25) * factor;
                Console.WriteLine("after H " + tempRect.Height);
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
            if (_statement.GetType() == typeof(Macro))
            {
                program.removeMacro((Macro)_statement);
            } else
            {
                program.removeStatementRecursive(_statement);
            }
            StackPanel parentStackPanel = this.Parent as StackPanel;
            parentStackPanel.Children.Remove(this);
            updateDepth(-1*(depth+1));
            //changeParentWidth(-1);
            Console.WriteLine(program.generateProgramAHILCode());
        }

        private void EditStatementButton(object sender, RoutedEventArgs routedEventArgs)
        {
            if (function != null)
            {
                ParameterInputWindow parameterInputWindow = new ParameterInputWindow(function, _statement);
                parameterInputWindow.ShowDialog();
            }
            else if (keyword != null)
            {
                ParameterInputWindow parameterInputWindow = new ParameterInputWindow(keyword, _statement);
                parameterInputWindow.ShowDialog();
            }
            else
            {
                ParameterInputWindow parameterInputWindow = new ParameterInputWindow(macro, _statement);
                parameterInputWindow.ShowDialog();
            }
            Console.WriteLine(program.generateProgramAHILCode());
        }

        public void setStatement(Statement s)
        {
            this._statement = s;
        }

        public StackPanel loadBlock(StackPanel parentStackPanel)
        {
            int numBlocksPerCycle = 5;
            int colorIndex = 0;

            
            //set the block color
            foreach (Functions func in Enum.GetValues(typeof(Functions)))
            {
                if (func == this.function)
                {
                    borderRect.Fill = (Brush)(SolidColorBrush)(FindResource("BlockColor" + (colorIndex).ToString()));
                }
                colorIndex++;
            }

            foreach (Keywords keyWord in Enum.GetValues(typeof(Keywords)))
            {
                if (keyWord == this.keyword)
                {
                    borderRect.Fill = (Brush)(SolidColorBrush)(FindResource("BlockColor" + (colorIndex).ToString()));
                }
                colorIndex++;
            }
            
            foreach (MacroKeyword macro in Enum.GetValues(typeof(MacroKeyword)))
            {
                if (macro == this.macro)
                {
                    borderRect.Fill = (Brush)(SolidColorBrush)(FindResource("BlockColor" + (colorIndex / numBlocksPerCycle).ToString()));
                }
                colorIndex++;
            }

            if (this.function != null)
            {
                //Set the block label for function
                dropZoneLabel.Content = fromEnum(function);

            }
            else if (this.keyword != null) 
            {
                //Set the block label for keyword
                if (keyword.ToString() == "For")
                {
                    dropZoneLabel.Content = "Loop";
                }
                else
                {
                    dropZoneLabel.Content = keyword.ToString();
                }
            }else{
                if (macro.ToString() == "GlobalDelay")
                {
                    dropZoneLabel.Content = "Global Delay";
                }
                else
                {
                    dropZoneLabel.Content = keyword.ToString();
                }
            }

            //Label colors are always white
            dropZoneLabel.Foreground = new SolidColorBrush(Colors.White);
            borderRect.StrokeDashArray = null;
            borderRect.Stroke = Brushes.Black;

            Style style = new Style(typeof(Button));
            ControlTemplate controlTemplate = new ControlTemplate(typeof(Button));
            FrameworkElementFactory borderFactory = new FrameworkElementFactory(typeof(Border));
            borderFactory.SetValue(Border.BackgroundProperty, Brushes.Transparent);
            FrameworkElementFactory contentPresenterFactory = new FrameworkElementFactory(typeof(ContentPresenter));
            borderFactory.AppendChild(contentPresenterFactory);
            controlTemplate.VisualTree = borderFactory;
            Setter setter = new Setter(Control.TemplateProperty, controlTemplate);
            style.Setters.Add(setter);

            //Make delete button and set styling
            Button deleteButton = new Button();
            deleteButton.Background = new SolidColorBrush(Colors.Transparent);
            deleteButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
            deleteButton.Width = 34;
            deleteButton.Height = 34;
            deleteButton.VerticalAlignment = VerticalAlignment.Top;
            deleteButton.HorizontalAlignment = HorizontalAlignment.Right;
            deleteButton.Margin = new Thickness(10);

            Image deleteButtonImage = new Image();
            deleteButtonImage.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/close2.png")));
            deleteButtonImage.Width = 17;
            deleteButtonImage.Height = 17;
            deleteButton.Content = deleteButtonImage;

            deleteButton.Cursor = Cursors.Hand;
            deleteButton.Style = style;
            deleteButton.Click += new RoutedEventHandler(DeleteStatementButton);

            //Make edit button and set styling
            Button editButton = new Button();
            editButton.Background = new SolidColorBrush(Colors.Transparent);
            editButton.BorderBrush = new SolidColorBrush(Colors.Transparent);
            editButton.Width = 34;
            editButton.Height = 34;
            editButton.VerticalAlignment = VerticalAlignment.Top;
            editButton.HorizontalAlignment = HorizontalAlignment.Left;
            editButton.Margin = new Thickness(0, 10, 0, 0);
            Image editButtonImage = new Image();
            //editButtonImage.Source = new BitmapImage(new Uri("C:\\Users\\zaidl\\Documents\\School\\Year 4\\ECE 498A\\AutoHelm\\Desktop_Application\\AutoHelm\\Assets\\gear.png"));
            editButtonImage.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/gear.png")));
            //editButtonImage.Source = new BitmapImage(new Uri(@"Assets/gear.png", UriKind.Relative));
            editButtonImage.Width = 18;
            editButtonImage.Height = 18;
            editButton.Content = editButtonImage;
            editButton.Cursor = Cursors.Hand;
            editButton.Style = style;
            editButton.Click += new RoutedEventHandler(EditStatementButton);

            //Add Block Icon
            dragBlockIcon.Margin = new Thickness(0, 35, 0, 0);
            dragBlockIcon.Width = 50;
            dragBlockIcon.Height = 50;
            dropZoneLabel.Margin = new Thickness(0, 0, 0, 0);

            if (function != null)
            {
                dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + function.ToString() + ".png")));
            }
            else if (this.keyword != null) 
            {
                dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + keyword.ToString() + ".png")));
            }else
            {
                dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + macro.ToString() + ".png")));
            }

            landingAreaGrid.Children.Add(editButton);
            landingAreaGrid.Children.Add(deleteButton);

            //StackPanel parentStackPanel = this.Parent as StackPanel;

            string oldDropZoneLabel = dropZoneLabel.Content.ToString();
            
            //If the keyword is a for loop we need to enlargen the block and also add a nested landing area
            if ((this.keyword == Keywords.For) || (parentStackPanel.Name.Equals("NestedStatemetnsPanel")))
            {

                if (this.keyword == Keywords.For)
                {
                    landingAreaGrid.Width = landingAreaGrid.Width + 35;
                    borderRect.Height = borderRect.Height + 150;
                    borderRect.Width = borderRect.Width + 35;
                    //NestedStatementsPanel.Children.Add(new BlockLandingArea(this)); //typically blank, need to replace it with all children
                }
                updateDepth(1);
                //changeParentDimensions(1);
                parentStackPanel.Children.Add(this);
                return NestedStatemetnsPanel;


            }
            else
            {
                parentStackPanel.Children.Add(this);
                return parentStackPanel;
            }


        }
    }
}
