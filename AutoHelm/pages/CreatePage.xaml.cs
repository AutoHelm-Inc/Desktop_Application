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
using AutoHelm.UserControls.DragAndDrop;
using Automation_Project.src.ast;
using Automation_Project.src.automation;

namespace AutoHelm.pages
{

    public partial class CreatePage : Page
    {
        private Button cycleElements;
        private List<DraggingStatementBlock> statementsAndFunctionBlocks;
        private int statementsAndFunctionBlocksIndex;
        private int numBlocksPerCycle;
        private AHILProgram program;

        public AHILProgram GetProgram() {
            return program;
        }

        public CreatePage(AHILProgram ahilProgram)
        {
            InitializeComponent();

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
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, (SolidColorBrush)FindResource("BlueAccent")));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.FontSizeProperty, 15.0));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(10)));
            cycleElements = new Button();
            cycleElements.Content = new TextBlock { Text = "Next", FontWeight = FontWeights.Bold, FontSize=16 };          
            cycleElements.Click += new RoutedEventHandler(CycleStatementsButtons);
            cycleElements.Style = cycleElementsButtonStyle;


            //Add all potential blocks to list
            statementsAndFunctionBlocks = new List<DraggingStatementBlock>();
            foreach (Functions func in Enum.GetValues(typeof(Functions)))
            {
                statementsAndFunctionBlocks.Add(new DraggingStatementBlock(func, (SolidColorBrush)FindResource("BlockColor"+ (colorIndex / numBlocksPerCycle).ToString())));
                colorIndex++;
            }

            foreach(Keywords keyWord in Enum.GetValues(typeof(Keywords)))
            {
                statementsAndFunctionBlocks.Add(new DraggingStatementBlock(keyWord, (SolidColorBrush)FindResource("BlockColor" + (colorIndex / numBlocksPerCycle).ToString())));
                colorIndex++;
            }

            updateBlocks(false);
            LandingAreaPanel.Children.Add(new BlockLandingArea(program));
        }

        private void runButtonClick(object sender, RoutedEventArgs e) {
            program.saveToFile();
            program.execute();
        }

        private void CycleStatementsButtons(object sender, RoutedEventArgs routedEventArgs)
        {
            updateBlocks(true);
        }

        private void updateBlocks(bool incrementIndex)
        {
            if (incrementIndex)
            {
                statementsAndFunctionBlocksIndex += numBlocksPerCycle;
            }

            if (statementsAndFunctionBlocksIndex >= statementsAndFunctionBlocks.Count - 1)
            {
                statementsAndFunctionBlocksIndex = 0;
            }

            StatementBlocksStackPanel.Children.Clear();
            StatementBlocksStackPanel.Children.Add(cycleElements);

            for (int i = statementsAndFunctionBlocksIndex; i < statementsAndFunctionBlocksIndex + numBlocksPerCycle; i++)
            {
                if (i < statementsAndFunctionBlocks.Count)
                {
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

    }
}
