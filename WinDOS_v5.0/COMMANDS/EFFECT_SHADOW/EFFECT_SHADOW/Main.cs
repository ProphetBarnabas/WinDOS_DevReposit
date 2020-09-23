using CLIShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFFECT_SHADOW
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_EFFECT_SHADOW;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-brad=[double]", false, "-brad=[blur radius]"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -s", false, "-brad=[blur radius] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -b", false, "-brad=[blur radius] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -depth=[double]", false, "-brad=[blur radius] -depth=[shadow depth]"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -depth=[double] -s", false, "-brad=[blur radius] -depth=[shadow depth] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -depth=[double] -b", false, "-brad=[blur radius] -depth=[shadow depth] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -depth=[double] -color=[string]", false, "-brad=[blur radius] -depth=[shadow depth] -color=[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -depth=[double] -color=[string] -s", false, "-brad=[blur radius] -depth=[shadow depth] -color=[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -depth=[double] -color=[string] -b", false, "-brad=[blur radius] -depth=[shadow depth] -color=[color name/ARGB] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -color=[string]", false, "-brad=[blur radius]  -color=[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -color=[string] -s", false, "-brad=[blur radius]  -color=[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-brad=[double] -color=[string] -b", false, "-brad=[blur radius]  -color=[color name/ARGB] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-depth=[double] -color=[string]", false, "-depth=[shadow depth] -color=[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("-depth=[double] -color=[string] -s", false, "-depth=[shadow depth] -color=[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-depth=[double] -color=[string] -b", false, "-depth=[shadow depth] -color=[color name/ARGB] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-depth=[double]", false, "-depth=[shadow depth] "));
            TABLE.Add(new CommandArgumentEntry("-depth=[double] -s", false, "-depth=[shadow depth] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-depth=[double] -b", false, "-depth=[shadow depth] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-color=[string]", false, "-color=[color name/ARGB]"));
            TABLE.Add(new CommandArgumentEntry("-color=[string] -s", false, "-color=[color name/ARGB] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-color=[string] -b", false, "-color=[color name/ARGB] -b(save to running and startup)"));
            CMD_EFFECT_SHADOW = new Command("EFFECT-SHADOW", TABLE, false, "Controls text area shadow blur radius, depth and color.", ExecutionLevel.User, CLIMode.Default);
            CMD_EFFECT_SHADOW.SetFunction(() => 
            {
                if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-brad"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SHADOW_BLUR_RADIUS", (double)CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-brad").Value);
                    }
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-depth"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SHADOW_DEPTH", (double)CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-depth").Value);
                    }
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-color"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SHADOW_COLOR", CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-color").Value);
                    }
                }
                else if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-brad"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SHADOW_BLUR_RADIUS", (double)CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-brad").Value);
                        EnvironmentVariables.SetToDefault("SHADOW_BLUR_RADIUS");
                    }
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-depth"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SHADOW_DEPTH", (double)CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-depth").Value);
                        EnvironmentVariables.SetToDefault("SHADOW_DEPTH");
                    }
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-color"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SHADOW_COLOR", CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-color").Value);
                        EnvironmentVariables.SetToDefault("SHADOW_COLOR");
                    }
                }
                else
                {
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-brad"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SHADOW_BLUR_RADIUS", (double)CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-brad").Value);
                    }
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-depth"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SHADOW_DEPTH", (double)CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-depth").Value);
                    }
                    if (CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Exists(x => x.Call == "-color"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SHADOW_COLOR", CMD_EFFECT_SHADOW.InputArgumentEntry.Arguments.Find(x => x.Call == "-color").Value);
                    }
                }
                return "";
            });
            return CMD_EFFECT_SHADOW;
        }
    }
}
