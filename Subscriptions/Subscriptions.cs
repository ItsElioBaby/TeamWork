using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TeamWork_Server.Subscriptions
{
    public class Subscriptions
    {
        static GroupDataManager manager;

        public static GroupDataManager Manager { get { return manager; } }

        public static void Initialize(string file)
        {
            DataObject o = new DataObject("NULL", null);
            GroupDataWriter writer = new GroupDataWriter();
            writer.WriteObject(o);
            writer.WriteAway(file);
        }

        public static void Initialize(string oldFile, bool bb)
        {
            manager = new GroupDataManager(oldFile);
        }

        public static DataObject[] GetSubscriptionsWhichEndToday()
        {
            long t = DateTime.UtcNow.ToBinary();
            return manager.Reader.GetObjects(t);
        }

        public static void RemoveSubscription(string name)
        {
            manager.Remove(name);
        }

        public static bool Contains(string name)
        {
            return manager.Contains(name);
        }

        public static bool Contains(DataObject obj)
        {
            return manager.Contains(obj);
        }

        public static void RemoveExpiredSubscriptions()
        {
            DataObject[] obs = GetSubscriptionsWhichEndToday();
            foreach (DataObject o in obs)
                manager.Remove(o);
        }
    }
}
