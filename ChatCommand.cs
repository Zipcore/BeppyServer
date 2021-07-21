using System.Collections.Generic;

namespace BeppyServer {
    public class ChatCommand
    {
        public ClientInfo Sender;
        public EChatType ChatType;
        public int EntityId;
        public string BaseMessage;
        public string Command;
        public List<string> Args;
        public string PlayerName;
        private List<int> recipientEntityIds;

        public ChatCommand(ClientInfo cInfo, EChatType chatType, int senderEntityId,
            string msg, string mainName, bool localizeMain, List<int> recipientEntityIds)
        {
            Sender = cInfo;
            ChatType = chatType;
            EntityId = senderEntityId;
            BaseMessage = msg;
            PlayerName = mainName;

            this.recipientEntityIds = recipientEntityIds;

            string[] commandAndArgs = BaseMessage.Split(' ');
            Command = commandAndArgs[0].Substring(1); // Remove slash

            Args = new List<string>();

            for (int i = 1; i < commandAndArgs.Length; i++)
            {
                Args.Add(commandAndArgs[i]);
            }
        }

        public void SetChatType(EChatType newType)
        {
            this.ChatType = newType;
        }

        public List<int> GetRecipients()
        {
            return recipientEntityIds;
        }
    }
}