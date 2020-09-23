using CLIShell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EFFECT_CURVE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_EFFECT_CURVE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[double]", false, "[curve value]"));
            TABLE.Add(new CommandArgumentEntry("[double] -s", false, "[curve value] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[double] -b", false, "[curve value] -b(save to running and startup)"));
            CMD_EFFECT_CURVE = new Command("EFFECT-CURVE", TABLE, false, "Changes the CRT distortion amount.", ExecutionLevel.User, CLIMode.Default);
            CMD_EFFECT_CURVE.SetFunction(() => 
            {
                if (CMD_EFFECT_CURVE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    EnvironmentVariables.ChangeDefaultValue("CRT_CURVE", (double)CMD_EFFECT_CURVE.InputArgumentEntry.Arguments[0].Value);
                }
                else if (CMD_EFFECT_CURVE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    EnvironmentVariables.ChangeCurrentValue("CRT_CURVE", (double)CMD_EFFECT_CURVE.InputArgumentEntry.Arguments[0].Value);
                    EnvironmentVariables.ChangeDefaultValue("CRT_CURVE", (double)CMD_EFFECT_CURVE.InputArgumentEntry.Arguments[0].Value);
                }
                else
                {
                    EnvironmentVariables.ChangeCurrentValue("CRT_CURVE", (double)CMD_EFFECT_CURVE.InputArgumentEntry.Arguments[0].Value);
                }   
                return "";
            });
            return CMD_EFFECT_CURVE;
        }
    }
}
