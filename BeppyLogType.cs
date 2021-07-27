using System;
using BepInEx.Logging;
using UnityEngine;

namespace BeppyServer {
    public class BeppyLogType {
        public static BeppyLogType Info = new BeppyLogType(LogLevel.Info);
        public static BeppyLogType Warning = new BeppyLogType(LogLevel.Warning);
        public static BeppyLogType Error = new BeppyLogType(LogLevel.Error);
        public static BeppyLogType Debug = new BeppyLogType(LogLevel.Debug);

        private BeppyLogType(LogType logType) {
            UnityType = logType;

            switch (UnityType) {
                case LogType.Assert:
                    BepType = LogLevel.Debug;
                    break;

                case LogType.Error:
                case LogType.Exception:
                    BepType = LogLevel.Error;
                    UnityType = LogType.Error;
                    break;

                case LogType.Warning:
                    BepType = LogLevel.Warning;
                    break;

                case LogType.Log:
                default:
                    BepType = LogLevel.Info;
                    break;
            }
        }

        private BeppyLogType(LogLevel level) {
            BepType = level;
            switch (BepType) {
                case LogLevel.Warning:
                    UnityType = LogType.Warning;
                    break;

                case LogLevel.Debug:
                    UnityType = LogType.Assert;
                    break;

                case LogLevel.Fatal:
                case LogLevel.Error:
                    UnityType = LogType.Error;
                    BepType = LogLevel.Error;
                    break;

                case LogLevel.Message:
                case LogLevel.All:
                case LogLevel.Info:
                default:
                    UnityType = LogType.Log;
                    BepType = LogLevel.Info;
                    break;
            }
        }

        public LogType UnityType { get; private set; }

        public LogLevel BepType { get; private set; }

        public static BeppyLogType Get<T>(T logType) where T : Enum {
            if (logType is LogLevel level)
                return new BeppyLogType(level);
            if (logType is LogType type)
                return new BeppyLogType(type);

            return new BeppyLogType(LogType.Log);
        }

        public override string ToString() {
            switch (BepType) {
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