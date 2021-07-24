using System;
using HarmonyLib;
using System.Collections.Generic;
using BepInEx.Logging;

namespace BeppyServer {
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


    [HarmonyPatch(typeof(GameManager))]
    public class GameManagerPatch
    {
        public delegate void GameStartCallback();
        public delegate void PlayerCommandCallback(ChatCommand message);

        public static GameStartCallback OnStartGame;
        public static PlayerCommandCallback OnPlayerCommand;

        [HarmonyPostfix]
        [HarmonyPatch("StartGame")]
        static void StartGame() => OnStartGame();

        [HarmonyPrefix]
        [HarmonyPatch("ChatMessageServer")]
        static bool ChatMessageServer(ref ClientInfo _cInfo, ref EChatType _chatType, ref int _senderEntityId,
            ref string _msg, ref string _mainName, ref bool _localizeMain, ref List<int> _recipientEntityIds)
        {
            // Someone could put a filter here for bad words, just change _msg.


            // pass if we don't receive a command
            if (_msg[0] != '/')
                return true;

            ChatCommand message = new ChatCommand(_cInfo, _chatType, _senderEntityId, _msg, _mainName, _localizeMain, _recipientEntityIds);
            OnPlayerCommand(message);

            // Commands don't get sent to players
            return false;
        }
    }

    [HarmonyPatch(typeof(World))]
    class WorldPatch
    {
        public delegate void SaveCallback();
        public static SaveCallback OnSaveWorld;

        [HarmonyPrefix]
        [HarmonyPatch("Cleanup")]
        static void Cleanup() => OnSaveWorld();
    }
}