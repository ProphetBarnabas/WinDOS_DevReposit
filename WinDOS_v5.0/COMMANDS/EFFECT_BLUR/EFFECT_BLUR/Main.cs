using CLIShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFFECT_BLUR
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_EFFECT_BLUR;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-rad=[double]", false, "-rad=[blur radius]"));
            TABLE.Add(new CommandArgumentEntry("-rad=[double] -s", false, "-rad=[blur radius] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-rad=[double] -b", false, "-rad=[blur radius] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-type=[string]", false, "-type=[Box/Gaussian]"));
            TABLE.Add(new CommandArgumentEntry("-type=[string] -s", false, "-type=[Box/Gaussian] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-type=[string] -b", false, "-type=[Box/Gaussian] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-rad=[double] -type=[string]", false, "-rad=[blur radius] -type=[Box/Gaussian]"));
            TABLE.Add(new CommandArgumentEntry("-rad=[double] -type=[string] -s", false, "-rad=[blur radius] -type=[Box/Gaussian] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-rad=[double] -type=[string] -b", false, "-rad=[blur radius] -type=[Box/Gaussian] -b(save to running and startup)"));
            CMD_EFFECT_BLUR = new Command("EFFECT-BLUR", TABLE, false, "Controls the text area blur effect radius and type.", ExecutionLevel.User, CLIMode.Default);
            CMD_EFFECT_BLUR.SetFunction(() => 
            {
                if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-rad"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("BLUR_RADIUS", CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Find(x => x.Call == "-rad").Value);
                    }
                    if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-type"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("BLUR_TYPE", CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Find(x => x.Call == "-type").Value);
                    }
                }
                else if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-rad"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("BLUR_RADIUS", CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Find(x => x.Call == "-rad").Value);
                        EnvironmentVariables.SetToDefault("BLUR_RADIUS");
                    }
                    if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-type"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("BLUR_TYPE", CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Find(x => x.Call == "-type").Value);
                        EnvironmentVariables.SetToDefault("BLUR_TYPE");
                    }
                }
                else
                {
                    if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-rad"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("BLUR_RADIUS", CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Find(x => x.Call == "-rad").Value);
                    }
                    if (CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Exists(x => x.Call == "-type"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("BLUR_TYPE", CMD_EFFECT_BLUR.InputArgumentEntry.Arguments.Find(x => x.Call == "-type").Value);
                    }
                }
                return "";
            });
            return CMD_EFFECT_BLUR;
        }
    }
}
