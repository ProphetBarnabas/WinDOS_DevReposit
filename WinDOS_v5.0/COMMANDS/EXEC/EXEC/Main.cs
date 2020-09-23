using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CLIShell;
using ICSharpCode.AvalonEdit;

namespace EXEC
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_EXEC;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[file path]"));
            CMD_EXEC = new Command("EXEC", TABLE, false, "Executes commands from the specified file(one command per line). Asynchronous commands will not wait for the previous command to finish.", ExecutionLevel.User, CLIMode.Default);
            CMD_EXEC.SetAsyncFunction(async () =>
            {
                string path = null;
                if (File.Exists(CMD_EXEC.InputArgumentEntry.Arguments[0].Value.ToString()) && CMD_EXEC.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    path = CMD_EXEC.InputArgumentEntry.Arguments[0].Value.ToString();
                }
                else if (File.Exists(EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_EXEC.InputArgumentEntry.Arguments[0].Value.ToString()))
                {
                    path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_EXEC.InputArgumentEntry.Arguments[0].Value.ToString();
                }
                else
                {
                    return "\nInvalid path!";
                }
                string[] lines = File.ReadAllLines(path);
                Command[] commands = new Command[lines.Length];
                for (int i = 0; i < lines.Length; i++)
                {
                    commands[i] = ((CommandManagement)EnvironmentVariables.GetCurrentValue("CMD_MGMT")).GetCommand(lines[i].Trim(), (CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")) == CLIMode.Default ? (CommandPool)EnvironmentVariables.GetCurrentValue("DEF_CMD_POOL") : (CommandPool)EnvironmentVariables.GetCurrentValue("REG_CMD_POOL"));
                }
                for (int i = 0; i < commands.Length; i++)
                {
                    try
                    {
                        if (commands[i].AsyncFunction == null)
                        {
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                            {
                                ((CommandManagement)EnvironmentVariables.GetCurrentValue("CMD_MGMT")).ExecuteCommand(commands[i]);
                            });      
                        }
                        else
                        {
                            await ((CommandManagement)EnvironmentVariables.GetCurrentValue("CMD_MGMT")).ExecuteAsyncCommand(commands[i]);
                        }
                    }
                    catch (Exception ex)
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                        {
                            IOInteractLayer.StandardError(commands[i], ex);
                        });       
                    }
                }
                return "";
            });
            return CMD_EXEC;
        }
    }
}
