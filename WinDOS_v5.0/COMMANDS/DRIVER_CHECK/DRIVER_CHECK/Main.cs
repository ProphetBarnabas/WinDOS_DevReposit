using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ToolShed.ThisPC;

namespace DRIVER_CHECK
{
    public class Main
    {
        public Command CMD_DRIVER_CHECK = new Command("DRIVER-CHECK", null, false, "Searches for driver issues and returns driver information accordingly.", ExecutionLevel.User, CLIMode.Default);

        public Command GetCommand()
        {
            CMD_DRIVER_CHECK.SetFunction(() => 
            {
                DriverInfo inf = new DriverInfo();
                DriverInfo[] issues = inf.GetDriverIssues();
                if (issues == null || issues.Length == 0)
                {
                    return "\nNo issues found.";
                }
                for (int i = 0; i < issues.Length; i++)
                {
                    IOInteractLayer.StandardOutput(CMD_DRIVER_CHECK, $"\nAccept pause: {issues[i].AcceptPause}" +
                                                                  $"\nAccept stop: {issues[i].AcceptStop}" +
                                                                  $"\nCaption: {issues[i].Caption}" +
                                                                  $"\nCreation class name: {issues[i].CreationClassName}" +
                                                                  $"\nDescription: {issues[i].Description}" +
                                                                  $"\nDesktop interact: {issues[i].DesktopInteract}" +
                                                                  $"\nDisplay name: {issues[i].DisplayName}" +
                                                                  $"\nError control: {issues[i].ErrorControl}" +
                                                                  $"\nExit code: {issues[i].ExitCode}" +
                                                                  $"\nInstall date: {issues[i].InstallDate}" +
                                                                  $"\nName: {issues[i].Name}" +
                                                                  $"\nPath name: {issues[i].PathName}" +
                                                                  $"\nService specific exit code: {issues[i].ServiceSpecificExitCode}" +
                                                                  $"\nService type: {issues[i].ServiceType}" +
                                                                  $"\nStarted: {issues[i].Started}" +
                                                                  $"\nStart mode: {issues[i].StartMode}" +
                                                                  $"\nStart name: {issues[i].StartName}" +
                                                                  $"\nState: {issues[i].State}" +
                                                                  $"\nStatus: {issues[i].Status}" +
                                                                  $"\nSystem creation class name: {issues[i].SystemCreationClassName}" +
                                                                  $"\nSystem name: {issues[i].SystemName}" +
                                                                  $"\nTag ID: {issues[i].TagId}\n");
                }
                return "";
            });
            return CMD_DRIVER_CHECK;
        }
    }
}
