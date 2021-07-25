using BepInEx;
using HarmonyLib;
using System;
using System.Collections.Generic;
using BepInEx.Configuration;
using BeppyServer.Patches;
using System.IO;

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

        private Permissions permissions;
        private BeppyCluster cluster;

        private ConfigEntry<string> permissionsFile;

        private static string GetCommandLineArg(string name, bool hasvalue=true)
        {
            var args = Environment.GetCommandLineArgs();
            foreach (var arg in args)
            {
                if (arg.ContainsCaseInsensitive(name))
                {
                    if (!hasvalue) return arg;
                    string[] kv = arg.Split('=');
                    if (kv.Length > 1)
                        return kv[1];
                }
            }

            return null;
        }

        public void Awake()
        {
            Instance = this;
            Console.BeppyConsole = Logger;

            permissionsFile = Config.Bind("General", "PermissionsFile", "permissions.json");

            permissions = new Permissions();
            cluster = new BeppyCluster(Config);

            new Harmony("beppyserver").PatchAll();

            GameManagerPatch.OnStartGame += OnStartGame;
            GameManagerPatch.OnPlayerCommand += OnPlayerCommand;
            GameStateManagerPatch.OnStartGameFinished += OnStartGameFinished;
            WorldPatch.OnSaveWorld += OnSaveWorld;

            // CommandLine Arg: -permissionsfile=<filename>
            // If specified then the server will use the filename as the permissions file.
            string permissionsFileArg = GetCommandLineArg("permissionsfile");

            // CommandLine Arg: -useclusterpermissions
            // If specified then the server will use permissions from the cluster instead of a file.
            // Takes precedence over -permissionsfile arg
            string useClusterPermissionsArg = GetCommandLineArg("useclusterpermissions", false);

            // IMPORTANT!! CommandLine Args take precedence over BepInEx's config file (beppyserver.cfg).
            if (useClusterPermissionsArg != null)
            {
                cluster.IsHandlingPermissions = true;
                permissions.LoadFromCluster(cluster);
            }
            else if (permissionsFileArg != null)
            {
                permissionsFile.Value = permissionsFileArg;
                permissions.LoadFromFile(permissionsFile.Value);
                cluster.IsHandlingPermissions = false;
            }
            else if (cluster.IsHandlingPermissions)
                permissions.LoadFromCluster(cluster);
            else
                permissions.LoadFromFile(permissionsFile.Value);

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

            if (cluster.IsHandlingPermissions)
                permissions.SaveToCluster(cluster);
            else
                permissions.SaveToFile(permissionsFile.Value);
        }

        // When the client has spawned he will have both an entityId and playerName.
        private void OnClientSpawned(ClientInfo obj)
        {
            var steamId = obj.playerId;
            var name = obj.playerName;

            if (permissions.GetPlayerBySteamId(steamId) == null)
            {
                Console.Log($"This is {name}'s first time playing. Creating permissions.");
                permissions.AddPlayer(new Player(steamId, name, "user"));
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
                Player p = permissions.GetPlayerByName(message.PlayerName);
                
                if (permissions.GroupHasPermission(p.Group, command.permission)
                    || p.HasPermission(command.permission))
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