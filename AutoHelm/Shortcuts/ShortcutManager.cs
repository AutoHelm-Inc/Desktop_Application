using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace AutoHelm.Shortcuts
{
    //Much of this class was built with help from the following tutorial: https://www.youtube.com/watch?v=qLxqoh1JLnM 
    public static class ShortcutManager
    {
        private delegate IntPtr LowLevelKeyboardProc(int ncode, IntPtr wParam, IntPtr lParam);
        private static List<GlobalShortcut> shortcuts { get; set; }
        private const int LOW_LEVEL_WINDOWS_HOOK_ID = 13; //internal value used by Windows to detect hook type
        private static IntPtr HookID = IntPtr.Zero;
        public static bool IsHookSetup { get; set; }
        private static LowLevelKeyboardProc LowLevelProc = HookCallBack;
        

        static ShortcutManager()
        {
            shortcuts = new List<GlobalShortcut>();
        }

        private static IntPtr HookCallBack(int nCode, IntPtr wParam, IntPtr lParam)
        {
            //hot keys are scanned here
            if(nCode >= 0)
            {
                //Check the shortcuts
                foreach(GlobalShortcut shortcut in shortcuts)
                {
                    if (Keyboard.Modifiers == shortcut.modifier && Keyboard.IsKeyDown(shortcut.key) && shortcut.canExecute)
                    {
                        shortcut.callback?.Invoke();
                    }
                }
            }

            return CallNextHookEx(HookID, nCode, wParam, lParam);
        }

        public static void systemHookSetup()
        {
            if(!IsHookSetup)
            {
                HookID = SetHook(LowLevelProc);
                IsHookSetup = true;
            }
        }

        public static void turnOffSystemHook()
        {
            if(IsHookSetup)
            {
                RemoveWindowsHookEx(HookID);
                IsHookSetup=false;
            }
        }

        public static void addShortcut(GlobalShortcut shortcut)
        {
            shortcuts.Add(shortcut);
        }

        public static void removeShortcut(GlobalShortcut shortcut)
        {
            shortcuts.Remove(shortcut);
        }

        private static IntPtr SetHook(LowLevelKeyboardProc LLproc)
        {
            using(Process currentProcess = Process.GetCurrentProcess())
            {
                using(ProcessModule? currentModule = currentProcess.MainModule)
                {
                    return SetWindowsHookEx(LOW_LEVEL_WINDOWS_HOOK_ID, LLproc, GetModuleHandle(currentModule?.ModuleName), 0);
                }
            }
        }

        //DLL Imports to make this work
        [DllImport ("user32.dll", CharSet=CharSet.Auto, SetLastError =true)]
        private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc llkp, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr RemoveWindowsHookEx(IntPtr hook);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hook, int nCode, IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string? lpModuleName);

    }
}
