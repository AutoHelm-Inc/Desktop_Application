using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoHelm.Shortcuts
{
    //Much of this class was built with help from the following tutorial: https://www.youtube.com/watch?v=qLxqoh1JLnM 
    public class GlobalShortcut
    {
        public ModifierKeys modifier {get;set;}
        public Key key { get;set;}
        public Action? callback { get;set;}
        public bool canExecute { get; set; }

        public GlobalShortcut(ModifierKeys modifier, Key key, Action callback, bool canExecute=true) { 
            this.modifier = modifier;
            this.key = key;
            this.callback = callback;
            this.canExecute = canExecute;
        }

    }
}
