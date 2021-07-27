using System;
using System.Collections.Generic;
using SimpleJSON;

namespace BeppyServer {
    public class Player {
        private readonly List<string> additionalPermissions;
        private DateTime lastPlayed;

        public Player(string steamId, string name, string group, List<string> additionalPermissions) {
            SteamId = steamId;
            Name = name;
            Group = group;
            this.additionalPermissions = additionalPermissions;
        }

        public Player(string steamId, string name, string group, params string[] additionalPermissions)
            : this(steamId, name, group, new List<string>(additionalPermissions)) { }

        public string SteamId { get; }

        public string Name { get; }

        public string Group { get; private set; }

        public bool Banned { get; private set; }

        public DateTime LastPlayed => lastPlayed;

        public void SetGroup(string group) {
            Group = group;
        }

        public void AddPermission(string permission) {
            additionalPermissions.Add(permission);
        }

        public bool HasPermission(string permission) {
            return additionalPermissions.Contains(permission);
        }

        public static Player Deserialize(JSONObject obj) {
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

            Player p = new Player(steamId, name, group, additionalPermissions) {
                Banned = banned,
                lastPlayed = dateLastPlayed
            };

            return p;
        }

        public JSONObject Serialize() {
            JSONObject obj = new JSONObject();
            obj.Add("steamId", new JSONString(SteamId));
            obj.Add("name", new JSONString(Name));
            obj.Add("group", new JSONString(Group));
            obj.Add("banned", new JSONBool(Banned));
            obj.Add("lastPlayed", new JSONString(lastPlayed.ToString("s")));

            JSONArray perms = new JSONArray();
            foreach (string perm in additionalPermissions)
                perms.Add(new JSONString(perm));

            obj.Add("additionalPermissions", perms);
            return obj;
        }
    }
}