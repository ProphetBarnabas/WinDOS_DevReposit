using CLIShell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CARET_BLINK
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CARET_BLINK;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-int=[int]", false, "-int=[fade in/out interval]"));
            TABLE.Add(new CommandArgumentEntry("-int=[int] -s", false, "-int=[blink interval] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-int=[int] -b", false, "-int=[blink interval] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-int=[int] -idle=[int]", false, "-int=[blink interval] -idle=[blink after idle time]"));
            TABLE.Add(new CommandArgumentEntry("-int=[int] -idle=[int] -s", false, "-int=[blink interval] -idle=[blink after idle time] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-int=[int] -idle=[int] -b", false, "-int=[blink interval] -idle=[blink after idle time] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-idle=[int]", false, "-idle=[blink after idle time]"));
            TABLE.Add(new CommandArgumentEntry("-idle=[int] -s", false, "-idle=[blink after idle time] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-idle=[int] -b", false, "-idle=[blink after idle time] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-n", false, "-n(disable blink)"));
            TABLE.Add(new CommandArgumentEntry("-n -s", false, "-n(disable blink) -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-n -b", false, "-n(disable blink) -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-s", false, "-s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-b", false, "-b(save to running and startup)"));
            CMD_CARET_BLINK = new Command("CARET-BLINK", TABLE, false, "Controls the blink behavior of the custom caret.", ExecutionLevel.User, CLIMode.Default);
            CMD_CARET_BLINK.SetFunction(() => 
            {
                if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-n"))
                {
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK", false);
                    }
                    else if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK", false);
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK", false);
                    }
                    else
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK", false);
                    }
                    return "";
                }
                else if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Count == 0)
                {
                    EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK", true);
                    return "";
                }
                else if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Count == 1)
                {
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK", true);
                    }
                    else if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK", true);
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK", true);
                    }
                    else if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-int"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK_INTERVAL", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-int").Value);
                    }
                    else if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-idle"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK_IDLE_TIME", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-idle").Value);
                    }
                    return "";
                }

                if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-int"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK_INTERVAL", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-int").Value);
                    }
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-idle"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK_IDLE_TIME", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-idle").Value);
                    }
                }
                else if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-int"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK_INTERVAL", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-int").Value);
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK_INTERVAL", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-int").Value);
                    }
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-idle"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BLINK_IDLE_TIME", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-idle").Value);
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK_IDLE_TIME", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-idle").Value);
                    }
                }
                else
                {
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-int"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK_INTERVAL", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-int").Value);
                    }
                    if (CMD_CARET_BLINK.InputArgumentEntry.Arguments.Exists(x => x.Call == "-idle"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BLINK_IDLE_TIME", (int)CMD_CARET_BLINK.InputArgumentEntry.Arguments.Find(x => x.Call == "-idle").Value);
                    }
                }
                return "";
            });
            return CMD_CARET_BLINK;
        }
    }
}
