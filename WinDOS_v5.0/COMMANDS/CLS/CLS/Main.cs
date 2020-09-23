using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ICSharpCode.AvalonEdit;

namespace CLS
{
    public class Main
    {
        public Command CMD_CLS = new Command("CLS", null, false, "Clears the text area.", ExecutionLevel.User, CLIMode.Any);

        public Command GetCommand()
        {
            CMD_CLS.SetFunction(() => { ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Clear(); return ""; });
            return CMD_CLS;
        }
    }
}
