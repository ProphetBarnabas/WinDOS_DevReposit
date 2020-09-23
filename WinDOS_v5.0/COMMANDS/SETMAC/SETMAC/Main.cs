using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CLIShell;
using ToolShed.ThisPC;

namespace SETMAC
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_SETMAC;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] [string]", false, "[interface name] [MAC address]"));
            CMD_SETMAC = new Command("SETMAC", TABLE, false, "Changes MAC address of the specified network interface.", ExecutionLevel.Administrator, CLIMode.Default);
            CMD_SETMAC.SetFunction(() =>
            {
                List<NetworkInterface> ifaces = new NetworkInterfaceDiscovery().GetInterfaces().Result.ToList();
                if (ifaces.Exists(x => x.Name == CMD_SETMAC.InputArgumentEntry.Arguments[0].Value.ToString()))
                {
                    PhysicalAddressManagement p_mgmt = new PhysicalAddressManagement(ifaces.Find(x => x.Name == CMD_SETMAC.InputArgumentEntry.Arguments[0].Value.ToString()));
                    if (Regex.IsMatch(CMD_SETMAC.InputArgumentEntry.Arguments[1].Value.ToString(), "^[0-9a-fA-F:]*$"))
                    {
                        p_mgmt.SetAddress(CMD_SETMAC.InputArgumentEntry.Arguments[1].Value.ToString().Replace(":", ""));
                    }
                    else
                    {
                        return "\nInvalid address!";
                    }
                }
                else
                {
                    return "\nInterface not found!";
                }
                return "";
            });
            return CMD_SETMAC;
        }
    }
}
