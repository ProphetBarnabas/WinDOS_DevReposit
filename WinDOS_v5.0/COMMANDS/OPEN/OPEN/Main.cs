using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OPEN
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_OPEN;

        public string path;

        public CountdownEvent c;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[file path]"));
            CMD_OPEN = new Command("OPEN", TABLE, false, "Displays the editable contents of the specified file. Press 'F1' to save, press 'F2' to save and close, press 'ESC' to discard and colse.", ExecutionLevel.User, CLIMode.Default);
            CMD_OPEN.SetAsyncFunction(async () => 
            {
                await Task.Delay(0);
                c = new CountdownEvent(1);
                if (File.Exists(CMD_OPEN.InputArgumentEntry.Arguments[0].Value.ToString()) && CMD_OPEN.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                {
                    path = CMD_OPEN.InputArgumentEntry.Arguments[0].Value.ToString();
                }
                else if (File.Exists(EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_OPEN.InputArgumentEntry.Arguments[0].Value.ToString()))
                {
                    path = EnvironmentVariables.GetCurrentValue("DIRECTORY") + CMD_OPEN.InputArgumentEntry.Arguments[0].Value.ToString();
                }
                else
                {
                    return "\nFile not found!";
                }
                EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Text", false);
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                {
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Clear();
                    IOInteractLayer.StandardOutput(CMD_OPEN, File.ReadAllText(path));
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown += Main_PreviewKeyDown;
                });
                c.Wait();
                return "";
            });
            return CMD_OPEN;
        }

        private void Main_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.F2)
            {
                File.WriteAllText(path, ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Text);
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Clear();
                EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Default", false);
                c.Signal();
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_PreviewKeyDown;
            }
            else if (e.Key == Key.F1)
            {
                File.WriteAllText(path, ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Text);
            }
            else if (e.Key == Key.Escape)
            {
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Clear();
                EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Default", false);
                c.Signal();
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_PreviewKeyDown;
            }
        }
    }
}
