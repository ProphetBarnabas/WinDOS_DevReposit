using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;

namespace SHMOTD
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_SHMOTD;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-n", false, "-n(disable MOTD)"));
            CMD_SHMOTD = new Command("SHMOTD", TABLE, false, "Enables/disables message of the day.", ExecutionLevel.User, CLIMode.Default);
            CMD_SHMOTD.SetFunction(() => 
            {
                if (CMD_SHMOTD.InputArgumentEntry.Arguments.Count == 0)
                {
                    EnvironmentVariables.ChangeCurrentValue("SHMOTD", true);
                    EnvironmentVariables.ChangeDefaultValue("SHMOTD", true);
                }
                else
                {
                    EnvironmentVariables.ChangeCurrentValue("SHMOTD", false);
                    EnvironmentVariables.ChangeDefaultValue("SHMOTD", false);
                }
                return "";
            });
            return CMD_SHMOTD;
        }
    }
}
