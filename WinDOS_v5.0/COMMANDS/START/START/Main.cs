using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace START
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_START;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[file path]"));
            TABLE.Add(new CommandArgumentEntry("[string] [string]", true, "[file path] [arguments]"));
            CMD_START = new Command("START", TABLE, false, "Attempts to start the specified executable, with the specified arguments.", ExecutionLevel.User, CLIMode.Default);
            CMD_START.SetFunction(() =>
            {
                if (CMD_START.InputArgumentEntry.Arguments.Count == 0)
                {
                    return "\nInvalid arguments!";
                }
                Process p = null;
                string path = null;
                if (File.Exists(CMD_START.InputArgumentEntry.Arguments[0].Value.ToString()) && CMD_START.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    path = CMD_START.InputArgumentEntry.Arguments[0].Value.ToString();
                }
                else if (File.Exists(EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_START.InputArgumentEntry.Arguments[0].Value.ToString()))
                {
                    path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_START.InputArgumentEntry.Arguments[0].Value.ToString();
                }
                else
                {
                    return "\nExecutable not found!";
                }
                p = new Process();
                p.StartInfo.FileName = path;
                p.StartInfo.WorkingDirectory = path.Replace(Path.GetFileName(path), "");
                p.StartInfo.UseShellExecute = false;
                p.StartInfo.CreateNoWindow = false;
                if (CMD_START.InputArgumentEntry.Arguments.Count == 2)
                {
                    p.StartInfo.Arguments = CMD_START.InputArgumentEntry.Arguments[1].Value.ToString();
                }
                try
                {
                    p.Start();
                }
                catch (Exception ex)
                {
                    IOInteractLayer.StandardError(CMD_START, ex);
                }
                return "";
            });
            return CMD_START;
        }
    }
}
