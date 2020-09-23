using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace HISTORY_GETENTRY
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_HISTORY_GETENTRY;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[int]", false, "[entry index]"));
            TABLE.Add(new CommandArgumentEntry("[int] -r", false, "[entry index] -r(reverse search)"));
            CMD_HISTORY_GETENTRY = new Command("HISTORY-GETENTRY", TABLE, false, "Returns the specified history entry of the current CLI mode.", ExecutionLevel.User, CLIMode.Any);
            CMD_HISTORY_GETENTRY.SetFunction(() => 
            {
                int entry = (int)CMD_HISTORY_GETENTRY.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value;
                string var_name = (string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default" ? "DEF_HIST_SIZE" : "REG_HIST_SIZE";
                if ((int)EnvironmentVariables.GetCurrentValue(var_name) >= entry && entry > 0)
                {
                    if (CMD_HISTORY_GETENTRY.InputArgumentEntry.Arguments.Exists(x => x.Call == "-r"))
                    {
                        IOInteractLayer.StandardOutput(CMD_HISTORY_GETENTRY ,"\n\t" + ((List<string>)EnvironmentVariables.GetCurrentValue(var_name))[((List<string>)EnvironmentVariables.GetCurrentValue(var_name)).Count - (int)CMD_HISTORY_GETENTRY.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value]);
                    }
                    else
                    {
                        IOInteractLayer.StandardOutput(CMD_HISTORY_GETENTRY, "\n\t" + ((List<string>)EnvironmentVariables.GetCurrentValue(var_name))[(int)CMD_HISTORY_GETENTRY.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value]);
                    }
                }
                return "";
            });
            return CMD_HISTORY_GETENTRY;
        }
    }
}
