using CLIShell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FULLSCREEN
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_FULLSCREEN;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-w", false, "-w(windowed)"));
            TABLE.Add(new CommandArgumentEntry("-s", false, "-s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-b", false, "-b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-w -s", false, "-w(windowed) -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-w -b", false, "-w(windowed) -b(save to running and startup)"));
            CMD_FULLSCREEN = new Command("FULLSCREEN", TABLE, false, "Switches between fullscreen and windowed mode.", ExecutionLevel.User, CLIMode.Default);
            CMD_FULLSCREEN.SetFunction(() => 
            {
                if (CMD_FULLSCREEN.InputArgumentEntry.Arguments.Count == 0)
                {
                    EnvironmentVariables.ChangeCurrentValue("FULLSCREEN", true);
                    return "";
                }
                switch (CMD_FULLSCREEN.InputArgumentEntry.Arguments[0].Call)
                {
                    case "-w":
                        if (CMD_FULLSCREEN.InputArgumentEntry.Arguments.Count == 2)
                        {
                            switch (CMD_FULLSCREEN.InputArgumentEntry.Arguments[1].Call)
                            {
                                case "-s":
                                    EnvironmentVariables.ChangeDefaultValue("FULLSCREEN", false);
                                    break;
                                case "-b":
                                    EnvironmentVariables.ChangeCurrentValue("FULLSCREEN", false);
                                    EnvironmentVariables.ChangeDefaultValue("FULLSCREEN", false);
                                    break;
                            }
                        }
                        else
                        {
                            EnvironmentVariables.ChangeCurrentValue("FULLSCREEN", false);
                        }
                        break;
                    case "-s":
                        EnvironmentVariables.ChangeDefaultValue("FULLSCREEN", true);
                        break;
                    case "-b":
                        EnvironmentVariables.ChangeCurrentValue("FULLSCREEN", true);
                        EnvironmentVariables.ChangeDefaultValue("FULLSCREEN", true);
                        break;
                }
                return "";
            });
            return CMD_FULLSCREEN;
        }
    }
}
