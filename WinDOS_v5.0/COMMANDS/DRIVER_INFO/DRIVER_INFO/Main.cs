using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ToolShed.ThisPC;

namespace DRIVER_INFO
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_DRIVER_INFO;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[driver name]"));
            CMD_DRIVER_INFO = new Command("DRIVER-INFO", TABLE, false, "Returns infromation about the specified driver.", ExecutionLevel.User, CLIMode.Default);
            CMD_DRIVER_INFO.SetFunction(() =>
            {
                if (CMD_DRIVER_INFO.InputArgumentEntry.Arguments.Count == 0)
                {
                    return "\nInvalid arguments!";
                }
                DriverInfo inf = new DriverInfo();
                List<DriverInfo> inf_list = inf.GetDriverInfo().ToList();
                if (inf_list.Exists(x => x.DisplayName == CMD_DRIVER_INFO.InputArgumentEntry.Arguments[0].Value.ToString()))
                {
                    inf = inf_list.Find(x => x.DisplayName == CMD_DRIVER_INFO.InputArgumentEntry.Arguments[0].Value.ToString());
                    IOInteractLayer.StandardOutput(CMD_DRIVER_INFO, $"\nAccept pause: {inf.AcceptPause}" +
                                                                    $"\nAccept stop: {inf.AcceptStop}" +
                                                                    $"\nCaption: {inf.Caption}" +
                                                                    $"\nCreation class name: {inf.CreationClassName}" +
                                                                    $"\nDescription: {inf.Description}" +
                                                                    $"\nDesktop interact: {inf.DesktopInteract}" +
                                                                    $"\nDisplay name: {inf.DisplayName}" +
                                                                    $"\nError control: {inf.ErrorControl}" +
                                                                    $"\nExit code: {inf.ExitCode}" +
                                                                    $"\nInstall date: {inf.InstallDate}" +
                                                                    $"\nName: {inf.Name}" +
                                                                    $"\nPath name: {inf.PathName}" +
                                                                    $"\nService specific exit code: {inf.ServiceSpecificExitCode}" +
                                                                    $"\nService type: {inf.ServiceType}" +
                                                                    $"\nStarted: {inf.Started}" +
                                                                    $"\nStart mode: {inf.StartMode}" +
                                                                    $"\nStart name: {inf.StartName}" +
                                                                    $"\nState: {inf.State}" +
                                                                    $"\nStatus: {inf.Status}" +
                                                                    $"\nSystem creation class name: {inf.SystemCreationClassName}" +
                                                                    $"\nSystem name: {inf.SystemName}" +
                                                                    $"\nTag ID: {inf.TagId}");
                }
                else
                {
                    IOInteractLayer.StandardOutput(CMD_DRIVER_INFO, "\nDriver not found!");
                }
                return "";
            });
            return CMD_DRIVER_INFO;
        }
    }
}
