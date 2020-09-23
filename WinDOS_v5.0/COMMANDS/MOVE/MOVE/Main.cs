using CLIShell;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MOVE
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_MOVE;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[source path] [destination directory]"));

            CMD_MOVE = new Command("MOVE", TABLE, false, "Moves the specified file system entry to the specified location.", ExecutionLevel.User, CLIMode.Default);
            CMD_MOVE.SetFunction(() => 
            {
                string in_path = (string)CMD_MOVE.InputArgumentEntry.Arguments[0].Value;
                string full_in_path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + in_path;
                string out_path = (string)CMD_MOVE.InputArgumentEntry.Arguments[1].Value;
                string full_out_path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + out_path;
                if (File.Exists(in_path) && in_path.Contains(":"))
                {
                    if (Directory.Exists(out_path) && out_path.Contains(":"))
                    {
                        File.Move(in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(in_path));
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        File.Move(in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(in_path));
                    }
                    else
                    {
                        return "\nInvalid destination path!";
                    }
                }
                else if (File.Exists(full_in_path))
                {
                    if (Directory.Exists(out_path) && out_path.Contains(":"))
                    {
                        File.Move(full_in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(full_in_path));
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        File.Move(full_in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(full_in_path));
                    }
                    else
                    {
                        return "\nInvalid destination path!";
                    }
                }
                else if (Directory.Exists(in_path) && in_path.Contains(":"))
                {
                    if (Directory.Exists(out_path) && out_path.Contains(":"))
                    {
                        Directory.Move(in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(in_path).Name);
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        Directory.Move(in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(in_path).Name);
                    }
                    else
                    {
                        return "\nInvalid destination path!";
                    }
                }
                else if (Directory.Exists(full_in_path))
                {
                    if (Directory.Exists(out_path) && out_path.Contains(":"))
                    {
                        Directory.Move(full_in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(full_in_path).Name);
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        Directory.Move(full_in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(full_in_path).Name);
                    }
                    else
                    {
                        return "\nInvalid destination path!";
                    }
                }
                else
                {
                    return "\nFile/directory not found!";
                }
                return "";
            });
            return CMD_MOVE;
        }
    }
}
