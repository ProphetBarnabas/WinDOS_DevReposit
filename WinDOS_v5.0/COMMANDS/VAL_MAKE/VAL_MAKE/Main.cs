using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CLIShell;
using Microsoft.Win32;

namespace VAL_MAKE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_VAL_MAKE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] [string] [string]", false, "[type(STR/EXSTR/MSTR/DWORD/QWORD/BINARY)] [name] [value]"));
            CMD_VAL_MAKE = new Command("VAL-MAKE", TABLE, false, "Creates a new value in the current subkey.", ExecutionLevel.Administrator, CLIMode.Regedit);
            CMD_VAL_MAKE.SetFunction(() => 
            {
                RegistryKey key = RegistryKey.OpenBaseKey((RegistryHive)Enum.Parse(typeof(RegistryHive), EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(0, EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\"))), RegistryView.Default).OpenSubKey(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().Substring(EnvironmentVariables.GetCurrentValue("SUBKEY").ToString().IndexOf("\\") + 1), true);
                string name = CMD_VAL_MAKE.InputArgumentEntry.Arguments[1].Value.ToString();
                string value = CMD_VAL_MAKE.InputArgumentEntry.Arguments[2].Value.ToString();
                string type_str = CMD_VAL_MAKE.InputArgumentEntry.Arguments[0].Value.ToString();
                RegistryValueKind type;
                switch (type_str)
                {
                    case "STR":
                        type = RegistryValueKind.String;
                        break;
                    case "EXSTR":
                        type = RegistryValueKind.ExpandString;
                        break;
                    case "MSTR":
                        type = RegistryValueKind.MultiString;
                        break;
                    case "DWORD":
                        type = RegistryValueKind.DWord;
                        break;
                    case "QWORD":
                        type = RegistryValueKind.QWord;
                        break;
                    case "BINARY":
                        type = RegistryValueKind.Binary;
                        break;
                    default:
                        return "\nInvalid type!";
                }
                try
                {
                    if (key.GetValueNames().Contains(name))
                    {
                        return "\nValue already exists!";
                    }
                    else
                    {
                        key.SetValue(name, value, type);
                        return "";
                    }
                }
                catch (Exception ex)
                {
                    return "\n" + ex.Message;
                }
            });
            return CMD_VAL_MAKE;
        }
    }
}
