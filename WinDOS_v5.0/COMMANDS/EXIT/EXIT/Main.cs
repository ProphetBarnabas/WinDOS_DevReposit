using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace EXIT
{
    public class Main
    {
        public Command CMD_EXIT = new Command("EXIT", null, false, "Closes WinDOS.", ExecutionLevel.User, CLIMode.Any);

        public Command GetCommand()
        {
            CMD_EXIT.SetFunction(() => 
            {
                switch ((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")))
                {
                    case CLIMode.Default:
                        Environment.Exit(0);
                        break;
                    case CLIMode.Regedit:
                        EnvironmentVariables.ChangeCurrentValue("CLI_MODE", CLIMode.Default);
                        break;
                }
                return "";
            });
            return CMD_EXIT;
        }
    }
}
