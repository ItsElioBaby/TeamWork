using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.IO;

namespace TeamWork_Server.HTTP
{
    public class FileTransfer
    {
        TWServerConfiguration config;
        public CodeUpdateService cus;

        public FileTransfer(TWServerConfiguration sconfig)
        {
            config = sconfig;
            cus = new CodeUpdateService();
        }

        public static string GetMD5Hash(string file)
        {
            byte[] data = File.ReadAllBytes(file);
            byte[] hData;
            string retval = "";
            using (MD5 md5 = MD5.Create())
                hData = md5.ComputeHash(data);
            foreach (byte b in hData)
                retval += b.ToString("X");
            return retval;
        }

        public static string[] GetFileNames(string projPath)
        {
            List<string> retvals = new List<string>();
            foreach (string file in Directory.GetFiles(projPath.Trim('\r')))
            {
                retvals.Add(file);
            }

            foreach (string dir in Directory.GetDirectories(projPath.Trim('\r')))
            {
                foreach (string file in Directory.GetFiles(dir))
                {
                    retvals.Add(file);
                }
            }
            return retvals.ToArray();
        }

        private static string GetFileNameForProject(string path, string pPath)
        {
            return path.Replace(pPath.Trim('\r'), "").Trim('\\');
        }

        private static string[] GetProjectFilesForExport(TWServerConfiguration c)
        {
            List<string> retvals = new List<string>();
            foreach (string f in GetFileNames(c.FolderPath))
            {
                retvals.Add(GetFileNameForProject(f, c.FolderPath) + ":" + GetMD5Hash(f));
            }
            return retvals.ToArray();
        }

        public void service_HttpContextReceived(object sender, HttpContextReceivedEventArgs e)
        {
            try
            {
                StreamReader reader = new StreamReader(e.Context.Request.InputStream);
                string l = reader.ReadLine();
                if(e.Context.Request.Headers["TYPE"] == "GetSource")
                {
                    BinaryWriter writer = new BinaryWriter(e.Context.Response.OutputStream);
                    if (File.Exists("stable\\source.zip"))
                    {
                        byte[] dat = File.ReadAllBytes("stable\\source.zip");
                        writer.Write(dat.Length);
                        writer.Write(dat);
                    }
                    else
                    {
                        writer.Write(new byte[] { });
                    }
                    writer.Flush();
                }

                if (e.Context.Request.Headers["TYPE"] == "GetFiles")
                {
                    string[] files = GetProjectFilesForExport(config);
                    MemoryStream memsr = new MemoryStream();
                    using (BinaryWriter writer = new BinaryWriter(memsr))
                    {
                        writer.Write(files.Length);
                        foreach (string f in files)
                            writer.Write(f);
                        writer.Flush();
                    }
                    byte[] data = memsr.ToArray();
                    e.Context.Response.OutputStream.Write(data, 0, data.Length);
                    Log.Info("Sending File Names to client.");
                }

                if (e.Context.Request.Headers["TYPE"] == "Download")
                {
                    Dictionary<string, string> vals = new Dictionary<string, string>();
                    foreach(string file in GetFileNames(config.FolderPath))
                    {
                        vals.Add(GetFileNameForProject(file, config.FolderPath), file);
                    }
                    if(vals.ContainsKey(e.Context.Request.Headers["FileName"]))
                    {
                        byte[] data = File.ReadAllBytes(vals[e.Context.Request.Headers["FileName"]]);
                        e.Context.Response.AddHeader("Result", "OK");
                        e.Context.Response.OutputStream.Write(data, 0, data.Length);
                    }
                    e.Context.Response.AddHeader("Result", "File not found.");
                }
                e.Context.Response.Close();
            }
            catch (Exception ex) {
            }
        }
    }
}
