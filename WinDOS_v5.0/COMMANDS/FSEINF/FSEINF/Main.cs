using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace FSEINF
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_FSEINF;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[path]"));
            CMD_FSEINF = new Command("FSEINF", TABLE, false, "Returns information about the specified file system entry.", ExecutionLevel.User, CLIMode.Default);
            CMD_FSEINF.SetFunction(() =>
            {
                string in_path = CMD_FSEINF.InputArgumentEntry.Arguments[0].Value.ToString();
                string in_full_path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + in_path;
                object inf = null;
                if (File.Exists(in_path) && in_path.Contains(":"))
                {
                    inf = new FileInfo(in_path);
                    return $"\nName: {((FileInfo)inf).Name}\nDate created: {((FileInfo)inf).CreationTime}\nDate modified: {((FileInfo)inf).LastWriteTime}\nDate accessed: {((FileInfo)inf).LastAccessTime}\nParent directory: {((FileInfo)inf).DirectoryName}\nAttributes: {((FileInfo)inf).Attributes}\nExtension: {((FileInfo)inf).Extension}\nRead only: {((FileInfo)inf).IsReadOnly}";
                }
                else if (File.Exists(in_full_path))
                {
                    inf = new FileInfo(in_full_path);
                    return $"\nName: {((FileInfo)inf).Name}\nDate created: {((FileInfo)inf).CreationTime}\nDate modified: {((FileInfo)inf).LastWriteTime}\nDate accessed: {((FileInfo)inf).LastAccessTime}\nParent directory: {((FileInfo)inf).DirectoryName}\nAttributes: {((FileInfo)inf).Attributes}\nExtension: {((FileInfo)inf).Extension}\nRead only: {((FileInfo)inf).IsReadOnly}";
                }
                else if (Directory.Exists(in_path) && in_path.Contains(":"))
                {
                    inf = new DirectoryInfo(in_path);
                    return $"\nName: {((DirectoryInfo)inf).Name}\nDate created: {((DirectoryInfo)inf).CreationTime}\nDate modified: {((DirectoryInfo)inf).LastWriteTime}\nDate accessed: {((DirectoryInfo)inf).LastAccessTime}\nParent directory: {((DirectoryInfo)inf).Parent}\nAttributes: {((DirectoryInfo)inf).Attributes}\nExtension: {((DirectoryInfo)inf).Extension}";
                }
                else if (Directory.Exists(in_full_path))
                {
                    inf = new FileInfo(in_full_path);
                    return $"\nName: {((DirectoryInfo)inf).Name}\nDate created: {((DirectoryInfo)inf).CreationTime}\nDate modified: {((DirectoryInfo)inf).LastWriteTime}\nDate accessed: {((DirectoryInfo)inf).LastAccessTime}\nParent directory: {((DirectoryInfo)inf).Parent}\nAttributes: {((DirectoryInfo)inf).Attributes}\nExtension: {((DirectoryInfo)inf).Extension}";
                }
                else
                {
                    return "\nFile system entry not found!";
                }
            });
            return CMD_FSEINF;
        }
    }
}
