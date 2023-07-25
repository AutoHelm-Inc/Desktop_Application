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

namespace AutoHelm.UserControls.DragAndDrop
{
    /// <summary>
    /// Interaction logic for ParamInputField.xaml
    /// </summary>
    public partial class ParamInputField : UserControl
    {
        private Type _type;
        public ParamInputField((string, Type) param) {
            InitializeComponent();
            InputLabel.Content = param.Item1;
            _type = param.Item2;
        }

        public Type getType() {
            return _type;
        }
    }
}
