using System;
using System.Net;
using HarmonyLib;

namespace BeppyServer.Patches {
    [HarmonyPatch(typeof(WebServer))]
    public class WebServerPatch {
        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, typeof(string[]), typeof(Func<HttpListenerRequest, string>))]
        private static void ConstructorPostfix(
            ref WebServer __instance, string[] prefixes, Func<HttpListenerRequest, string> method
        ) {
            Console.Log("WebServer Constructor1 Postfixed");
        }

        [HarmonyPostfix]
        [HarmonyPatch(MethodType.Constructor, typeof(Func<HttpListenerRequest, string>), typeof(string[]))]
        private static void ConstructorPostfix(
            ref WebServer __instance, Func<HttpListenerRequest, string> method, params string[] prefixes
        ) {
            Console.Log("WebServer Constructor2 Postfixed");
        }

        [HarmonyPrefix]
        [HarmonyPatch("Run")]
        private static void RunPrefix() {
            Console.Log("Run Prefixed");
        }


        [HarmonyPostfix]
        [HarmonyPatch("Run")]
        private static void RunPostfix() {
            Console.Log("Run Postfixed");
        }

        [HarmonyPrefix]
        [HarmonyPatch("Stop")]
        private static void StopPrefix() {
            Console.Log("Stop Prefixed");
        }

        [HarmonyPostfix]
        [HarmonyPatch("Stop")]
        private static void StopPostfix() {
            Console.Log("Stop Postfixed");
        }
    }
}