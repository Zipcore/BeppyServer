using System;
using System.Diagnostics;
using System.Text;
using System.Windows.Forms;

namespace BeppyServer
{
    abstract class SystemConsole
    {
        public abstract void Log(BeppyLogType level, string message);
        public abstract void Translate(BeppyLogType type, ref string message);
    }

    class UnixConsole : SystemConsole
    {
        // We don't know what the object is yet.
        private static GUIWindowConsole console;
        public override void Log(BeppyLogType level, string message)
        {

            console.AddLine(message);
        }

        public override void Translate(BeppyLogType type, ref string message)
        {
            throw new NotImplementedException();
        }
    }

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
