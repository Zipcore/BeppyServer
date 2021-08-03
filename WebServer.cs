using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using BepInEx.Configuration;

namespace BeppyServer {

    [Serializable]
    public struct ServerStatus {
        public int connections;
        public int maxConnections;
    }
    
    public class WebServer {

        private TcpListener listener;

        private ConfigEntry<int> webserverConnPort;
        private ConfigEntry<string> webserverConnIp;
        
        public delegate ServerStatus StatusCallback();
        public StatusCallback GetServerStatus;

        public WebServer(ConfigFile config) {
            webserverConnIp = config.Bind("WebServer", "LocalAddress", "localhost");
            webserverConnPort = config.Bind("WebServer", "UniquePort", 25000);
        }

        public void Open() {
            IPAddress localAddr = IPAddress.Parse(webserverConnIp.Value);
            listener = new TcpListener(localAddr, webserverConnPort.Value);

            try {
                listener.BeginAcceptTcpClient(ConnectionCallback, listener);
            } catch (Exception e) {
                Console.Exception(e);
            }
        }

        public void Close() {
            listener.Stop();
        }

        private byte[] GetSerializedStatus() {
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, GetServerStatus());
                return ms.ToArray();
            }
        }
        
        private void ConnectionCallback(IAsyncResult ar) {
            TcpListener state = (TcpListener) ar.AsyncState;
            TcpClient client = state.EndAcceptTcpClient(ar);
            
            // Begin accepting any messages immediately afterwards.
            state.BeginAcceptSocket(ConnectionCallback, listener);
            
            // Begins by writing the status asynchronously
            try {
                NetworkStream stream = client.GetStream();
                byte[] sendBytes = GetSerializedStatus();
                stream.BeginWrite(sendBytes, 0, 256, SendFinishedCallback, client);
            } catch (Exception e) {
                client.Close();
                Console.Exception(e);
            }
        }

        // Is called when the stream finishes writing.
        // Begins recieve callback if nothing failed.
        private void SendFinishedCallback(IAsyncResult ar) {
            TcpClient client = (TcpClient) ar.AsyncState;

            Tuple<TcpClient, byte[]> newState = new Tuple<TcpClient, byte[]>(client, new byte[256]);
            
            try {
                NetworkStream stream = client.GetStream();
                stream.EndWrite(ar);
                stream.BeginRead(newState.Item2, 0, 256, ReceiveCallback, newState);
            } catch (Exception e) {
                client.Close();
                Console.Exception(e);
            }
        }

        // Is called when the stream finishes reading.
        // Ends the stream as a whole.
        private void ReceiveCallback(IAsyncResult ar) {
            Tuple<TcpClient, byte[]> state = (Tuple<TcpClient, byte[]>) ar.AsyncState;
            TcpClient client = state.Item1;
            byte[] readBytes = state.Item2;
            
            // TODO: Convert readBytes into structure identical to the WebServer's command

            try {
                NetworkStream stream = client.GetStream();
                int read = stream.EndRead(ar);
            } catch (Exception e) {
                Console.Exception(e);
            } finally {
                client.Close();
            }
            
        }
    }
}