using System;
using System.Collections.Generic;
using HarmonyLib;

#if WIN32
namespace BeppyServer.Patches
{
    [HarmonyPatch(typeof(WinFormConnection))]
    public class WinFormConnectionPatch
    {

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(WinFormInstance) })]
        static void ConstructorPostfix(WinFormConnection __instance)
        {
            Console.NativeConsole = new WindowsConsole(__instance);
        }

        [HarmonyPrefix]
        [HarmonyPatch("SendLog")]
        static bool SendLogPrefix(ref string _text, string _trace, UnityEngine.LogType _type)
        {
            Console.Translate(_type, ref _text);
            return true;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SendLine")]
        static bool SendLinePrefix(ref string _line)
        {
            Console.Log(_line);
            return false;
        }

        [HarmonyPrefix]
        [HarmonyPatch("SendLines")]
        static bool SendLinesPrefix(List<string> _output)
        {
            foreach (string line in _output)
            {
                Console.Log(line);
            }

            return false;
        }
    }
}
#endif
