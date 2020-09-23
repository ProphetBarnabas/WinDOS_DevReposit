using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace SYSFONT
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_SYSFONT;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-family=[string]", false, "-family=[family name]"));
            TABLE.Add(new CommandArgumentEntry("-style=[string]", false, "-style=[Regular/Oblique/Italic]"));
            TABLE.Add(new CommandArgumentEntry("-size=[int]", false, "-size=[font size>0]"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -s", false, "-family=[family name] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-style=[string] -s", false, "-style=[Regular/Oblique/Italic] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-size=[int] -s", false, "-size=[font size>0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -b", false, "-family=[family name] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-style=[string] -b", false, "-style=[Regular/Oblique/Italic] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-size=[int] -b", false, "-size=[font size>0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -style=[string]", false, "-family=[family name]"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -style=[string] -s", false, "-family=[family name] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -style=[string] -b", false, "-family=[family name] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -size=[int]", false, "-family=[family name] -size=[font size>0]"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -size=[int] -s", false, "-family=[family name] -size=[font size>0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -size=[int] -b", false, "-family=[family name] -size=[font size>0] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-size=[int] -style=[string]", false, "-size=[font size>0] -style=[Regular/Oblique/Italic]"));
            TABLE.Add(new CommandArgumentEntry("-size=[int] -style=[string] -s", false, "-size=[font size>0] -style=[Regular/Oblique/Italic] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-size=[int] -style=[string] -b", false, "-size=[font size>0] -style=[Regular/Oblique/Italic] -b(save to running and startup)"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -style=[string] -size=[int]", false, "-family=[family name] -style=[Regular/Oblique/Italic] -size=[font size>0]"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -style=[string] -size=[int] -s", false, "-family=[family name] -style=[Regular/Oblique/Italic] -size=[font size>0] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("-family=[string] -style=[string] -size=[int] -b", false, "-family=[family name] -style=[Regular/Oblique/Italic] -size=[font size>0] -b(save to running and startup)"));
            CMD_SYSFONT = new Command("SYSFONT", TABLE, false, "Sets system font properties.", ExecutionLevel.User, CLIMode.Default);
            CMD_SYSFONT.SetFunction(() => 
            {
                if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-s"))
                {
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-family"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("FONT_FAMILY", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-family").Value);
                    }
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-style"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("FONT_STYLE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-style").Value);
                    }
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-size"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("FONT_SIZE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-size").Value);
                    }
                }
                else if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-b"))
                {
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-family"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("FONT_FAMILY", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-family").Value);
                        EnvironmentVariables.ChangeCurrentValue("FONT_FAMILY", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-family").Value);
                    }
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-style"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("FONT_STYLE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-style").Value);
                        EnvironmentVariables.ChangeCurrentValue("FONT_STYLE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-style").Value);
                    }
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-size"))
                    {
                        EnvironmentVariables.ChangeDefaultValue("FONT_SIZE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-size").Value);
                        EnvironmentVariables.ChangeCurrentValue("FONT_SIZE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-size").Value);
                    }
                }
                else
                {
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-family"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("FONT_FAMILY", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-family").Value);
                    }
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-style"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("FONT_STYLE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-style").Value);
                    }
                    if (CMD_SYSFONT.InputArgumentEntry.Arguments.Exists(x => x.Call == "-size"))
                    {
                        EnvironmentVariables.ChangeCurrentValue("FONT_SIZE", CMD_SYSFONT.InputArgumentEntry.Arguments.Find(x => x.Call == "-size").Value);
                    }
                }
                return "";
            });
            return CMD_SYSFONT;
        }
    }
}
