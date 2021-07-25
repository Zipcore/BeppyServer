namespace BeppyServer
{
    abstract class SystemConsole
    {
        public abstract void Log(BeppyLogType level, string message);
        public abstract void Translate(BeppyLogType type, ref string message);
    }
}
