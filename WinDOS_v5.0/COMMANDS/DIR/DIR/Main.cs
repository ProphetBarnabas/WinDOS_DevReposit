using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace DIR
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_DIR;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[path/result filter]"));
            TABLE.Add(new CommandArgumentEntry("[string] [string]", true, "[path] [result filter]"));
            CMD_DIR = new Command("DIR", TABLE, true, "Returns the file system entries contained by the current directory, or the provided relative/full path.", ExecutionLevel.User, CLIMode.Default);
            CMD_DIR.SetFunction(() =>
            {
                string[] entries = null;
                if (CMD_DIR.InputArgumentEntry.Arguments.Count == 0)
                {
                    entries = Directory.GetFileSystemEntries((string)EnvironmentVariables.GetCurrentValue("DIRECTORY"));
                    for (int i = 0; i < entries.Length; i++)
                    {
                        IOInteractLayer.StandardOutput(CMD_DIR, "\n\t" + entries[i]);
                    }
                }
                else
                {
                    Interpreter interpreter;
                    string dir = null;
                    if (CMD_DIR.InputArgumentEntry.Arguments.Count == 1)
                    {
                        dir = Directory.Exists((string)CMD_DIR.InputArgumentEntry.Arguments[0].Value) && CMD_DIR.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":") ? (string)CMD_DIR.InputArgumentEntry.Arguments[0].Value : Directory.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_DIR.InputArgumentEntry.Arguments[0].Value) ? (string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_DIR.InputArgumentEntry.Arguments[0].Value : null;
                        if (dir == null)
                        {
                            interpreter = new Interpreter((string)CMD_DIR.InputArgumentEntry.Arguments[0].Value);
                            entries = Directory.GetFileSystemEntries((string)EnvironmentVariables.GetCurrentValue("DIRECTORY"));
                            for (int i = 0; i < entries.Length; i++)
                            {
                                if (interpreter.GetResult(entries[i]))
                                {
                                    IOInteractLayer.StandardOutput(CMD_DIR, "\n\t" + entries[i]);
                                }
                            }
                        }
                        else
                        {
                            entries = Directory.GetFileSystemEntries(dir);
                            for (int i = 0; i < entries.Length; i++)
                            {
                                IOInteractLayer.StandardOutput(CMD_DIR, "\n\t" + entries[i]);
                            }
                        }
                    }
                    else
                    {
                        dir = Directory.Exists((string)CMD_DIR.InputArgumentEntry.Arguments[0].Value) && CMD_DIR.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":") ? (string)CMD_DIR.InputArgumentEntry.Arguments[0].Value : Directory.Exists((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_DIR.InputArgumentEntry.Arguments[0].Value) ? (string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_DIR.InputArgumentEntry.Arguments[0].Value : null;
                        
                        if (dir != null)
                        {
                            interpreter = new Interpreter((string)CMD_DIR.InputArgumentEntry.Arguments[1].Value);
                            entries = Directory.GetFileSystemEntries(dir);
                            for (int i = 0; i < entries.Length; i++)
                            {
                                if (interpreter.GetResult(entries[i]))
                                {
                                    IOInteractLayer.StandardOutput(CMD_DIR, "\n\t" + entries[i]);
                                }
                            }
                        }
                    }
                }
                return "";
            });
            return CMD_DIR;
        }
    }
}
