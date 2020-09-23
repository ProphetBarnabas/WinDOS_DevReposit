using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace MOTD
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_MOTD;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false));
            CMD_MOTD = new Command("MOTD", TABLE, false, "Sets the text of the message of the day, that is shown on startup if enabled.", ExecutionLevel.User, CLIMode.Default);
            CMD_MOTD.SetFunction(() => 
            {
                if (CMD_MOTD.InputArgumentEntry.Arguments.Count == 0)
                {
                    EnvironmentVariables.ChangeCurrentValue("MOTD", "");
                    EnvironmentVariables.ChangeDefaultValue("MOTD", "");
                }
                else
                {
                    EnvironmentVariables.ChangeCurrentValue("MOTD", CMD_MOTD.InputArgumentEntry.Arguments[0].Value);
                    EnvironmentVariables.ChangeDefaultValue("MOTD", CMD_MOTD.InputArgumentEntry.Arguments[0].Value);
                }
                return "";
            });
            return CMD_MOTD;
        }
    }
}
