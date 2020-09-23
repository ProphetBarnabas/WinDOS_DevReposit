using CLIShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HISTORY_SETSIZE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_HISTORY_SETSIZE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[int]", false, "[new size]"));
            TABLE.Add(new CommandArgumentEntry("[int] -s", true, "[new size] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[int] -b", true, "[new size] -b(save to running and startup)"));
            CMD_HISTORY_SETSIZE = new Command("HISTORY-SETSIZE", TABLE, false, "Sets the history size of the current CLI mode. If the new size is lower than the previous one, some of the entries will be discarded.", ExecutionLevel.User, CLIMode.Any);
            CMD_HISTORY_SETSIZE.SetFunction(() => 
            {
                if ((string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default")
                {
                    if (CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments.Count == 2)
                    {
                        if (CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[1].Call == "-s")
                        {
                            EnvironmentVariables.ChangeDefaultValue("DEF_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                        }
                        else
                        {
                            EnvironmentVariables.ChangeDefaultValue("DEF_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeCurrentValue("DEF_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                        }
                    }
                    else
                    {
                        EnvironmentVariables.ChangeCurrentValue("DEF_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                    }
                }
                else
                {
                    if (CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments.Count == 2)
                    {
                        if (CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[1].Call == "-s")
                        {
                            EnvironmentVariables.ChangeDefaultValue("REG_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                        }
                        else
                        {
                            EnvironmentVariables.ChangeDefaultValue("REG_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                            EnvironmentVariables.ChangeCurrentValue("REG_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                        }
                    }
                    else
                    {
                        EnvironmentVariables.ChangeCurrentValue("REG_HIST_SIZE", (int)CMD_HISTORY_SETSIZE.InputArgumentEntry.Arguments[0].Value);
                    }
                }
                return "";
            });
            return CMD_HISTORY_SETSIZE;
        }
    }
}
