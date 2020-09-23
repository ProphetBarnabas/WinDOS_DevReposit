using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace CARET_SHAPE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CARET_SHAPE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-w=[int]", false, "-w=[width>=0]"));
            TABLE.Add(new CommandArgumentEntry("-h=[int]", false, "-h=[height>=0]"));
            TABLE.Add(new CommandArgumentEntry("-border=[int]", false, "-border=[boder thickness>=0]"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -s", false, "-w=[width>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-h=[int] -s", false, "-h=[height>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-border=[int] -s", false, "-border=[boder thickness>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -b", false, "-w=[width>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-h=[int] -b", false, "-h=[height>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-border=[int] -b", false, "-border=[boder thickness>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -h=[int]", false, "-w=[width>=0] -h=[height>=0]"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -h=[int] -s", false, "-w=[width>=0] -h=[height>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -h=[int] -b", false, "-w=[width>=0] -h=[height>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -border=[int]", false, "-w=[width>=0] -border=[boder thickness>=0]"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -border=[int] -s", false, "-w=[width>=0] -border=[boder thickness>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -border=[int] -b", false, "-w=[width>=0] -border=[boder thickness>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-border=[int] -h=[int]", false, "-border=[boder thickness>=0] -h=[height>=0]"));
            TABLE.Add(new CommandArgumentEntry("-border=[int] -h=[int] -s", false, "-border=[boder thickness>=0] -h=[height>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-border=[int] -h=[int] -b", false, "-border=[boder thickness>=0] -h=[height>=0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -h=[int] -border=[int]", false, "-w=[width>=0] -h=[height>0] -border=[boder thickness>=0]"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -h=[int] -border=[int] -s", false, "-w=[width>=0] -h=[height>=0] -border=[boder thickness>=0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-w=[int] -h=[int] -border=[int] -b", false, "-w=[width>=0] -h=[height>=0] -border=[boder thickness>=0] -b(save to running and startup)"));
            CMD_CARET_SHAPE = new Command("CARET-SHAPE", TABLE, false, "Sets the dimensions of the custom caret. If argument '-w'(width) or '-h'(height) is 0, the caret dimensions are set automatically(automatic dimensions are optimized for the default font and are not guaranteed to work correctly when using any other font).", ExecutionLevel.User, CLIMode.Default);
            CMD_CARET_SHAPE.SetFunction(() => 
            {
                if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-w"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_WIDTH", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-w").Value);
                    }
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BORDER", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-b").Value);
                    }
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-h"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_HEIGHT", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-h").Value);
                    }
                }
                else if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-w"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_WIDTH", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-w").Value);
                        EnvironmentVariables.SetToDefault("SYS_CARET_WIDTH");
                    }
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_BORDER", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-b").Value);
                        EnvironmentVariables.SetToDefault("SYS_CARET_BORDER");
                    }
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-h"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("SYS_CARET_HEIGHT", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-h").Value);
                        EnvironmentVariables.SetToDefault("SYS_CARET_HEIGHT");
                    }
                }
                else
                {
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-w"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_WIDTH", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-w").Value);
                    }
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_BORDER", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-b").Value);
                    }
                    if (CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-h"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("SYS_CARET_HEIGHT", CMD_CARET_SHAPE.InputArgumentEntry.Arguments.Find(x => x.Call == "-h").Value);
                    }
                }
                return "";
            });
            return CMD_CARET_SHAPE;
        }
    }
}
