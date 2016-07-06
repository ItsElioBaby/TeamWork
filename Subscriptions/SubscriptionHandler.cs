using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using twlib;

namespace TeamWork_Server.Subscriptions
{
    public class SubscriptionHandler
    {
        private UserCommand cmd;

        public SubscriptionHandler(UserCommand c)
        {
            cmd = c;
        }

        private string CreateSub()
        {
            int d = (int)DateTime.UtcNow.ToBinary() * new Random(DateTime.UtcNow.Second).Next();
            return d.ToString("X");
        }

        public object[] Handle()
        {
            List<object> obs = new List<object>();

            if (cmd.Key != long.Parse(CommandsFactory.Variables["admin_master_key"].ToString()))
            {
                obs.Add(false);
                obs.Add("Error: Client's key is different compared with the server's one.");
                return obs.ToArray();
            }

            bool hasArgs = (cmd.Command == "/checksub");
            Log.Info("Handling command '" + cmd.Command + "'" + (hasArgs ? "\n\r->Arguments: " + cmd.Arguments[0] : ""));

            if (cmd.Command == "/checksub")
            {
                if (!Subscriptions.Contains(cmd.Arguments[0]))
                {
                    obs.Add(false);
                    obs.Add("Subscription has not been found.");
                    return obs.ToArray();
                }
                List<DataObject> expired = Subscriptions.GetSubscriptionsWhichEndToday().ToList();
                bool expired_t = false;
                foreach (DataObject o in expired)
                {
                    if (o.ObjectName == cmd.Arguments[0])
                    {
                        expired_t = true;
                    }
                }
                if (expired_t)
                {
                    obs.Add(false);
                    obs.Add("Error: Subscription has expired.");
                    Subscriptions.RemoveSubscription(cmd.Arguments[0]);
                    return obs.ToArray();
                }
                else
                {
                    obs.Add(true);
                    obs.Add("");
                    return obs.ToArray();
                }
            }
                else if (cmd.Command == "/registersub")
                {
                    string s = CreateSub();
                    Subscriptions.Manager.AddObject(new DataObject(s, DateTime.UtcNow.AddDays(30)));
                    Subscriptions.Manager.Save(@".\subscriptions.dg");
                    obs.Add(true);
                    obs.Add(s);
                    return obs.ToArray();
                }
            else if (cmd.Command == "/checkcoder")
            {
                if (CommandsFactory.Variables["admin_coder_key"].ToString() == cmd.Arguments[0])
                {
                    obs.Add(true);
                    obs.Add("Coder key has been vailidated.");
                    return obs.ToArray();
                }
                else
                {
                    obs.Add(false);
                    obs.Add("Coder key is invalid.");
                    return obs.ToArray();
                }
            }
            else
            {
                obs.Add(false);
                obs.Add("Error: Invalid UserCommand sent to the server.");
                return obs.ToArray();
            }
        }
    }
}
