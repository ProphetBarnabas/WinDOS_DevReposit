using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace COLOR
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_COLOR;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("[string] -s", true, "[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -b", true, "[color name/ARGB] -b(save to running and startup)"));
            CMD_COLOR = new Command("COLOR", TABLE, false, "Changes all color properties(except BACKCOLOR and SYS_CARET_BORDER_COLOR) to the specified value.", ExecutionLevel.User, CLIMode.Default);
            CMD_COLOR.SetFunction(() =>
            {
                if (CMD_COLOR.InputArgumentEntry.Arguments.Count == 1)
                {
                    EnvironmentVariables.ChangeCurrentValue("FORECOLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                    EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FILL_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                    EnvironmentVariables.ChangeCurrentValue("SYS_CARET_SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                    EnvironmentVariables.ChangeCurrentValue("SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                }
                else 
                {
                    switch (CMD_COLOR.InputArgumentEntry.Arguments[1].Call)
                    {
                        case "-s":
                            EnvironmentVariables.ChangeDefaultValue("FORECOLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FILL_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeDefaultValue("SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            break;
                        case "-b":
                            EnvironmentVariables.ChangeCurrentValue("FORECOLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FILL_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeCurrentValue("SYS_CARET_SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeCurrentValue("SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeDefaultValue("FORECOLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FILL_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeDefaultValue("SYS_CARET_SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            EnvironmentVariables.ChangeDefaultValue("SHADOW_COLOR", CMD_COLOR.InputArgumentEntry.Arguments[0].Value.ToString());
                            break;
                    }
                }
                return "";
            });
            return CMD_COLOR;
        }
    }
}
