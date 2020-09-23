using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using CLIShell;
using System.Security.Cryptography;
using System.IO;
using System.Windows;
using ICSharpCode.AvalonEdit;
using System.Threading;
using System.Windows.Input;
using System.Diagnostics;

namespace HASHCRACK
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_HASHCRACK;

        public enum HashType { SHA1, SHA256, SHA384, SHA512 }

        public CancellationTokenSource CTS;

        public CancellationToken CT;

        public int processedWords = 0;

        public double progress = 0;

        public double count = 0;

        public string currentWord = null;

        public string currentResult = null;

        public Command GetCommand()
        {
            CTS = new CancellationTokenSource();
            CT = CTS.Token;
            TABLE.Add(new CommandArgumentEntry("[string] [string]", true, "[wordlist path] [hash]"));
            CMD_HASHCRACK = new Command("HASHCRACK", TABLE, false, "Starts a search for a matching hash value in the specified wordlist. Supported hash types: SHA1, SHA256, SHA384, SHA512.", ExecutionLevel.User, CLIMode.Default);
            CMD_HASHCRACK.SetAsyncFunction(async () =>
            {
                processedWords = 0;
                progress = 0;
                count = 0;
                currentWord = null;
                currentResult = null;
                try
                {
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = true;
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown += Main_KeyDown;
                    });
                    HashType hashType;

                    string result = null;
                    async Task HASH_TASK(string word)
                    {
                        await Task.Delay(0);
                        object hashingObject = null;
                        string hash = null;
                        if (result == null)
                        {
                            switch (hashType)
                            {
                                case HashType.SHA1:
                                    hashingObject = SHA1.Create();
                                    hash = BitConverter.ToString(((SHA1)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(word))).Replace("-", "");
                                    break;
                                case HashType.SHA256:
                                    hashingObject = SHA256.Create();
                                    hash = BitConverter.ToString(((SHA256)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(word))).Replace("-", "");
                                    break;
                                case HashType.SHA384:
                                    hashingObject = SHA384.Create();
                                    hash = BitConverter.ToString(((SHA384)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(word))).Replace("-", "");
                                    break;
                                case HashType.SHA512:
                                    hashingObject = SHA512.Create();
                                    hash = BitConverter.ToString(((SHA512)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(word))).Replace("-", "");
                                    break;
                            }
                            if (hash == CMD_HASHCRACK.InputArgumentEntry.Arguments[1].Value.ToString().ToUpper())
                            {
                                result = word;
                            }
                        }
                        
                        currentResult = hash;
                        currentWord = word;
                        processedWords++;
                        progress = processedWords / count * 100;
                        word = null;
                        hash = null;
                        hashingObject = null;
                    }

                    List<Task> taskList = new List<Task>();
                    Regex hexCheck = new Regex("^[a-fA-F0-9]*$");
                    switch (CMD_HASHCRACK.InputArgumentEntry.Arguments[1].Value.ToString().Length)
                    {
                        case 40 when hexCheck.IsMatch(CMD_HASHCRACK.InputArgumentEntry.Arguments[1].Value.ToString()):
                            hashType = HashType.SHA1;
                            break;
                        case 64 when hexCheck.IsMatch(CMD_HASHCRACK.InputArgumentEntry.Arguments[1].Value.ToString()):
                            hashType = HashType.SHA256;
                            break;
                        case 96 when hexCheck.IsMatch(CMD_HASHCRACK.InputArgumentEntry.Arguments[1].Value.ToString()):
                            hashType = HashType.SHA384;
                            break;
                        case 128 when hexCheck.IsMatch(CMD_HASHCRACK.InputArgumentEntry.Arguments[1].Value.ToString()):
                            hashType = HashType.SHA512;
                            break;
                        default:
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                            {
                                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_KeyDown;
                            });
                            return "\nIncorrect hash format or unsupported type!";
                    }
                    string path = null;
                    if (File.Exists(CMD_HASHCRACK.InputArgumentEntry.Arguments[0].Value.ToString()) && CMD_HASHCRACK.InputArgumentEntry.Arguments[0].Value.ToString().Contains(":"))
                    {
                        path = CMD_HASHCRACK.InputArgumentEntry.Arguments[0].Value.ToString();
                    }
                    else if (File.Exists(EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + CMD_HASHCRACK.InputArgumentEntry.Arguments[0].Value.ToString()))
                    {
                        path = EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + CMD_HASHCRACK.InputArgumentEntry.Arguments[0].Value.ToString();
                    }
                    if (path == null)
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                        {
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_KeyDown;
                        });
                        return "\nFile not found!";
                    }
                    string[] lines = File.ReadAllLines(path);
                    count = lines.Length;
                    taskList.Add(Task.Run(async () => { await HASH_TASK(lines[0]); }));
                    for (int i = 0; i < lines.Length; i++)
                    {
                        if (CT.IsCancellationRequested)
                        {
                            ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                            {
                                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_KeyDown;
                            });
                            return "\nProcess interrupted.";
                        }
                        if (result != null)
                        {
                            break;
                        }
                        taskList.RemoveAll(x => x.IsCompleted);
                        taskList.Add(Task.Run(async () => { await HASH_TASK(lines[i]); }));
                    }
                    await Task.WhenAll(taskList).ContinueWith(t => t.Dispose());
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_KeyDown;
                    });
                    if (result == null)
                    {
                        return "\nNo matching hash found!";
                    }
                    return $"\nResult: {result}";
                }
                catch (Exception ex)
                {
                    ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).Dispatcher.Invoke(() =>
                    {
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                        ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_KeyDown;
                        IOInteractLayer.StandardError(CMD_HASHCRACK, ex);
                    });
                    return "";
                }
            });
            return CMD_HASHCRACK;
        }

        private void Main_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CTS.Cancel();
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).IsReadOnly = false;
                ((TextEditor)EnvironmentVariables.GetCurrentValue("IOFIELD")).PreviewKeyDown -= Main_KeyDown;
            }
            if (e.Key == Key.Enter)
            {
                IOInteractLayer.StandardOutput(CMD_HASHCRACK, $"\nProcessed words: {processedWords}\nProgress: {progress:0.00}%\nLast word: {currentWord}\nLast hash: {currentResult}");
            }
        }
    }
}
