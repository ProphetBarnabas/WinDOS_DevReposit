using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using CLIShell;

namespace MH
{
    public class Main
    {
        public ArgumentTable TABLE = new ArgumentTable();

        public Command CMD_MH;

        public Command GetCommand()
        {
            TABLE.Add(new CommandArgumentEntry("[string] -sha1", true));
            TABLE.Add(new CommandArgumentEntry("[string] -sha256", true));
            TABLE.Add(new CommandArgumentEntry("[string] -sha384", true));
            TABLE.Add(new CommandArgumentEntry("[string] -sha512", true));
            CMD_MH = new Command("MH", TABLE, false, "Converts the specified text to the specified hash type.", ExecutionLevel.User, CLIMode.Default);
            CMD_MH.SetFunction(() =>
            {
                object hashingObject;
                string result = string.Empty;
                switch (CMD_MH.InputArgumentEntry.Arguments[1].Call)
                {
                    case "-sha1":
                        hashingObject = SHA1.Create();
                        result = BitConverter.ToString(((SHA1)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(CMD_MH.InputArgumentEntry.Arguments[0].Value.ToString()))).Replace("-", "");
                        break;
                    case "-sha256":
                        hashingObject = SHA256.Create();
                        result = BitConverter.ToString(((SHA256)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(CMD_MH.InputArgumentEntry.Arguments[0].Value.ToString()))).Replace("-", "");
                        break;
                    case "-sha384":
                        hashingObject = SHA384.Create();
                        result = BitConverter.ToString(((SHA384)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(CMD_MH.InputArgumentEntry.Arguments[0].Value.ToString()))).Replace("-", "");
                        break;
                    case "-sha512":
                        hashingObject = SHA512.Create();
                        result = BitConverter.ToString(((SHA512)hashingObject).ComputeHash(Encoding.UTF8.GetBytes(CMD_MH.InputArgumentEntry.Arguments[0].Value.ToString()))).Replace("-", "");
                        break;
                }
                Clipboard.SetText(result);
                return $"\nHash: {result}\n(copied to clipboard)";
            });
            return CMD_MH;
        }
    }
}
