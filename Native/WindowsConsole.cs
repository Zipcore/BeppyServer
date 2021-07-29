#if WIN32
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace BeppyServer.Native
{
    internal class WindowsConsole : SystemConsole
    {
        private readonly WinFormConnection formInstance;
        private RichTextBox Terminal() => (RichTextBox)formInstance.Controls[0];
        private TextBox InputBox() => (TextBox)formInstance.Controls[1];

        private readonly ManualResetEvent inputCompleted;
        private string input;

        public WindowsConsole(WinFormConnection instance)
        {
            formInstance = instance;
            inputCompleted = new ManualResetEvent(false);
            Log(BeppyLogType.Info, "BeppyServer Latched!");
        }

        private void WindowsConsole_KeyUp(object sender, KeyEventArgs e) {
            
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
                inputCompleted.Set();
            else if (char.IsLetter((char)e.KeyCode))
                input += (char) e.KeyCode;
        }

        public override void Log(BeppyLogType level, string message)
        {
            string trace = new StackTrace().GetFrame(2).GetMethod().Name;
            formInstance.SendLog(message, trace, level.UnityType);
        }

        public override void Translate(BeppyLogType type, ref string message)
        {
            message = $"{DateTime.Now:g} {type} {message}";
        }

        public override string GetInput(string message) {
            input = "";
            InputBox().KeyUp += WindowsConsole_KeyUp;

            // Save previous state
            bool wasEnabled = InputBox().Enabled;
            bool wasReadOnly = InputBox().ReadOnly;

            // Force enabled input box.
            InputBox().Enabled = true;
            InputBox().ReadOnly = false;

            Log(BeppyLogType.Info, message);
            
            // Await input
            inputCompleted.WaitOne();
            
            // Reset
            inputCompleted.Reset();
            InputBox().KeyUp -= WindowsConsole_KeyUp;
            InputBox().Enabled = wasEnabled;
            InputBox().ReadOnly = wasReadOnly;
            return input;
        }
    }
}

#endif