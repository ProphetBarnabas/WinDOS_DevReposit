using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace CSK
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CSK;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[subkey path]"));
            TABLE.Add(new CommandArgumentEntry("[string] -s", true, "[subkey path] -s(save to startup)"));
            TABLE.Add(new CommandArgumentEntry("[string] -b", true, "[subkey path] -b(save to running and startup)"));
            CMD_CSK = new Command("CSK", TABLE, false, "Changes the current subkey.", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_CSK.SetFunction(() =>
            {
                if (CMD_CSK.InputArgumentEntry.Arguments.Count == 0)
                {
                    return "\nInvalid arguments!";
                }
                RegistryHive hiveKey;
                string subKey = null;
                if (CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString() == "\\stsk")
                {
                    EnvironmentVariables.SetToDefault("SUBKEY");
                }
                else if (CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString() == "..")
                {
                    if (EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Count(x => x == '\\') > 1)
                    {
                        string path = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString();
                        path = path.Remove(path.Length - 1, 1);
                        path = path.Remove(path.LastIndexOf("\\"));
                        if (CMD_CSK.InputArgumentEntry.Arguments.Count == 1)
                        {
                            EnvironmentVariables.ChangeCurrentValue("SUBKEY", path + "\\");
                        }
                        else if (CMD_CSK.InputArgumentEntry.Arguments[1].Call == "-s")
                        {
                            EnvironmentVariables.ChangeDefaultValue("SUBKEY", path + "\\");
                        }
                        else
                        {
                            EnvironmentVariables.ChangeDefaultValue("SUBKEY", path + "\\");
                            EnvironmentVariables.ChangeCurrentValue("SUBKEY", path + "\\");
                        }
                    }
                }
                else
                {
                    try
                    {
                        if (CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString().Contains("\\"))
                        {
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                            subKey = CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\") + 1);
                        }
                        else
                        {
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString());
                            subKey = "";
                        }
                    }
                    catch (Exception)
                    {
                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                        subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1) + CMD_CSK.InputArgumentEntry.Arguments[0].Value.ToString();
                    }
                    try
                    {
                        RegistryKey newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                        if (newKey == null)
                        {
                            return "\nSubkey not found!";
                        }
                        if (CMD_CSK.InputArgumentEntry.Arguments.Count == 1)
                        {
                            EnvironmentVariables.ChangeCurrentValue("SUBKEY", hiveKey.ToString() + "\\" + subKey + ((hiveKey.ToString() + "\\" + subKey).EndsWith("\\") ? "" : "\\"));
                        }
                        else if (CMD_CSK.InputArgumentEntry.Arguments[1].Call == "-s")
                        {
                            EnvironmentVariables.ChangeDefaultValue("SUBKEY", hiveKey.ToString() + "\\" + subKey + ((hiveKey.ToString() + "\\" + subKey).EndsWith("\\") ? "" : "\\"));
                        }
                        else
                        {
                            EnvironmentVariables.ChangeDefaultValue("SUBKEY", hiveKey.ToString() + "\\" + subKey + ((hiveKey.ToString() + "\\" + subKey).EndsWith("\\") ? "" : "\\"));
                            EnvironmentVariables.ChangeCurrentValue("SUBKEY", hiveKey.ToString() + "\\" + subKey + ((hiveKey.ToString() + "\\" + subKey).EndsWith("\\") ? "" : "\\"));
                        }
                    }
                    catch (Exception ex)
                    {
                        IOInteractLayer.StandardError(CMD_CSK, ex);
                    }
                }
                return "";
            });
            return CMD_CSK;
        }
    }
}
