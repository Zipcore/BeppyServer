namespace BeppyServer.Native {
    public abstract class SystemConsole {
        public abstract void Log(BeppyLogType level, string message);
        public abstract void Translate(BeppyLogType type, ref string message);

        public abstract string GetInput(string message);
    }
}