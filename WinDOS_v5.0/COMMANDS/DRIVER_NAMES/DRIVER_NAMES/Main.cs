using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ToolShed.ThisPC;

namespace DRIVER_NAMES
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_DRIVER_NAMES;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[name filter]"));
            CMD_DRIVER_NAMES = new Command("DRIVER-NAMES", TABLE, true, "Returns driver names.", ExecutionLevel.User, CLIMode.Default);
            CMD_DRIVER_NAMES.SetFunction(() => 
            {
                DriverInfo inf = new DriverInfo();
                if (CMD_DRIVER_NAMES.InputArgumentEntry.Arguments.Count == 0)
                {
                    inf.GetDriverInfo().ToList().Select(x => x.DisplayName).ToList().ForEach(x => 
                    {
                        IOInteractLayer.StandardOutput(CMD_DRIVER_NAMES, "\n" + x);
                    });
                }
                else
                {
                    Interpreter interpreter = new Interpreter(CMD_DRIVER_NAMES.InputArgumentEntry.Arguments[0].Value.ToString());
                    inf.GetDriverInfo().ToList().Select(x => x.DisplayName).ToList().ForEach(x =>
                    {
                        if (interpreter.GetResult(x))
                        {
                            IOInteractLayer.StandardOutput(CMD_DRIVER_NAMES, "\n" + x);
                        }  
                    });
                }
                return "";
            });
            return CMD_DRIVER_NAMES;
        }
    }
}
