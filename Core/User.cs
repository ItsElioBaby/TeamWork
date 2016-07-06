using System;
using System.Collections.Generic;
using System.IO;

namespace TeamWork_Server.Core
{
    public class User
    {
        public static Dictionary<int, User> Users = new Dictionary<int, User>();

        public static GroupDataManager Manager;

        private int id;
        private string name;
        private string password;
        private string role;
        private bool online;

        private DateTime last_heartBeat;

        public int ID{get{return id;}}
        public string Name { get { return name; } }
        public string Password { get { return password; } }
        public string Role { get { return role; } }
        public bool IsOnline { get { return online; } set { online = value; } }

        public DateTime LastHeartbeat { get { return last_heartBeat; } }

        public User()
        {
            Manager = new GroupDataManager(@".\users.dg");
        }

        public User(string n, string p, string r, int i)
        {
            name = n;
            password = p;
            role = r;
            id = i;
            last_heartBeat = DateTime.Now;
            if(!Users.ContainsKey(id) && !Manager.Contains(id.ToString()))
            {
                Manager.AddObject(new DataObject(i.ToString(), this.ToList()));
                Log.Info("Adding user to storage... (this may take a while)");
                Manager.Save(false);
            }
        }

        public byte[] ToList()
        {
            MemoryStream memsr = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(memsr);

            writer.Write(name);
            writer.Write(password);
            writer.Write(role);
            writer.Write(id);

            writer.Flush();
            return memsr.ToArray();
        }

        public void ChangeName(string nName)
        {
            if (Users.ContainsKey(id))
                Users[id].name = nName;
            this.name = nName;
        }

        public void ChangePassword(string nPassword)
        {
            if (Users.ContainsKey(id))
                Users[id].password = nPassword;
            this.password = nPassword;
        }

        public void ChangeRole(string nRole)
        {
            if (Users.ContainsKey(id))
                Users[id].role = nRole;
            this.role = nRole;
        }

        public void Beat()
        {
            if (Users.ContainsKey(id))
                Users[id].last_heartBeat = DateTime.Now;
            this.last_heartBeat = DateTime.Now;
            Log.Info("Received Heart-Beat message from user ID: " + id.ToString());
        }

        public void Disconnect()
        {
            if (Users.ContainsKey(id))
                Users.Remove(id);
            Log.Info("User with ID: " + id.ToString() + " has been disconnected!");
        }
    }
}
