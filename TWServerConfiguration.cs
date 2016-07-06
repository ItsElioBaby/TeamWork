using twlib.Projects;
using System.IO;

namespace TeamWork_Server
{
    public class TWServerConfiguration
    {
        private ProjectFile projFile;

        public TWServerConfiguration(string file)
        {
            Log.Info("Using TWPROJ configuration file:\n\r     ->" + Path.GetFileNameWithoutExtension(file));
            projFile = new ProjectFile(file);
        }

        public string GetFileName(string path)
        {
            string[] paths = path.Split('\\');
            return paths[paths.Length - 1];
        }

       /* public Dictionary<string, string> Files
        {
            get
            {
                Dictionary<string, string> l = new Dictionary<string,string>();
                foreach(string f in projFile.GetFileNames())
                {
                    l.Add(GetFileName(f), f);
                }
                return l;
            }
        }*/

        public string[] Authors { get { return projFile.Authors; } }
        public string Website { get { return projFile.Website; } }
        public string Description { get { return projFile.Description.Trim(); } }
        public string Language { get { return projFile.Language.Trim(); } }
        public string ProjectName { get { return projFile.ProjectName.Trim(); } }
        public string MasterKey { get { return projFile.MasterKey; } }
        public string[] Files { get { return projFile.GetFileNames(); } }
        public string FolderPath { get { return projFile.GetData("Folder")[0]; } }
        public string Version { get { return projFile.GetData("Version")[0]; } }

        public string GetFileText(string fname)
        {
            foreach (string f in projFile.GetFileNames())
            {
                if (GetFileName(f).Trim() == fname.Trim())
                {
                    return File.ReadAllText(f.Trim());
                }
            }
            return "ERROR: NO SUCH FILE EXISTS IN THE SERVER.";
        }
    }
}
