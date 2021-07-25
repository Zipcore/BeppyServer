using System.Collections.Generic;
using HarmonyLib;

namespace BeppyServer.Patches
{
    // Created even AFTER the WebServer "Run" is postfixed
    [HarmonyPatch(typeof(ControlPanel))]
    public class ControlPanelPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor)]
        static void ConstructorPostfix(ref ControlPanel __instance)
        {
            Console.Log("ControlPanel Constructor Postfixed");
        }

        // The following methods get called every time they normally are called for the console
        // The normal server performance isn't effected, runs on separate thread
        // Having all these threads, though, the CPU is probably dying.
        [HarmonyPrefix]
        [HarmonyPatch("SendLine")]
        static void SendLinePrefix(string _text)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch("SendLines")]
        static void SendLinesPrefix(List<string> _output)
        {
        }

        [HarmonyPrefix]
        [HarmonyPatch("SendLog")]
        static void SendLogPrefix(ref string _msg, string _trace, UnityEngine.LogType _type)
        {

        }
    }
}
