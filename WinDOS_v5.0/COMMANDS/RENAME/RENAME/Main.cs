using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace RENAME
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_RENAME;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[path] [new name]"));

            CMD_RENAME = new Command("RENAME", TABLE, false, "Renames the file system entry", ExecutionLevel.User, CLIMode.Default);
            CMD_RENAME.SetFunction(() => 
            {
                if (File.Exists((string)CMD_RENAME.InputArgumentEntry.Arguments[0].Value) && CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    File.Move(CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString(), CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString().Replace(Path.GetFileName(CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString()), CMD_RENAME.InputArgumentEntry.Arguments[1].Value.ToString()));
                }
                else if (File.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_RENAME.InputArgumentEntry.Arguments[0].Value))
                {
                    File.Move(EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString(), EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString().Replace(Path.GetFileName(EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString()), CMD_RENAME.InputArgumentEntry.Arguments[1].Value.ToString()));
                }
                else if (Directory.Exists((string)CMD_RENAME.InputArgumentEntry.Arguments[0].Value) && CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    Directory.Move(CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString(), CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString().Replace(new DirectoryInfo(CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString()).Name, CMD_RENAME.InputArgumentEntry.Arguments[1].Value.ToString()));
                }
                else if (Directory.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_RENAME.InputArgumentEntry.Arguments[0].Value))
                {
                    Directory.Move(EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString(), EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString().Replace(new DirectoryInfo(EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_RENAME.InputArgumentEntry.Arguments[0].Value.ToString()).Name, CMD_RENAME.InputArgumentEntry.Arguments[1].Value.ToString()));
                }
                else
                {
                    return "\nInvalid path!";
                }
                return "";
            });
            return CMD_RENAME;
        }
    }
}
