using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

namespace BeppyServer {
    public class PermissionException : Exception
    {
        public PermissionException(string message) : base(message) { }
        public PermissionException(string message, Exception inner) : base(message, inner) { }
    }

    public class PermissionGroup
    {
        public List<string> permissions = new List<string>();

        public PermissionGroup(List<string> permissions)
        {
            this.permissions = permissions;
        }

        public PermissionGroup(params string[] perms)
            : this(new List<string>(perms))
        { }

        public static PermissionGroup Deserialize(JSONObject obj)
        {
            var permGroup = new PermissionGroup();
            
            var permsJson = obj["permissions"];
            foreach (JSONNode node in permsJson)
                permGroup.permissions.Add(node.Value);

            return permGroup;
        }

        public JSONObject Serialize()
        {
            JSONObject groupObject = new JSONObject();

            JSONArray permissions = new JSONArray();
            foreach (string perm in this.permissions)
                permissions.Add(new JSONString(perm));

            groupObject.Add("permissions", permissions);
            return groupObject;
        }
    }

    public class Permissions
    {
        private Dictionary<string, PermissionGroup> groups;
        private List<Player> players;

        public void LoadFromFile(string fileName)
        {
            this.groups = new Dictionary<string, PermissionGroup>();
            this.players = new List<Player>();

            if (!File.Exists(fileName))
                SaveDefaultPermissions(fileName);

            string serialized = File.ReadAllText(fileName);
            JSONNode rootNode = JSON.Parse(serialized);

            var groups = rootNode["groups"];
            var users = rootNode["users"];

            foreach (var pair in groups.AsObject)
                this.groups.Add(pair.Key, PermissionGroup.Deserialize(pair.Value.AsObject));

            foreach (var pair in users.AsObject)
                this.players.Add(Player.Deserialize(pair.Value.AsObject));
        }

        public Player GetPlayerByName(string name)
        {
            foreach (var player in players)
                if (player.Name.Equals(name))
                    return player;
            return null;
        }

        public Player GetPlayerBySteamId(string steamId)
        {
            foreach (var player in players)
                if (player.SteamId.Equals(steamId))
                    return player;
            return null;
        }

        public void AddPlayer(Player player) => players.Add(player);

        public void LoadFromCluster(BeppyCluster cluster)
        {

        }

        public void SaveDefaultPermissions(string permissionsFile)
        {
            PermissionGroup admin = new PermissionGroup("all");
            PermissionGroup user = new PermissionGroup();
            Player blankUser = new Player("0", "admin", "admin");

            groups.Add("admin", admin);
            groups.Add("user", user);
            players.Add(blankUser);

            SaveToFile(permissionsFile);
        }

        // Expensive. Do not use unless server is experiencing downtime or is restarting/shuttingdown
        public void SaveToFile(string fileName)
        {
            JSONObject groupObject = new JSONObject();

            foreach (KeyValuePair<string, PermissionGroup> pair in groups)
                groupObject.Add(pair.Key, pair.Value.Serialize());

            JSONObject userObject = new JSONObject();
            foreach (var player in players)
                userObject.Add(player.Name, player.Serialize());

            JSONNode rootNode = new JSONObject();
            rootNode.Add("groups", groupObject);
            rootNode.Add("users", userObject);
            string serialized = rootNode.ToString(4);

            try
            {
                File.WriteAllText(fileName, serialized);
            }
            catch (Exception e)
            {
                throw new PermissionException("Could not save permissions file.", e);
            }
        }

        public void SaveToCluster(BeppyCluster cluster)
        {

        }

        public void AddGroup(string groupName)
        {
            if (groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} already exists.");

            groups.Add(groupName, new PermissionGroup());
        }

        public void AddPermissionToGroup(string groupName, string permission)
        {
            if (!groups.ContainsKey(groupName))
                throw new PermissionException($"Group name \"{groupName}\" does not exist.");

            groups[groupName].permissions.Add(permission);
        }

        public bool GroupHasPermission(string groupName, string permission)
        {
            if (!groups.ContainsKey(groupName))
                return false;

            if (groups[groupName].permissions == null)
                return false;

            return groups[groupName].permissions.Contains(permission)
                || groups[groupName].permissions.Contains("all");
        }

        public void RemovePermissionFromGroup(string groupName, string permission)
        {
            if (!groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            groups[groupName].permissions.Remove(permission);
        }

        public List<string> GetGroupPermissions(string groupName)
        {
            if (!groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            return groups[groupName].permissions;
        }
    }
}