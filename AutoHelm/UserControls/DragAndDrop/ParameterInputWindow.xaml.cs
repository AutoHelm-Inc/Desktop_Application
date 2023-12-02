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
        private MacroKeyword? macro;
        private Statement _statement;

        public ParameterInputWindow(Functions? blockFunction, Statement statement)
        {
            InitializeComponent();
            this.function = blockFunction;
            _statement = statement;
            List<(string, Type)> paramsList = getParamListForFunc(blockFunction);
            List<dynamic> functionArgs = ((SimpleStatement)statement).getArguments();

            for (int i = 0; i < paramsList.Count; i++) {
                (string, Type) param = paramsList[i];
                string initValue = "";
                if (i < functionArgs.Count) {
                    initValue = functionArgs[i].ToString();
                }
                InputParamsPanel.Children.Add(new ParamInputField(param, init: initValue));
            }
            paramWindowTitle.Content = function.ToString();
        }

        public ParameterInputWindow(Keywords? blockKeyword, Statement statement)
        {
            InitializeComponent();
            this.keyword = blockKeyword;
            _statement = statement;
            List<(string, Type)> paramsList = getParamListForFunc(blockKeyword);
            string initValue = "";
            if (blockKeyword == Keywords.For) {
                initValue = ((ForLoop)statement).getRepititionCount().ToString();
            }
            foreach ((string, Type) param in paramsList)
            {
                InputParamsPanel.Children.Add(new ParamInputField(param, init: initValue));
            }
            paramWindowTitle.Content = keyword.ToString();
        }

        public ParameterInputWindow(MacroKeyword? blockKeyword, Statement statement)
        {
            InitializeComponent();
            this.macro = blockKeyword;
            _statement = statement;
            List<(string, Type)> paramsList = getParamListForFunc(blockKeyword);
            List<dynamic> macroArgs = ((Macro)statement).getArguments();

            for (int i = 0; i < paramsList.Count; i++)
            {
                (string, Type) param = paramsList[i];
                string initValue = "";
                if (i < macroArgs.Count)
                {
                    initValue = macroArgs[i].ToString();
                }
                InputParamsPanel.Children.Add(new ParamInputField(param, init: initValue));
            }
            paramWindowTitle.Content = macro.ToString();
        }

        private void saveButtonClick(object sender, RoutedEventArgs routedEventArgs)
        {
            if (_statement.GetType() == typeof(SimpleStatement)) {
                ((SimpleStatement)_statement).setArguments(new List<dynamic>());
            } else if (_statement.GetType() == typeof(Macro)) {
                ((Macro)_statement).setArguments(new List<dynamic>());
            }

            foreach (ParamInputField child in InputParamsPanel.Children) {
                if (child.InputField.Text == "") {
                    continue;
                }
                if (child.getType() == typeof(int)) {
                    int parsedValue = int.Parse(child.InputField.Text);
                    if (_statement.GetType() == typeof(SimpleStatement)) {
                        ((SimpleStatement)_statement).addArgument(parsedValue);
                    }
                    else if (keyword == Keywords.For) {
                        ((ForLoop)_statement).setRepititionCount(parsedValue);
                    } else if (_statement.GetType() == typeof(Macro))
                    {
                        ((Macro)_statement).addArgument(parsedValue);
                    }
                } else {
                    ((SimpleStatement)_statement).addArgument(child.InputField.Text);
                }
            }
            this.Close();
        }

        public List<(string, Type)> getParamListForFunc(dynamic? funcOrKeyword)
        {
            List<(string, Type)> paramsList = new List<(string, Type)>();
            if (funcOrKeyword is Functions)
            {
                if ((Functions)funcOrKeyword == (Functions.Run))
                {
                    paramsList.Add(("Program/File", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.SwitchWindow)
                {
                    paramsList.Add(("Program", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.Close)
                {
                    paramsList.Add(("Program", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.FileCreate)
                {
                    paramsList.Add(("File Path", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.DirCreate) {
                    paramsList.Add(("Directory Path", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.Save)
                {
                    paramsList.Add(("File", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.Move)
                {
                    paramsList.Add(("Old Path", typeof(string)));
                    paramsList.Add(("New Path", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.Del)
                {
                    paramsList.Add(("File Path", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.WriteLine)
                {
                    paramsList.Add(("Line", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.Write)
                {
                    paramsList.Add(("Words", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.PressKey)
                {
                    paramsList.Add(("Keys", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.EmailsGet)
                {
                    paramsList.Add(("Email Address", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.FilesGet)
                {
                    paramsList.Add(("Folder Path", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.MouseMove) {
                    paramsList.Add(("X", typeof(int)));
                    paramsList.Add(("Y", typeof(int)));
                }
                else if ((Functions)funcOrKeyword == Functions.Click)
                {
                    paramsList.Add(("X", typeof(int)));
                    paramsList.Add(("Y", typeof(int)));
                    paramsList.Add(("Button", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.SaveAs) {
                    paramsList.Add(("File Path", typeof(string)));
                }
                else if ((Functions)funcOrKeyword == Functions.Sleep) {
                    paramsList.Add(("Sleep for MS", typeof(int)));
                }

            }
            else if (funcOrKeyword is Keywords)
            {
                if ((Keywords)funcOrKeyword == Keywords.If)
                {
                    paramsList.Add(("Condition", typeof(string)));
                }
                else if ((Keywords)funcOrKeyword == Keywords.Elif)
                {
                    paramsList.Add(("Condition", typeof(string)));
                }
                else if ((Keywords)funcOrKeyword == Keywords.For)
                {
                    paramsList.Add(("Iterations", typeof(int)));
                }
            }
            else if (funcOrKeyword is MacroKeyword)
            {
                if ((MacroKeyword)funcOrKeyword == MacroKeyword.GlobalDelay)
                {
                    paramsList.Add(("Global Delay", typeof(int)));
                }
            }
            
            return paramsList;
        }
    }
}
