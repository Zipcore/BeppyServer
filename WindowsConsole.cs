#if WIN32
using System;
using System.Windows.Forms;
using System.Diagnostics;

namespace BeppyServer
{
    class WindowsConsole : SystemConsole
    {
        private WinFormConnection FormInstance;
        private RichTextBox Terminal() => (RichTextBox)FormInstance.Controls[0];
        private TextBox InputBox() => (TextBox)FormInstance.Controls[1];

        public WindowsConsole(WinFormConnection instance)
        {
            FormInstance = instance;
            Log(BeppyLogType.Info, "BeppyServer Latched!");
        }

        public override void Log(BeppyLogType level, string message)
        {
            string trace = new StackTrace().GetFrame(2).GetMethod().Name;
            FormInstance.SendLog(message, trace, level.UnityType);
        }

        public override void Translate(BeppyLogType type, ref string message)
        {
            message = $"{DateTime.Now:g} {type} {message}";
        }
    }
}

#endif