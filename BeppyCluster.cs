using System.Collections.Generic;
using SimpleJSON;

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

        private string serverName;
        private string databaseType;
        private string dbIp;
        private string dbUsername;
        private string dbPassword;

        public void LoadFromFile(string fileName)
        {

        }
    }
}
