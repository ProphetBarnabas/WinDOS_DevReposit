using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace RD
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_RD;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[path]"));
            TABLE.Add(new CommandArgumentEntry("[string] -a", false, "[path], -a(remove content if any)"));
            CMD_RD = new Command("RD", TABLE, false, "Removes the directory at the provided relative/full path.", ExecutionLevel.User, CLIMode.Default);
            CMD_RD.SetFunction(() => 
            {
                if (Directory.Exists((string)CMD_RD.InputArgumentEntry.Arguments[0].Value) && CMD_RD.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    Directory.Delete((string)CMD_RD.InputArgumentEntry.Arguments[0].Value, CMD_RD.InputArgumentEntry.Arguments.Exists(x => x.Call == "-a"));
                }
                else if (Directory.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_RD.InputArgumentEntry.Arguments[0].Value))
                {
                    Directory.Delete((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_RD.InputArgumentEntry.Arguments[0].Value, CMD_RD.InputArgumentEntry.Arguments.Exists(x => x.Call == "-a"));
                }
                else
                {
                    return "\nInvalid path!";
                }
                return "";
            });
            return CMD_RD;
        }
    }
}
