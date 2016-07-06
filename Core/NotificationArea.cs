using System;
using System.Collections.Generic;
using twlib;
using System.IO;

namespace TeamWork_Server.Core
{
    public class NotificationArea
    {
        public static List<Notification> Notifications = new List<Notification>();

        public static void DumpNotifications()
        {
            if(Notifications.Count > 0)
            {
                Log.Info("Dumping notifications data...");
                int per = 10 / Notifications.Count;
                Console.WriteLine("Dumping notifications.");
                int c = 0;
                MemoryStream memsr = new MemoryStream();
                BinaryWriter writer = new BinaryWriter(memsr);
                writer.Write(Notifications.Count);
                foreach (var n in Notifications)
                {
                    if (c == per)
                    {
                        Console.Write(".");
                        c = 0;
                    }
                    writer.Write(n.ToString() + "\n");
                    ++c;
                }
                writer.Flush();
                File.WriteAllBytes("notifications.dat", memsr.ToArray());
            }
        }

        public static void ParseNotifications()
        {
            if(File.Exists("notifications.dat"))
            {
                var fa = File.GetLastWriteTime("notifications.dat");
                if (DateTime.Now.Subtract(fa).Days >= 10)
                {
                    Log.Info("Notifications are now old. Deleting them...");
                    File.Delete("notifications.dat");
                    return;
                }

                string[] nots = File.ReadAllLines("notifications.dat");
                foreach(var n in nots)
                {
                    Notifications.Add(Notification.Parse(n));
                }
                Log.Info("Parsed notifications");
            }
        }
    }
}
