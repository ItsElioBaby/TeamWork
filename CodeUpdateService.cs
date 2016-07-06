using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using TeamWork_Server.HTTP;
using TeamWork_Server.Core;

namespace TeamWork_Server
{

    public class CodeUpdateTemplate
    {
        public string Author;
        public long Date;
        public string File;
        public string Code;
    }

    public class CodeUpdateService
    {
        public CodeUpdateService()
        {
        }

        private string RemoveControlCharacters(string inString)
        {
            if (inString == null) return null;

            StringBuilder newString = new StringBuilder();
            char ch;

            for (int i = 0; i < inString.Length; i++)
            {

                ch = inString[i];

                if (!char.IsControl(ch))
                {
                    newString.Append(ch);
                }
            }
            return newString.ToString();
        }

        private List<CodeUpdateTemplate> CodeUpdates = new List<CodeUpdateTemplate>();

        private void DumpCodeUpdates()
        {
            if ((int)CommandsFactory.Variables["code_dump"] == 1)
            {
                Task.Run(delegate
                {
                    Log.Info("Dumping code updates... (this may take a while)");
                    FileStream fs = File.Open("code_updates.tw", FileMode.OpenOrCreate);
                    BinaryWriter writer = new BinaryWriter(fs);
                    writer.Write(CodeUpdates.Count);
                    foreach (var v in CodeUpdates)
                    {
                        writer.Write(v.Author);
                        writer.Write(v.Date);
                        writer.Write(v.File);
                        writer.Write(v.Code);
                    }
                    writer.Flush();
                    fs.Close();
                });
            }
        }

        public void service_HttpContextReceived(object sender, HttpContextReceivedEventArgs e)
        {
            if (e.Context.Request.Headers["packet_type"] == "CodeSubmit")
            {
                int id = int.Parse(e.Context.Request.Headers["UID"]);
                BinaryWriter writer = new BinaryWriter(e.Context.Response.OutputStream);
                if(CredentialManager.HasPermission(Credentials.CanSubmitUpdates, User.Users[id].Role))
                {
                    BinaryReader reader = new BinaryReader(e.Context.Request.InputStream);
                    CodeUpdateTemplate temp = new CodeUpdateTemplate();
                    temp.Author = reader.ReadString();
                    temp.Date = reader.ReadInt64();
                    temp.File = reader.ReadString();
                    temp.Code = reader.ReadString();

                    CodeUpdates.Add(temp);

                    Log.Info("Received a new code update for file '" + temp.File + "'.");
                    ChatService.BroadcastMessage("[SERVER]: Received a new code update for file '" + temp.File + "', from: " + temp.Author + ".");
                    writer.Write(true);
                    DumpCodeUpdates();
                }
                else
                {
                    writer.Write(false);
                    writer.Write("User has not sufficient permissions to access this feature.");
                }
                writer.Flush();
                e.Context.Response.Close();
            }
            if (e.Context.Request.Headers["packet_type"] == "CodeReview")
            {
                int id = int.Parse(e.Context.Request.Headers["UID"]);
                BinaryWriter writer = new BinaryWriter(e.Context.Response.OutputStream);
                if (!CredentialManager.HasPermission(Credentials.CanDownload, User.Users[id].Role))
                {
                    writer.Write(false);
                    writer.Write("User has not sufficient permissions to access this feature.");
                }
                else
                {
                    string fName = e.Context.Request.Headers["file_name"];
                    Log.Info("Sending modification entries for file " + fName);
                    var updates = from c in CodeUpdates.ToArray() where c.File == fName orderby c.Date select c;
                    writer.Write(true);
                    writer.Write(updates.Count());
                    foreach (var v in updates)
                    {
                        writer.Write(v.Author);
                        writer.Write(v.Date);
                        writer.Write(v.File);
                        writer.Write(v.Code);
                    }
                }
                writer.Flush();
                e.Context.Response.Close();

            }
        }
    }
}
