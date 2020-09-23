using CLIShell;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISTORY_CLEAR
{
    public class Main
    {
        public Command CMD_HISTORY_CLEAR;

        public Command GetCommand()
        {
            CMD_HISTORY_CLEAR = new Command("HISTORY-CLEAR", null, false, "Clears the input history of the current CLI mode.", ExecutionLevel.User, CLIMode.Any);
            CMD_HISTORY_CLEAR.SetFunction(() => 
            {
                EnvironmentVariables.ChangeCurrentValue((string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default" ? "DEF_HISTORY" : "REG_HISTORY", new List<string>());
                return "";
            });
            return CMD_HISTORY_CLEAR;
        }
    }
}
