using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace DEL
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_DEL;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[file path]"));

            CMD_DEL = new Command("DEL", TABLE, false, "Removes the file at the provided relative/full path.", ExecutionLevel.User, CLIMode.Default);
            CMD_DEL.SetFunction(() =>
            {
                if (File.Exists((string)CMD_DEL.InputArgumentEntry.Arguments[0].Value) && CMD_DEL.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    File.Delete((string)CMD_DEL.InputArgumentEntry.Arguments[0].Value);
                }
                else if (File.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_DEL.InputArgumentEntry.Arguments[0].Value))
                {
                    File.Delete((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_DEL.InputArgumentEntry.Arguments[0].Value);
                }
                else
                {
                    return "\nFile not found!";
                }
                return "";
            });
            return CMD_DEL;
        }
    }
}
