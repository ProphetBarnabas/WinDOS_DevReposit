using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace CARET
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CARET;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-n", true, "-n(disable caret)"));
            TABLE.Add(new CommandArgumentEntry("-s", false, "-s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-b", false, "-b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-n -s", true, "-n(disable caret) -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-n -b", true, "-n(disable caret) -b(save to running and startup)"));
            CMD_CARET = new Command("CARET", TABLE, false, "Enables/disables the custom caret.", ExecutionLevel.User, CLIMode.Default);
            CMD_CARET.SetFunction(() => 
            {
                if (CMD_CARET.InputArgumentEntry.Arguments.Count == 0)
                {
                    EnvironmentVariables.ChangeCurrentValue("SYS_CARET", true);
                }
                else if (CMD_CARET.InputArgumentEntry.Arguments.Count == 1)
                {
                    switch (CMD_CARET.InputArgumentEntry.Arguments[0].Call)
                    {
                        case "-n":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET", false);
                            break;
                        case "-s":
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET", true);
                            break;
                        case "-b":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET", true);
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET", true);
                            break;
                    }
                }
                else
                {
                    switch (CMD_CARET.InputArgumentEntry.Arguments[1].Call)
                    {
                        case "-s":
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET", false);
                            break;
                        case "-b":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET", false);
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET", false);
                            break;
                    }
                }
                return "";
            });
            return CMD_CARET;
        }
    }
}
