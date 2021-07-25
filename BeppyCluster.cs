using BepInEx;
using BepInEx.Configuration;

namespace BeppyServer
{
    // For every server that wants to be a part of a cluster of servers
    // we need to allow them to write either a file containing the cluster information
    // or specify runtime arguments

    // In doing so, we want to specify the following:
    //  Server Name
    //  Database Type
    //  Database IP
    //  Database Username
    //  Database Password

    public class BeppyCluster
    {
        public bool IsHandlingPermissions { get; set; }

        private ConfigEntry<string> serverName;
        private ConfigEntry<string> databaseType;
        private ConfigEntry<string> dbIp;
        private ConfigEntry<string> dbUsername;
        private ConfigEntry<string> dbPassword;
        private ConfigEntry<bool> handlePermissions;

        public BeppyCluster(ConfigFile Config)
        {
            serverName = Config.Bind("Cluster", "LocalServerName", "7 Days to Die Server");
            databaseType = Config.Bind("Cluster", "DatabaseType", "MongoDB");
            dbIp = Config.Bind("Cluster", "DatabaseIP", "127.0.0.1:27017");
            dbUsername = Config.Bind("Cluster", "DatabaseUsername", "");
            dbPassword = Config.Bind("Cluster", "DatabasePassword", "");
            handlePermissions = Config.Bind("Cluster", "UseClusterPermissions", false);
        }
    }
}
