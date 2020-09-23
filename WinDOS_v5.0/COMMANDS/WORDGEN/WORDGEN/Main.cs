using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;
using CLIShell;
using ICSharpCode.AvalonEdit;

namespace WORDGEN
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_WORDGEN;

        public double progress = 0;
        public double processedWords = 0;
        public string currentWord = string.Empty;

        public CancellationTokenSource CTS = new CancellationTokenSource();
        public CancellationToken CT;

        public Command GetCommand()
        {
            CT = CTS.Token;
            TABLE.Add(new CommandArgumentEntry("[string] [int]", true, "[charset] [word length]"));
            TABLE.Add(new CommandArgumentEntry("[string] [int] -out=[string]", true, "[charset] [word length], -out=[output path]"));
            CMD_WORDGEN = new Command("WORDGEN", TABLE, false, "Generates all possible combinations of the specified characters(duplcates will be removed) at the specified length. The output file can be specified using the argumet'-out'. If the output file is not specified, wordgen attempts to create one in the current directory by the name of 'wordgen.txt'.", ExecutionLevel.User, CLIMode.Default);

            CMD_WORDGEN.SetAsyncFunction(async () => 
            {
                try
                {
                    await Task.Delay(0);
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() => 
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = true;
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown += Main_PreviewKeyDown;
                        IOInteractLayer.StandardOutput(CMD_WORDGEN, "\n");
                    });
                    char[] charset = ((string)CMD_WORDGEN.InputArgumentEntry.Arguments[0].Value).ToCharArray().Distinct().ToArray();
                    int wordLength = (int)CMD_WORDGEN.InputArgumentEntry.Arguments[1].Value;
                    int[] indexArray = new int[wordLength];
                    for (int i = 0; i < indexArray.Length; i++)
                    {
                        indexArray[i] = 0;
                    }
                    double count = Math.Pow(charset.Length, wordLength);
                    string filePath = string.Empty;
                    StreamWriter sw = null;
                    if (CMD_WORDGEN.InputArgumentEntry.Arguments.Count == 3)
                    {
                        string path = (string)CMD_WORDGEN.InputArgumentEntry.Arguments[2].Value;
                        string full_path = (string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + (string)CMD_WORDGEN.InputArgumentEntry.Arguments[2].Value;
                        if (Directory.Exists(path.Remove(path.LastIndexOf("\\"))) && path.Contains(":"))
                        {
                            sw = new StreamWriter(path);
                            filePath = path;
                        }
                        else if (Directory.Exists(full_path.Remove(full_path.LastIndexOf("\\"))))
                        {
                            sw = new StreamWriter(full_path);
                            filePath = full_path;
                        }
                    }
                    else
                    {
                        sw = new StreamWriter((string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + "wordgen.txt");
                        filePath = (string)EnvironmentVariables.GetCurrentValue("DIRECTORY") + "wordgen.txt";
                    }
                    StringBuilder sb = new StringBuilder();

                    for (int k = 0; k < count; k++)
                    {
                        if (!CT.IsCancellationRequested)
                        {
                            for (int i = 0; i < indexArray.Length; i++)
                            {
                                if (indexArray[i] == charset.Length)
                                {
                                    indexArray[i] = 0;
                                    if (i + 1 < wordLength)
                                    {
                                        indexArray[i + 1]++;
                                    }
                                }
                            }
                            for (int i = 0; i < wordLength; i++)
                            {
                                sb.Append(charset[indexArray[i]]);
                            }
                            sw.WriteLine(sb.ToString());
                            currentWord = sb.ToString();
                            sb.Clear();
                            processedWords++;
                            progress = processedWords / count * 100;
                            indexArray[0]++;
                        }
                        else
                        {
                            break;
                        }
                    }
                    sw.Close();
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_PreviewKeyDown;
                    });
                    if (!CT.IsCancellationRequested)
                    {
                        FileInfo inf = new FileInfo(filePath);
                        processedWords = 0;
                        progress = 0;
                        currentWord = string.Empty;
                        return $"Process completed.\nGenerated words: {count}\nFile size: {inf.Length} bytes\n{(string)EnvironmentVariables.GetCurrentValue("DIRECTORY")}> ";
                    }
                    return ""; 
                }
                catch (Exception ex)
                {
                    IOInteractLayer.StandardError(CMD_WORDGEN, ex);
                    return "";
                }     
            });
            return CMD_WORDGEN;
        }

        private void Main_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                IOInteractLayer.StandardOutput(CMD_WORDGEN, $"\nGenerated: {processedWords}\nLast word: {currentWord}\nProgress: {progress:0.00}%\n");
            }
            if (e.Key == Key.Escape)
            {
                CTS.Cancel();
                IOInteractLayer.StandardOutput(CMD_WORDGEN, "\nProcess interrupted.");
            }
        }
    }
}
