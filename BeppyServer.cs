using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BeppyServer.Patches;
using HarmonyLib;

namespace BeppyServer {
    [BepInPlugin("beppyserver", "BeppyServer", "1.0.0.0")]
    public class BeppyServer : BaseUnityPlugin {
        // Things to note:
        // PooledBinaryReader.ReadString reads a length-prefixed string. The length is the ONE byte before the string
        // Unity does not like properties apparently..
        // Prefabs are individual game objects, as are chunks.
        // Each Zombie is a set of gameobjects that make them up, NOT individual prefabs (but probably stored as entity with references to each)
        // "Physics" is a gameObject, don't exactly know what that entails. Maybe it's entity physics, maybe it's prefab physics (like falling platforms), maybe both?

        // TODOS
        // Add VAC interception and give server owners the option to not allow people who are VAC banned (Steamworks)
        // Create way to edit the serverconfig while server is running
        // 

        public static BeppyServer Instance;
        private BeppyCluster cluster;

        private Permissions permissions;

        private ConfigEntry<string> permissionsFile;

        public void Awake() {
            Instance = this;
            Console.BeppyConsole = Logger;

            string serverName = GamePrefs.GetString(EnumGamePrefs.GameName);
            Console.Log($"Server Name: {serverName}");

            permissionsFile = Config.Bind("General", "PermissionsFile", "permissions.json");

            permissions = new Permissions();
            cluster = new BeppyCluster(Config);

            new Harmony("beppyserver").PatchAll();

            GameManagerPatch.OnStartGame += OnStartGame;
            GameManagerPatch.OnBeforeStartGame += OnBeforeStartGame;
            GameManagerPatch.OnPlayerCommand += OnPlayerCommand;
            GameStateManagerPatch.OnStartGameFinished += OnStartGameFinished;
            WorldPatch.OnSaveWorld += OnSaveWorld;

            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args) Console.Log(arg);

            // CommandLine Arg: -permissionsfile=<filename>
            // If specified then the server will use the filename as the permissions file.
            string permissionsFileArg = GetCommandLineArg("permissionsfile");

            // CommandLine Arg: -useclusterpermissions
            // If specified then the server will use permissions from the cluster instead of a file.
            // Takes precedence over -permissionsfile arg
            string useClusterPermissionsArg = GetCommandLineArg("useclusterpermissions", false);

            // IMPORTANT!! CommandLine Args take precedence over BepInEx's config file (beppyserver.cfg).
            if (useClusterPermissionsArg != null) {
                cluster.IsHandlingPermissions = true;
            } else if (permissionsFileArg != null) {
                permissionsFile.Value = permissionsFileArg;
                cluster.IsHandlingPermissions = false;
            }

            Console.Log("BeppyServer Loaded!");
        }

        private static string GetCommandLineArg(string name, bool hasvalue = true) {
            string[] args = Environment.GetCommandLineArgs();
            foreach (string arg in args) {
                if (!arg.ContainsCaseInsensitive(name))
                    continue;
                if (!hasvalue) return arg;
                string[] kv = arg.Split('=');
                if (kv.Length > 1)
                    return kv[1];
            }

            return null;
        }

        // Returning false causes break in game.
        public void OnBeforeStartGame() {
            if (cluster.IsHandlingPermissions) {
                if (!cluster.VerifyDatabase()) {
                    string shouldCreate = "";
                    while (!shouldCreate.EqualsCaseInsensitive("y") && !shouldCreate.EqualsCaseInsensitive("n"))
                        shouldCreate = Console.GetInput("Would you like me to create them for you [Y/n]: ");
                    if (shouldCreate.EqualsCaseInsensitive("n"))
                        throw new Exception("Please create the database then relaunch the server.");

                    cluster.SetupDatabase();
                    permissions.LoadFromCluster(cluster);
                }
            } else {
                permissions.LoadFromFile(permissionsFile.Value);
            }
        }

        // 2021-07-11T12:14:12 4.698 INF StartAsServer
        public void OnStartGame() {
            ConnectionManager.OnClientAdded += Instance.OnClientAdded;
            ConnectionManager.OnClientDisconnected += Instance.OnClientDisconnected;
            GameManager.Instance.OnClientSpawned += Instance.OnClientSpawned;
        }

        public void OnStartGameFinished() {
            Console.Log("OnStartGame Called");
        }

        public void OnSaveWorld() {
            Console.Log("Saving permissions.");

            if (cluster.IsHandlingPermissions)
                permissions.SaveToCluster(cluster);
            else
                permissions.SaveToFile(permissionsFile.Value);
        }

        // When the client has spawned he will have both an entityId and playerName.
        private void OnClientSpawned(ClientInfo obj) {
            string steamId = obj.playerId;
            string name = obj.playerName;

            if (permissions.GetPlayerBySteamId(steamId) == null) {
                Console.Log($"This is {name}'s first time playing. Creating permissions.");
                permissions.AddPlayer(new Player(steamId, name, "user"));
            }
        }

        // Client does not include playerName until he has successfully joined server.
        // This is called before that.
        private void OnClientAdded(ClientInfo clientInfo) { }

        public void OnClientDisconnected(ClientInfo clientInfo) { }

        public void SendMessage(ClientInfo client, string message) {
            GameManager.Instance.ChatMessageServer(client, EChatType.Whisper, client.entityId, message,
                "Server", false, new List<int> {client.entityId});
        }

        public void OnPlayerCommand(ChatCommand message) {
            Console.Log($"Command \"{message.Command}\" issued from player {message.PlayerName}");

            try {
                PlayerCommand command = Commands.GetCommand(message.Command);
                Player p = permissions.GetPlayerByName(message.PlayerName);
                if (permissions.GroupHasPermission(p.Group, command.permission)
                    || p.HasPermission(command.permission))
                    command.Execute(message.Sender, message.Args);
                else
                    SendMessage(message.Sender, "You do not have permission to use that command.");
            } catch (Exception e) {
                if (e is CommandArgsException || e is InvalidCommandException)
                    SendMessage(message.Sender, e.Message);
                else
                    Console.Exception(e);
            }
        }

        // Happens when the server receives a packet.
        public static void OnParsePackage(PooledBinaryReader reader, ClientInfo sender, ref NetPackage result) { }

        public static ClientInfo GetClientInfo(string playerName) {
            return ConsoleHelper.ParseParamPlayerName(playerName);
        }
    }
}