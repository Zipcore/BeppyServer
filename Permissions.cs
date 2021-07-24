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

        public JSONObject serialize()
        {
            JSONObject groupObject = new JSONObject();

            JSONArray permissions = new JSONArray();
            foreach (string perm in this.permissions)
                permissions.Add(new JSONString(perm));

            groupObject.Add("permissions", permissions);
            return groupObject;
        }
    }

    public class PermissionUser
    {
        public string steamId;
        public string group;
        public List<string> additionalPermissions = new List<string>();

        public PermissionUser(string steamId, string group, List<string> additionalPermissions)
        {
            this.steamId = steamId;
            this.group = group;
            this.additionalPermissions = new List<string>(additionalPermissions);
        }

        public PermissionUser(string steamId, string group, params string[] additionalPermissions)
            : this(steamId, group, new List<string>(additionalPermissions))
        { }

        public static PermissionUser Deserialize(JSONObject obj)
        {
            var steamId = obj["steamId"].Value;
            var group = obj["group"].Value;

            var additionalPermissions = new List<string>();
            var permissions = obj["additionalPermissions"].AsArray;
            foreach (JSONNode perm in permissions)
                additionalPermissions.Add(perm.Value);

            return new PermissionUser(steamId, group, additionalPermissions);
        }

        public JSONObject serialize()
        {
            JSONObject obj = new JSONObject();
            obj.Add("steamId", new JSONString(steamId));
            obj.Add("group", new JSONString(group));

            JSONArray perms = new JSONArray();
            foreach (string perm in additionalPermissions)
                perms.Add(new JSONString(perm));

            obj.Add("additionalPermissions", perms);
            return obj;
        }
    }

    public class Permissions
    {
        private Dictionary<string, PermissionGroup> groups;
        private Dictionary<string, PermissionUser> users;

        private string fileName;

        // Loads PermissionDocument from Config.PermissionsFile
        public Permissions(string fileName)
        {
            groups = new Dictionary<string, PermissionGroup>();
            users = new Dictionary<string, PermissionUser>();

            this.fileName = fileName;
            if (!File.Exists(fileName))
                saveDefaultPermissions(fileName);

            string serialized = File.ReadAllText(fileName);
            deserialize(serialized);
        }

        public void deserialize(string serialized)
        {
            JSONNode rootNode = JSON.Parse(serialized);

            var groups = rootNode["groups"];
            var users = rootNode["users"];

            foreach (var pair in groups.AsObject)
                this.groups.Add(pair.Key, PermissionGroup.Deserialize(pair.Value.AsObject));

            foreach (var pair in users.AsObject)
                this.users.Add(pair.Key, PermissionUser.Deserialize(pair.Value.AsObject));
        }

        public string serialize()
        {
            JSONObject groupObject = new JSONObject();

            foreach (KeyValuePair<string, PermissionGroup> pair in groups)
                groupObject.Add(pair.Key, pair.Value.serialize());

            JSONObject userObject = new JSONObject();
            foreach (KeyValuePair<string, PermissionUser> pair in users)
                userObject.Add(pair.Key, pair.Value.serialize());

            JSONNode rootNode = new JSONObject();
            rootNode.Add("groups", groupObject);
            rootNode.Add("users", userObject);
            return rootNode.ToString(4);
        }

        public void saveDefaultPermissions(string permissionsFile)
        {
            PermissionGroup admin = new PermissionGroup("all");
            PermissionGroup user = new PermissionGroup();
            PermissionUser blankUser = new PermissionUser("0", "admin");

            groups.Add("admin", admin);
            groups.Add("user", user);
            users.Add("admin", blankUser);

            save();
        }

        // Expensive. Do not use unless server is experiencing downtime or is restarting/shuttingdown
        public void save()
        {
            try
            {
                File.WriteAllText(fileName, serialize());
            }
            catch (Exception e)
            {
                throw new PermissionException("Could not save permissions file.", e);
            }
        }

        public void addUser(string name, string steamId, string group, params string[] additionalPermissions)
        {
            users.Add(name, new PermissionUser(steamId, group, additionalPermissions));
        }

        public void addGroup(string groupName)
        {
            if (groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} already exists.");

            groups.Add(groupName, new PermissionGroup());
        }

        public void addPermissionToUser(string name, string permission)
        {
            if (!users.ContainsKey(name) || users[name] == null)
                throw new PermissionException($"Could not find user {name}");

            if (users[name].additionalPermissions == null)
            {
                users[name].additionalPermissions = new List<string>() { permission };
                return;
            }

            users[name].additionalPermissions.Add(permission);
        }

        public void setUserGroup(string name, string groupName)
        {
            if (!users.ContainsKey(name))
                throw new PermissionException($"Could not find user {name}");

            users[name].group = groupName;
        }

        private string lookupBySteamId(string steamId)
        {
            foreach (KeyValuePair<string, PermissionUser> pair in users)
                if (pair.Value.steamId.ToLower().Equals(steamId.ToLower()))
                    return pair.Key;

            throw new PermissionException($"Could not find user by steamId {steamId}");
        }

        public void addPermissionToGroup(string groupName, string permission)
        {
            if (!groups.ContainsKey(groupName))
                throw new PermissionException($"Group name \"{groupName}\" does not exist.");

            groups[groupName].permissions.Add(permission);
        }

        public bool groupHasPermission(string groupName, string permission)
        {
            if (!groups.ContainsKey(groupName))
                return false;

            if (groups[groupName].permissions == null)
                return false;

            return groups[groupName].permissions.Contains(permission)
                || groups[groupName].permissions.Contains("all");
        }

        public bool userHasPermission(string name, string permission)
        {
            if (!users.ContainsKey(name))
                return false;

            if (users[name].additionalPermissions != null)
                if (users[name].additionalPermissions.Contains(permission) || users[name].additionalPermissions.Contains("all"))
                    return true;

            return groupHasPermission(users[name].group, permission);
        }

        public void removePermissionFromGroup(string groupName, string permission)
        {
            if (!groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            groups[groupName].permissions.Remove(permission);
        }

        public bool isUserInGroup(string name, string groupName)
        {
            if (!users.ContainsKey(name))
                throw new PermissionException($"User {name} does not exist.");

            return users[name].group.ToLower().Equals(groupName.ToLower());
        }

        public bool doesUserExist(string name) => users.ContainsKey(name);

        public List<string> getGroupPermissions(string groupName)
        {
            if (!groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            return groups[groupName].permissions;
        }

        public List<string> getUserPermissions(string name)
        {
            if (!users.ContainsKey(name))
                throw new PermissionException($"User {name} does not exist.");

            return users[name].additionalPermissions;
        }
    }
}