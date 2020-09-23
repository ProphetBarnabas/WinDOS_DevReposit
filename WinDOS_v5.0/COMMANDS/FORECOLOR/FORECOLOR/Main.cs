using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace FORECOLOR
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_FORECOLOR;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("[string] -s", true, "[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -b", true, "[color name/ARGB] -b(save to running and startup)"));
            CMD_FORECOLOR = new Command("FORECOLOR", TABLE, false, "Changes the foreground color of the text area.", ExecutionLevel.User, CLIMode.Default);
            CMD_FORECOLOR.SetFunction(() => 
            {
                switch (CMD_FORECOLOR.InputArgumentEntry.Arguments.Last().Call)
                {
                    case "-s":
                        EnvironmentVariables.ChangeDefaultValue("FORECOLOR", (string)CMD_FORECOLOR.InputArgumentEntry.Arguments[0].Value);
                        break;
                    case "-b":
                        EnvironmentVariables.ChangeDefaultValue("FORECOLOR", (string)CMD_FORECOLOR.InputArgumentEntry.Arguments[0].Value);
                        EnvironmentVariables.ChangeCurrentValue("FORECOLOR", (string)CMD_FORECOLOR.InputArgumentEntry.Arguments[0].Value);
                        break;
                    default:
                        EnvironmentVariables.ChangeCurrentValue("FORECOLOR", (string)CMD_FORECOLOR.InputArgumentEntry.Arguments[0].Value);   
                        break;
                }        
                return "";
            });
            return CMD_FORECOLOR;
        }
    }
}
