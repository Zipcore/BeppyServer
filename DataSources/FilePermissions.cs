using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

namespace BeppyServer.DataSources {
    public class FilePermissions : IDataSource {

        private string fileName;
        
        public FilePermissions(string fileName) {
            this.fileName = fileName;
        }

        private void SaveDefaultFile(Permissions permissions) {
            PermissionGroup admin = new PermissionGroup("all");
            PermissionGroup user = new PermissionGroup();
            Player blankUser = new Player("0", "admin", "admin", false, DateTime.Now);

            permissions.Groups.Add("admin", admin);
            permissions.Groups.Add("user", user);
            permissions.Players.Add(blankUser);

            SaveToFile(fileName, permissions);
        }
        
        public Permissions Load() {
            Permissions permissions = new Permissions();

            if (!File.Exists(fileName))
                SaveDefaultFile(permissions);

            string serialized = File.ReadAllText(fileName);
            JSONNode rootNode = JSON.Parse(serialized);

            JSONNode groups = rootNode["groups"];
            JSONNode users = rootNode["users"];

            foreach (KeyValuePair<string, JSONNode> pair in groups.AsObject)
                permissions.Groups.Add(pair.Key, DeserializeGroup(pair.Value.AsObject));

            foreach (KeyValuePair<string, JSONNode> pair in users.AsArray)
                permissions.Players.Add(DeserializePlayer(pair.Value.AsObject));

            return permissions;
        }

        public void Save(Permissions permissions) {
            SaveToFile(fileName, permissions);
        }

        private static PermissionGroup DeserializeGroup(JSONObject obj) {
            PermissionGroup permGroup = new PermissionGroup();

            JSONNode permsJson = obj["permissions"];
            foreach (JSONNode node in permsJson)
                permGroup.Permissions.Add(node.Value);

            return permGroup;
        }

        private static JSONObject SerializeGroup(PermissionGroup group) {
            JSONObject groupObject = new JSONObject();

            JSONArray permissions = new JSONArray();
            foreach (string perm in group.Permissions)
                permissions.Add(new JSONString(perm));

            groupObject.Add("permissions", permissions);
            return groupObject;
        }

        private static Player DeserializePlayer(JSONObject obj) {
            string name = obj["name"].Value;
            string steamId = obj["steamId"].Value;
            string group = obj["group"].Value;
            bool banned = obj["banned"].AsBool;
            string lastPlayed = obj["lastPlayed"].Value;

            List<string> additionalPermissions = new List<string>();
            JSONArray permissions = obj["additionalPermissions"].AsArray;
            foreach (JSONNode perm in permissions)
                additionalPermissions.Add(perm.Value);

            if (!DateTime.TryParse(lastPlayed, out DateTime dateLastPlayed))
                dateLastPlayed = DateTime.Now;

            Player p = new Player(steamId, name, group, banned, dateLastPlayed, additionalPermissions.ToArray());
            return p;
        }

        private static JSONObject SerializePlayer(Player p) {
            JSONObject obj = new JSONObject();
            obj.Add("steamId", new JSONString(p.SteamId));
            obj.Add("name", new JSONString(p.Name));
            obj.Add("group", new JSONString(p.Group));
            obj.Add("banned", new JSONBool(p.Banned));
            obj.Add("lastPlayed", new JSONString(p.LastPlayed.ToString("s")));

            JSONArray perms = new JSONArray();
            foreach (string perm in p.GetAdditionalPermissions())
                perms.Add(new JSONString(perm));

            obj.Add("additionalPermissions", perms);
            return obj;
        }

        // Expensive. Do not use unless server is experiencing downtime or is restarting/shuttingdown
        public static void SaveToFile(string fileName, Permissions permissions) {
            
            JSONObject groupObject = new JSONObject();

            foreach (KeyValuePair<string, PermissionGroup> pair in permissions.Groups)
                groupObject.Add(pair.Key, SerializeGroup(pair.Value));

            JSONArray userArray = new JSONArray();
            foreach (Player player in permissions.Players)
                userArray.Add(SerializePlayer(player));

            JSONNode rootNode = new JSONObject();
            rootNode.Add("groups", groupObject);
            rootNode.Add("users", userArray);
            string serialized = rootNode.ToString(4);

            try {
                File.WriteAllText(fileName, serialized);
            } catch (Exception e) {
                throw new PermissionException("Could not save permissions file.", e);
            }
        }
    }
}