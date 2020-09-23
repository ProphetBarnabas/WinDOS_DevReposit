using CLIShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HELP
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_HELP;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[name filter]"));
            CMD_HELP = new Command("HELP", TABLE, true, "Returns descriptions of all commands in the current CLI mode.", ExecutionLevel.User, CLIMode.Any);
            CMD_HELP.SetFunction(() => 
            {
                string var_name = (string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default" ? "DEF_CMD_POOL" : "REG_CMD_POOL";
                if (CMD_HELP.InputArgumentEntry.Arguments.Count == 0)
                {
                    ((CommandPool)EnvironmentVariables.GetCurrentValue(var_name)).GetPool().ForEach((x) =>
                    {
                        IOInteractLayer.StandardOutput(CMD_HELP, "\n" + x.Description);
                    });
                }
                else
                {
                    Interpreter interpreter = new Interpreter((string)CMD_HELP.InputArgumentEntry.Arguments[0].Value);
                    ((CommandPool)EnvironmentVariables.GetCurrentValue(var_name)).GetPool().ForEach((x) =>
                    {
                        if (interpreter.GetResult(x.Call.ToLower()))
                        {
                            IOInteractLayer.StandardOutput(CMD_HELP, "\n" + x.Description);
                        }
                    });
                }
                return "";
            });
            return CMD_HELP;
        }
    }
}
