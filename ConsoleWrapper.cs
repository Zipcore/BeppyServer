using System;
using System.Text.RegularExpressions;
using BepInEx.Logging;
using BeppyServer.Native;

namespace BeppyServer {
    public static class Console {
        private const string LOG_TIME_PATTERN
            = @"([0-9]{4}-[0-9]{2}-[0-9]{2}T[0-9]{2}:[0-9]{2}:[0-9]{2}\s+[0-9]+.[0-9]{3}\s+(INF|WRN|ERR)\s+)";

        public static SystemConsole NativeConsole;
        public static ManualLogSource BeppyConsole;

        private static void Log(LogLevel level, string message, params string[] args) {
            string final = string.Format(message, args);
            if (NativeConsole != null)
                NativeConsole.Log(BeppyLogType.Get(level), final);
            BeppyConsole.Log(level, final);
        }

        // Gets called before finally sending to replace formatting
        // Keeps everything consistent. Runs on "separate" thread from main server.
        // Doesn't drop performance (on Windows confirmed, unknown on Linux)
        public static void Translate<T>(T type, ref string text) where T : Enum {
            if (NativeConsole == null)
                return;

            text = Regex.Replace(text, LOG_TIME_PATTERN, "");
            NativeConsole.Translate(BeppyLogType.Get(type), ref text);
        }

        // "Usable" methods
        public static void Log(string message, params string[] args) {
            Log(LogLevel.Info, message, args);
        }

        public static void Error(string message, params string[] args) {
            Log(LogLevel.Error, message, args);
        }

        public static void Exception(Exception e) {
            Log(LogLevel.Error, e.Message);
        }

        public static void Warning(string message, params string[] args) {
            Log(LogLevel.Warning, message, args);
        }

        public static string GetInput(string message) {
            string input = NativeConsole.GetInput(message);
            Log("Input was " + input);
            return input;
        }
    }
}