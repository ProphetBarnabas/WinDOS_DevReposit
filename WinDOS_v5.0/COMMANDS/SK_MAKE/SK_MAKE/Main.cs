using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace SK_MAKE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_SK_MAKE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[subkey path]"));
            CMD_SK_MAKE = new Command("SK-MAKE", TABLE, false, "Creates a new subkey.", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_SK_MAKE.SetFunction(() =>
            {
                if (CMD_SK_MAKE.InputArgumentEntry.Arguments.Count == 0)
                {
                    return "\nInvalid arguments!";
                }
                RegistryHive hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                string subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1);
                RegistryKey newKey = null;
                try
                {
                    if (CMD_SK_MAKE.InputArgumentEntry.Arguments[0].Value.ToString().Contains("\\"))
                    {
                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_MAKE.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_SK_MAKE.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                        subKey = CMD_SK_MAKE.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_SK_MAKE.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\") + 1);
                    }
                    else
                    {
                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_MAKE.InputArgumentEntry.Arguments[0].Value.ToString());
                        subKey = "";
                    }

                    newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                    if (newKey == null)
                    {
                        try
                        {
                            RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).CreateSubKey(subKey);
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_SK_MAKE, ex);
                        }    
                    }
                    else
                    {
                        return "\nSubkey already exists!";
                    }
                }
                catch (Exception) 
                {
                    subKey += CMD_SK_MAKE.InputArgumentEntry.Arguments[0].Value.ToString();
                    newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                    if (newKey == null)
                    {
                        try
                        {
                            RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).CreateSubKey(subKey);
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_SK_MAKE, ex);
                        }
                    }
                    else
                    {
                        return "\nSubkey already exists!";
                    }
                }
                return "";
            });
            return CMD_SK_MAKE;
        }
    }
}
