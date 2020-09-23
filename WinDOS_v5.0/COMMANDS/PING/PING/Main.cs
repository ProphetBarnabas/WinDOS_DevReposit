using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CLIShell;
using ICSharpCode.AvalonEdit;
using System.Windows.Input;
using System.Diagnostics;

namespace PING
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_PING;

        public CancellationTokenSource CTS;

        public CancellationToken CT;

        public int Sent = 0;

        public int Received = 0;

        public int Lost = 0;

        public List<long> Times = new List<long>();

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string]", false, "[IP address/hostname]"));
            TABLE.Add(new CommandArgumentEntry("[string] -t", true, "[IP address/hostname] -t(ping indefinitely)"));

            CMD_PING = new Command("PING", TABLE, false, "", ExecutionLevel.User, CLIMode.Default);
            CMD_PING.SetAsyncFunction(async () =>
            {
                if (CMD_PING.InputArgumentEntry.Arguments.Count == 0)
                {
                    return "Invalid arguments!";
                }
                else
                {
                    Times = new List<long>();
                    Sent = 0;
                    Received = 0;
                    Lost = 0;
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = true;
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown += Main_PreviewKeyDown;
                    });
                    CTS = new CancellationTokenSource();
                    CT = CTS.Token;
                    Ping p = new Ping();
                    PingReply r = null;
                    async Task PING_TASK()
                    {
                        try
                        {
                            Sent++;
                            r = p.Send(CMD_PING.InputArgumentEntry.Arguments[0].Value.ToString(), 5000);
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                            {
                                IOInteractLayer.StandardOutput(CMD_PING, $"\nReply from {r.Address}: bytes={r.Buffer.Length} time={r.RoundtripTime}ms TTL={r.Options.Ttl}");
                                Times.Add(r.RoundtripTime);
                            });
                            Received++;
                        }
                        catch (PingException ex)
                        {
                            Lost++;
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                            {
                                IOInteractLayer.StandardError(CMD_PING, ex);
                            });
                        }
                        if (!CT.IsCancellationRequested && CMD_PING.InputArgumentEntry.Arguments.Exists(x => x.Call == "-t"))
                        {
                            await Task.Delay(1000);
                            await PING_TASK();
                        }
                    }

                    if (CMD_PING.InputArgumentEntry.Arguments.Exists(x => x.Call == "-t"))
                    {
                        await PING_TASK();
                    }
                    else
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            await PING_TASK();
                            await Task.Delay(1000);
                        }
                    }
                }
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                {
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_PreviewKeyDown;
                });
                return $"\nPing statistics for {CMD_PING.InputArgumentEntry.Arguments[0].Value}:\n\tPackets: Sent = {Sent}, Received = {Received}, Lost = {Lost}, ({Lost / Sent * 100:0.00}% loss),\nApproximate round trip times in milli-seconds:\n\tMinimum = {Times.Min()}ms, Maximum = {Times.Max()}ms, Average = {Times.Average():0}ms";
            });
            return CMD_PING;
        }

        private void Main_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CTS.Cancel();
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_PreviewKeyDown;
            }
        }
    }
}
