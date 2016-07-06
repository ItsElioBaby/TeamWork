// AWAIT CLIENT CONNECTION IN ORDER TO RECEIVE INFO REGARDING THE SERVER.

using System;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TeamWork_Server.Core
{
    public class MasterTool
    {
        private static string ip = "127.0.0.1";
        private static int port = 44588;
        private static UdpClient connectionClient;
        private static bool connected = false;

        public static int TotalViews = 0;
        public static int TotalForks = 0;


        public static bool Authenticate(int version, TWServerConfiguration config)
        {
            connectionClient = new UdpClient();
            connectionClient.Client.SendBufferSize = 65507;
            connectionClient.Client.ReceiveBufferSize = 65507;
            try
            {
                connectionClient.Connect(ip, port);
                connected = true;

                // Submit server details.

                IPEndPoint ipend = new IPEndPoint(IPAddress.Any, 0);
                MemoryStream memsr = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(memsr);
                writer.Write((byte)0x1B);
                writer.Write(version);
                writer.Write(config.ProjectName.Trim());
                writer.Write(config.Description.Trim());
                writer.Write(config.Language.Trim());
                writer.Write(User.Users.Count);
                writer.Flush();

                byte[] data = memsr.ToArray();
                connectionClient.Send(data, data.Length);

                byte[] ok = connectionClient.Receive(ref ipend);
                if (ok[0] == 0x1)
                {
                    Log.Info("Connected to master server...");
                    return true;
                }
                else if (ok[0] == 0x00)
                {
                    Log.Error("Invalid server version.");
                }
                return false;
            }
            catch(Exception)
            {
                Log.Error("Couldn't reach master server. Your project is not visible online. :(");
                return true;
            }
        }

        private static void sendHeartbeat(object includeStatus)
        {
            if(connected)
            {
                bool inc = (bool)includeStatus;
                MemoryStream memsr = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(memsr);
                writer.Write((byte)0x1A); // Heartbeat.
                writer.Write(inc);
                if (inc)
                {
                    writer.Write(User.Users.Count);
                    writer.Write(TotalViews);
                    writer.Write(TotalForks);
                }
                writer.Flush();
                byte[] data = memsr.ToArray();
                connectionClient.Send(data, data.Length);
                //Log.Info("Sending heartbeat to master server...");
            }
        }

        public static void SendHeartbeat(bool includeStatus)
        {
            Thread t = new Thread(sendHeartbeat);
            t.IsBackground = true;
            t.Start(includeStatus);
        }
    }
}
