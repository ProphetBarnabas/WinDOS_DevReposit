using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CLIShell;
using ToolShed.Networking;
using ToolShed.ThisPC;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;
using System.Diagnostics;
using System.Threading;
using System.Net.NetworkInformation;
using System.Net;
using System.Text.RegularExpressions;

namespace NMAP
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_NMAP;

        public DeviceDiscovery disc = new DeviceDiscovery();

        private CancellationTokenSource cts;

        private CancellationToken ct;

        private Process cmdProcess;

        private CountdownEvent countdown;

        private Ping ping;

        public async Task GetDevices(string[] ipAddresses, ToolShed.ThisPC.NetworkInterface[] interfaces, bool resolveHostnames)
        {
            await Task.Delay(0);
            cts = new CancellationTokenSource();
            ct = cts.Token;
            countdown = new CountdownEvent(1);
            async Task GHE_TASK(string entry)
            {
                if (!ct.IsCancellationRequested)
                {
                    await Task.Delay(0);
                    string name = "";
                    try
                    {
                        IPHostEntry hostEntry = Dns.GetHostEntry(entry.Split(' ')[0]);
                        name = hostEntry.HostName;
                    }
                    catch (Exception)
                    {
                        name = "Unknown";
                    }
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            IOInteractLayer.StandardOutput(CMD_NMAP, $"\nName: {name} || IP address: {entry.Split(' ')[0]} || MAC address: {entry.Split(' ')[1].Replace("-", ":").ToUpper()}");
                        }
                        catch (Exception) { }
                    });
                }
            }

            Regex spacefix = new Regex("[ ]{2,}");
            for (int i = 0; i < ipAddresses.Length; i++)
            {
                if (!ct.IsCancellationRequested)
                {
                    ping = new Ping();
                    ping.PingCompleted += Ping_PingCompleted;
                    countdown.AddCount();
                    try
                    {
                        ping.SendAsync(ipAddresses[i], 100);
                    }
                    catch (Exception) {}
                    GC.Collect();
                }
                else
                {
                    return;
                }
            }
            countdown.Signal();
            countdown.Wait();
            string[] entries = null;
            List<Task> taskList = new List<Task>();
            List<string> checked_ips = new List<string>();
            for (int i = 0; i < interfaces.Length; i++)
            {
                if (interfaces[i].Status == "Up" && interfaces[i].Type != "Loopback" && interfaces[i].Type != "Tunnel")
                {
                    if (!ct.IsCancellationRequested)
                    {
                        cmdProcess = new Process();
                        cmdProcess.StartInfo.CreateNoWindow = true;
                        cmdProcess.StartInfo.UseShellExecute = false;
                        cmdProcess.StartInfo.FileName = "arp";
                        cmdProcess.StartInfo.Arguments = "-a -N " + interfaces[i].IPAddress;
                        cmdProcess.StartInfo.RedirectStandardOutput = true;
                        cmdProcess.Start();
                        entries = cmdProcess.StandardOutput.ReadToEnd().Split('\n');
                        cmdProcess.Close();
                        for (int j = 3; j < entries.Length - 1; j++)
                        {
                            if (!ct.IsCancellationRequested)
                            {
                                entries[j] = spacefix.Replace(entries[j].Trim(), " ");
                                if (!checked_ips.Contains(entries[j].Split(' ')[0]))
                                {
                                    checked_ips.Add(entries[j].Split(' ')[0]);
                                    if (resolveHostnames)
                                    {
                                        taskList.Add(GHE_TASK(entries[j]));
                                    }
                                    else
                                    {
                                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                                        {
                                            IOInteractLayer.StandardOutput(CMD_NMAP, $"\nIP address: {entries[j].Split(' ')[0]} || MAC address: {entries[j].Split(' ')[1].Replace("-", ":").ToUpper()}"); 
                                        });
                                    }
                                }
                            }
                            else
                            {
                                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                                {
                                    IOInteractLayer.StandardOutput(CMD_NMAP, "\nProcess interrupted.");
                                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).KeyDown -= Main_KeyDown;
                                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                                });
                                return;
                            }
                        }
                        await Task.WhenAll(taskList);
                    }
                    else
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                        {
                            IOInteractLayer.StandardOutput(CMD_NMAP, "\nProcess interrupted.");
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).KeyDown -= Main_KeyDown;
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                        });
                        return;
                    }
                }
            }
        }

        private void Ping_PingCompleted(object sender, PingCompletedEventArgs e)
        {
            countdown.Signal();
        }

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("-rhost", false, "-rhost(resolve hostnames)"));
            CMD_NMAP = new Command("NMAP", TABLE, false, "Attempts to discover all connacted devices on all connected networks.", ExecutionLevel.User, CLIMode.Default);
            CMD_NMAP.SetAsyncFunction(async () =>
            {
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                {
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = true;
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).KeyDown += Main_KeyDown;
                    IOInteractLayer.StandardOutput(CMD_NMAP, "\n");
                });
                NetworkInterfaceDiscovery nics = new NetworkInterfaceDiscovery();
                ToolShed.ThisPC.NetworkInterface[] interfaces = await nics.GetInterfaces();
                NetworkInfo inf = new NetworkInfo();
                string[] ip_arr = inf.GetAddresses(interfaces);
                await GetDevices(ip_arr, interfaces, CMD_NMAP.InputArgumentEntry.Arguments.Exists(x => x.Call == "-rhost"));
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                {
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).KeyDown -= Main_KeyDown;
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                });
                return "";
            });
            return CMD_NMAP;
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                cts.Cancel();
            }
        }
    }
}
