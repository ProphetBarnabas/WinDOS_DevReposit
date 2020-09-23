using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace MD
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_MD;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[path]"));

            CMD_MD = new Command("MD", TABLE, false, "Creates a new directory at the provided relative/full path.", ExecutionLevel.User, CLIMode.Default);
            CMD_MD.SetFunction(() =>
            {
                if (CMD_MD.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    Directory.CreateDirectory((string)CMD_MD.InputArgumentEntry.Arguments[0].Value);
                }
                else
                {
                    Directory.CreateDirectory((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_MD.InputArgumentEntry.Arguments[0].Value);
                }
                return "";
            });
            return CMD_MD;
        }
    }
}
