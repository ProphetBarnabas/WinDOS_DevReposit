using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace CARET_FOOTPRINT
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CARET_FOOTPRINT;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-s", false, "-s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-b", false, "-b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-n", false, "-n(disable footprint)"));
            TABLE.Add(new CommandArgumentEntry("-n -s", false, "-n(disable footprint) -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-n -b", false, "-n(disable footprint) -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-out=[int]", false, "-out=[footprint fade out time>0]"));
            TABLE.Add(new CommandArgumentEntry("-out=[int] -s", true, "-out=[footprint fade out time>0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-out=[int] -b", true, "-out=[footprint fade out time>0] -b(save to running and startup)"));
            CMD_CARET_FOOTPRINT = new Command("CARET-FOOTPRINT", TABLE, false, "Contorls custom caret footprint fade out duration in milliseconds or enables/disables footprint.", ExecutionLevel.User, CLIMode.Default);
            CMD_CARET_FOOTPRINT.SetFunction(() => 
            {
                if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-out"))
                {
                    if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments.Count == 2)
                    {
                        if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments[1].Call == "-s")
                        {
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FOOTPRINT_FADEOUT_SPEED", CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments[1].Value);
                        }
                        else if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments[1].Call == "-b")
                        {
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FOOTPRINT_FADEOUT_SPEED", CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments[1].Value);
                            EnvironmentVariables.SetToDefault("SYS_CARET_FOOTPRINT_FADEOUT_SPEED");
                        }
                    }
                    else
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FOOTPRINT_FADEOUT_SPEED", CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments[0].Value);
                    }
                }
                else if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-n"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FOOTPRINT", false);
                    }
                    else
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FOOTPRINT", true);
                    }
                }
                else if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-n"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FOOTPRINT", false);
                        EnvironmentVariables.SetToDefault("SYS_CARET_FOOTPRINT");
                    }
                    else
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FOOTPRINT", true);
                        EnvironmentVariables.SetToDefault("SYS_CARET_FOOTPRINT");
                    }
                }
                else
                {
                    if (CMD_CARET_FOOTPRINT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-n"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FOOTPRINT", false);
                    }
                    else
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FOOTPRINT", true);
                    }
                }
                return "";
            });
            return CMD_CARET_FOOTPRINT;
        }
    }
}
