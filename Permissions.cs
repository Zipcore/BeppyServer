using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UniLinq;

namespace BeppyServer {
    public class PermissionException : Exception {
        public PermissionException(string message) : base(message) { }
        public PermissionException(string message, Exception inner) : base(message, inner) { }
    }

    public class PermissionGroup {
        public List<string> Permissions;

        public PermissionGroup(params string[] perms) {
            Permissions = new List<string>(perms);
        }
    }

    public class Permissions {
        public Dictionary<string, PermissionGroup> Groups;
        public List<Player> Players;

        public Permissions() {
            Groups = new Dictionary<string, PermissionGroup>();
            Players = new List<Player>();
        }
        
        public Player GetPlayerByName(string name) {
            return Players.First(p => p.Name.EqualsCaseInsensitive(name));
        }

        public Player GetPlayerBySteamId(string steamId) {
            return Players.First(p => p.SteamId.EqualsCaseInsensitive(steamId));
        }

        [NotNull]
        public PermissionGroup GetPlayerGroup(Player player) {
            return Groups.First(p => p.Key.EqualsCaseInsensitive(player.Group)).Value;
        }
        
        public void AddPlayer(Player player) {
            Players.Add(player);
        }

        public void AddGroup(string groupName) {
            if (Groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} already exists.");

            Groups.Add(groupName, new PermissionGroup());
        }

        public void AddPermissionToGroup(string groupName, string permission) {
            if (!Groups.ContainsKey(groupName))
                throw new PermissionException($"Group name \"{groupName}\" does not exist.");

            Groups[groupName].Permissions.Add(permission);
        }

        public bool GroupHasPermission(string groupName, string permission) {
            if (!Groups.ContainsKey(groupName))
                return false;

            if (Groups[groupName].Permissions == null)
                return false;

            return Groups[groupName].Permissions.Contains(permission)
                   || Groups[groupName].Permissions.Contains("all");
        }

        public void RemovePermissionFromGroup(string groupName, string permission) {
            if (!Groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            Groups[groupName].Permissions.Remove(permission);
        }

        public List<string> GetGroupPermissions(string groupName) {
            if (!Groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            return Groups[groupName].Permissions;
        }

        public List<Player> GetPlayersInGroup(string groupName) {
            return Players.FindAll(p => p.Group.EqualsCaseInsensitive(groupName));
        }
    }
}