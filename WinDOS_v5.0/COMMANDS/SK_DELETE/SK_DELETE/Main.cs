using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace SK_DELETE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_SK_DELETE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[subkey path]"));
            TABLE.Add(new CommandArgumentEntry("[string] -r", true, "[subkey path] -r(remove subkey and all child elements)"));
            CMD_SK_DELETE = new Command("SK-DELETE", TABLE, false, "Removes the specified subkey.", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_SK_DELETE.SetFunction(() => 
            {
                if (CMD_SK_DELETE.InputArgumentEntry.Arguments.Count == 0)
                {
                    return "\nInvalid arguments!";
                }
                RegistryHive hiveKey;
                string subKey = null;
                RegistryKey newKey = null;
                try
                {
                    hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_DELETE.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_SK_DELETE.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf('\\')));
                    subKey = CMD_SK_DELETE.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_SK_DELETE.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf('\\') + 1);
                }
                catch (Exception ex)
                {
                    hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf('\\')));
                    subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf('\\') + 1) + CMD_SK_DELETE.InputArgumentEntry.Arguments[0].Value.ToString();
                }
                try
                {
                    newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                    if (newKey == null)
                    {
                        Debug.WriteLine(hiveKey.ToString() + " || " + CMD_SK_DELETE.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_SK_DELETE.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf('\\') + 1));
                        return "\nSubkey not found!";
                    }
                    else if (newKey == RegistryKey.OpenBaseKey((RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf('\\'))), RegistryView.Default).OpenSubKey(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf('\\') + 1)))
                    {
                        return "\nCannot delete current subkey.";
                    }
                    else
                    {
                        if (CMD_SK_DELETE.InputArgumentEntry.Arguments.Exists(x => x.Call == "-r"))
                        {
                            RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).DeleteSubKeyTree(subKey);
                        }
                        else
                        {
                            RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).DeleteSubKey(subKey);
                        }
                    }
                }
                catch (Exception ex)
                {
                    IOInteractLayer.StandardError(CMD_SK_DELETE, ex);
                }
                return "";
            });
            return CMD_SK_DELETE;
        }
    }
}
