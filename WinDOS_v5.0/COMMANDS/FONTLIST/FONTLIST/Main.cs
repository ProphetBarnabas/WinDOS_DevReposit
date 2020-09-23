using CLIShell;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Diagnostics;

namespace FONTLIST
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_FONTLIST;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[name filter]"));

            CMD_FONTLIST = new Command("FONTLIST", TABLE, true, "Returns all installed and built-in font names.", ExecutionLevel.User, CLIMode.Default);
            CMD_FONTLIST.SetFunction(() =>
            {
                System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();
                List<string> built_in_names = new List<string>();
                List<string> installed_names = new List<string>();
                built_in_names.Add("3270Medium");
                built_in_names.Add("Amstrad");
                built_in_names.Add("BIOS");
                built_in_names.Add("CGA");
                built_in_names.Add("EGA8");
                built_in_names.Add("EGA9");
                built_in_names.Add("MDA");
                built_in_names.Add("TandyNew225");
                built_in_names.Add("TandyNewTV");
                built_in_names.Add("VGA8");
                built_in_names.Add("VGA9");
                Interpreter interpreter = null;
                if (CMD_FONTLIST.InputArgumentEntry.Arguments.Count == 1)
                {
                    interpreter = new Interpreter((string)CMD_FONTLIST.InputArgumentEntry.Arguments[0].Value);
                }
                if (CMD_FONTLIST.InputArgumentEntry.Arguments.Count == 1)
                {
                    for (int i = 0; i < built_in_names.Count; i++)
                    {
                        if (!interpreter.GetResult(built_in_names[i]))
                        {
                            built_in_names.RemoveAt(i);
                            i--;
                        }
                    }
                }
                for (int i = 0; i < ifc.Families.Length; i++)
                {
                    if (CMD_FONTLIST.InputArgumentEntry.Arguments.Count == 0 || interpreter.GetResult(ifc.Families[i].Name))
                    {
                        installed_names.Add(ifc.Families[i].Name);
                    }
                }

                if (built_in_names.Count > 0)
                {
                    IOInteractLayer.StandardOutput(CMD_FONTLIST, "\nBuilt-in fonts:");
                    for (int i = 0; i < built_in_names.Count; i++)
                    {
                        IOInteractLayer.StandardOutput(CMD_FONTLIST, "\n\t" + built_in_names[i]);
                    }
                }
                if (installed_names.Count > 0)
                {
                    IOInteractLayer.StandardOutput(CMD_FONTLIST, "\nInstalled fonts:");
                    for (int i = 0; i < installed_names.Count; i++)
                    {
                        IOInteractLayer.StandardOutput(CMD_FONTLIST, "\n\t" + installed_names[i]);
                    }
                }
                return "";
            });
            return CMD_FONTLIST;
        }
    }
}
