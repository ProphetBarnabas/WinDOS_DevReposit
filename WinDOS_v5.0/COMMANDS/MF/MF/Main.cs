using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace MF
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_MF;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[file path]"));
            CMD_MF = new Command("MF", TABLE, true, "Creates a new file at the provided relative or full path.", ExecutionLevel.User, CLIMode.Default);
            CMD_MF.SetFunction(() => 
            {
                string path = CMD_MF.InputArgumentEntry.Arguments[0].Value.ToString();
                path = path.Last() == '\\' ? path.Remove(path.Length - 1, 1) : path;
                string full_path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + path;
                if (Directory.Exists(path.Remove(path.LastIndexOf("\\"))) && path.Contains(":"))
                {
                    File.Create(path).Close();
                }
                else if (Directory.Exists(full_path.Remove(full_path.LastIndexOf("\\"))))
                {
                    File.Create(full_path).Close();
                }
                else
                {
                    return "\nDirectory not found!";
                }
                return "";
            });
            return CMD_MF;
        }
    }
}
