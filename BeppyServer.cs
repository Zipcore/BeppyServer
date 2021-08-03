using System;
using System.Collections.Generic;
using BepInEx;
using BepInEx.Configuration;
using BeppyServer.Patches;
using BeppyServer.DataSources;
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

        private WebServer webServer;
        private Cluster cluster;
        
        private IDataSource dataSource;
        private Permissions permissions;

        public void Awake() {
            ConfigEntry<bool> clusterEnabled = Config.Bind("General", "ClusterEnabled", false);
            ConfigEntry<bool> webserverEnabled = Config.Bind("General", "WebServerEnabled", false);
            ConfigEntry<string> permissionsFile = Config.Bind("General", "PermissionsFile", "permissions.json",
                "Permissions file name if ClusterEnabled is false.");

            ConfigEntry<string> webserverRootAddress = Config.Bind("General", "WebServerURL", "http://localhost/",
                "URL for players to open in their browser.");
            
            Console.BeppyConsole = Logger;
            
            webServer = new WebServer(Config);
            cluster = new Cluster(Config);
            
            webServer.GetServerStatus += GetServerStatus;
            GameManagerPatch.OnStartGame += OnStartGame;
            GameManagerPatch.OnBeforeStartGame += OnBeforeStartGame;
            GameManagerPatch.OnPlayerCommand += OnPlayerCommand;
            GameStateManagerPatch.OnStartGameFinished += OnStartGameFinished;
            WorldPatch.OnSaveWorld += OnSaveWorld;

            string serverName = GamePrefs.GetString(EnumGamePrefs.GameName);
            Console.Log($"Server Name: {serverName}");
            
            new Harmony("beppyserver").PatchAll();

            if (clusterEnabled.Value)
                dataSource = cluster.InitializePermissions();
            else
                dataSource = new FilePermissions(permissionsFile.Value);

            permissions = dataSource.Load();

            if (webserverEnabled.Value)
                webServer.Open();

            Console.Log("BeppyServer Loaded!");
        }

        private ServerStatus GetServerStatus() {
            int curConn = WorldManager.World.Players.Count;
            int maxConn = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount);

            return new ServerStatus {
                connections = curConn,
                maxConnections = maxConn
            };
        }

        public void OnBeforeStartGame() {
        }

        // 2021-07-11T12:14:12 4.698 INF StartAsServer
        public void OnStartGame() {
            ConnectionManager.OnClientAdded += OnClientAdded;
            ConnectionManager.OnClientDisconnected += OnClientDisconnected;
            GameManager.Instance.OnClientSpawned += OnClientSpawned;
        }

        public void OnStartGameFinished() {
            Console.Log("OnStartGame Called");
        }

        public void OnSaveWorld() {
            Console.Log("Saving permissions.");
            dataSource.Save(permissions);
            webServer.Close();
        }

        // When the client has spawned he will have both an entityId and playerName.
        private void OnClientSpawned(ClientInfo obj) {
            string steamId = obj.playerId;
            string name = obj.playerName;
            
            Player p = permissions.GetPlayerBySteamId(steamId);
            if (p == null) {
                Console.Log($"This is {name}'s first time playing. Creating permissions.");
                permissions.AddPlayer(new Player(steamId, name, "user", false, DateTime.Now));
            } else {
                p.LastPlayed = DateTime.Now;
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

        public static ClientInfo GetClientInfo(string playerName) => ConsoleHelper.ParseParamPlayerName(playerName);
    }
}