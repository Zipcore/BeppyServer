using System;
using System.Collections.Generic;
using SimpleJSON;

namespace BeppyServer {
    public class Player {
        private readonly List<string> additionalPermissions;

        public string SteamId { get; }

        public string Name { get; }

        public string Group { get; private set; }

        public bool Banned;

        public DateTime LastPlayed;
        
        public Player(string steamId, string name, string group, bool banned,
            DateTime lastPlayed, params string[] additionalPermissions) {
            SteamId = steamId;
            Name = name;
            Group = group;
            Banned = banned;
            LastPlayed = lastPlayed;
            this.additionalPermissions = new List<string>(additionalPermissions);
        }

        public void SetGroup(string group) {
            Group = group;
        }

        public void AddPermission(string permission) {
            additionalPermissions.Add(permission);
        }

        public bool HasPermission(string permission) {
            return additionalPermissions.Contains(permission);
        }

        public List<string> GetAdditionalPermissions() => additionalPermissions;
    }
}