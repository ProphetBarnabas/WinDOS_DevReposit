using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ToolShed.ThisPC;

namespace RESETMAC
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_RESETMAC;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[interface name]"));
            CMD_RESETMAC = new Command("RESETMAC", TABLE, false, "Resets the MAC address of the specified network interface.", ExecutionLevel.Administrator, CLIMode.Default);
            CMD_RESETMAC.SetFunction(() =>
            {
                List<NetworkInterface> iface = new NetworkInterfaceDiscovery().GetInterfaces().Result.ToList();
                if (iface.Exists(x => x.Name == CMD_RESETMAC.InputArgumentEntry.Arguments[0].Value.ToString()))
                {
                    PhysicalAddressManagement p_mgmt = new PhysicalAddressManagement(iface.Find(x => x.Name == CMD_RESETMAC.InputArgumentEntry.Arguments[0].Value.ToString()));
                    p_mgmt.ResetAddress();
                    return "";
                }
                else
                {
                    return "\nInterface not found!";
                }
            });
            return CMD_RESETMAC;
        }
    }
}
