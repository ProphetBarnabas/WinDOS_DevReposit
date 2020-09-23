using CLIShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFFECT_VIGNETTE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_EFFECT_VIGNETTE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[double]", false, "[size]"));
            TABLE.Add(new CommandArgumentEntry("[double] -sm=[double]", true, "[size] -sm=[smoothness value]"));
            TABLE.Add(new CommandArgumentEntry("[double] -er=[double]", true, "[size] -er=[edge rounding]"));
            TABLE.Add(new CommandArgumentEntry("[double] -sm=[double] -er=[double]", true, "[size] -sm=[smoothness value]"));
            TABLE.Add(new CommandArgumentEntry("[double] -s", false, "[size] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -sm=[double] -s", true, "[size] -sm=[smoothness value] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -er=[double] -s", true, "[size] -er=[edge rounding] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -sm=[double] -er=[double] -s", true, "[size] -sm=[smoothness value] -er=[edge rounding] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -b", false, "[size] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -sm=[double] -b", true, "[size] -sm=[smoothness value] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -er=[double] -b", true, "[size] -er=[edge rounding] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -sm=[double] -er=[double] -b", true, "[size] -sm=[smoothness value] -er=[edge rounding] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-sm=[double]", false, "-sm=[smoothness value]"));
            TABLE.Add(new CommandArgumentEntry("-er=[double]", false, "-er=[edge rounding]"));
            TABLE.Add(new CommandArgumentEntry("-sm=[double] -er=[double]", true, "-sm=[smoothness value] -er=[edge rounding]"));
            TABLE.Add(new CommandArgumentEntry("-sm=[double] -s", false, "-sm=[smoothness value] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-er=[double] -s", false, "-er=[edge rounding] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-sm=[double] -er=[double] -s", true, "-sm=[smoothness value] -er=[edge rounding] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-sm=[double] -b", false, "-sm=[smoothness value] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-er=[double] -b", false, "-er=[edge rounding] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-sm=[double] -er=[double] -b", true, "-sm=[smoothness value] -er=[edge rounding] -b(save to running and startup)"));

            CMD_EFFECT_VIGNETTE = new Command("EFFECT-VIGNETTE", TABLE, false, "Controls vignette effect size, smoothness and edge rounding.", ExecutionLevel.User, CLIMode.Default);
            CMD_EFFECT_VIGNETTE.SetFunction(() =>
            {
                if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-sm"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_VIGNETTE_SMOOTHNESS", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-sm").Value);
                    }
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-er"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_VIGNETTE_EDGE_ROUNDING", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-er").Value);
                    }
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == ""))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_VIGNETTE_SIZE", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                    }
                }
                else if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-sm"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_VIGNETTE_SMOOTHNESS", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-sm").Value);
                        EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_SMOOTHNESS", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-sm").Value);
                    }
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-er"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_VIGNETTE_EDGE_ROUNDING", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-er").Value);
                        EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_EDGE_ROUNDING", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-er").Value);
                    }
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == ""))
                    {
                        EnvironmentVariables.ChangeDefaultValue("CRT_VIGNETTE_SIZE", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                        EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_SIZE", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                    }
                }
                else
                {
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-sm"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_SMOOTHNESS", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-sm").Value);
                    }
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-er"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_EDGE_ROUNDING", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "-er").Value);
                    }
                    if (CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Exists(x => x.Call == ""))
                    {
                        EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_SIZE", CMD_EFFECT_VIGNETTE.InputArgumentEntry.Arguments.Find(x => x.Call == "").Value);
                    }
                }
                return "";
            });
            return CMD_EFFECT_VIGNETTE;
        }
    }
}
