using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace twlib
{
    public class Notification
    {
        private string user;
        private string text;
        private DateTime time;

        public string User { get { return user; } }
        public string Text { get { return text; } }
        public DateTime Time { get { return time; } }

        public Notification(string u, string t, DateTime tim)
        {
            user = u;
            text = t;
            time = tim;
        }

        public override string ToString()
        {
            string data = user + "|" + text + "|" + time.ToBinary().ToString();
            return data;
        }

        public static Notification Parse(string data)
        {
            string[] datas = data.Split('|');
            string user = datas[0];
            string text = datas[1];
            long time = long.Parse(datas[2]);

            return new Notification(user, text, DateTime.FromBinary(time));
        }
    }
}
