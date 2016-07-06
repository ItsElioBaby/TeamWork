using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Net;
using System.Net.Sockets;

namespace TeamWork_Server
{
    public class Backforth
    {
        public string Name;

        private StreamReader reader;
        private StreamWriter writer;

        public Backforth(Stream sr)
        {
            reader = new StreamReader(sr);
            writer = new StreamWriter(sr);

            Name = reader.ReadLine();
        }

        int status = -1;

        public void Start()
        {
            if (status == -1)
            {
                new Thread(xrun).Start();
            }
            status = 1;
            Log.Debug("Backforth Initialized.");
        }

        public void Pause()
        {
            status = 0;
        }

        public void Stop()
        {
            status = -1;
        }

        public void SendMessage(string msg)
        {
            writer.WriteLine(msg);
            writer.Flush();
        }

        private void xrun()
        {
            try
            {
                while (true)
                {
                    //if (status == 1)
                    //{
                    string msg = reader.ReadLine();
                    ChatService.BroadcastMessage(Name + ": " + msg);
                    Log.Info(Name + ": " + msg);
                   /* }
                    if (status == -1)
                    {
                        Thread.CurrentThread.Abort();
                    }*/
                }
            }
            catch (Exception) {
                Log.Debug(Name + ": Backforth disconnected.");
                ChatService.clients.Remove(this);
            }
        }
    }
    
    public class ChatService
    {
        TcpListener lis = new TcpListener(IPAddress.Any, 22021);

        public static List<Backforth> clients = new List<Backforth>();

        public static void BroadcastMessage(string msg)
        {
            foreach (Backforth cl in clients)
            {
                cl.SendMessage(msg);
            }
        }

        public static void PrivateMessage(string msg, string username)
        {
            foreach (Backforth cl in clients)
            {
                if (cl.Name == username)
                {
                    cl.SendMessage("PM: " + msg);
                }
            }
        }

        public void Start()
        {
            lis.Start();
            //new Thread(xrun).Start();
            lis.BeginAcceptTcpClient(Accepted, null);
            Log.Debug("Chat Service is online.");
        }

        /*private void xrun()
        {
            while (true)
            {
                if (CommandsFactory.ServiceStats["Chat"])
                {
                    if (lis.Pending())
                    {
                        Log.Info("Pending client connection...");
                        TcpClient tcpcl = lis.AcceptTcpClient();
                        Backforth bf = new Backforth(tcpcl.GetStream());
                        if (!clients.Contains(bf))
                        {
                            Log.Info("Registering client '" + bf.Name + "'.");
                            clients.Add(bf);
                            bf.Start();
                        }
                    }
                }
                else
                {
                    Log.Debug("Chat Service is offline.");
                }
            }
        }*/

        private void Accepted(IAsyncResult tt)
        {
            TcpClient client = lis.EndAcceptTcpClient(tt);
            Backforth bf = new Backforth(client.GetStream());
            if (!clients.Contains(bf))
            {
                Log.Info("Registering client '" + bf.Name + "'.");
                clients.Add(bf);
                bf.Start();
            }
            lis.BeginAcceptTcpClient(Accepted, null);
        }
    }
}
