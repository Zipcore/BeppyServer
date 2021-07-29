// ReSharper disable InconsistentNaming

#if WIN32
using System;
using System.Collections.Generic;
using BeppyServer.Native;
using HarmonyLib;

namespace BeppyServer.Patches
{
    [HarmonyPatch(typeof(WinFormConnection))]
    public class WinFormConnectionPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, typeof(WinFormInstance))]
        private static void ConstructorPostfix(WinFormConnection __instance)
        {
            Console.NativeConsole = new WindowsConsole(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch("SendLog")]
        private static bool SendLogPrefix(ref string _text, string _trace, UnityEngine.LogType _type)
        {
            Console.Translate(_type, ref _text);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SendLine")]
        private static bool SendLinePrefix(ref string _line)
        {
            Console.Translate(UnityEngine.LogType.Log, ref _line);
            return true;
        }
    }
}
#endif