using System.Collections.Generic;

namespace BeppyServer {
    public class ChatCommand {
        public List<string> Args;

        public string BaseMessage;
        public EChatType ChatType;
        public string Command;

        public int EntityId;
        public string PlayerName;
        private readonly List<int> recipientEntityIds;
        public ClientInfo Sender;

        // Handles command parsing
        public ChatCommand(
            ClientInfo cInfo, EChatType chatType, int senderEntityId,
            string msg, string mainName, bool localizeMain, List<int> recipientEntityIds
        ) {
            Sender = cInfo;
            ChatType = chatType;
            EntityId = senderEntityId;
            PlayerName = mainName;
            BaseMessage = msg;

            this.recipientEntityIds = recipientEntityIds;

            string[] commandAndArgs = BaseMessage.Split(' ');
            Command = commandAndArgs[0].Substring(1); // Remove slash

            Args = new List<string>();

            for (int i = 1; i < commandAndArgs.Length; i++) Args.Add(commandAndArgs[i]);
        }

        public string SteamId => Sender.playerId;

        public void SetChatType(EChatType newType) {
            ChatType = newType;
        }

        public List<int> GetRecipients() {
            return recipientEntityIds;
        }
    }
}