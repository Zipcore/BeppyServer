using System.Collections.Generic;
using HarmonyLib;

namespace BeppyServer.Patches
{
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
}
