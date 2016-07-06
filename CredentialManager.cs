using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace TeamWork_Server
{
    public enum Credentials : byte
    {
        CanUpload,
        CanDownload,
        CanModifySettings,
        CanSubmitUpdates,
        CanModifyUsers,
        CanBan,
        CanPostNews,
        CanUseChat,
        Invalid
    }

    public class Role
    {
        public string Name;
        public Credentials[] Priviledges;

        public Role(string name, Credentials[] prv)
        {
            Name = name;
            Priviledges = prv;
        }
    }

    public class CredentialManager
    {
        public static string Version = "v1";

        public static Credentials TranslateCredentials(string cred)
        {
            switch (cred.ToLower())
            {
                case "canupload":
                    return Credentials.CanUpload;
                case "candownload":
                    return Credentials.CanDownload;
                case "canmodifysettings":
                    return Credentials.CanModifySettings;
                case "cansubmitupdates":
                    return Credentials.CanSubmitUpdates;
                case "canmodifyusers":
                    return Credentials.CanModifyUsers;
                case "canban":
                    return Credentials.CanBan;
                case "canpostnews":
                    return Credentials.CanPostNews;
                case "canusechat":
                    return Credentials.CanUseChat;
                default:
                    return Credentials.Invalid;
            }
        }

        private static List<Role> Roles = new List<Role>();

        public static bool IsRolePresent(string name)
        {
            foreach (var v in Roles)
                if (v.Name == name)
                    return true;
            return false;
        }

        public static void AddRole(Role role)
        {
            Roles.Add(role);
        }

        public static Role FindRole(string name)
        {
            var v = from i in Roles where i.Name == name select i;
            return v.First();
        }

        public static bool HasPermission(Credentials cred, string cRole)
        {
            if (!IsRolePresent(cRole))
                return false;
            Role cr = FindRole(cRole);

            return cr.Priviledges.Contains(cred);
        }

        public static Role[] GetAvailableRoles()
        {
            List<Role> cRoles = new List<Role>();
            if (Directory.Exists("credentials"))
            {
                foreach (var file in Directory.GetFiles("credentials"))
                {
                    string[] lines = File.ReadAllLines(file);
                    if (lines[0].Replace("@", "") != Version)
                    {
                        Log.Error("Invalid version schema for file: " + Path.GetFileName(file));
                        continue;
                    }

                    if (lines[2].Length > 0)
                    {
                        Log.Error("Invalid schema format for file: " + Path.GetFileName(file));
                        continue;
                    }

                    List<Credentials> priv = new List<Credentials>();

                    if (lines[3].Contains(','))
                    {
                        string[] creds = lines[2].Split(',');
                        foreach (var c in creds)
                            priv.Add(TranslateCredentials(c));
                    }
                    else
                    {
                        priv.Add(TranslateCredentials(lines[3]));
                    }
                    string name = lines[1].Split(':')[1];
                    if (IsRolePresent(name))
                        continue;
                    Role r = new Role(name, priv.ToArray());
                    cRoles.Add(r);
                    Log.Info("Parsed role \"" + r.Name + "\"");
                }
            }
            Roles.AddRange(cRoles);
            return cRoles.ToArray();
        }
    }
}
