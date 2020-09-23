using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace VAL_DELETE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_VAL_DELETE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[name/filter]"));
            CMD_VAL_DELETE = new Command("VAL-DELETE", TABLE, true, "Removes all values that matches the specified filter in the current subkey.", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_VAL_DELETE.SetFunction(() => 
            {
                RegistryKey key = RegistryKey.OpenBaseKey((RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\"))), RegistryView.Default).OpenSubKey(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") +1), true);
                Interpreter interpreter = new Interpreter(CMD_VAL_DELETE.InputArgumentEntry.Arguments[0].Value.ToString());
                string[] names = key.GetValueNames();
                int removed = 0;
                try
                {

                    for (int i = 0; i < names.Length; i++)
                    {
                        if (interpreter.GetResult(names[i]))
                        {
                            key.DeleteValue(names[i]);
                            removed++;
                        }
                    }
                }
                catch (Exception ex)
                {
                    IOInteractLayer.StandardError(CMD_VAL_DELETE, ex);
                }
                return $"\nRemoved: {removed} values";
            });
            return CMD_VAL_DELETE;
        }
    }
}
