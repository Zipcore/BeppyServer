using System;
using System.Collections.Generic;
using SimpleJSON;

namespace BeppyServer
{
    public class Player
    {
        private string steamId;
        private string name;
        private string group;
        private List<string> additionalPermissions;
        private bool banned = false;
        private DateTime lastPlayed;

        public string SteamId { get { return steamId; } }
        public string Name { get { return name; } }
        public string Group { get { return group; } }
        public bool Banned { get { return banned; } }
        public DateTime LastPlayed { get { return lastPlayed; } }

        public Player(string steamId, string name, string group, List<string> additionalPermissions)
        {
            this.steamId = steamId;
            this.name = name;
            this.group = group;
            this.additionalPermissions = additionalPermissions;
        }

        public Player(string steamId, string name, string group, params string[] additionalPermissions)
            : this(steamId, name, group, new List<string>(additionalPermissions))
        { }

        public void SetGroup(string group)
        {
            this.group = group;
        }

        public void AddPermission(string permission) => additionalPermissions.Add(permission);
        public bool HasPermission(string permission) => additionalPermissions.Contains(permission);

        public static Player Deserialize(JSONObject obj)
        {
            var name = obj["name"].Value;
            var steamId = obj["steamId"].Value;
            var group = obj["group"].Value;
            var banned = obj["banned"].AsBool;
            var lastPlayed = obj["lastPlayed"].Value;

            var additionalPermissions = new List<string>();
            var permissions = obj["additionalPermissions"].AsArray;
            foreach (JSONNode perm in permissions)
                additionalPermissions.Add(perm.Value);

            if (!DateTime.TryParse(lastPlayed, out DateTime dateLastPlayed))
                dateLastPlayed = DateTime.Now;

            Player p = new Player(steamId, name, group, additionalPermissions)
            {
                banned = banned,
                lastPlayed = dateLastPlayed
            };

            return p;
        }

        public JSONObject Serialize()
        {
            JSONObject obj = new JSONObject();
            obj.Add("steamId", new JSONString(steamId));
            obj.Add("name", new JSONString(name));
            obj.Add("group", new JSONString(group));
            obj.Add("banned", new JSONBool(banned));
            obj.Add("lastPlayed", new JSONString(lastPlayed.ToString("s")));

            JSONArray perms = new JSONArray();
            foreach (string perm in additionalPermissions)
                perms.Add(new JSONString(perm));

            obj.Add("additionalPermissions", perms);
            return obj;
        }
    }
}
