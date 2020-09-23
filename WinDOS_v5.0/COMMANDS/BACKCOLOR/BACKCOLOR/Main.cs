using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace BACKCOLOR
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_BACKCOLOR;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("[string] -s", true, "[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -b", true, "[color name/ARGB] -b(save to running and startup)"));
            CMD_BACKCOLOR = new Command("BACKCOLOR", TABLE, false, "Changes the background color of the text area.", ExecutionLevel.User, CLIMode.Default);
            CMD_BACKCOLOR.SetFunction(() => 
            {
                switch (CMD_BACKCOLOR.InputArgumentEntry.Arguments.Last().Call)
                {
                    case "-s":
                        EnvironmentVariables.ChangeDefaultValue("BACKCOLOR", CMD_BACKCOLOR.InputArgumentEntry.Arguments[0].Value);
                        break;
                    case "-b":
                        EnvironmentVariables.ChangeDefaultValue("BACKCOLOR", CMD_BACKCOLOR.InputArgumentEntry.Arguments[0].Value);
                        EnvironmentVariables.ChangeCurrentValue("BACKCOLOR", CMD_BACKCOLOR.InputArgumentEntry.Arguments[0].Value);
                        break;
                    default:
                        EnvironmentVariables.ChangeCurrentValue("BACKCOLOR", CMD_BACKCOLOR.InputArgumentEntry.Arguments[0].Value);
                        break;
                }
                return "";
            });
            return CMD_BACKCOLOR;
        }
    }
}
