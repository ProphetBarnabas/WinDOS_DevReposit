using CLIShell;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CONFIG_RUNNING
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_CONFIG_RUNNING;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-save", false));
            TABLE.Add(new CommandArgumentEntry("-save=[string]", false, "-save=[file path]"));
            TABLE.Add(new CommandArgumentEntry("-load=[string]", false, "-load=[file path]"));
            CMD_CONFIG_RUNNING = new Command("CONFIG-RUNNING", TABLE, false, "Saves/loads running config from/to the specified file.", ExecutionLevel.User, CLIMode.Default);
            CMD_CONFIG_RUNNING.SetFunction(() =>
            {
                if (CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments.Count == 0)
                {
                    EnvironmentVariables.GetAll().ForEach((x) =>
                    {
                        if (x.VarType == VariableType.Constant)
                        {
                            IOInteractLayer.StandardOutput(CMD_CONFIG_RUNNING, $"\n\t{x.Name} = {x.CurrentValue}");
                        }    
                    });
                }
                else if (CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value != null && CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString() != "")
                {
                    if (CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Call == "-save")
                    {
                        string path = string.Empty;
                        if (CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString().Contains("\\"))
                        {
                            if (Directory.Exists(CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString().Remove(CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString().LastIndexOf('\\'))) && CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString().Contains(':'))
                            {
                                path = CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString();
                            }
                            else if (Directory.Exists(EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString().Remove(CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString().LastIndexOf('\\'))))
                            {
                                path = EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString();
                            }
                        }
                        else
                        {
                            path = EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString();
                        }
                        if (path != string.Empty)
                        {
                            using (StreamWriter sw = new StreamWriter(CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString()))
                            {
                                EnvironmentVariables.FindAll(x => x.VarType == VariableType.Constant).ForEach((x) =>
                                {
                                    sw.WriteLine($"{x.ValueType.FullName}:{x.Name}={x.CurrentValue}");
                                });
                                sw.Close();
                            }
                            return $"\nRunning config saved at: {Path.GetFullPath(path)}";
                        }
                        else
                        {
                            IOInteractLayer.StandardOutput(CMD_CONFIG_RUNNING, "\nInvalid path!");
                        }
                    }
                    else
                    {
                        string path = string.Empty;
                        if (File.Exists(CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString()) && CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString().Contains(':'))
                        {
                            path = CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString();
                        }
                        else if (File.Exists(EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString()))
                        {
                            path = EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + CMD_CONFIG_RUNNING.InputArgumentEntry.Arguments[0].Value.ToString();
                        }
                        if (path != string.Empty)
                        {
                            List<string> lines = File.ReadAllLines(path).ToList();
                            Type currentType;
                            string name;
                            string value;
                            lines.ForEach((x) =>
                            {
                                currentType = Type.GetType(x.Split(':')[0]);
                                x = x.Remove(0, x.IndexOf(':') + 1);
                                name = x.Split('=')[0];
                                value = x.Split('=')[1];
                                try
                                {
                                    EnvironmentVariables.ChangeCurrentValue(name, value);
                                }
                                catch (EnvironmentVariableException ex)
                                {
                                    IOInteractLayer.StandardError(CMD_CONFIG_RUNNING, ex);
                                }
                            });
                            return $"\nRunning config loaded from: {Path.GetFullPath(path)}";
                        }
                        else
                        {
                            IOInteractLayer.StandardOutput(CMD_CONFIG_RUNNING, "\nConfig file not found!");
                        }
                    }
                }
                else
                {
                    using (StreamWriter sw = new StreamWriter("config.ini"))
                    {
                        List<string> names = new List<string>();
                        EnvironmentVariables.FindAll(x => x.VarType == VariableType.Constant).ForEach((x) =>
                        {
                            sw.WriteLine($"{x.ValueType.FullName}:{x.Name}={x.CurrentValue}");
                            names.Add(x.Name);
                        });
                        sw.Close();
                        names.ForEach((x) =>
                        {
                            EnvironmentVariables.ChangeDefaultValue(x, EnvironmentVariables.GetCurrentValue(x), false);
                        });
                    }
                    return $"\nRunning config saved at: {Path.GetFullPath("config.ini")}";
                }
                return "";
            });
            return CMD_CONFIG_RUNNING;
        }
    }
}
