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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoHelm.Shortcuts;
using System.Windows.Threading;
using AutoHelm.user_controls;
using AutoHelm.UserControls.DragAndDrop;
using Automation_Project.src.ast;
using Automation_Project.src.automation;
using System.Threading;
using AutoHelm.UserControls.Assistant;

namespace AutoHelm.pages
{

    public partial class CreatePage : Page
    {
        private Button cycleElementsNext;
        private Button cycleElementsPrevious;
        private StackPanel buttonStackPanel;
        private List<DraggingStatementBlock> statementsAndFunctionBlocks;
        private int statementsAndFunctionBlocksIndex;
        private int numBlocksPerCycle;
        private AHILProgram program;
        private static GlobalShortcut? killWorkflowShortcut;

        public delegate void MyEventHandler(object source, EventArgs e, CreatePage p);
        public static event MyEventHandler OpenNewCreatePageEvent;

        public AHILProgram GetProgram() {
            return program;
        }

        public CreatePage(AHILProgram ahilProgram)
        {
            Console.WriteLine("createpage");
            InitializeComponent();
            //DrawDots();

            statementsAndFunctionBlocksIndex = 0;
            numBlocksPerCycle = 5;
            int colorIndex = 0;

            if (ahilProgram != null)
            {
                program = ahilProgram;
                loadProgram(program);
            }
            else
            {
                program = new AHILProgram();
            }

            Style cycleElementsButtonStyle = new Style(typeof(Button));
            buttonStackPanel = new StackPanel();
            buttonStackPanel.HorizontalAlignment = HorizontalAlignment.Center;

            cycleElementsButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, (SolidColorBrush)FindResource("BlueAccent")));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.FontSizeProperty, 15.0));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(50,10,50,10)));

            Image arrowImageNext = new Image();
            arrowImageNext.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/arrow.png")));
            arrowImageNext.Width = 24;
            arrowImageNext.Height = 24;

            cycleElementsNext = new Button();
            cycleElementsNext.Content = arrowImageNext;
            cycleElementsNext.Click += new RoutedEventHandler(CycleStatementsButtonsNext);
            cycleElementsNext.Style = cycleElementsButtonStyle;
            cycleElementsNext.Cursor = Cursors.Hand;
            cycleElementsNext.HorizontalAlignment = HorizontalAlignment.Stretch;

            Image arrowImagePrevious = new Image();
            arrowImagePrevious.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/arrow-2.png")));
            arrowImagePrevious.Width = 24;
            arrowImagePrevious.Height = 24;

            cycleElementsPrevious = new Button();
            cycleElementsPrevious.Content = arrowImagePrevious;
            cycleElementsPrevious.Click += new RoutedEventHandler(CycleStatementsButtonsPrevious);
            cycleElementsPrevious.Style = cycleElementsButtonStyle;
            cycleElementsPrevious.Cursor = Cursors.Hand;
            cycleElementsPrevious.HorizontalAlignment = HorizontalAlignment.Stretch;

            buttonStackPanel.Orientation = Orientation.Horizontal;
            buttonStackPanel.Children.Add(cycleElementsPrevious);
            buttonStackPanel.Children.Add(cycleElementsNext);

            //Add all potential blocks to list
            statementsAndFunctionBlocks = new List<DraggingStatementBlock>();
            foreach (Functions func in Enum.GetValues(typeof(Functions)))
            {
                statementsAndFunctionBlocks.Add(new DraggingStatementBlock(func, (SolidColorBrush)FindResource("BlockColor"+ (colorIndex).ToString())));
                colorIndex++;
            }

            foreach(Keywords keyWord in Enum.GetValues(typeof(Keywords)))
            {
                statementsAndFunctionBlocks.Add(new DraggingStatementBlock(keyWord, (SolidColorBrush)FindResource("BlockColor" + (colorIndex).ToString())));
                colorIndex++;
            }

            updateBlocks(3);
            LandingAreaPanel.Children.Add(new BlockLandingArea(program));

            //remove killing workflow shortcut tied to any previously opened workflows
            if(killWorkflowShortcut != null)
            {
                ShortcutManager.removeShortcut(killWorkflowShortcut);
            }

            //set up kill running workflow keyboard shortcut for currently open workflow
            killWorkflowShortcut = new GlobalShortcut(ModifierKeys.Control, Key.CapsLock, killWorkflow);
            ShortcutManager.addShortcut(killWorkflowShortcut);
            
        }

        public void killWorkflow()
        {
            Console.WriteLine("Kill key pressed!");
            program.killRunningProgram();
        }

        private void assistantButtonClick(object sender, RoutedEventArgs e)
        {
            AssistantWindow window = new AssistantWindow();
            window.ShowDialog();

            // window.text is empty when user closes the assistant popup instead of submitting it, so return
            if (window.text == String.Empty) return;

            //todo: send window.text as prompt for ai assistant
            AHILProgram newProgram = new AHILProgram();
            CreatePage page = new CreatePage(newProgram);
            OpenNewCreatePage(sender, e, page);
        }

        private void OpenNewCreatePage(object sender, RoutedEventArgs e, CreatePage p)
        {
            OpenNewCreatePageEvent(this, null, p);
        }

        private void runButtonClick(object sender, RoutedEventArgs e) {
            program.saveToFile();

            String p = Directory.GetParent(System.Environment.CurrentDirectory)?.Parent?.Parent?.FullName;
            System.Windows.Forms.NotifyIcon ni = new System.Windows.Forms.NotifyIcon();
            int bIndex1 = 0;
            int bIndex2 = 0;


            if (p != null)
            {
                //Load Tray Icon Before Execution
                ni.Icon = new System.Drawing.Icon(p + "/pages/MainWindow/autohelm_logo.ico");
                ni.Visible = true;
                ni.Text = "Running AutoHelm Workflow...";

                //Apply Borders to TopBar and CreatePage Before Execution
                Border b1 = new Border();
                b1.BorderBrush = Brushes.Red;
                b1.BorderThickness = new Thickness(5, 0, 5, 5);

                Border b2 = new Border();
                b2.BorderBrush = Brushes.Red;
                b2.BorderThickness = new Thickness(5, 5, 5, 0);

                bIndex1 = ((Grid)this.Content).Children.Add(b1);
                bIndex2 = ((Grid)TopBar.self.Content).Children.Add(b2);

                //Force a Render Update
                DispatcherFrame frame = new DispatcherFrame();
                Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Render, new DispatcherOperationCallback(delegate (object parameter)
                {
                    frame.Continue = false;
                    return null;
                }), null);

                Dispatcher.PushFrame(frame);
                Application.Current.Dispatcher.Invoke(DispatcherPriority.Background,
                                              new Action(delegate { }));

            }

            program.execute();

            //Remove Borders After Execution
            ((Grid)this.Content).Children.RemoveAt(bIndex1);
            ((Grid)TopBar.self.Content).Children.RemoveAt(bIndex2);

            //Remove Tray Icon After Execution
            ni.Visible = false;
        }

        private void CycleStatementsButtonsNext(object sender, RoutedEventArgs routedEventArgs)
        {
            updateBlocks(1);
        }
        private void CycleStatementsButtonsPrevious(object sender, RoutedEventArgs routedEventArgs)
        {
            updateBlocks(2);
        }

        private void updateBlocks(int incrementIndex)
        {
            if (incrementIndex == 1)
            {
                statementsAndFunctionBlocksIndex += numBlocksPerCycle;
            }
            else if (incrementIndex == 2)
            {
                statementsAndFunctionBlocksIndex -= numBlocksPerCycle;
            }

            if (statementsAndFunctionBlocksIndex >= statementsAndFunctionBlocks.Count - 1)
            {
                statementsAndFunctionBlocksIndex = 0;
            }else if (statementsAndFunctionBlocksIndex < 0)
            {
                statementsAndFunctionBlocksIndex = statementsAndFunctionBlocks.Count - 2;
            }

            StatementBlocksStackPanel.Children.Clear();
            StatementBlocksStackPanel.Children.Add(buttonStackPanel);

            for (int i = statementsAndFunctionBlocksIndex; i < statementsAndFunctionBlocksIndex + numBlocksPerCycle; i++)
            {
                if (i < statementsAndFunctionBlocks.Count)
                {
                    ScaleTransform scaleTransform = new ScaleTransform(1, 1);
                    statementsAndFunctionBlocks[i].RenderTransformOrigin = new Point(0.5, 0.5);
                    statementsAndFunctionBlocks[i].RenderTransform = scaleTransform;

                    statementsAndFunctionBlocks[i].MouseEnter += (sender, e) =>
                    {
                        DoubleAnimation animation = new DoubleAnimation(1.1, TimeSpan.FromSeconds(0.1));
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };

                    statementsAndFunctionBlocks[i].MouseLeave += (sender, e) =>
                    {
                        DoubleAnimation animation = new DoubleAnimation(1, TimeSpan.FromSeconds(0.2));
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animation);
                        scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animation);
                    };

                    StatementBlocksStackPanel.Children.Add(statementsAndFunctionBlocks[i]);
                }
            }
        }

        private void loadProgram(AHILProgram ahilProgram)
        {
            //First we create the empty landing area assocaited with the ahilProgram
            BlockLandingArea blaProgram = new BlockLandingArea(ahilProgram);

            //Next we iterate through each statement
            //By default we put "LandingAreaPanel" because we want it to be added to the panel related to the CreatePage
            foreach (Statement s in ahilProgram.getStatements())
            {
                if (s is SimpleStatement)
                {
                    loadSimpleStatement((SimpleStatement)s, LandingAreaPanel, null);
                }
                else if (s is NestedStructure)
                {
                    loadNestedStruct((NestedStructure)s, LandingAreaPanel, null);
                }
            }
        }

        //Note that in these function, stack panel can change due to nesting. For instance For(5){ Run "Notepad.exe"} the
        //StackPanel would be the NestedStackPanel of the For Block
        private void loadSimpleStatement(SimpleStatement s, StackPanel stackPanel, BlockLandingArea? parent)
        {
            //For simple statements, we simply create a new block landing area with no parent or keyword since they are only functions
            SimpleStatement ss = (SimpleStatement)s;
            BlockLandingArea bla = new BlockLandingArea(ss.getFunction(), null, parent);
            //Set the arguments
            bla.setStatement(ss);
            //Then physically render the block
            bla.loadBlock(stackPanel);
            bla.AllowDrop = false;
        }

        private void loadNestedStruct(NestedStructure ns, StackPanel stackPanel, BlockLandingArea? parent)
        {
            if (ns is ForLoop)
            {
                ForLoop fl = (ForLoop)ns;
                BlockLandingArea bla = new BlockLandingArea(null, Keywords.For, parent);
                bla.setStatement(fl);
                StackPanel newPanel = bla.loadBlock(stackPanel);

                foreach (Statement s in fl.getStatements())
                {
                    if (s is SimpleStatement)
                    {
                        loadSimpleStatement((SimpleStatement)s, newPanel, bla);
                    }
                    else if (s is NestedStructure)
                    {
                        loadNestedStruct((NestedStructure)s, newPanel, bla);
                    }

                }

                newPanel.Children.Add(new BlockLandingArea(bla));
                bla.AllowDrop = false;


            }
        }
        
        private void DrawDots()
        {
            int dotSize = 3;
            int rows = 1000;
            int columns = 65;
            int spacing = 32;

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    var pixel = new Rectangle();
                    pixel.Fill = Brushes.Black;
                    pixel.Width = dotSize;
                    pixel.Height = dotSize;

                    blankCanvas.Children.Add(pixel);
                    Canvas.SetLeft(pixel, j * (dotSize + spacing));
                    Canvas.SetTop(pixel, i * (dotSize + spacing));
                }
            }
        }
    }
}
