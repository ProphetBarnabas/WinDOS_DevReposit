using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace CONFIG_STARTUP
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CONFIG_STARTUP;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-load", false));
            CMD_CONFIG_STARTUP = new Command("CONFIG-STARTUP", TABLE, false, "Displays or loads startup config.", ExecutionLevel.User, CLIMode.Default);
            CMD_CONFIG_STARTUP.SetFunction(() =>
            {
                if (CMD_CONFIG_STARTUP.InputArgumentEntry.Arguments.Count == 0)
                {
                    EnvironmentVariables.GetAll().ForEach((x) =>
                    {
                        if (x.VarType == VariableType.Constant)
                        {
                            IOInteractLayer.StandardOutput(CMD_CONFIG_STARTUP, $"\n\t{x.Name} = {x.DefaultValue}");
                        }
                    });
                }
                else
                {
                    List<string> names = EnvironmentVariables.FindAll(x => x.VarType == VariableType.Constant).Select(x => x.Name).ToList();
                    names.ForEach((x) =>
                    {
                        EnvironmentVariables.ChangeCurrentValue(x, EnvironmentVariables.GetDefaultValue(x));
                    });
                }
                return "";
            });
            return CMD_CONFIG_STARTUP;
        }
    }
}
