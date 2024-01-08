﻿using System;
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
using System.Windows.Threading;
using AutoHelm.user_controls;
using AutoHelm.UserControls.DragAndDrop;
using Automation_Project.src.ast;
using Automation_Project.src.automation;
using Emgu.CV.Structure;
using Emgu.CV;
using Tesseract;
using Ocr;

namespace AutoHelm.pages
{

    public partial class CreatePage : System.Windows.Controls.Page
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
            Console.WriteLine("createpage");
            InitializeComponent();
            DrawDots();

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

            /*
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap("C:\\Users\\AyaanAnishaFamily\\Pictures\\ford.PNG");
            System.Drawing.Rectangle rect = new System.Drawing.Rectangle(0, 0, bitmap.Width, bitmap.Height);
            System.Drawing.Imaging.BitmapData data = bitmap.LockBits(rect, System.Drawing.Imaging.ImageLockMode.ReadWrite, bitmap.PixelFormat);
            Image<Bgr, byte> outputImage = new Image<Bgr, byte>(bitmap.Width, bitmap.Height, data.Stride, data.Scan0);
            Image<Gray, Byte> grayImage = outputImage.Convert<Gray, Byte>();

            grayImage = grayImage.Resize(outputImage.Width *2 , outputImage.Height * 2, Emgu.CV.CvEnum.Inter.Linear);

            grayImage = grayImage.SmoothBilateral(9, 75, 75);

            grayImage = grayImage.ThresholdAdaptive(new Gray(255), Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.Binary, 31, new Gray(0.03));

            grayImage.Save("C:\\Users\\AyaanAnishaFamily\\Pictures\\Grey.PNG");
            */

            //Image<Bgr, byte> image = new Image<Bgr, byte>("C:\\Users\\AyaanAnishaFamily\\Pictures\\ahmedin4k.PNG");
            //Image<Gray, Byte> grayImage = image.Convert<Gray, Byte>();

            //grayImage = grayImage.Resize(grayImage.Width * 2, grayImage.Height * 2, Emgu.CV.CvEnum.Inter.Linear);

            //grayImage = grayImage.SmoothBilateral(9, 75, 75);

            //grayImage = grayImage.ThresholdAdaptive(new Gray(255), Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.Binary, 21, 4);

            //grayImage.Save("C:\\Users\\AyaanAnishaFamily\\Pictures\\Grey.PNG");

            //Image<Bgr, byte> image = new Image<Bgr, byte>("C:\\Users\\AyaanAnishaFamily\\Pictures\\Test1.PNG");
            //Image<Gray, Byte> grayImage = image.Convert<Gray, Byte>();

            //grayImage = grayImage.Resize(grayImage.Width * 3, grayImage.Height * 3, Emgu.CV.CvEnum.Inter.Cubic);

            //grayImage = grayImage.SmoothGaussian(1, 1, 0,0 );

            //CvInvoke.AdaptiveThreshold(grayImage, grayImage, 255, Emgu.CV.CvEnum.AdaptiveThresholdType.GaussianC, Emgu.CV.CvEnum.ThresholdType.BinaryInv, 31, 0.03);


            //grayImage.Save("C:\\Users\\AyaanAnishaFamily\\Pictures\\Grey.PNG");

            //var img = Pix.LoadFromFile("C:\\Users\\AyaanAnishaFamily\\Pictures\\Grey.PNG"); 
            //var ocr = new TesseractEngine("C:\\Users\\AyaanAnishaFamily\\Documents\\University\\FYDP\\Desktop_Application\\AutoHelm\\bin\\Debug\\net6.0-windows\\ocrResources", "eng", EngineMode.Default); 
            //var page = ocr.Process(img);
            //string text = page.GetText();
            //Console.WriteLine(page.GetText());
            OcrController ocr = new OcrController("C:\\Users\\AyaanAnishaFamily\\Pictures\\Test1.PNG", true);
            ocr.performOcr();

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
