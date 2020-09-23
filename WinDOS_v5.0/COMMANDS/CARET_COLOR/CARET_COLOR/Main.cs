using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace CARET_COLOR
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CARET_COLOR;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("[string] -s", true, "[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -b", true, "[color name/ARGB] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-border=[string] -s", true, "-border=[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-border=[string] -b", true, "-border=[color name/ARGB] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -border=[string]", true, "[color name/ARGB] -border=[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("[string] -border=[string] -s", true, "[color name/ARGB] -border=[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -border=[string] -b", true, "[color name/ARGB] -border=[color name/ARGB] -b(save to running and startup)"));
            CMD_CARET_COLOR = new Command("CARET-COLOR", TABLE, false, "Changes the color of custom caret.", ExecutionLevel.User, CLIMode.Default);
            CMD_CARET_COLOR.SetFunction(() => 
            {
                if (CMD_CARET_COLOR.InputArgumentEntry.Arguments.Count == 1)
                {
                    switch (CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Call)
                    {
                        case "":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FILL_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            break;
                        case "-border":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            break;
                    }
                }
                else if (CMD_CARET_COLOR.InputArgumentEntry.Arguments.Count == 2)
                {
                    switch (CMD_CARET_COLOR.InputArgumentEntry.Arguments[1].Call)
                    {
                        case "-border":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FILL_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[1].Value);
                            break;
                        case "-s" when CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Call == "":
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FILL_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            break;
                        case "-b" when CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Call == "":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FILL_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FILL_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            break;
                        case "-s" when CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Call == "-border":
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            break;
                        case "-b" when CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Call == "-border":
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            break;
                    }
                }
                else
                {
                    switch (CMD_CARET_COLOR.InputArgumentEntry.Arguments[2].Call)
                    {
                        case "-s":
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[1].Value);
                            break;
                        case "-b":
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[1].Value);
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BORDER_COLOR", CMD_CARET_COLOR.InputArgumentEntry.Arguments[1].Value);
                            break;
                    }
                }
                return "";
            });
            return CMD_CARET_COLOR;
        }
    }
}
