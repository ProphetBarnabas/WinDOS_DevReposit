using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace VAL_GET
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_VAL_GET;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[registry path/value name filter]"));
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[registry path] [value name filter]"));
            CMD_VAL_GET = new Command("VAL-GET", TABLE, true, "Returns all values in the specified subkey(current subkey if not specified).", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_VAL_GET.SetFunction(() =>
            {
                RegistryHive hiveKey;
                string subKey = null;
                RegistryKey newKey = null;
                string[] values = null;
                Interpreter interpreter = null;
                switch (CMD_VAL_GET.InputArgumentEntry.Arguments.Count)
                {
                    case 0:
                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                        subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1);
                        newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                        try
                        {
                            values = newKey.GetValueNames();
                            for (int i = 0; i < values.Length; i++)
                            {
                                IOInteractLayer.StandardOutput(CMD_VAL_GET, $"\n{newKey.GetValueKind(values[i])}:{values[i]}={newKey.GetValue(values[i])}");
                            }
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_VAL_GET, ex);
                        }
                        break;
                    case 1:
                        try
                        {
                            //Check if input is full path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                            subKey = CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\") + 1);
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                        }
                        catch (Exception)
                        {
                            //Check if input is relative path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                            subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1) + CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString();
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                            if (newKey == null)
                            {
                                try
                                {
                                    //Check if input is hive
                                    if (CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Contains("\\"))
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                                    }
                                    else
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString());
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
                                    interpreter = new Interpreter(CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString());
                                }
                            }
                        }
                        try
                        {
                            values = newKey.GetValueNames();
                            if (interpreter == null)
                            {
                                for (int i = 0; i < values.Length; i++)
                                {
                                    IOInteractLayer.StandardOutput(CMD_VAL_GET, $"\n{newKey.GetValueKind(values[i])}:{values[i]}={newKey.GetValue(values[i])}");
                                }
                            }
                            else
                            {
                                for (int i = 0; i < values.Length; i++)
                                {
                                    if (interpreter.GetResult(values[i]))
                                    {
                                        IOInteractLayer.StandardOutput(CMD_VAL_GET, $"\n{newKey.GetValueKind(values[i])}:{values[i]}={newKey.GetValue(values[i])}");
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_VAL_GET, ex);
                        }
                        break;
                    case 2:
                        try
                        {
                            //Check if input is full path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                            subKey = CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\") + 1);
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                        }
                        catch (Exception)
                        {
                            //Check if input relative full path
                            hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\")));
                            subKey = EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1) + CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString();
                            newKey = RegistryKey.OpenBaseKey(hiveKey, RegistryView.Default).OpenSubKey(subKey);
                            if (newKey == null)
                            {
                                try
                                {
                                    //Check if input is hive
                                    if (CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Contains("\\"))
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().Substring(0, CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString().IndexOf("\\")));
                                    }
                                    else
                                    {
                                        hiveKey = (RegistryHive)Enum.Parse(typeof(RegistryHive), CMD_VAL_GET.InputArgumentEntry.Arguments[0].Value.ToString());
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
                            interpreter = new Interpreter(CMD_VAL_GET.InputArgumentEntry.Arguments[1].Value.ToString());
                            values = newKey.GetValueNames();
                            for (int i = 0; i < values.Length; i++)
                            {
                                if (interpreter.GetResult(values[i]))
                                {
                                    IOInteractLayer.StandardOutput(CMD_VAL_GET, $"\n{newKey.GetValueKind(values[i])}:{values[i]}={newKey.GetValue(values[i])}");
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            IOInteractLayer.StandardError(CMD_VAL_GET, ex);
                        }
                        break;
                }
                return "";
            });
            return CMD_VAL_GET;
        }
    }
}
