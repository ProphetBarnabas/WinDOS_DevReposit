using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace EFFECT_SCANLINE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_EFFECT_SCANLINE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[double]", false, "[density]"));
            TABLE.Add(new CommandArgumentEntry("-speed=[double]", false, "-speed=[up>0>down]"));
            TABLE.Add(new CommandArgumentEntry("[double] -speed=[double]", true, "[density] -speed=[up>0>down]"));
            TABLE.Add(new CommandArgumentEntry("[double] -s", true, "[density] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-speed=[double] -s", true, "-speed=[up>0>down] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -speed=[double] -s", true, "[density] -speed=[up>0>down] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -b", true, "[density] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-speed=[double] -b", true, " -speed=[up>0>down] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -speed=[double] -b", true, "[density] -speed=[up>0>down] -b(save to running and startup)"));
            CMD_EFFECT_SCANLINE = new Command("EFFECT-SCANLINE", TABLE, false, "Changes scan line density and optionally speed and direction.", ExecutionLevel.User, CLIMode.Default);
            CMD_EFFECT_SCANLINE.SetFunction(() =>
            {
                if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-speed"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_SCANLINE_SPEED", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "-speed").Value);
                    }
                    if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == ""))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_SCANLINE_DENSITY", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                    }
                }
                else if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-speed"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_SCANLINE_SPEED", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "-speed").Value);
                        EnvironmentVariables.ChangeCurrentValue("CRT_SCANLINE_SPEED", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "-speed").Value);
                    }
                    if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == ""))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_SCANLINE_DENSITY", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                        EnvironmentVariables.ChangeCurrentValue("CRT_SCANLINE_DENSITY", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                    }
                }
                else
                {
                    if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-speed"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("CRT_SCANLINE_SPEED", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "-speed").Value);
                    }
                    if (CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Exists(x => x.Call == ""))
                    {
                        EnvironmentVariables.ChangeCurrentValue("CRT_SCANLINE_DENSITY", (double)CMD_EFFECT_SCANLINE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                    }
                }
                return "";
            });
            return CMD_EFFECT_SCANLINE;
        }
    }
}
