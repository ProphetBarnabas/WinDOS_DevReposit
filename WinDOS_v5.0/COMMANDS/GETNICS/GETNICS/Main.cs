using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ToolShed.ThisPC;

namespace GETNICS
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_GETNICS;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-a", false, "-a(show all information)"));
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[name filter]"));
            TABLE.Add(new CommandArgumentEntry("[string] -a", true, "[name filter] -a(show all information)"));
            CMD_GETNICS = new Command("GETNICS", TABLE, true, "Returns interface information.", ExecutionLevel.User, CLIMode.Default);
            CMD_GETNICS.SetFunction(() =>
            {
                NetworkInterfaceDiscovery nics = new NetworkInterfaceDiscovery();
                NetworkInterface[] nic_array = nics.GetInterfaces().Result;
                if (CMD_GETNICS.InputArgumentEntry.Arguments.Count == 0)
                {
                    for (int i = 0; i < nic_array.Length; i++)
                    {
                        IOInteractLayer.StandardOutput(CMD_GETNICS, $"\nName: {nic_array[i].Name}\nIP Address: {nic_array[i].IPAddress}\nDefault gateway: {nic_array[i].DefaultGateway}\nSubnet mask: {nic_array[i].SubnetMask}\n");
                    }
                }
                else if (CMD_GETNICS.InputArgumentEntry.Arguments.Count == 1)
                {
                    if (CMD_GETNICS.InputArgumentEntry.Arguments[0].Call == "-a")
                    {
                        for (int i = 0; i < nic_array.Length; i++)
                        {
                            IOInteractLayer.StandardOutput(CMD_GETNICS, $"\nName: {nic_array[i].Name}\nIP address: {nic_array[i].IPAddress}\nDefault gateway: {nic_array[i].DefaultGateway}\nSubnet mask: {nic_array[i].SubnetMask}\nMAC address: {nic_array[i].MACAddress}\nInstance ID: {nic_array[i].InstanceID}\nSpeed: {nic_array[i].Speed}\nStatus: {nic_array[i].Status}\nType: {nic_array[i].Type}\nDescription: {nic_array[i].Description}\n");
                        }
                    }
                    else
                    {
                        Interpreter interpreter = new Interpreter(CMD_GETNICS.InputArgumentEntry.Arguments[0].Value.ToString());
                        for (int i = 0; i < nic_array.Length; i++)
                        {
                            if (interpreter.GetResult(nic_array[i].Name))
                            {
                                IOInteractLayer.StandardOutput(CMD_GETNICS, $"\nName: {nic_array[i].Name}\nIP Address: {nic_array[i].IPAddress}\nDefault gateway: {nic_array[i].DefaultGateway}\nSubnet mask: {nic_array[i].SubnetMask}\n");
                            }
                        }
                    }
                }
                else
                {
                    for (int i = 0; i < nic_array.Length; i++)
                    {
                        Interpreter interpreter = new Interpreter(CMD_GETNICS.InputArgumentEntry.Arguments[0].Value.ToString());
                        if (interpreter.GetResult(nic_array[i].Name))
                        {
                            IOInteractLayer.StandardOutput(CMD_GETNICS, $"\nName: {nic_array[i].Name}\nIP address: {nic_array[i].IPAddress}\nDefault gateway: {nic_array[i].DefaultGateway}\nSubnet mask: {nic_array[i].SubnetMask}\nMAC address: {nic_array[i].MACAddress}\nInstance ID: {nic_array[i].InstanceID}\nSpeed: {nic_array[i].Speed}\nStatus: {nic_array[i].Status}\nType: {nic_array[i].Type}\nDescription: {nic_array[i].Description}\n");
                        }
                    }
                }
                return "";
            });
            return CMD_GETNICS;
        }
    }
}
