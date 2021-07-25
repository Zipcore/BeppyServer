using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BeppyServer.Patches;

namespace BeppyServer
{

    [BepInPlugin("beppyserver", "BeppyServer", "1.0.0.0")]
    public class BeppyServer : BaseUnityPlugin {

        // Things to note:
        // PooledBinaryReader.ReadString reads a length-prefixed string. The length is the ONE byte before the string
        // Unity does not like properties apparently..
        // Prefabs are individual game objects, as are chunks.
        // Each Zombie is a set of gameobjects that make them up, NOT individual prefabs (but probably stored as entity with references to each)
        // "Physics" is a gameObject, don't exactly know what that entails. Maybe it's entity physics, maybe it's prefab physics (like falling platforms), maybe both?

        public static BeppyServer Instance;

        private ConfigEntry<string> permissionsFile;
        private ConfigEntry<string[]> clusterServers; // TODO

        private Permissions permissions;

        public void Awake()
        {
            Instance = this;
            Console.BeppyConsole = Logger;
            GameManagerPatch.OnStartGame += OnStartGame;
            GameManagerPatch.OnPlayerCommand += OnPlayerCommand;
            GameStateManagerPatch.OnStartGameFinished += OnStartGameFinished;
            WorldPatch.OnSaveWorld += OnSaveWorld;

            permissionsFile = Config.Bind(
                "General",
                "Permissions File",
                "permissions.json",
                "The file for which player permissions are written to and read from."
            );

            permissions = new Permissions(permissionsFile.Value);
            var harmony = new Harmony("beppyserver");
            harmony.PatchAll();

            Console.Log("BeppyServer Loaded!");
        }

        // 2021-07-11T12:14:12 4.698 INF StartAsServer
        public void OnStartGame()
        {
            ConnectionManager.OnClientAdded += Instance.OnClientAdded;
            ConnectionManager.OnClientDisconnected += Instance.OnClientDisconnected;
            GameManager.Instance.OnClientSpawned += Instance.OnClientSpawned;
        }

        public void OnStartGameFinished()
        {
            Console.Log("OnStartGame Called");
        }

        public void OnSaveWorld()
        {
            Console.Log("Saving permissions.");
            permissions.save();
        }

        // When the client has spawned he will have both an entityId and playerName.
        private void OnClientSpawned(ClientInfo obj)
        {
            var playerName = obj.playerName;
            var steamId = obj.playerId;

            if (!permissions.doesUserExist(playerName))
            {
                Console.Log($"This is {playerName}'s first time playing. Creating permissions.");
                permissions.addUser(playerName, steamId, "users");
            }
        }

        // Client does not include playerName until he has successfully joined server.
        // This is called before that.
        private void OnClientAdded(ClientInfo _clientInfo) { }

        public void OnClientDisconnected(ClientInfo _clientInfo)
        {
            var playerName = _clientInfo.playerName;
        }

        public void SendMessage(ClientInfo client, string message)
        {
            GameManager.Instance.ChatMessageServer(client, EChatType.Whisper, client.entityId, message, "Server", false, new List<int>() { client.entityId });
        }

        public void OnPlayerCommand(ChatCommand message)
        {
            Console.Log($"Command \"{message.Command}\" issued from player {message.PlayerName}");

            try
            {
                PlayerCommand command = Commands.GetCommand(message.Command);
                if (permissions.userHasPermission(message.PlayerName, command.permission))
                    command.Execute(message.Sender, message.Args);
                else
                    SendMessage(message.Sender, "You do not have permission to use that command.");
            }
            catch (Exception e)
            {
                if (e is CommandArgsException || e is InvalidCommandException)
                    SendMessage(message.Sender, e.Message);
                else
                    Console.Exception(e);
            }
        }

        // Happens when the server receives a packet.
        public static void OnParsePackage(PooledBinaryReader _reader, ClientInfo _sender, ref NetPackage __result) {
        }

        public static ClientInfo GetClientInfo(string playerName) => ConsoleHelper.ParseParamPlayerName(playerName);
    }
}