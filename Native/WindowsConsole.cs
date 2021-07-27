#if WIN32
using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading.Tasks;

namespace BeppyServer.Native
{
    internal class WindowsConsole : SystemConsole
    {
        private WinFormConnection FormInstance;
        private RichTextBox Terminal() => (RichTextBox)FormInstance.Controls[0];
        private TextBox InputBox() => (TextBox)FormInstance.Controls[1];

        private TaskCompletionSource<bool> InputCompleted;
        private string input;

        public WindowsConsole(WinFormConnection instance)
        {
            FormInstance = instance;
            Log(BeppyLogType.Info, "BeppyServer Latched!");
        }

        private void WindowsConsole_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Return)
            {
                input = InputBox().Text;
                InputCompleted.SetResult(true);
            }
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

        public override string GetInput(string message)
        {
            InputBox().KeyUp += WindowsConsole_KeyUp;

            // Save previous state
            var wasEnabled = InputBox().Enabled;
            var wasReadOnly = InputBox().ReadOnly;

            // Force enabled input box.
            InputCompleted = new TaskCompletionSource<bool>();
            InputBox().Enabled = true;
            InputBox().ReadOnly = false;

            // Await our new event
            InputCompleted.Task.Wait();

            // Record and Reset
            string recordedInput = input;
            InputBox().Enabled = wasEnabled;
            InputBox().ReadOnly = wasReadOnly;
            InputBox().KeyUp -= WindowsConsole_KeyUp;
            input = "";

            return recordedInput;
        }
    }
}

#endif