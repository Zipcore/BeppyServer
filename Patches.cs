using System;
using HarmonyLib;
using System.Collections.Generic;

namespace BeppyServer {

    [HarmonyPatch(typeof(GameManager))]
    class GameManagerPatch
    {
        [HarmonyPostfix]
        [HarmonyPatch("StartGame")]
        static void StartGame() => BeppyServer.OnStartGame();

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
            BeppyServer.OnPlayerCommand(message);

            // Commands don't get sent to players
            return false;
        }
    }

    [HarmonyPatch(typeof(NetPackageManager))]
    class NetPackageManagerPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("ParsePackage")]
        public static bool ParsePackagePrefix(PooledBinaryReader _reader, ClientInfo _sender, ref NetPackage __result)
        {
            BeppyServer.OnParsePackage(_reader, _sender, ref __result);
            return true;
        }
    }
}