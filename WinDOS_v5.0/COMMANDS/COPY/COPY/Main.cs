using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace COPY
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_COPY;

        public void CopyDir(string source, string dest)
        {
            string[] files = Directory.GetFiles(source, "*", SearchOption.AllDirectories);
            for (int i = 0; i < files.Length; i++)
            {
                File.Copy(files[i], dest + (dest.EndsWith("\\") ? "" : "\\") + Path.GetFileName(files[i]));
            }
        }

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[source path] [destination directory]"));

            CMD_COPY = new Command("COPY", TABLE, false, "Copies the specified file/directory to the specified location.", ExecutionLevel.User, CLIMode.Default);

            CMD_COPY.SetFunction(() => 
            {
                string in_path = (string)CMD_COPY.InputArgumentEntry.Arguments[0].Value;
                string full_in_path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_COPY.InputArgumentEntry.Arguments[0].Value;
                string out_path = (string)CMD_COPY.InputArgumentEntry.Arguments[1].Value;
                string full_out_path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_COPY.InputArgumentEntry.Arguments[1].Value;
                if (File.Exists(in_path) && in_path.Contains(":"))
                {
                    if (Directory.Exists(out_path) && out_path.Contains(":"))
                    {
                        File.Copy(in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(in_path));
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        File.Copy(in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(in_path));
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
                        File.Copy(full_in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(full_in_path));
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        File.Copy(full_in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + Path.GetFileName(full_in_path));
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
                        CopyDir(in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(in_path).Name);
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        CopyDir(in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(in_path).Name);
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
                        CopyDir(full_in_path, out_path + (out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(full_in_path).Name);
                    }
                    else if (Directory.Exists(full_out_path))
                    {
                        CopyDir(full_in_path, full_out_path + (full_out_path.EndsWith("\\") ? "" : "\\") + new DirectoryInfo(full_in_path).Name);
                    }
                    else
                    {
                        return "\nInvalid destination path!";
                    }
                }
                else
                {
                    return "\nSource file/directory not found!";
                }
                return "";
            });
            return CMD_COPY;
        }
    }
}
