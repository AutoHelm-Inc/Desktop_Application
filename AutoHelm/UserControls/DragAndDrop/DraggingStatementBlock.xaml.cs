using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
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
using System.Xml.Linq;
using Automation_Project.src.ast;

namespace AutoHelm.UserControls.DragAndDrop
{
    public partial class DraggingStatementBlock : UserControl
    {
        private DraggingBlockAdorner blockAdornment;
        private XandYCoordinates draggingPoint = new XandYCoordinates();
        [DllImport("user32.dll")]
        static extern void GetCursorPos(ref XandYCoordinates p);
        private BlockDataToTransfer blockData;
        private Functions? function;
        private Keywords? keyword;
        private MacroKeyword? macro;

        public DraggingStatementBlock()
        {
            InitializeComponent();
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

        public DraggingStatementBlock(Functions? function, SolidColorBrush background)
        {
            InitializeComponent();
            String name = fromEnum(function);
          
            dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + function.ToString() + ".png")));
            dragBlockLabel.Content = name;
            borderRect.Fill = background;
            this.function = function;
            this.keyword = null;
            this.macro = null;
        }

        public DraggingStatementBlock(Keywords? keyword, SolidColorBrush background)
        {
            InitializeComponent();
            String name = keyword.ToString();

            if (name == "For")
            {
                name = "Loop";
            }

            dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + keyword.ToString() + ".png")));
            dragBlockLabel.Content = name;
            borderRect.Fill = background;
            this.keyword = keyword;
            this.function = null;
            this.macro = null;
        }

        public DraggingStatementBlock(MacroKeyword? macro, SolidColorBrush background)
        {
            InitializeComponent();
            String name = macro.ToString();

            if (name == "GlobalDelay")
            {
                name = "Global Delay";
            }

            dragBlockIcon.Source = new BitmapImage(new Uri(System.IO.Path.GetFullPath("../../../Assets/BlockIcons/" + macro.ToString() + ".png")));
            dragBlockLabel.Content = name;
            borderRect.Fill = background;
            this.macro = macro;

            this.function = null;
            this.keyword = null;
        }

        private struct XandYCoordinates
        {
            public int XVal;
            public int YVal;
            public Point GetPoint(Point pointOffset)
            {
                return new Point(XVal + pointOffset.X, YVal + pointOffset.Y);
            }
        }

        public struct BlockDataToTransfer
        {
            public SolidColorBrush backgroundColor;
            public SolidColorBrush labelColor;
            public Functions? function;
            public Keywords? keyword;
            public MacroKeyword? macro;

            public BlockDataToTransfer(Rectangle borderRect, Functions? function, SolidColorBrush labelColor)
            {
                this.backgroundColor = (SolidColorBrush)borderRect.Fill;
                this.labelColor = labelColor;
                this.function = function;
                this.keyword = null;
                this.macro = null;
            }

            public BlockDataToTransfer(Rectangle borderRect, Keywords? keyword, SolidColorBrush labelColor)
            {
                this.backgroundColor = (SolidColorBrush)borderRect.Fill;
                this.labelColor = labelColor;
                this.keyword = keyword;
                this.function = null;
                this.macro = null;
            }

            public BlockDataToTransfer(Rectangle borderRect, MacroKeyword? macro, SolidColorBrush labelColor)
            {
                this.backgroundColor = (SolidColorBrush)borderRect.Fill;
                this.labelColor = labelColor;

                this.macro = macro;
                this.function = null;
                this.keyword = null;
            }
        }

        private class DraggingBlockAdorner : Adorner
        {
            Rect rectangleWhileDragging;
            Brush color;
            public Point centerPoint;
            public DraggingBlockAdorner(DraggingStatementBlock block, SolidColorBrush color) : base(block)
            {
                rectangleWhileDragging = new Rect(block.RenderSize);
                this.color = color;
                this.IsHitTestVisible = false;
                centerPoint = new Point(-rectangleWhileDragging.Width * 0.5, -rectangleWhileDragging.Height * 0.5);
            }
            protected override void OnRender(DrawingContext drawingContext)
            {
                drawingContext.DrawRectangle(color, null, rectangleWhileDragging);
            }
        }

        private void AdornmentApplicationWithMovingRect(object sender, GiveFeedbackEventArgs feedbackEventArgument)
        {
            GetCursorPos(ref draggingPoint);
            Point rectanglePosition = this.PointFromScreen(draggingPoint.GetPoint(blockAdornment.centerPoint));
            blockAdornment.Arrange(new Rect(rectanglePosition, blockAdornment.DesiredSize));
        }

        private void MouseMovementTracking(object sender, MouseEventArgs mouseEventArgument)
        {
            //Check if user is currently clicking on the box
            if (mouseEventArgument.LeftButton == MouseButtonState.Pressed)
            {
                if(function != null)
                {
                    this.blockData = new BlockDataToTransfer(borderRect, function, (SolidColorBrush)dragBlockLabel.Foreground);
                }
                else if (keyword != null)
                {
                    this.blockData = new BlockDataToTransfer(borderRect, keyword, (SolidColorBrush)dragBlockLabel.Foreground);
                }
                else
                {
                    this.blockData = new BlockDataToTransfer(borderRect, macro, (SolidColorBrush)dragBlockLabel.Foreground);
                }
                var blockDataObject = new DataObject("DRAG_BLOCK_DATA", blockData);
                blockAdornment = new DraggingBlockAdorner(this, (SolidColorBrush)borderRect.Fill);

                var adornerLayer = AdornerLayer.GetAdornerLayer(this);
                adornerLayer.Add(blockAdornment);
                DragDrop.DoDragDrop(this, blockDataObject, DragDropEffects.Move);
                adornerLayer.Remove(blockAdornment);

            }
        }

        public void setBlockData(BlockDataToTransfer blockData)
        {
            this.blockData = blockData;
        }

        public BlockDataToTransfer getBlockData()
        {
            return this.blockData;
        }

    }
}
