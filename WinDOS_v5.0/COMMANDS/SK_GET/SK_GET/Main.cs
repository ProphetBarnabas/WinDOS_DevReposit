using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace SK_GET
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_SK_GET;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[registry path/filter]"));
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[registry path] [filter]"));
            CMD_SK_GET = new Command("SK-GET", TABLE, true, "Returns all subkeys in the specified subkey(current subkey if not specified).", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_SK_GET.SetFunction(() => 
            {
                RegistryHive hiveKey;
                string subKey = null;
                RegistryKey newKey = null;
                string[] keys = null;
                Interpreter interpreter = null;
                switch (CMD_SK_GET.InputArgumentEntry.Arguments.Count)
                {
                    case 0:
                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                        subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1);
                        newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                        try
                        {
                            keys = newKey.GetSubKeyNames();
                            for (int i = 0; i < keys.Length; i++)
                            {
                                IOInteractLayer.StandardOutput(CMD_SK_GET, "\n" + keys[i]);
                            }
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_SK_GET, ex);
                        }
                        break;
                    case 1:
                        try
                        {
                            //Check if input is full path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                            subKey = CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\") + 1);
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                        }
                        catch (Exception)
                        {
                            //Check if input is relative path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                            subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1) + CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString();
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                            if (newKey == null)
                            {
                                try
                                {
                                    //Check if input is hive
                                    if (CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Contains("\\"))
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                                    }
                                    else
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString());
                                    }
                                    subKey = "";
                                    newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                                }
                                catch (Exception)
                                {
                                    //Input is filter
                                    hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                                    subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1);
                                    newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                                    interpreter = new Interpreter(CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString());
                                }
                            }
                        }
                        try
                        {
                            keys = newKey.GetSubKeyNames();
                            if (interpreter == null)
                            {
                                for (int i = 0; i < keys.Length; i++)
                                {
                                    IOInteractLayer.StandardOutput(CMD_SK_GET, "\n" + keys[i]);
                                }
                            }
                            else
                            {
                                for (int i = 0; i < keys.Length; i++)
                                {
                                    if (interpreter.GetResult(keys[i]))
                                    {
                                        IOInteractLayer.StandardOutput(CMD_SK_GET, "\n" + keys[i]);
                                    }   
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_SK_GET, ex);
                        }
                        break;
                    case 2:
                        try
                        {
                            //Check if input is full path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                            subKey = CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\") + 1);
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                        }
                        catch (Exception)
                        {
                            //Check if input relative full path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                            subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1) + CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString();
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                            if (newKey == null)
                            {
                                try
                                {
                                    //Check if input is hive
                                    if (CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Contains("\\"))
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                                    }
                                    else
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_SK_GET.InputArgumentEntry.Arguments[0].Value.ToString());
                                    }
                                    subKey = "";
                                    newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                                }
                                catch (Exception)
                                {
                                    return "\nSubkey not found!";
                                }
                            }
                        }
                        try
                        {
                            interpreter = new Interpreter(CMD_SK_GET.InputArgumentEntry.Arguments[1].Value.ToString());
                            keys = newKey.GetSubKeyNames();
                            for (int i = 0; i < keys.Length; i++)
                            {
                                if (interpreter.GetResult(keys[i]))
                                {
                                    IOInteractLayer.StandardOutput(CMD_SK_GET, "\n" + keys[i]);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_SK_GET, ex);
                        }
                        break;
                }
                return "";
            });
            return CMD_SK_GET;
        }
    }
}
