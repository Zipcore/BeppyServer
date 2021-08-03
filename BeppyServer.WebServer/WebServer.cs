using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using BepInEx;
using BepInEx.Configuration;

namespace BeppyServer.WebServer {
    
    [BepInPlugin("beppyserver.webserver", "BeppyServer.WebServer", "1.0.0.0")]
    [BepInDependency("beppyserver")]
    public class WebServer : BaseUnityPlugin {

        private BeppyServer beppyServer;
        private TcpListener listener;

        private ConfigEntry<int> webserverConnPort;
        private ConfigEntry<string> webserverConnIp;

        private void Awake() {
            beppyServer = FindObjectOfType<BeppyServer>();
            
            webserverConnIp = Config.Bind("WebServer", "LocalAddress", "localhost");
            webserverConnPort = Config.Bind("WebServer", "UniquePort", 25000);

            IPAddress localAddr = IPAddress.Parse(webserverConnIp.Value);
            listener = new TcpListener(localAddr, webserverConnPort.Value);

            try {
                listener.BeginAcceptTcpClient(ConnectionCallback, listener);
            } catch (Exception e) {
                Console.Exception(e);
            }
        }
        
        private static byte[] GetSerializedStatus() {
            ServerStatus status = new ServerStatus {
                connections = WorldManager.World.Players.Count,
                maxConnections = GamePrefs.GetInt(EnumGamePrefs.ServerMaxPlayerCount)
            };
            
            BinaryFormatter bf = new BinaryFormatter();
            using (MemoryStream ms = new MemoryStream()) {
                bf.Serialize(ms, status);
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
            (TcpClient client, byte[] readBytes) = (Tuple<TcpClient, byte[]>) ar.AsyncState;

            try {
                NetworkStream stream = client.GetStream();
                int read = stream.EndRead(ar);
                // TODO: Convert readBytes into structure identical to the WebServer's command
                beppyServer.SendMessage("WebServerCommandReceived", readBytes);
            } catch (Exception e) {
                Console.Exception(e);
            } finally {
                client.Close();
            }
            
        }

        private void OnApplicationQuit() {
            listener.Stop();
        }
    }
}