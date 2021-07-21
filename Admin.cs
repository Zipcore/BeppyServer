using System.Collections.Generic;

namespace BeppyServer {
    public class Admin
    {
        public static void KickAll(ClientInfo sender, string reason = "")
        {
            CommandSenderInfo csi = new CommandSenderInfo();
            csi.IsLocalGame = false;
            csi.RemoteClientInfo = sender;

            ConsoleCmdKickAll cmd = new ConsoleCmdKickAll();
            cmd.Execute(new List<string>() { reason }, csi);
        }

        public static void KillAll(ClientInfo sender)
        {
            CommandSenderInfo csi = new CommandSenderInfo();
            csi.IsLocalGame = false;
            csi.RemoteClientInfo = sender;

            ConsoleCmdKillAll cmd = new ConsoleCmdKillAll();
            cmd.Execute(new List<string>(), csi);
        }

        public static void KickPlayer(ClientInfo sender, string nameOrEntityIdOrSteamId, string reason = "")
        {
            CommandSenderInfo csi = new CommandSenderInfo();
            csi.IsLocalGame = false;
            csi.RemoteClientInfo = sender;

            List<string> args = new List<string>() { nameOrEntityIdOrSteamId, reason };
            ConsoleCmdKick cmd = new ConsoleCmdKick();
            cmd.Execute(args, csi);
        }

        public static void BanPlayer(ClientInfo sender, string nameOrEntityIdOrSteamId, string reason = "")
        {
            CommandSenderInfo csi = new CommandSenderInfo();
            csi.IsLocalGame = false;
            csi.RemoteClientInfo = sender;

            List<string> args = new List<string>() { nameOrEntityIdOrSteamId, reason };
            ConsoleCmdBan cmd = new ConsoleCmdBan();
            cmd.Execute(args, csi);
        }

        public static void KickPlayer(ClientInfo sender, int entityId, string reason = "") => KickPlayer(sender, entityId.ToString(), reason);
        public static void KickPlayer(ClientInfo sender, ulong steamId, string reason = "") => KickPlayer(sender, steamId.ToString(), reason);
        public static void BanPlayer(ClientInfo sender, int entityId, string reason = "") => BanPlayer(sender, entityId.ToString(), reason);
        public static void BanPlayer(ClientInfo sender, ulong steamId, string reason = "") => BanPlayer(sender, steamId.ToString(), reason);
    }
}