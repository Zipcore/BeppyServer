using System;
using System.Collections.Generic;
using System.IO;
using SimpleJSON;

/*
 *  PERMISSIONS DOCUMENT EXAMPLE STRUCTURE
 *  
 *  {
 *      "groups": {
 *          "groupName": {
 *              "permissions": [
 *                  "cmdBan", "cmdKick"
 *              ]
 *          }
 *      },
 *      
 *      "users": {
 *          "username": {
 *              "steamId": "steamId",
 *              "group": "groupName",
 *              "additionalPermissions": []
 *          }
 *      }
 *  }
 */

namespace BeppyServer {
    public class PermissionException : Exception
    {
        public PermissionException(string message) : base(message) { }
        public PermissionException(string message, Exception inner) : base(message, inner) { }
    }

    public class PermissionGroup
    {
        public List<string> permissions = new List<string>();

        public static PermissionGroup Deserialize(JSONObject obj)
        {
            PermissionGroup group = new PermissionGroup
            {
                permissions = new List<string>()
            };

            var permissions = obj["permissions"];
            foreach (JSONNode node in permissions)
                group.permissions.Add(node.Value);

            return group;
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

    public class PermissionUser
    {
        public string steamId;
        public string group;
        public List<string> additionalPermissions = new List<string>();

        public static PermissionUser Deserialize(JSONObject obj)
        {
            PermissionUser user = new PermissionUser()
            {
                steamId = obj["steamId"].Value,
                group = obj["group"].Value
            };

            user.additionalPermissions = new List<string>();
            var permissions = obj["additionalPermissions"].AsArray;
            foreach (JSONNode perm in permissions)
                user.additionalPermissions.Add(perm.ToString());

            return user;
        }

        public JSONObject Serialize()
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

    public static class Permissions
    {
        private static Dictionary<string, PermissionGroup> Groups = new Dictionary<string, PermissionGroup>();
        private static Dictionary<string, PermissionUser> Users = new Dictionary<string, PermissionUser>();

        // Loads PermissionDocument from Config.PermissionsFile
        public static void Load()
        {
            if (!File.Exists(Config.PermissionsFile))
                SaveDefaultPermissions(Config.PermissionsFile);

            Groups = new Dictionary<string, PermissionGroup>();
            Users = new Dictionary<string, PermissionUser>();

            string serialized = File.ReadAllText(Config.PermissionsFile);
            Deserialize(serialized);
        }

        public static void Deserialize(string serialized)
        {
            JSONNode rootNode = JSON.Parse(serialized);

            var groups = rootNode["groups"];
            var users = rootNode["users"];

            foreach (var pair in groups.AsObject)
                Groups.Add(pair.Key, PermissionGroup.Deserialize(pair.Value.AsObject));

            foreach (var pair in users.AsObject)
                Users.Add(pair.Key, PermissionUser.Deserialize(pair.Value.AsObject));
        }

        public static string Serialize()
        {
            JSONObject groupObject = new JSONObject();

            foreach (KeyValuePair<string, PermissionGroup> pair in Groups)
                groupObject.Add(pair.Key, pair.Value.Serialize());

            JSONObject userObject = new JSONObject();
            foreach (KeyValuePair<string, PermissionUser> pair in Users)
                userObject.Add(pair.Key, pair.Value.Serialize());

            JSONNode rootNode = new JSONObject();
            rootNode.Add("groups", groupObject);
            rootNode.Add("users", userObject);
            return rootNode.ToString(4);
        }

        public static void SaveDefaultPermissions(string permissionsFile)
        {
            PermissionGroup admin = new PermissionGroup()
            {
                permissions = new List<string>() { "all" }
            };

            PermissionGroup user = new PermissionGroup()
            {
                permissions = new List<string>()
            };

            PermissionUser blankUser = new PermissionUser()
            {
                steamId = "0",
                group = "admin"
            };

            Groups.Add("admin", admin);
            Groups.Add("user", user);
            Users.Add("admin", blankUser);

            Save();
        }

        // Expensive. Do not use unless server is experiencing downtime or is restarting/shuttingdown
        public static void Save()
        {
            try
            {
                File.WriteAllText(Config.PermissionsFile, Serialize());
            }
            catch (Exception e)
            {
                throw new PermissionException("Could not save permissions file.", e);
            }
        }

        public static void AddGroup(string groupName)
        {
            if (Groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} already exists.");

            Groups.Add(groupName, CreateDefaultGroup());
        }

        private static PermissionGroup CreateDefaultGroup()
        {
            PermissionGroup group = new PermissionGroup
            {
                permissions = new List<string>()
            };
            return group;
        }

        public static void CreateNewUser(string name, string steamId, string group = "user", params string[] additionalPermissions)
        {
            PermissionUser user = new PermissionUser
            {
                steamId = steamId,
                group = group,
                additionalPermissions = new List<string>(additionalPermissions)
            };

            Users.Add(name, user);
        }

        public static void AddPermissionToUser(string name, string permission)
        {
            if (!Users.ContainsKey(name))
                throw new PermissionException($"Could not find user {name}");

            if (Users[name] == null)
                Users.Add(name, new PermissionUser());

            if (Users[name].additionalPermissions == null)
            {
                Users[name].additionalPermissions = new List<string>() { permission };
                return;
            }

            Users[name].additionalPermissions.Add(permission);
        }

        public static void AddUserToGroup(string name, string groupName)
        {
            if (!Users.ContainsKey(name))
                throw new PermissionException($"Could not find user {name}");

            Users[name].group = groupName;
        }

        private static string LookupBySteamId(string steamId)
        {
            foreach (KeyValuePair<string, PermissionUser> pair in Users)
                if (pair.Value.steamId.ToLower().Equals(steamId.ToLower()))
                    return pair.Key;

            throw new PermissionException($"Could not find user by steamId {steamId}");
        }

        public static void AddPermissionToGroup(string groupName, string permission)
        {
            if (!Groups.ContainsKey(groupName))
                throw new PermissionException($"Group name \"{groupName}\" does not exist.");

            // Hopefully this never actually happens, but just in case...
            if (Groups[groupName].permissions == null)
            {
                Groups[groupName].permissions = new List<string>() { permission };
                return;
            }

            Groups[groupName].permissions.Add(permission);
        }

        public static bool GroupHasPermission(string groupName, string permission)
        {
            if (!Groups.ContainsKey(groupName))
                return false;

            if (Groups[groupName].permissions == null)
                return false;

            return Groups[groupName].permissions.Contains(permission) || Groups[groupName].permissions.Contains("all");
        }

        public static bool UserHasPermission(string name, string permission)
        {
            if (!Users.ContainsKey(name))
                return false;

            if (Users[name].additionalPermissions != null)
                if (Users[name].additionalPermissions.Contains(permission) || Users[name].additionalPermissions.Contains("all"))
                    return true;

            return GroupHasPermission(Users[name].group, permission);
        }

        public static void RemovePermissionFromGroup(string groupName, string permission)
        {
            if (!Groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            if (Groups[groupName].permissions == null)
                return;

            Groups[groupName].permissions.Remove(permission);
        }

        public static bool IsUserInGroup(string name, string groupName)
        {
            if (!Users.ContainsKey(name))
                throw new PermissionException($"User {name} does not exist.");

            return Users[name].group.ToLower().Equals(groupName.ToLower());
        }

        public static bool DoesUserExist(string name) => Users.ContainsKey(name);

        public static List<string> GetGroupPermissions(string groupName)
        {
            if (!Groups.ContainsKey(groupName))
                throw new PermissionException($"Group {groupName} does not exist.");

            return Groups[groupName].permissions;
        }

        public static List<string> GetUserPermissions(string name)
        {
            if (!Users.ContainsKey(name))
                throw new PermissionException($"User {name} does not exist.");

            return Users[name].additionalPermissions;
        }
    }
}