using BepInEx.Logging;
using UnityEngine;

namespace BeppyServer
{
    public class BeppyLogType
    {
        private LogType unityType;
        private LogLevel bepLevel;

        public static BeppyLogType Info = new BeppyLogType(LogLevel.Info);
        public static BeppyLogType Warning = new BeppyLogType(LogLevel.Warning);
        public static BeppyLogType Error = new BeppyLogType(LogLevel.Error);
        public static BeppyLogType Debug = new BeppyLogType(LogLevel.Debug);

        public LogType UnityType
        {
            get { return unityType; }
            private set { this.unityType = value; }
        }

        public LogLevel BepType
        {
            get { return bepLevel; }
            private set { this.bepLevel = value; }
        }

        private BeppyLogType(LogType logType)
        {
            unityType = logType;

            switch (unityType) {
                case LogType.Assert:
                    bepLevel = LogLevel.Debug;
                    break;

                case LogType.Error:
                case LogType.Exception:
                    bepLevel = LogLevel.Error;
                    unityType = LogType.Error;
                    break;

                case LogType.Warning:
                    bepLevel = LogLevel.Warning;
                    break;

                case LogType.Log:
                default:
                    bepLevel = LogLevel.Info;
                    break;
            }
        }

        private BeppyLogType(LogLevel level)
        {
            bepLevel = level;
            switch (bepLevel)
            {
                case LogLevel.Warning:
                    unityType = LogType.Warning;
                    break;

                case LogLevel.Debug:
                    unityType = LogType.Assert;
                    break;

                case LogLevel.Fatal:
                case LogLevel.Error:
                    unityType = LogType.Error;
                    bepLevel = LogLevel.Error;
                    break;

                case LogLevel.Message:
                case LogLevel.All:
                case LogLevel.Info:
                default:
                    unityType = LogType.Log;
                    bepLevel = LogLevel.Info;
                    break;
            }
        }

        public static BeppyLogType Get<T>(T logType) where T : System.Enum
        {
            if (logType is LogLevel level)
                return new BeppyLogType(level);
            else if (logType is LogType type)
                return new BeppyLogType(type);

            return new BeppyLogType(LogType.Log);
        }

        public override string ToString()
        {
            switch (bepLevel)
            {
                case LogLevel.Warning:
                    return "WRN";

                case LogLevel.Debug:
                    return "DBG";

                case LogLevel.Error:
                    return "ERR";

                case LogLevel.Info:
                default:
                    return "INF";

            }
        }
    }
}
