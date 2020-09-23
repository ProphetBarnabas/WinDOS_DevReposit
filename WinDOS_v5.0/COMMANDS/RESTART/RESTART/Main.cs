using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CLIShell;

namespace RESTART
{
    public class Main
    {
        public Command CMD_RESTART = new Command("RESTART", null, false, "Restarts WinDOS.", ExecutionLevel.User, CLIMode.Any);

        public Command GetCommand()
        {
            CMD_RESTART.SetFunction(() => 
            {
                System.Windows.Forms.Application.Restart();
                Application.Current.Shutdown();
                return "";
            });
            
            return CMD_RESTART;
        }
    }
}
