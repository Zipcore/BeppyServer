using BepInEx;
using HarmonyLib;
using BepInEx.Logging;
using System;
using System.Collections.Generic;
using System.IO;

namespace BeppyServer {

    [BepInPlugin("com.github.beemerwt.beppyserver", "BeppyServer", "1.0.0.0")]
    public class BeppyServer : BaseUnityPlugin {

        // Things to note:
        // PooledBinaryReader.ReadString reads a length-prefixed string. The length is the ONE byte before the string
    // // We don't need to initialize Harmony and call "PatchAll" because MelonServer already does it for us.
        // Unity does not like properties apparently..
        // Prefabs are individual game objects, as are chunks.
        // Each Zombie is a set of gameobjects that make them up, NOT individual prefabs (but probably stored as entity with references to each)
        // "Physics" is a gameObject, don't exactly know what that entails. Maybe it's entity physics, maybe it's prefab physics (like falling platforms), maybe both?
        // (Sdtd)Console.print works but Instance.Output does not.

        public static ManualLogSource BeppyLogger;

        public void Awake()
        {
            BeppyLogger = Logger;
            Logger.LogInfo("BeppyServer Loaded!");

            var harmony = new Harmony("beppyserver");
            harmony.PatchAll();

            // ConnectionManager.VerboseNetLogging = false;
            Permissions.Load();
        }

        // 2021-07-11T12:14:12 4.698 INF StartAsServer
        public static void OnStartGame()
        {
            BeppyLogger.LogInfo("BeppyServer Latched.");
            ConnectionManager.OnClientAdded += OnClientAdded;
            ConnectionManager.OnClientDisconnected += OnClientDisconnected;

            GameManager.Instance.OnClientSpawned += OnClientSpawned;
        }

        // When the client has spawned he will have both an entityId and playerName.
        private static void OnClientSpawned(ClientInfo obj)
        {
            var playerName = obj.playerName;
            var steamId = obj.playerId;

            BeppyLogger.LogInfo($"{playerName} joined.");
            if (!Permissions.DoesUserExist(playerName))
            {
                BeppyLogger.LogInfo($"This is {playerName}'s first time playing. Creating permissions.");
                Permissions.CreateNewUser(playerName, steamId);
            }
        }

        // Client does not include playerName until he has successfully joined server.
        // This is called before that.
        private static void OnClientAdded(ClientInfo _clientInfo) { }

        public static void Log(string message, params string[] args) {
            BeppyLogger.LogInfo(string.Format(message, args));
        }

        public static void Error(Exception exception) {
            BeppyLogger.LogError(exception);
        }

        private static void OnClientDisconnected(ClientInfo _clientInfo)
        {
            var playerName = _clientInfo.playerName;
            BeppyLogger.LogInfo($"{playerName} disconnected.");
        }

        public static void SendMessage(int entityId, string message)
        {
            // Is not the null message we're getting from sending command
            // but we're also not getting any message.
            GameManager.Instance.ChatMessageClient(EChatType.Whisper, 0, message, "Server", false, new List<int>() { entityId });
        }

        public static void OnPlayerCommand(ChatCommand message)
        {
            BeppyLogger.LogInfo("Command Issued from player: " + message.PlayerName);

            try
            {
                PlayerCommand command = Commands.GetCommand(message.Command);
                if (Permissions.UserHasPermission(message.PlayerName, command.permission))
                    command.Execute(message.Sender, message.Args);
                else
                    SendMessage(message.EntityId, "You do not have permission to use that command.");
            }
            catch (Exception e)
            {
                if (e is CommandArgsException)
                    SendMessage(message.EntityId, e.Message);
                else
                    BeppyLogger.LogError(e);
            }
        }

        // Happens when the server receives a packet.
        public static void OnParsePackage(PooledBinaryReader _reader, ClientInfo _sender, ref NetPackage __result) {
        }

        public static ClientInfo GetClientInfo(string playerName) => ConsoleHelper.ParseParamPlayerName(playerName);

        public void OnApplicationQuit()
        {
            BeppyLogger.LogInfo("Shutdown requested.");
            BeppyLogger.LogInfo("Saving permissions.");
            Permissions.Save();
        }

        public static short ReverseShort(short num)
        {
            byte[] b = BitConverter.GetBytes(num);
            Array.Reverse(b);
            return BitConverter.ToInt16(b, 0);
        }

        public static T MarshalPackage<T>(PooledBinaryReader reader) where T : INetPackage, new()
        {
            reader.BaseStream.Seek(0, SeekOrigin.Begin);
            T package = new T();
            package.Decode(reader);
            return package;
        }
    }
}