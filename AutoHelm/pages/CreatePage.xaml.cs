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
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using AutoHelm.UserControls.DragAndDrop;
using Automation_Project.src.ast;

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

        public CreatePage()
        {
            InitializeComponent();
            DrawDots();

            statementsAndFunctionBlocksIndex = 0;
            numBlocksPerCycle = 5;
            int colorIndex = 0;
            program = new AHILProgram();

            Style cycleElementsButtonStyle = new Style(typeof(Button));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.BackgroundProperty, (SolidColorBrush)FindResource("BlueAccent")));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.ForegroundProperty, Brushes.White));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.FontSizeProperty, 15.0));
            cycleElementsButtonStyle.Setters.Add(new Setter(Button.PaddingProperty, new Thickness(10)));
            cycleElements = new Button();
            cycleElements.Content = new TextBlock { Text = "Next", FontWeight = FontWeights.Bold, FontSize=16 };          
            cycleElements.Click += new RoutedEventHandler(CycleStatementsButtons);
            cycleElements.Style = cycleElementsButtonStyle;
            cycleElements.Cursor = Cursors.Hand;


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
        private void DrawDots()
        {
            int dotSize = 3;
            int rows = 36;
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
