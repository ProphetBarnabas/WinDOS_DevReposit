using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace VAL_RENAME
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_VAL_RENAME;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[old name] [new name]"));
            CMD_VAL_RENAME = new Command("VAL-RENAME", TABLE, false, "", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_VAL_RENAME.SetFunction(() => 
            {
                RegistryKey key = RegistryKey.OpenBaseKey((RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\"))), RegistryView.Default).OpenSubKey(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1), true);
                string[] names = null;
                try
                {
                    names = key.GetValueNames();
                    if (names.Contains(CMD_VAL_RENAME.InputArgumentEntry.Arguments[0].Value.ToString()))
                    {
                        if (names.Contains(CMD_VAL_RENAME.InputArgumentEntry.Arguments[1].Value.ToString()))
                        {
                            return "\nA value with the specified new name already exist!";
                        }
                        else
                        {
                            string name = CMD_VAL_RENAME.InputArgumentEntry.Arguments[0].Value.ToString();
                            RegistryValueKind type = key.GetValueKind(name);
                            object value = key.GetValue(name);
                            key.DeleteValue(name);
                            key.SetValue(CMD_VAL_RENAME.InputArgumentEntry.Arguments[1].Value.ToString(), value, type);
                        }
                    }
                    else
                    {
                        return "\nValue not found!";
                    }
                }
                catch (Exception ex)
                {
                    IOInteractLayer.StandardError(CMD_VAL_RENAME, ex);
                }
                return "";
            });
            return CMD_VAL_RENAME;
        }
    }
}
