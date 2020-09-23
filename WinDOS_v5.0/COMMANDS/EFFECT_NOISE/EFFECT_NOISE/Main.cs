using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace EFFECT_NOISE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_EFFECT_NOISE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[double]", false, "[noise value]"));
            TABLE.Add(new CommandArgumentEntry("[double] -s", true, "[noise value] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -b", true, "[noise value] -b(save to running and startup)"));
            CMD_EFFECT_NOISE = new Command("EFFECT-NOISE", TABLE, false, "Controls noise effect intensity.", ExecutionLevel.User, CLIMode.Default);
            CMD_EFFECT_NOISE.SetFunction(() =>
            {
                if (CMD_EFFECT_NOISE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    EnvironmentVariables.ChangeDefaultValue("CRT_NOISE_INTENSITY", (double)CMD_EFFECT_NOISE.InputArgumentEntry.Arguments[0].Value);
                }
                else if (CMD_EFFECT_NOISE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    EnvironmentVariables.ChangeDefaultValue("CRT_NOISE_INTENSITY", (double)CMD_EFFECT_NOISE.InputArgumentEntry.Arguments[0].Value);
                    EnvironmentVariables.ChangeCurrentValue("CRT_NOISE_INTENSITY", (double)CMD_EFFECT_NOISE.InputArgumentEntry.Arguments[0].Value);
                }
                else
                {
                    EnvironmentVariables.ChangeCurrentValue("CRT_NOISE_INTENSITY", (double)CMD_EFFECT_NOISE.InputArgumentEntry.Arguments[0].Value);
                }
                return "";
            });
            return CMD_EFFECT_NOISE;
        }
    }
}
