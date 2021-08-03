using BepInEx.Configuration;
using BeppyServer.DataSources;

namespace BeppyServer {
    public class Cluster {

        private ConfigEntry<string> clusterServerName;
        private ConfigEntry<string> dbConnectionString; 
        
        
        public Cluster(ConfigFile config) {
            clusterServerName = config.Bind("Cluster", "LocalServerName", "7 Days to Die Server");
            dbConnectionString = config.Bind("Cluster", "DBConnectionString", "");

        }

        public ClusterPermissions InitializePermissions() {
            return new ClusterPermissions(dbConnectionString.Value);
        }
    }
}