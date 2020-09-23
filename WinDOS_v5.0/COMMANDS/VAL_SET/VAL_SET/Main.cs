using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace VAL_SET
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_VAL_SET;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[name] [value]"));
            CMD_VAL_SET = new Command("VAL-SET", TABLE, false, "Modifies an existing registry value in the current subkey.", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_VAL_SET.SetFunction(() =>
            {
                RegistryKey key = RegistryKey.OpenBaseKey((RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\"))), RegistryView.Default).OpenSubKey(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1), true);
                if (CMD_VAL_SET.InputArgumentEntry.Arguments[0].Value.ToString().ToList().Contains('=') && !CMD_VAL_SET.InputArgumentEntry.Arguments[0].Value.ToString().EndsWith("="))
                {
                    string name = CMD_VAL_SET.InputArgumentEntry.Arguments[0].Value.ToString();
                    string value = CMD_VAL_SET.InputArgumentEntry.Arguments[1].Value.ToString();
                    RegistryValueKind type = key.GetValueKind(name);
                    if (key.GetValueNames().Contains(name))
                    {
                        try
                        {
                            if (type == RegistryValueKind.DWord || type == RegistryValueKind.QWord || type == RegistryValueKind.Binary)
                            {
                                key.SetValue(name, int.Parse(value), type);
                            }
                            else
                            {
                                key.SetValue(name, value);
                            }
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_VAL_SET, ex);
                        }
                    }
                    else
                    {
                        return "\nValue not found!";
                    }
                }
                else
                {
                    return "\nIncorrect format!";
                }
                return "";
            });
            return CMD_VAL_SET;
        }
    }
}
