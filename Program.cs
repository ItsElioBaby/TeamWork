using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TeamWork_Server.HTTP;
using System.IO;
using TeamWork_Server.Core;
using TeamWork_Server.Subscriptions;
using System.Threading;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using ICSharpCode.SharpZipLib.Zip;

namespace TeamWork_Server
{
    class Program
    {
        static string GetData(string p)
        {
            string[] ff = File.ReadAllLines(@".\config.ini");
            foreach (string f in ff)
            {
                if(f.Contains(p))
                    return f.Replace(p + "=", "").Replace('"', '\0').Trim('\0');
            }
            return "";
        }

        static bool CheckLastBeat(DateTime lBeat)
        {
            if (DateTime.Now.Subtract(lBeat).Seconds > 10)
            {
                return false;
            }
            return true;
        }

        [DllImport("user32")]
        public static extern bool GetAsyncKeyState(int vkey);

        static void Check()
        {
            while(true)
            {
                for(int i = 0; i < User.Users.Count; i++)
                {
                    if (User.Users[i] != null)
                    {
                        if (!CheckLastBeat(User.Users[i].LastHeartbeat))
                        {
                            if (User.Users[i].IsOnline)
                            {
                                User.Users[i].IsOnline = false;
                                Log.Info(User.Users[i].Name + " missed a heart-beat. Setting user status to offline.");
                            }
                        }
                    }
                }
                if(GetAsyncKeyState((int)Keys.F3))
                {
                    Log.Info("Setting new stable release...");
                    FastZip fz = new FastZip();
                    fz.CreateZip("stable\\source.zip", config.FolderPath.TrimEnd('\r'), true, "");
                }
                Thread.Sleep(100);
            }
        }

        static void SendHeartbeat()
        {
            while(true)
            {
                MasterTool.SendHeartbeat(false);
                Thread.Sleep(new TimeSpan(0, 2, 30));
            }
        }

        static TWServerConfiguration config;

        // Declare the SetConsoleCtrlHandler function
        // as external and receiving a delegate.
        [DllImport("Kernel32")]
        public static extern bool SetConsoleCtrlHandler(HandlerRoutine Handler, bool Add);

        // A delegate type to be used as the handler routine 
        // for SetConsoleCtrlHandler.
        public delegate bool HandlerRoutine(CtrlTypes CtrlType);

        // An enumerated type for the control messages
        // sent to the handler routine.
        public enum CtrlTypes
        {
            CTRL_C_EVENT = 0,
            CTRL_BREAK_EVENT,
            CTRL_CLOSE_EVENT,
            CTRL_LOGOFF_EVENT = 5,
            CTRL_SHUTDOWN_EVENT
        }

        private static bool ConsoleCtrlCheck(CtrlTypes ctrlType)
        {
            Log.Info("Dumping users data... (this may take a while)");
            User.Manager.Save(true);
            NotificationArea.DumpNotifications();
            Log.Info("Quitting server...");
            return true;
        }

        private static int cVersion = 0x20155;

        [STAThread]
        static void Main(string[] args)
        {
            SetConsoleCtrlHandler(new HandlerRoutine(ConsoleCtrlCheck), true);
            //Application.EnableVisualStyles();
            //Application.Run(new Server());
            Log.Initialize("TeamWork Server.txt", LogLevel.All, false);
            /*Subscriptions:
            if (File.Exists(@".\subscriptions.dg"))
            {
                Subscriptions.Subscriptions.Initialize(@".\subscriptions.dg", false);
                Log.Info("Subscriptions file found! Initialized Subscriptions data grouper.");
            }
            else
            {
                Log.Info("Subscriptions file not found! Creating a new one and retyring...");
                Subscriptions.Subscriptions.Initialize(@".\subscriptions.dg");
                Log.Info("Subscription storage has been created. Press any key to continue...");
                Console.ReadKey();
                Console.Clear();
                goto Subscriptions;
            }*/
            CommandsFactory.Initialize();
            Console.Title = "TeamWork Server v0.2b";
            CommandsFactory.Variables["code_dump"] = GetData("AutoCodeDump");
            long rp = long.Parse(GetData("RconPassword"));
            Log.Debug("Using RCON Password: " + rp.ToString());

           

            //Fill in the users data...
            Thread.Sleep(100);
            User u = new User();
            Log.Info("Parsing users, this may take a while...");
            DataObject[] users = User.Manager.Reader.GetObjects();
            if(users != null && users.Length > 1)
            {
                foreach (var user in users)
                {
                    if(user.ObjectValue != null && !User.Users.ContainsKey(int.Parse(user.ObjectName)))
                    {
                        MemoryStream memsr = new MemoryStream((byte[])user.ObjectValue);
                        BinaryReader reader = new BinaryReader(memsr);
                        string nick = reader.ReadString();
                        string pass = reader.ReadString();
                        string role = reader.ReadString();
                        int id = reader.ReadInt32();

                        User.Users.Add(id, new User(nick, pass, role, id));
                    }
                }
            }

            TWServerConfiguration c = new TWServerConfiguration(@GetData("StartupProject"));
            config = c;
            if (!File.Exists("stable\\source.zip"))
            {
                FastZip fz = new FastZip();
                fz.CreateZip("stable\\source.zip", config.FolderPath.TrimEnd('\r'), true, "");
                Log.Info("Added stable source archive...");
            }
            CommandsFactory.Variables["admin_master_key"] = long.Parse(c.MasterKey);
            RedirectService redirectService = new RedirectService("+", c);
            redirectService.Start();
            //RConHandler rch = new RConHandler(rp, c);
            //rch.Start();
            //SubscriptionService ss = new SubscriptionService();
            //ss.Start();
            ChatService cs = new ChatService();
            cs.Start();
            Thread t = new Thread(Check);
            t.IsBackground = true;
            t.Start();
            CredentialManager.GetAvailableRoles();
            NotificationArea.ParseNotifications();
            if(MasterTool.Authenticate(cVersion, c))
            {
                Thread t2 = new Thread(SendHeartbeat);
                t2.IsBackground = true;
                t2.Start();
            }
            ListFileNames(c);
            while (true)
            {
                Log.WriteAway();
            }
        }
    }
}
