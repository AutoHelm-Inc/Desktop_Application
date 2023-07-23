using Automation_Project.src.ast;
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

namespace AutoHelm.UserControls.DragAndDrop
{

    public partial class ParameterInputWindow : Window
    {
        private Functions? function;
        private Keywords? keyword;

        public ParameterInputWindow(Functions? blockFunction)
        {
            InitializeComponent();
            this.function = blockFunction;
            List<string> paramsList = getParamListForFunc(blockFunction);
            
            foreach (string param in paramsList)
            {
                InputParamsPanel.Children.Add(new ParamInputField(param));
            }
            paramWindowTitle.Content = function.ToString();
        }

        public ParameterInputWindow(Keywords? blockKeyword)
        {
            InitializeComponent();
            this.keyword = blockKeyword;
            List<string> paramsList = getParamListForFunc(blockKeyword);
            
            foreach (string param in paramsList)
            {
                InputParamsPanel.Children.Add(new ParamInputField(param));
            }
            paramWindowTitle.Content = keyword.ToString();
        }

        private void saveButtonCLick(object sender, RoutedEventArgs routedEventArgs)
        {
            
        }

        public List<string> getParamListForFunc(dynamic? funcOrKeyword)
        {
            List<string> paramsList = new List<string>();
            if (funcOrKeyword is Functions)
            {
                if ((Functions)funcOrKeyword == (Functions.Run))
                {
                    paramsList.Add("Program/File");
                }
                else if ((Functions)funcOrKeyword == Functions.SwitchWindow)
                {
                    paramsList.Add("Program");
                }
                else if ((Functions)funcOrKeyword == Functions.Close)
                {
                    paramsList.Add("Program");
                }
                else if ((Functions)funcOrKeyword == Functions.Create)
                {
                    paramsList.Add("File Path");
                }
                else if ((Functions)funcOrKeyword == Functions.Save)
                {
                    paramsList.Add("File");
                }
                else if ((Functions)funcOrKeyword == Functions.Move)
                {
                    paramsList.Add("Old path");
                    paramsList.Add("New path");
                }
                else if ((Functions)funcOrKeyword == Functions.Del)
                {
                    paramsList.Add("File Path");
                }
                else if ((Functions)funcOrKeyword == Functions.WrtLine)
                {
                    paramsList.Add("Line");
                }
                else if ((Functions)funcOrKeyword == Functions.Write)
                {
                    paramsList.Add("Words");
                }
                else if ((Functions)funcOrKeyword == Functions.PressKey)
                {
                    paramsList.Add("Key");
                }
                else if ((Functions)funcOrKeyword == Functions.EmailsGet)
                {
                    paramsList.Add("Email Address");
                }
                else if ((Functions)funcOrKeyword == Functions.FilesGet)
                {
                    paramsList.Add("Folder Path");
                }
                else if ((Functions)funcOrKeyword == Functions.Click)
                {
                    paramsList.Add("Button");
                }
            }
            else
            {
                if ((Keywords)funcOrKeyword == Keywords.If)
                {
                    paramsList.Add("Condition");
                }
                else if ((Keywords)funcOrKeyword == Keywords.Elif)
                {
                    paramsList.Add("Condition");
                }
                else if ((Keywords)funcOrKeyword == Keywords.For)
                {
                    paramsList.Add("Iterations");
                }
            }
            
            return paramsList;
        }
    }
}
