using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CLIShell;
using System.Security.Cryptography.X509Certificates;

namespace CARET_FADE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CARET_FADE;

        public Command GetCommand()
        {



            TABLE.Add(new CommandArgumentEntry("-in=[int]", false, "-in=[fade in duration>=0]"));
            TABLE.Add(new CommandArgumentEntry("-in=[int] -s", false, "-in=[fade in duration>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-in=[int] -b", false, "-in=[fade in duration>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-out=[int]", false, "-out=[fade out duration>=0]"));
            TABLE.Add(new CommandArgumentEntry("-out=[int] -s", false, "-out=[fade out duration>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-out=[int] -b", false, "-out=[fade out duration>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-in=[int] -out=[int]", false, "-in=[fade in duration>=0] -out=[fade out duration>=0]"));
            TABLE.Add(new CommandArgumentEntry("-in=[int] -out=[int] -s", false, "-in=[fade in duration>=0] -out=[fade out duration>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-in=[int] -out=[int] -b", false, "-in=[fade in duration>=0] -out=[fade out duration>=0] -b(save to running and startup)"));
            CMD_CARET_FADE = new Command("CARET-FADE", TABLE, false, "Sets custom caret fade in/out durations in milliseconds.", ExecutionLevel.User, CLIMode.Default);
            CMD_CARET_FADE.SetFunction(() => 
            {
                
                if (CMD_CARET_FADE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-in"))
                {
                    int _in = (int)CMD_CARET_FADE.InputArgumentEntry.Arguments.Find(x => x.Call == "-in").Value;
                    if (CMD_CARET_FADE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FADEIN_SPEED", _in);
                    }
                    else if (CMD_CARET_FADE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FADEIN_SPEED", _in);
                        EnvironmentVariables.SetToDefault("SYS_CARET_FADEIN_SPEED");
                    }
                    else
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FADEIN_SPEED", _in);
                    }
                }
                if (CMD_CARET_FADE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-out"))
                {
                    int _out = (int)CMD_CARET_FADE.InputArgumentEntry.Arguments.Find(x => x.Call == "-out").Value;
                    if (CMD_CARET_FADE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FADEOUT_SPEED", _out);
                    }
                    else if (CMD_CARET_FADE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_FADEOUT_SPEED", _out);
                        EnvironmentVariables.SetToDefault("SYS_CARET_FADEOUT_SPEED");
                    }
                    else
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_FADEOUT_SPEED", _out);
                    }
                }
                return "";
            });
            return CMD_CARET_FADE;
        }
    }
}
