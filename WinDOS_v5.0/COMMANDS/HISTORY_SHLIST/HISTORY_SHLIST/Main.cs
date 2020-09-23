using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace HISTORY_SHLIST
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_HISTORY_SHLIST;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[result filter]"));
            CMD_HISTORY_SHLIST = new Command("HISTORY-SHLIST", TABLE, true, "Returns the input history of the current CLI mode.", ExecutionLevel.User, CLIMode.Any);
            CMD_HISTORY_SHLIST.SetFunction(() => 
            {
                string var_name = (string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default" ? "DEF_HISTORY" : "REG_HISTORY";
                if (CMD_HISTORY_SHLIST.InputArgumentEntry.Arguments.Count == 1)
                {
                    Interpreter interpreter = new Interpreter((string)CMD_HISTORY_SHLIST.InputArgumentEntry.Arguments[0].Value);
                    for (int i = 1; i < ((List<string>)EnvironmentVariables.GetCurrentValue(var_name)).Count; i++)
                    {
                        if (interpreter.GetResult(((List<string>)EnvironmentVariables.GetCurrentValue(var_name))[i]))
                        {
                            IOInteractLayer.StandardOutput(CMD_HISTORY_SHLIST, "\n\t" + i + " " + ((List<string>)EnvironmentVariables.GetCurrentValue(var_name))[i]);
                        }
                    }
                }
                else
                {
                    for (int i = 1; i < ((List<string>)EnvironmentVariables.GetCurrentValue(var_name)).Count; i++)
                    {
                        IOInteractLayer.StandardOutput(CMD_HISTORY_SHLIST, "\n\t" + i + " " + ((List<string>)EnvironmentVariables.GetCurrentValue(var_name))[i]);
                    }
                }
                return "";
            });
            return CMD_HISTORY_SHLIST;
        }
    }
}
