using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Net;
using TeamWork_Server.HTTP;
using twlib;
using System.Threading;

namespace TeamWork_Server.Core
{
    public class RedirectService
    {
        HttpService service;
        FileTransfer fService;
        TWServerConfiguration cfg;

        static public int HB_TIMEOUT = 0x5;

        public RedirectService(string host, TWServerConfiguration config)
        {
            service = new HttpService(host, 55987);
            cfg = config;
            service.HttpContextReceived += service_HttpContextReceived;
            fService = new FileTransfer(config);
            service.HttpContextReceived += fService.service_HttpContextReceived;
            service.HttpContextReceived += fService.cus.service_HttpContextReceived;
        }

        private List<CodeUpdateTemplate> CodeUpdates = new List<CodeUpdateTemplate>();

        public void Start()
        {
            service.Start();
            Log.Debug("Redirection Service is now available!");
        }

        

        private void service_HttpContextReceived(object sender, HttpContextReceivedEventArgs e)
        {
            HttpListenerRequest req = e.Context.Request;

            switch(req.Headers["TYPE"])
            {
                case "HeartBeat":
                    int id = int.Parse(req.Headers["UID"]);
                    User.Users[id].Beat();
                break;

                case "Notification":
                string data = req.Headers["NOTIFICATION_DATA"];
                Notification notification = Notification.Parse(data);
                NotificationArea.Notifications.Add(notification);
                Log.Info("A notification has been submited!");
                break;

                case "NewUser":
                ThreadPool.QueueUserWorkItem(delegate
                {
                    string[] user_credentials = req.Headers["uCred"].Split('|');
                    User u = new User(user_credentials[0], user_credentials[1], user_credentials[2], User.Users.Count + 1);
                    User.Users.Add(u.ID, u);
                    MemoryStream memsr = new MemoryStream();
                    BinaryWriter writer = new BinaryWriter(memsr);
                    writer.Write(u.Name);
                    writer.Write(u.Password);
                    writer.Write(u.Role);
                    writer.Write(u.ID);
                    writer.Flush();

                    byte[] bts = memsr.ToArray();
                    e.Context.Response.OutputStream.Write(bts, 0, bts.Length);
                    Log.Info("New user has been created!");
                    e.Context.Response.Close();
                });
                break;

                case "Login":
                string[] user_details = req.Headers["uCred"].Split('|');
                Log.Info("Trying to login user " + user_details[0]);
                foreach (var v in User.Users.Values)
                {
                    if (v.Name == user_details[0])
                    {
                        if (v.Password == user_details[1])
                        {
                            e.Context.Response.AddHeader("LG_RESULT", "OK");
                            BinaryWriter writer = new BinaryWriter(e.Context.Response.OutputStream);
                            writer.Write(v.ToList().ToArray());
                            writer.Flush();
                        }
                        else
                            e.Context.Response.AddHeader("LG_RESULT", "The specified password was invalid.");
                        goto BREAK;
                    }
                }
                e.Context.Response.AddHeader("LG_RESULT", "Username could not be found. Please register before proceeding.");
            BREAK:
                e.Context.Response.Close();
                break;

                case "ParseNotifications":  // We store notifications as binary-encoded data on a file then parse only the latest ones (3 days old max). This is nasty but works so I don't really care for this at the moment /tehe
                var notificaitons = from n in NotificationArea.Notifications where DateTime.Now.Subtract(n.Time).Days <= 3 select n;
                StringBuilder str = new StringBuilder();
                if (notificaitons.Count() > 0)
                {
                    foreach (var n in notificaitons)
                    {
                        str.AppendLine(n.ToString());
                    }
                }
                else {
                        str.AppendLine("No new notifications in the past 3 days. :/");
                    }
                    StreamWriter sr = new StreamWriter(e.Context.Response.OutputStream);
                    sr.Write(str.ToString());
                    sr.Flush();
                    e.Context.Response.Close();
                 break;

                case "GetInfo":
                    BinaryWriter writer221 = new BinaryWriter(e.Context.Response.OutputStream);
                    writer221.Write(cfg.ProjectName);
                    writer221.Write(cfg.Description);
                    writer221.Write(cfg.Language);
                    writer221.Write(cfg.Version);
                    writer221.Flush();
                    e.Context.Response.Close();
                    break;
            }
        }
    }
}
