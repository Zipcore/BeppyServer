using System;

namespace BeppyServer.WebServer {
    
    [Serializable]
    public struct ServerStatus {
        public int connections;
        public int maxConnections;
    }
}