using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace REGEDIT
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_REGEDIT;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-s", false, "-s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-b", false, "-b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-ns", false, "-ds(change CLI mode to default in startup)"));
            TABLE.Add(new CommandArgumentEntry("-nb", false, "-ds(change CLI mode to default in running and startup)"));
            CMD_REGEDIT = new Command("REGEDIT", TABLE, false, "Sets CLI mode to 'Regedit'.", ExecutionLevel.Administrator, CLIMode.Default);
            CMD_REGEDIT.SetFunction(() =>
            {
                if (CMD_REGEDIT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    EnvironmentVariables.ChangeDefaultValue("CLI_MODE", "Regedit");
                }
                else if (CMD_REGEDIT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    EnvironmentVariables.ChangeDefaultValue("CLI_MODE", "Regedit");
                    EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Regedit");
                }
                else if (CMD_REGEDIT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-ns"))
                {
                    EnvironmentVariables.ChangeDefaultValue("CLI_MODE", "Default");
                }
                else if (CMD_REGEDIT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-nb"))
                {
                    EnvironmentVariables.ChangeDefaultValue("CLI_MODE", "Default");
                    EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Default");
                }
                else
                {
                    EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Regedit");
                }
                return "";
            });
            return CMD_REGEDIT;
        }
    }
}
