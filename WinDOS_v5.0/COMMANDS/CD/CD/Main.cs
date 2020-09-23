using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace CD
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CD;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[path]"));
            TABLE.Add(new CommandArgumentEntry("[string] -s", true, "[path] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -b", true, "[path] -b(save to running and startup)"));
            CMD_CD = new Command("CD", TABLE, false, "Changes the current(and/or startup) directory.", ExecutionLevel.User, CLIMode.Default);
            CMD_CD.SetFunction(() => 
            {
                string path = CMD_CD.InputArgumentEntry.Arguments[0].Value.ToString();
                if (path == "..")
                {
                    if (EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString().Count(x => x == '\\') > 1)
                    {
                        path = EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString();
                        path = path.Remove(path.Length - 1, 1);
                        path = path.Remove(path.LastIndexOf('\\'));
                    }
                    else
                    {
                        path = EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString();
                    }
                }
                else if (path == "\\sdir")
                {
                    path = EnvironmentVariables.GetDefaultValue("DIRECTORY").ToString();
                }
                else if (path == "\\wdir")
                {
                    path = Environment.CurrentDirectory;
                }
                else if ((Directory.Exists(path) && path.Contains(":")) || Directory.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + path))
                {
                    if (Directory.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + path))
                    {
                        path = (string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + path;
                    }
                }
                else
                {
                    return "\nInvalid path!";
                }
                path = path + (path[path.Length - 1] == '\\' ? "" : "\\");
                switch (CMD_CD.InputArgumentEntry.Arguments.Last().Call)
                {
                    case "-s":
                        EnvironmentVariables.ChangeDefaultValue("DIRECTORY", path);
                        break;
                    case "-b":
                        EnvironmentVariables.ChangeDefaultValue("DIRECTORY", path);
                        EnvironmentVariables.ChangeCurrentValue("DIRECTORY", path);
                        break;
                    default:
                        EnvironmentVariables.ChangeCurrentValue("DIRECTORY", path);
                        break;
                }
                return "";
            });
            return CMD_CD;
        }
    }
}
