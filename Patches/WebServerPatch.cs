using System;
using HarmonyLib;

namespace BeppyServer.Patches
{
    [HarmonyPatch(typeof(WebServer))]
    public class WebServerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(string[]), typeof(Func<System.Net.HttpListenerRequest, string>) })]
        static void ConstructorPostfix(ref WebServer __instance, string[] prefixes, System.Func<System.Net.HttpListenerRequest, string> method)
        {
            Console.Log("WebServer Constructor1 Postfixed");
        }

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, new Type[] { typeof(Func<System.Net.HttpListenerRequest, string>), typeof(string[]) })]
        static void ConstructorPostfix(ref WebServer __instance, System.Func<System.Net.HttpListenerRequest, string> method, params string[] prefixes)
        {
            Console.Log("WebServer Constructor2 Postfixed");
        }

        [HarmonyPrefix]
        [HarmonyPatch("Run")]
        static void RunPrefix()
        {
            Console.Log("Run Prefixed");
        }


        [HarmonyPostfix]
        [HarmonyPatch("Run")]
        static void RunPostfix()
        {
            Console.Log("Run Postfixed");
        }

        [HarmonyPrefix]
        [HarmonyPatch("Stop")]
        static void StopPrefix()
        {
            Console.Log("Stop Prefixed");
        }

        [HarmonyPostfix]
        [HarmonyPatch("Stop")]
        static void StopPostfix()
        {
            Console.Log("Stop Postfixed");
        }
    }
}
