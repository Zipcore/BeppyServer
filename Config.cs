using System;
using System.IO;
using SimpleJSON;

namespace BeppyServer {
    [Serializable]
    public class ClusterServer
    {
        public int port;
        public string ip;

        public JSONObject GetJSON()
        {
            JSONObject thisObject = new JSONObject();
            thisObject.Add("port", new JSONNumber(port));
            thisObject.Add("ip", new JSONString(ip));
            return thisObject;
        }
    }

    public static class Config
    {

        public const string CONFIG_FILENAME = "config.json";
        public const string DEFAULT_PERMISSIONS_FILE = "permissions.json";

        // Forward variables
        public static string PermissionsFile;

        public static ClusterServer[] ClusterServers;

        public static void Load()
        {
            if (!File.Exists(CONFIG_FILENAME))
                SaveDefaultConfig(CONFIG_FILENAME);

            string serialized = File.ReadAllText(CONFIG_FILENAME);
            Deserialize(serialized);
        }

        public static void SaveDefaultConfig(string configFilename)
        {
            PermissionsFile = DEFAULT_PERMISSIONS_FILE;
            ClusterServers = new ClusterServer[0];

            string jsonString = Serialize();
            File.WriteAllText(configFilename, jsonString);
        }

        private static void Deserialize(string serialized)
        {
            JSONNode rootNode = JSON.Parse(serialized);

            PermissionsFile = rootNode["PermissionsFile"].Value;

            var serversArray = rootNode["ClusterServers"].AsArray;
            ClusterServers = new ClusterServer[serversArray.Count];

            int i = 0;
            foreach (JSONNode node in serversArray)
            {
                var serverObj = node.AsObject;
                ClusterServers[i++] = new ClusterServer
                {
                    port = serverObj["port"].AsInt,
                    ip = serverObj["ip"].ToString()
                };
            }
        }

        private static string Serialize()
        {
            JSONNode rootNode = new JSONObject();
            rootNode.Add("PermissionsFile", new JSONString(PermissionsFile));

            JSONArray serverArray = new JSONArray();

            foreach (ClusterServer server in ClusterServers)
                serverArray.Add(server.GetJSON());

            rootNode.Add("ClusterServers", serverArray);
            return rootNode.ToString(4);
        }
    }
}