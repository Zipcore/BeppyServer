using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace BeppyServer.Patches {
    // Created even AFTER the WebServer "Run" is postfixed
    [HarmonyPatch(typeof(ControlPanel))]
    public class ControlPanelPatch {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor)]
        private static void ConstructorPostfix(ref ControlPanel __instance) {
            Console.Log("ControlPanel Constructor Postfixed");
        }

        // The following methods get called every time they normally are called for the console
        // The normal server performance isn't effected, runs on separate thread
        // Having all these threads, though, the CPU is probably dying.
        [HarmonyPrefix]
        [HarmonyPatch("SendLine")]
        private static void SendLinePrefix(string _text) { }

        [HarmonyPrefix]
        [HarmonyPatch("SendLines")]
        private static void SendLinesPrefix(List<string> _output) { }

        [HarmonyPrefix]
        [HarmonyPatch("SendLog")]
        private static void SendLogPrefix(ref string _msg, string _trace, LogType _type) { }
    }
}