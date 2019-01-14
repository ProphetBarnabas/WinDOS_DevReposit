using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using static WinDOS.DiscEngine;
using static WinDOS.Bootloader;
using System.Security.Cryptography;
using NmapSharp;

namespace WinDOS
{
    public partial class DiscOperatingSystem : Form
    {
        private Bootloader bl = new Bootloader();

        private DiscEngine engine = new DiscEngine();

        private int lastTextLength;

        private int currentPos;

        private int defaultInputHistoryIndex = 0;

        private int regeditInputHistoryIndex = 0;

        private string[] defaultInputHistory = new string[20];

        private string[] regeditInputHistory = new string[20];

        private string newLine = String.Empty;

        private string resumeInput;

        private string userInput = String.Empty;

        public Rectangle originalBonds;

        public DateTime ThreadStartTime;

        public Thread t;

        public BootOption BootSource;

        public bool CanWrite = true;

        public bool FileIsOpen = false;

        public bool WordgenIsRunning = false;

        public double WordgenComboCount;

        public double WordgenCurrentIndex;

        public string OpenFilePath;

        public DiscOperatingSystem()
        {
            InitializeComponent();
            this.FormClosing += DiscOperatingSystem_FormClosing;
            this.Shown += DiscOperatingSystem_Shown;
            inputField.Click += InputField_Click;
            inputField.DoubleClick += InputField_DoubleClick;
            inputField.KeyDown += InputField_KeyDown;
            inputField.PreviewKeyDown += InputField_PreviewKeyDown;
            inputField.TextChanged += InputField_TextChanged;
            this.SizeChanged += DiscOperatingSystem_SizeChanged;
        }

        private void DiscOperatingSystem_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                t.Abort();
            }
            catch (Exception)
            {
            }
            if (engine.Emulation)
            {
                engine.Emulation = false;
                Process.Start("explorer.exe");
            }
        }

        private void AutoCompleteExecute(string command, string[] commandArray)
        {
            for (int i = 0; i < commandArray.Length; i++)
            {
                if (commandArray[i].StartsWith(command.ToLower()))
                {
                    AutoCompleteReplace(command, commandArray[i]);
                    break;
                }
            }
        }

        private void AutoCompleteReplace(string commandToReplace, string newCommand)
        {
            string lastLine = inputField.Lines.Last();
            if (inputField.Lines.Length > 1)
            {
                inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                inputField.AppendText(Environment.NewLine + lastLine.Remove(lastLine.Length - commandToReplace.Length) + newCommand);
            }
            else
            {
                inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                inputField.Clear();
                inputField.AppendText(lastLine.Remove(lastLine.Length - commandToReplace.Length) + newCommand);
            }
        }

        private void AutoComplete(string input, bool regedit)
        {
            if (!regedit)
            {
                string[] startupList = { "startup-config", "startup-dir", "startup-fcolor", "startup-bcolor", "startup-font", "startup-fullscreen", "startup-emulation", "startup-subkey", "startup-shmotd", "startup-motd" };
                string[] driverList = { "driver-info", "driver-check" };
                string[] eraseList = { "erase-ini-config", "erase-registry-config" };
                string[] cmdList = { "cd", "dir", "md", "rd", "mf", "fc", "open", "copy", "move", "del", "ren", "forecolor", "backcolor", "setfont", "fullscreen", "regedit", "emulation", "wordgen", "hashcrack", "mh", "biosinfo", "getnics", "nmap", "setmac", "resetmac", "copy-current-config", "exec", "start", "cls", "help", "restart", "exit" };
                if ("startup-".StartsWith(input))
                {
                    AutoCompleteReplace(input, "startup-");
                }
                else if ("driver-".StartsWith(input))
                {
                    AutoCompleteReplace(input, "driver-");
                }
                else if ("erase-".StartsWith(input))
                {
                    AutoCompleteReplace(input, "erase-");
                }
                else if (input.StartsWith("startup-"))
                {
                    AutoCompleteExecute(input, startupList);
                }
                else if (input.StartsWith("driver-"))
                {
                    AutoCompleteExecute(input, driverList);
                }
                else if (input.StartsWith("erase-"))
                {
                    AutoCompleteExecute(input, eraseList);
                }
                else
                {
                    AutoCompleteExecute(input, cmdList);
                }
            }
            else
            {
                string[] skList = { "sk-get", "sk-make", "sk-delete" };
                string[] valList = { "val-get", "val-set", "val-make", "val-delete", "val-rename" };
                if ("sk-".StartsWith(input))
                {
                    AutoCompleteReplace(input, "sk-");
                }
                else if ("val-".StartsWith(input))
                {
                    AutoCompleteReplace(input, "val-");
                }
                else if ("csk".StartsWith(input))
                {
                    AutoCompleteReplace(input, "csk");
                }
                else if (input.StartsWith("sk-"))
                {
                    AutoCompleteExecute(input, skList);
                }
                else if (input.StartsWith("val-"))
                {
                    AutoCompleteExecute(input, valList);
                }
            }
           
            SendKeys.Send("{BACKSPACE}");
        }

        private void UpdateInputHistory()
        {
            if (userInput != String.Empty)
            {
                if (!engine.RegistryEditor)
                {
                    for (int i = 19; i > 1; i--)
                    {
                        defaultInputHistory[i] = defaultInputHistory[i - 1];
                    }
                    defaultInputHistory[1] = userInput;
                }
                else
                {
                    for (int i = 19; i > 1; i--)
                    {
                        regeditInputHistory[i] = regeditInputHistory[i - 1];
                    }
                    regeditInputHistory[1] = userInput;
                }
            }
        }

        private void RemoveLines()
        {
            inputField.Invoke(new MethodInvoker(() => {
                List<string> lines = inputField.Lines.ToList();
                while (inputField.Text.Length >= inputField.MaxLength)
                {
                    lines.RemoveRange(0, 100);
                    inputField.Lines = lines.ToArray();
                    inputField.SelectionStart = inputField.Text.Length;
                    inputField.ScrollToCaret();
                }
            }));
            try
            {
                t.Abort();
            }
            catch (Exception)
            {
            }
        }

        private void InputField_TextChanged(object sender, EventArgs e)
        {
            if (inputField.Text.Length >= inputField.MaxLength)
            {
                t = new Thread(RemoveLines);
                t.Start();
            }
        }

        private void InputField_DoubleClick(object sender, EventArgs e)
        {
            if (!FileIsOpen)
            {
                inputField.SelectionStart = inputField.Text.Length;
            }
        }

        private void DiscOperatingSystem_Shown(object sender, EventArgs e)
        {
            originalBonds = this.Bounds;
            if (!bl.RegistryBootSequence(true, engine, this))
            {
                if (!bl.INIBootSequence(true, engine, this))
                {
                    bl.BasicBootSequence(true, false, true, engine, this);
                }
                else
                {
                    BootSource = BootOption.INI;
                }
            }
            else
            {
                BootSource = BootOption.Registry;
            }
            engine.ApplySettings(this, true);
            inputField.Focus();
        }

        private void InputField_KeyDown(object sender, KeyEventArgs e)
        {
            if (CanWrite)
            {
                try
                {
                    if (lastTextLength != inputField.Lines.Last().Length && currentPos != inputField.Lines.Last().Length && !FileIsOpen)
                    {
                        currentPos = inputField.Lines.Last().Length;
                    }
                }
                catch (Exception)
                {
                }
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        if (!FileIsOpen)
                        {
                            e.SuppressKeyPress = true;
                            if (!engine.RegistryEditor)
                            {
                                userInput = inputField.Lines.Last().Substring(engine.CurrentDirectory.Length + 2);
                                defaultInputHistoryIndex = 0;
                                UpdateInputHistory();
                                engine.ExecuteCommand(engine.GetCommand(inputField.Lines.Last().Substring(engine.CurrentDirectory.Length + 2)), false, this, bl);
                            }
                            else
                            {
                                userInput = inputField.Lines.Last().Substring(engine.CurrentSubkey.Length + 2);
                                regeditInputHistoryIndex = 0;
                                UpdateInputHistory();
                                engine.ExecuteCommand(engine.GetCommand(inputField.Lines.Last().Substring(engine.CurrentSubkey.Length + 2)), false, this, bl);
                            }
                        }
                        break;
                    case Keys.Back:
                        try
                        {
                            if (!engine.RegistryEditor)
                            {
                                if (inputField.Lines.Last().Length == engine.CurrentDirectory.Length + 2 && !FileIsOpen)
                                {
                                    e.SuppressKeyPress = true;
                                }
                            }
                            else
                            {
                                if (inputField.Lines.Last().Length == engine.CurrentSubkey.Length + 2 && !FileIsOpen)
                                {
                                    e.SuppressKeyPress = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                        }
                        break;
                    case Keys.Left:
                        if (!FileIsOpen)
                        {
                            currentPos--;
                            if (!engine.RegistryEditor)
                            {
                                if (currentPos < engine.CurrentDirectory.Length + 2)
                                {
                                    currentPos++;
                                    e.SuppressKeyPress = true;
                                }
                            }
                            else
                            {
                                if (currentPos < engine.CurrentSubkey.Length + 2)
                                {
                                    currentPos++;
                                    e.SuppressKeyPress = true;
                                }
                            }
                        }
                        break;
                    case Keys.Right:
                        if (!FileIsOpen)
                        {
                            if (!engine.RegistryEditor)
                            {
                                if (currentPos >= engine.CurrentDirectory.Length + 2 && inputField.Lines.Last().Length > currentPos)
                                {
                                    currentPos++;
                                }
                            }
                            else
                            {
                                if (currentPos >= engine.CurrentSubkey.Length + 2 && inputField.Lines.Last().Length > currentPos)
                                {
                                    currentPos++;
                                }
                            }
                        }
                        break;
                    case Keys.Up:
                        if (!FileIsOpen)
                        {
                            e.SuppressKeyPress = true;
                            try
                            {
                                if (!engine.RegistryEditor)
                                {
                                    if (18 >= defaultInputHistoryIndex)
                                    {
                                        if (defaultInputHistory[defaultInputHistoryIndex + 1] != null)
                                        {
                                            defaultInputHistoryIndex++;
                                            if (defaultInputHistoryIndex == 1)
                                            {
                                                resumeInput = userInput = inputField.Lines.Last().Substring(engine.CurrentDirectory.Length + 2);
                                            }

                                            if (inputField.Lines.Length > 1)
                                            {
                                                newLine = Environment.NewLine;
                                            }
                                            else
                                            {
                                                newLine = String.Empty;
                                            }
                                            inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                                            inputField.AppendText(newLine + engine.CurrentDirectory + "> " + defaultInputHistory[defaultInputHistoryIndex]);
                                        }
                                    }
                                }
                                else
                                {
                                    if (18 >= regeditInputHistoryIndex)
                                    {
                                        if (regeditInputHistory[regeditInputHistoryIndex + 1] != null)
                                        {
                                            regeditInputHistoryIndex++;
                                            if (regeditInputHistoryIndex == 1)
                                            {
                                                resumeInput = userInput = inputField.Lines.Last().Substring(engine.CurrentSubkey.Length + 2);
                                            }

                                            if (inputField.Lines.Length > 1)
                                            {
                                                newLine = Environment.NewLine;
                                            }
                                            else
                                            {
                                                newLine = String.Empty;
                                            }
                                            inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                                            inputField.AppendText(newLine + engine.CurrentSubkey + "> " + regeditInputHistory[regeditInputHistoryIndex]);
                                        }
                                    }
                                }
                            }
                            catch (Exception)
                            {
                            }
                        }
                        break;
                    case Keys.Down:
                        if (!FileIsOpen)
                        {
                            e.SuppressKeyPress = true;
                            try
                            {
                                if (!engine.RegistryEditor)
                                {
                                    if (defaultInputHistoryIndex > 1)
                                    {
                                        if (defaultInputHistory[defaultInputHistoryIndex - 1] != null)
                                        {
                                            defaultInputHistoryIndex--;

                                            if (inputField.Lines.Length > 1)
                                            {
                                                newLine = Environment.NewLine;
                                            }
                                            else
                                            {
                                                newLine = String.Empty;
                                            }
                                            inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                                            inputField.AppendText(newLine + engine.CurrentDirectory + "> " + defaultInputHistory[defaultInputHistoryIndex]);
                                        }
                                    }
                                    else if (defaultInputHistoryIndex == 1)
                                    {
                                        defaultInputHistoryIndex--;
                                        if (inputField.Lines.Length > 1)
                                        {
                                            newLine = Environment.NewLine;
                                        }
                                        else
                                        {
                                            newLine = String.Empty;
                                        }
                                        inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                                        inputField.AppendText(newLine + engine.CurrentDirectory + "> " + resumeInput);
                                    }
                                }
                                else
                                {
                                    if (regeditInputHistoryIndex > 1)
                                    {
                                        if (regeditInputHistory[regeditInputHistoryIndex - 1] != null)
                                        {
                                            regeditInputHistoryIndex--;

                                            if (inputField.Lines.Length > 1)
                                            {
                                                newLine = Environment.NewLine;
                                            }
                                            else
                                            {
                                                newLine = String.Empty;
                                            }
                                            inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                                            inputField.AppendText(newLine + engine.CurrentSubkey + "> " + regeditInputHistory[regeditInputHistoryIndex]);
                                        }
                                    }
                                    else if (regeditInputHistoryIndex == 1)
                                    {
                                        regeditInputHistoryIndex--;
                                        if (inputField.Lines.Length > 1)
                                        {
                                            newLine = Environment.NewLine;
                                        }
                                        else
                                        {
                                            newLine = String.Empty;
                                        }
                                        inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                                        inputField.AppendText(newLine + engine.CurrentSubkey + "> " + resumeInput);
                                    }
                                }   
                            }
                            catch (Exception)
                            {
                            }
                        }
                        break;
                    case Keys.F2:
                        if (FileIsOpen)
                        {
                            e.SuppressKeyPress = true;
                            FileIsOpen = false;
                            File.WriteAllText(OpenFilePath, inputField.Text);
                            OpenFilePath = null;
                            inputField.Clear();
                            inputField.AppendText(engine.CurrentDirectory + "> ");
                        }
                        break;
                    case Keys.Escape:
                        if (FileIsOpen)
                        {
                            e.SuppressKeyPress = true;
                            e.SuppressKeyPress = true;
                            FileIsOpen = false;
                            OpenFilePath = null;
                            inputField.Clear();
                            inputField.AppendText(engine.CurrentDirectory + "> ");
                        }
                        break;
                }

                if (e.KeyCode != Keys.Enter && e.KeyCode != Keys.Up && e.KeyCode != Keys.Down && inputField.Lines.Length > 0)
                {
                    lastTextLength = inputField.Lines.Last().Length;
                }
            }
            else
            {
                switch (e.KeyCode)
                {
                    case Keys.Enter:
                        e.SuppressKeyPress = true;
                        if (WordgenIsRunning)
                        {
                            ThreadProgress tProg = engine.GetProgress(ThreadStartTime, DateTime.Now - ThreadStartTime, WordgenComboCount, WordgenCurrentIndex);
                            inputField.AppendText(String.Format("{0}Start time: {1}{0}Time elapsed: {2}{0}Time left: {3}{0}Progress: {4:0.00}%{0}", Environment.NewLine, tProg.StartTime.ToString(), tProg.TimeElapsed, tProg.TimeLeft, tProg.Percentage));
                        }
                        break;
                    case Keys.Escape:
                        try
                        {
                            t.Abort();
                        }
                        catch (Exception)
                        {
                        }
                        CanWrite = true;
                        inputField.AppendText(Environment.NewLine + engine.CurrentDirectory + "> ");
                        break;
                    default:
                        e.SuppressKeyPress = true;
                        break;
                }
            }

        }

        private void InputField_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            switch (e.KeyData)
            {
                case Keys.Tab:
                    if (!FileIsOpen)
                    {
                        e.IsInputKey = true;
                        if (!engine.RegistryEditor)
                        {
                            AutoComplete(inputField.Lines.Last().Substring(engine.CurrentDirectory.Length + 2), engine.RegistryEditor);
                        }
                        else
                        {
                            AutoComplete(inputField.Lines.Last().Substring(engine.CurrentSubkey.Length + 2), engine.RegistryEditor);
                        }              
                    }
                    break;
            }
        }

        private void DiscOperatingSystem_SizeChanged(object sender, EventArgs e)
        {
            if (!engine.FullscreenMode)
            {
                inputField.Size = new Size(this.Width + 20, this.Height - 39);
            }
        }

        private void InputField_Click(object sender, EventArgs e)
        {
            if (!FileIsOpen)
            {
                inputField.SelectionStart = inputField.Text.Length;
            }
        }
    }

    public class Bootloader
    {
        public enum BootOption { Registry, INI, Basic };

        private static void BootLogWriter(bool canWrite, string text, int interval, DiscOperatingSystem form)
        {
            if (canWrite)
            {
                form.inputField.AppendText(text);
                Thread.Sleep(interval);
            }
        }

        public RegistryKey startupConfigKey;

        private string keyPath = "SOFTWARE\\WinDOS\\";

        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        public PrivateFontCollection fonts = new PrivateFontCollection();

        public Font CP437;

        private FontFamily FontFamilyDetection(string familyName, bool displayBootProgress, DiscOperatingSystem form)
        {
            FontFamily ff = null;
            try
            {
                if (familyName == "CP437")
                {
                    ff = fonts.Families[0];
                    BootLogWriter(displayBootProgress, "Font family is set to: CP437" + Environment.NewLine, 500, form);
                }
                else
                {
                    ff = new FontFamily(familyName.Replace("_", " "));
                    BootLogWriter(displayBootProgress, "Font family is set to: " + familyName.Replace("_", " ") + Environment.NewLine, 500, form);
                }
            }
            catch (Exception)
            {
                ff = fonts.Families[0];
                BootLogWriter(displayBootProgress, "Invalid font family... Font family is set to: CP437" + Environment.NewLine, 500, form);
            }
            return ff;
        }

        private int FontSizeDetection(string fSize, bool displayBootProgress, DiscOperatingSystem form)
        {
            int fs = int.Parse(fSize);
            if (fs == 0)
            {
                fs = 16;
                BootLogWriter(displayBootProgress, "Invalid font size... Font size is set to: 16" + Environment.NewLine, 500, form);
            }
            else
            {
                BootLogWriter(displayBootProgress, "Font size is set to: " + fs.ToString() + Environment.NewLine, 500, form);
            }
            return fs;
        }

        private FontStyle FontStyleDetection(string style, bool displayBootProgress, DiscOperatingSystem form)
        {
            FontStyle fs = FontStyle.Regular;
            switch (style)
            {
                case "regular":
                    BootLogWriter(displayBootProgress, "Font style is set to: Regular" + Environment.NewLine, 500, form);
                    break;
                case "bold":
                    fs = FontStyle.Bold;
                    BootLogWriter(displayBootProgress, "Font style is set to: Bold" + Environment.NewLine, 500, form);
                    break;
                case "italic":
                    fs = FontStyle.Italic;
                    BootLogWriter(displayBootProgress, "Font style is set to: Italic" + Environment.NewLine, 500, form);
                    break;
                default:
                    BootLogWriter(displayBootProgress, "Invalid font style... Font style is set to: Regular" + Environment.NewLine, 500, form);
                    break;
            }
            return fs;
        }

        private void LoadCP437()
        {
            byte[] fontData = Properties.Resources.Perfect_DOS_VGA_437_Win;
            IntPtr fontPtr = Marshal.AllocCoTaskMem(fontData.Length);
            Marshal.Copy(fontData, 0, fontPtr, fontData.Length);
            uint dummy = 0;
            fonts.AddMemoryFont(fontPtr, Properties.Resources.Perfect_DOS_VGA_437_Win.Length);
            AddFontMemResourceEx(fontPtr, (uint)Properties.Resources.Perfect_DOS_VGA_437_Win.Length, IntPtr.Zero, ref dummy);
            Marshal.FreeCoTaskMem(fontPtr);
            CP437 = new Font(fonts.Families[0], 16);
        }

        public bool RegistryBootSequence(bool displayBootProgress, DiscEngine engine, DiscOperatingSystem form)
        {
            try
            {
                startupConfigKey = Registry.LocalMachine.CreateSubKey(keyPath);
                BootLogWriter(displayBootProgress, "BOOTING FROM REGISTRY" + Environment.NewLine, 500, form);
                /*STARTUP DIRECTORY*/
                try
                {
                    engine.CurrentDirectory = startupConfigKey.GetValue("StartupDirectory").ToString();
                    if (!Directory.Exists(engine.CurrentDirectory))
                    {
                        engine.CurrentDirectory = "C:\\";
                        startupConfigKey.SetValue("StartupDirectory", "C:\\");
                        BootLogWriter(displayBootProgress, "Directory not found... Startup directory is set to: C:\\" + Environment.NewLine, 500, form);
                    }
                    else
                    {
                        BootLogWriter(displayBootProgress, "Startup directory is set to: " + engine.CurrentDirectory + Environment.NewLine, 500, form);
                    }
                    if (engine.CurrentDirectory.Last() != '\\')
                    {
                        engine.CurrentDirectory += '\\';
                    }
                }
                catch (NullReferenceException)
                {
                    engine.CurrentDirectory = "C:\\";
                    startupConfigKey.SetValue("StartupDirectory", "C:\\");
                    BootLogWriter(displayBootProgress, "Failed to load startup directory... Startup directory is set to: C:\\" + Environment.NewLine, 500, form);
                }
                engine.StartupDirectory = engine.CurrentDirectory;
                /*###################################################################################################*/
                /*COLOR SETTINGS*/
                try
                {
                    engine.ForegroundColor = Color.FromName(startupConfigKey.GetValue("ForegroundColor").ToString());
                    engine.BackgroundColor = Color.FromName(startupConfigKey.GetValue("BackgroundColor").ToString());
                    if (engine.ForegroundColor == engine.BackgroundColor || engine.ForegroundColor.ToKnownColor() == 0 || engine.BackgroundColor.ToKnownColor() == 0 || engine.BackgroundColor.ToKnownColor() == engine.ForegroundColor.ToKnownColor())
                    {
                        engine.ForegroundColor = Color.Silver;
                        engine.BackgroundColor = Color.Black;
                        BootLogWriter(displayBootProgress, "Invalid color settings..." + Environment.NewLine, 500, form);
                    }
                }
                catch (Exception)
                {
                    engine.ForegroundColor = Color.Silver;
                    engine.BackgroundColor = Color.Black;
                    startupConfigKey.SetValue("ForegroundColor", "Silver");
                    startupConfigKey.SetValue("BackgroundColor", "Black");
                }
                BootLogWriter(displayBootProgress, "Foreground color is set to: " + engine.ForegroundColor.Name + Environment.NewLine, 500, form);
                BootLogWriter(displayBootProgress, "Background color is set to: " + engine.BackgroundColor.Name + Environment.NewLine, 500, form);
                /*###################################################################################################*/
                /*FONT SETTINGS*/
                try
                {
                    BootLogWriter(displayBootProgress, "Loading system font..." + Environment.NewLine, 500, form);
                    LoadCP437();
                    FontFamily fFamily = null;
                    FontStyle fStyle = FontStyle.Regular;
                    int fSize = 16;
                    string[] sysFont = startupConfigKey.GetValue("SystemFont").ToString().Split(' ');
                    switch (sysFont.Length)
                    {
                        case 1:
                            fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            break;
                        case 2:
                            if (engine.onlyDigit.IsMatch(sysFont[1]))
                            {
                                fSize = FontSizeDetection(sysFont[1], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            else
                            {
                                fStyle = FontStyleDetection(sysFont[1], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            break;
                        case 3:
                            if (engine.onlyDigit.IsMatch(sysFont[1]))
                            {
                                fSize = FontSizeDetection(sysFont[1], displayBootProgress, form);
                                fStyle = FontStyleDetection(sysFont[2], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            else
                            {
                                fSize = FontSizeDetection(sysFont[2], displayBootProgress, form);
                                fStyle = FontStyleDetection(sysFont[1], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            break;
                        default:
                            throw new Exception("Falied to load system font!");
                    }
                    engine.SystemFont = new Font(fFamily, fSize, fStyle);
                    BootLogWriter(displayBootProgress, "System font loaded successfully!" + Environment.NewLine, 500, form);
                }
                catch (Exception)
                {
                    engine.SystemFont = CP437;
                    startupConfigKey.SetValue("SystemFont", "CP437");
                    BootLogWriter(displayBootProgress, "Falied to load system font... System font is set to: CP437, 16, Regular" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*FULLSCREEN MODE*/
                try
                {
                    switch (int.Parse(startupConfigKey.GetValue("FullscreenMode").ToString()))
                    {
                        case 0:
                            engine.FullscreenMode = false;
                            BootLogWriter(displayBootProgress, "Fullscreen mode is set to: 0" + Environment.NewLine, 500, form);
                            break;
                        case 1:
                            engine.FullscreenMode = true;
                            BootLogWriter(displayBootProgress, "Fullscreen mode is set to: 1" + Environment.NewLine, 500, form);
                            break;
                        default:
                            engine.FullscreenMode = true;
                            startupConfigKey.SetValue("FullscreenMode", 1, RegistryValueKind.DWord);
                            BootLogWriter(displayBootProgress, "Invalid numeric value... Fullscreen mode is set to: 1" + Environment.NewLine, 500, form);
                            break;
                    }
                }
                catch (NullReferenceException)
                {
                    engine.FullscreenMode = true;
                    startupConfigKey.SetValue("FullscreenMode", 1, RegistryValueKind.DWord);
                    BootLogWriter(displayBootProgress, "Failed to load fullscreen mode... Fullscreen mode is set to: 1" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*EMULATION*/
                try
                {
                    switch (int.Parse(startupConfigKey.GetValue("Emulation").ToString()))
                    {
                        case 0:
                            engine.Emulation = false;
                            BootLogWriter(displayBootProgress, "Emulation is set to: 0" + Environment.NewLine, 500, form);
                            break;
                        case 1:
                            engine.Emulation = true;
                            BootLogWriter(displayBootProgress, "Emulation is set to: 1" + Environment.NewLine, 500, form);
                            break;
                        default:
                            engine.Emulation = true;
                            startupConfigKey.SetValue("Emulation", 0, RegistryValueKind.DWord);
                            BootLogWriter(displayBootProgress, "Invalid numeric value... Emulation is set to: 0" + Environment.NewLine, 500, form);
                            break;
                    }
                }
                catch (NullReferenceException)
                {
                    engine.Emulation = true;
                    startupConfigKey.SetValue("Emulation", 0, RegistryValueKind.DWord);
                    BootLogWriter(displayBootProgress, "Failed to load emulation... Emulation is set to: 0" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*SHOW MESSAGE OF THE DAY*/
                try
                {
                    switch (int.Parse(startupConfigKey.GetValue("ShowMessageOfTheDay").ToString()))
                    {
                        case 0:
                            engine.ShowMessageOfTheDay = false;
                            BootLogWriter(displayBootProgress, "Show message of the day is set to: 0" + Environment.NewLine, 500, form);
                            break;
                        case 1:
                            engine.ShowMessageOfTheDay = true;
                            BootLogWriter(displayBootProgress, "Show message of the day is set to: 1" + Environment.NewLine, 500, form);
                            break;
                        default:
                            engine.ShowMessageOfTheDay = true;
                            startupConfigKey.SetValue("ShowMessageOfTheDay", 1, RegistryValueKind.DWord);
                            BootLogWriter(displayBootProgress, "Invalid numeric value... Show message of the day is set to: 1" + Environment.NewLine, 500, form);
                            break;
                    }
                }
                catch (NullReferenceException)
                {
                    engine.ShowMessageOfTheDay = true;
                    startupConfigKey.SetValue("ShowMessageOfTheDay", 1, RegistryValueKind.DWord);
                    BootLogWriter(displayBootProgress, "Failed to load show message of the day... Show message of the day is set to: 1" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*MESSAGE OF THE DAY*/
                try
                {
                    engine.MessageOfTheDay = startupConfigKey.GetValue("MessageOfTheDay").ToString();
                }
                catch (NullReferenceException)
                {
                    startupConfigKey.SetValue("MessageOfTheDay", @"Type ""help"" for more information!");
                    engine.MessageOfTheDay = @"Type ""help"" for more information!";
                    BootLogWriter(displayBootProgress, "Failed to load message of the day...", 500, form);
                }
                BootLogWriter(displayBootProgress, "Message of the day is set to: " + engine.MessageOfTheDay + Environment.NewLine, 500, form);
                /*###################################################################################################*/
                /*STARTUP SUBKEY*/
                try
                {
                    engine.CurrentSubkey = startupConfigKey.GetValue("StartupSubkey").ToString();
                    if (engine.CurrentSubkey.StartsWith("HKEY_CLASSES_ROOT") && engine.CurrentSubkey.Replace("HKEY_CLASSES_ROOT", "").Length > 0)
                    {
                        if (Registry.ClassesRoot.OpenSubKey(engine.CurrentSubkey.Replace("HKEY_CLASSES_ROOT\\", ""), true) == null)
                        {
                            engine.CurrentSubkey = null;
                        }
                    }
                    else if (engine.CurrentSubkey.StartsWith("HKEY_CURRENT_CONFIG") && engine.CurrentSubkey.Replace("HKEY_CURRENT_CONFIG", "").Length > 0)
                    {
                        if (Registry.CurrentConfig.OpenSubKey(engine.CurrentSubkey.Replace("HKEY_CURRENT_CONFIG\\", ""), true) == null)
                        {
                            engine.CurrentSubkey = null;
                        }
                    }
                    else if (engine.CurrentSubkey.StartsWith("HKEY_CURRENT_USER") && engine.CurrentSubkey.Replace("HKEY_CURRENT_USER", "").Length > 0)
                    {
                        if (Registry.CurrentUser.OpenSubKey(engine.CurrentSubkey.Replace("HKEY_CURRENT_USER\\", ""), true) == null)
                        {
                            engine.CurrentSubkey = null;
                        }
                    }
                    else if (engine.CurrentSubkey.StartsWith("HKEY_LOCAL_MACHINE") && engine.CurrentSubkey.Replace("HKEY_LOCAL_MACHINE", "").Length > 0)
                    {
                        if (Registry.LocalMachine.OpenSubKey(engine.CurrentSubkey.Replace("HKEY_LOCAL_MACHINE\\", ""), true) == null)
                        {
                            engine.CurrentSubkey = null;
                        }
                    }
                    else if (engine.CurrentSubkey.StartsWith("HKEY_USERS") && engine.CurrentSubkey.Replace("HKEY_USERS", "").Length > 0)
                    {
                        if (Registry.Users.OpenSubKey(engine.CurrentSubkey.Replace("HKEY_USERS\\", ""), true) == null)
                        {
                            engine.CurrentSubkey = null;
                        }
                    }
                    else if (engine.CurrentSubkey != "HKEY_CLASSES_ROOT" || engine.CurrentSubkey != "HKEY_CURRENT_CONFIG" || engine.CurrentSubkey != "HKEY_CURRENT_USER" || engine.CurrentSubkey != "HKEY_LOCAL_MACHINE" || engine.CurrentSubkey != "HKEY_USERS" || engine.CurrentSubkey != "HKEY_CLASSES_ROOT" || engine.CurrentSubkey != "HKEY_CURRENT_CONFIG\\" || engine.CurrentSubkey != "HKEY_CURRENT_USER\\" || engine.CurrentSubkey != "HKEY_LOCAL_MACHINE\\" || engine.CurrentSubkey != "HKEY_USERS\\")
                    {
                        engine.CurrentSubkey = null;
                    }
                    else
                    {
                        engine.CurrentSubkey = null;
                    }

                    if (engine.CurrentSubkey == null)
                    {
                        engine.CurrentSubkey = "HKEY_CURRENT_USER\\";
                        startupConfigKey.SetValue("StartupSubkey", "HKEY_CURRENT_USER\\");
                        BootLogWriter(displayBootProgress, "Subkey not found... Startup subkey is set to: HKEY_CURRENT_USER\\" + Environment.NewLine, 500, form);
                    }
                    else
                    {
                        if (engine.CurrentSubkey.Last() != '\\')
                        {
                            engine.CurrentSubkey += '\\';
                        }
                        BootLogWriter(displayBootProgress, "Startup subkey is set to: " + engine.CurrentSubkey + Environment.NewLine, 500, form);
                    }
                }
                catch (Exception)
                {
                    engine.CurrentSubkey = "HKEY_CURRENT_USER\\";
                    startupConfigKey.SetValue("StartupSubkey", "HKEY_CURRENT_USER\\");
                    BootLogWriter(displayBootProgress, "Failed to load startup subkey... Startup subkey is set to: HKEY_CURRENT_USER\\" + Environment.NewLine, 500, form);
                }
                engine.StartupSubkey = engine.CurrentSubkey;
                /*###################################################################################################*/
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public bool INIBootSequence(bool displayBootProgress, DiscEngine engine, DiscOperatingSystem form)
        {
            try
            {
                string[] INILines = File.ReadAllLines("BOOT.ini");
                BootLogWriter(displayBootProgress, "BOOTING FROM BOOT.ini" + Environment.NewLine, 500, form);
                /*STARTUP DIRECTORY*/
                try
                {
                    engine.CurrentDirectory = INILines[0];
                    if (!Directory.Exists(engine.CurrentDirectory))
                    {
                        engine.CurrentDirectory = "C:\\";
                        INILines[0] = "C:\\";
                        BootLogWriter(displayBootProgress, "Directory not found... Startup directory is set to: C:\\" + Environment.NewLine, 500, form);
                    }
                    else
                    {
                        BootLogWriter(displayBootProgress, "Startup directory is set to: " + engine.CurrentDirectory + Environment.NewLine, 500, form);
                    }
                    if (engine.CurrentDirectory.Last() != '\\')
                    {
                        engine.CurrentDirectory += '\\';
                    } 
                }
                catch (Exception)
                {
                    engine.CurrentDirectory = "C:\\";
                    INILines[0] = "C:\\";
                    BootLogWriter(displayBootProgress, "Failed to load startup directory... Startup directory is set to: C:\\" + Environment.NewLine, 500, form);
                }
                engine.StartupDirectory = engine.CurrentDirectory;
                /*###################################################################################################*/
                /*COLOR SETTINGS*/
                try
                {
                    engine.ForegroundColor = Color.FromName(INILines[1]);
                    engine.BackgroundColor = Color.FromName(INILines[2]);
                    if (engine.ForegroundColor == engine.BackgroundColor || engine.ForegroundColor.ToKnownColor() == 0 || engine.BackgroundColor.ToKnownColor() == 0 || engine.BackgroundColor.ToKnownColor() == engine.ForegroundColor.ToKnownColor())
                    {
                        engine.ForegroundColor = Color.Silver;
                        engine.BackgroundColor = Color.Black;
                        BootLogWriter(displayBootProgress, "Invalid color settings..." + Environment.NewLine, 500, form);
                    }
                }
                catch (Exception)
                {
                    engine.ForegroundColor = Color.Silver;
                    engine.BackgroundColor = Color.Black;
                    INILines[1] = "Silver";
                    INILines[2] = "Black";
                    BootLogWriter(displayBootProgress, "Failed to load color settings..." + Environment.NewLine, 500, form);
                }
                BootLogWriter(displayBootProgress, "Foreground color is set to: " + engine.ForegroundColor.Name + Environment.NewLine, 500, form);
                BootLogWriter(displayBootProgress, "Background color is set to: " + engine.BackgroundColor.Name + Environment.NewLine, 500, form);
                /*###################################################################################################*/
                /*FONT SETTINGS*/
                try
                {
                    BootLogWriter(displayBootProgress, "Loading system font..." + Environment.NewLine, 500, form);
                    LoadCP437();
                    FontFamily fFamily = null;
                    FontStyle fStyle = FontStyle.Regular;
                    int fSize = 16;
                    string[] sysFont = INILines[3].Split(' ');
                    switch (sysFont.Length)
                    {
                        case 1:
                            fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            break;
                        case 2:
                            if (engine.onlyDigit.IsMatch(sysFont[1]))
                            {
                                fSize = FontSizeDetection(sysFont[1], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            else
                            {
                                fStyle = FontStyleDetection(sysFont[1], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            break;
                        case 3:
                            if (engine.onlyDigit.IsMatch(sysFont[1]))
                            {
                                fSize = FontSizeDetection(sysFont[1], displayBootProgress, form);
                                fStyle = FontStyleDetection(sysFont[2], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            else
                            {
                                fSize = FontSizeDetection(sysFont[2], displayBootProgress, form);
                                fStyle = FontStyleDetection(sysFont[1], displayBootProgress, form);
                                fFamily = FontFamilyDetection(sysFont[0], displayBootProgress, form);
                            }
                            break;
                        default:
                            throw new Exception("Falied to load system font!");
                    }
                    engine.SystemFont = new Font(fFamily, fSize, fStyle);
                    BootLogWriter(displayBootProgress, "System font loaded successfully!" + Environment.NewLine, 500, form);
                }
                catch (Exception)
                {
                    engine.SystemFont = CP437;
                    INILines[3] = "CP437";
                    BootLogWriter(displayBootProgress, "Falied to load system font... System font is set to: CP437, 16, Regular" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*FULLSCREEN MODE*/
                try
                {
                    switch (int.Parse(INILines[4]))
                    {
                        case 0:
                            engine.FullscreenMode = false;
                            BootLogWriter(displayBootProgress, "Fullscreen mode is set to: 0" + Environment.NewLine, 500, form);
                            break;
                        case 1:
                            engine.FullscreenMode = true;
                            BootLogWriter(displayBootProgress, "Fullscreen mode is set to: 1" + Environment.NewLine, 500, form);
                            break;
                        default:
                            engine.FullscreenMode = true;
                            INILines[4] = "1";
                            BootLogWriter(displayBootProgress, "Invalid numeric value... Fullscreen mode is set to: 1" + Environment.NewLine, 500, form);
                            break;
                    }
                }
                catch (Exception)
                {
                    engine.FullscreenMode = true;
                    INILines[4] = "1";
                    BootLogWriter(displayBootProgress, "Failed to load fullscreen mode... Fullscreen mode is set to: 1" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*EMULATION*/
                try
                {
                    switch (int.Parse(INILines[5]))
                    {
                        case 0:
                            engine.Emulation = false;
                            BootLogWriter(displayBootProgress, "Emulation is set to: 0" + Environment.NewLine, 500, form);
                            break;
                        case 1:
                            engine.Emulation = true;
                            BootLogWriter(displayBootProgress, "Emulation is set to: 1" + Environment.NewLine, 500, form);
                            break;
                        default:
                            engine.Emulation = true;
                            INILines[5] = "0";
                            BootLogWriter(displayBootProgress, "Invalid numeric value... Emulation is set to: 0" + Environment.NewLine, 500, form);
                            break;
                    }
                }
                catch (Exception)
                {
                    engine.FullscreenMode = true;
                    INILines[5] = "0";
                    BootLogWriter(displayBootProgress, "Failed to load emulation... Emulation is set to: 0" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*SHOW MESSAGE OF THE DAY*/
                try
                {
                    switch (int.Parse(INILines[6]))
                    {
                        case 0:
                            engine.ShowMessageOfTheDay = false;
                            BootLogWriter(displayBootProgress, "Show message of the day is set to: 0" + Environment.NewLine, 500, form);
                            break;
                        case 1:
                            engine.ShowMessageOfTheDay = true;
                            BootLogWriter(displayBootProgress, "Show message of the day is set to: 1" + Environment.NewLine, 500, form);
                            break;
                        default:
                            engine.ShowMessageOfTheDay = true;
                            INILines[5] = "1";
                            BootLogWriter(displayBootProgress, "Invalid numeric value... Show message of the day is set to: 1" + Environment.NewLine, 500, form);
                            break;
                    }
                }
                catch (Exception)
                {
                    engine.ShowMessageOfTheDay = true;
                    INILines[6] = "1";
                    BootLogWriter(displayBootProgress, "Failed to load show message of the day... Show message of the day is set to: 1" + Environment.NewLine, 500, form);
                }
                /*###################################################################################################*/
                /*MESSAGE OF THE DAY*/
                try
                {
                    engine.MessageOfTheDay = INILines[7];
                }
                catch (Exception)
                {
                    INILines[6] = @"Type ""help"" for more information!";
                    engine.MessageOfTheDay = INILines[7];
                    BootLogWriter(displayBootProgress, "Failed to load message of the day... ", 500, form);
                }
                BootLogWriter(displayBootProgress, "Message of the day is set to: " + engine.MessageOfTheDay + Environment.NewLine, 500, form);
                /*###################################################################################################*/
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public void BasicBootSequence(bool displayBootProgress, bool createRegistryConfig, bool createINIConfig, DiscEngine engine, DiscOperatingSystem form)
        {
            BootLogWriter(displayBootProgress, "BASIC BOOT" + Environment.NewLine, 500, form);
            LoadCP437();
            engine.CurrentDirectory = "C:\\";
            engine.StartupDirectory = engine.CurrentDirectory;
            engine.ForegroundColor = Color.Silver;
            engine.BackgroundColor = Color.Black;
            engine.SystemFont = CP437;
            engine.FullscreenMode = true;
            engine.Emulation = false;
            engine.ShowMessageOfTheDay = true;
            engine.MessageOfTheDay = @"Type ""help"" for more information!";
            engine.CurrentSubkey = "HKEY_CURRENT_USER";
            engine.StartupSubkey = engine.CurrentSubkey;
            if (createRegistryConfig)
            {
                try
                {
                    BootLogWriter(displayBootProgress, "Creating registry config..." + Environment.NewLine, 500, form);
                    startupConfigKey.SetValue("StartupDirectory", "C:\\");
                    startupConfigKey.SetValue("ForegroundColor", "Silver");
                    startupConfigKey.SetValue("BackgroundColor", "Black");
                    startupConfigKey.SetValue("SystemFont", "CP437");
                    startupConfigKey.SetValue("FullscreenMode", 1, RegistryValueKind.DWord);
                    startupConfigKey.SetValue("Emulation", 0, RegistryValueKind.DWord);
                    startupConfigKey.SetValue("ShowMessageOfTheDay", 1, RegistryValueKind.DWord);
                    startupConfigKey.SetValue("MessageOfTheDay", @"Type ""help"" for more information!");
                    startupConfigKey.SetValue("StartupSubkey", "HKEY_CURRENT_USER\\");
                    form.BootSource = BootOption.Registry;
                }
                catch (Exception)
                {
                    BootLogWriter(displayBootProgress, "Failed to create registry config..." + Environment.NewLine, 500, form);
                    form.BootSource = BootOption.Basic;
                }
            }
            if (createINIConfig)
            {
                try
                {
                    BootLogWriter(displayBootProgress, "Creating INI config..." + Environment.NewLine, 500, form);
                    StreamWriter sw = new StreamWriter("BOOT.ini");
                    sw.Write(String.Format(@"C:\\{0}Silver{0}Black{0}CP437{0}1{0}0{0}1{0}Type ""help"" for more information!", Environment.NewLine));
                    sw.Close();
                    form.BootSource = BootOption.INI;
                }
                catch (Exception)
                {
                    BootLogWriter(displayBootProgress, "Failed to create INI config..." + Environment.NewLine, 500, form);
                    form.BootSource = BootOption.Basic;
                }
            }
        }
    }

    public class DiscEngine
    {
        public enum HashType { SHA512, SHA384, SHA256, SHA1 };

        private Process p;

        private Regex hex = new Regex("^[0-9a-fA-F]*$");

        public Regex onlyDigit = new Regex("^[0-9]*$");

        private RegistryKey CurrentHiveKey;

        public string CurrentDirectory;

        public string CurrentSubkey;

        public string StartupSubkey;

        public string StartupDirectory;

        public Color ForegroundColor { get; set; }

        public Color BackgroundColor { get; set; }

        public Font SystemFont;

        public string MessageOfTheDay;

        public bool ShowMessageOfTheDay;

        public bool FullscreenMode;

        public bool Emulation;

        public bool RegistryEditor;

        private DiscOperatingSystem discOS;

        public void ApplySettings(DiscOperatingSystem form, bool simulateLoading)
        {
            form.inputField.ForeColor = ForegroundColor;
            form.inputField.BackColor = BackgroundColor;
            form.inputField.Font = SystemFont;
            if (FullscreenMode)
            {
                FULLSCREEN("1", form);
            }
            if (Emulation)
            {
                ProcessStartInfo taskKill = new ProcessStartInfo("taskkill", "/F /IM explorer.exe");
                taskKill.WindowStyle = ProcessWindowStyle.Hidden;
                p = new Process();
                p.StartInfo = taskKill;
                p.Start();
                p.WaitForExit();
            }
            if (simulateLoading)
            {
                form.inputField.AppendText(Environment.NewLine + "Starting WinDOS - v" + Assembly.GetEntryAssembly().GetName().Version.ToString());
                for (int i = 0; i < 5; i++)
                {
                    form.inputField.AppendText(".");
                    Thread.Sleep(500);
                }
            }
            if (CurrentSubkey != null)
            {
                if (CurrentSubkey.Contains("HKEY_CLASSES_ROOT"))
                {
                    CurrentHiveKey = Registry.ClassesRoot;
                }
                else if (CurrentSubkey.Contains("HKEY_CURRENT_CONFIG"))
                {
                    CurrentHiveKey = Registry.CurrentConfig;
                }
                else if (CurrentSubkey.Contains("HKEY_CURRENT_USER"))
                {
                    CurrentHiveKey = Registry.CurrentUser;
                }
                else if (CurrentSubkey.Contains("HKEY_LOCAL_MACHINE"))
                {
                    CurrentHiveKey = Registry.LocalMachine;
                }
                else if (CurrentSubkey.Contains("HKEY_USERS"))
                {
                    CurrentHiveKey = Registry.Users;
                }
            }
            form.inputField.Clear();
            if (ShowMessageOfTheDay && MessageOfTheDay != "null")
            {
                form.inputField.AppendText(MessageOfTheDay + Environment.NewLine);
            }
            form.inputField.AppendText(CurrentDirectory + "> ");
        }

        public struct Command
        {
            public string FullCommand;

            public string Call;

            public string[] Params;
        }

        public struct ThreadProgress
        {
            public TimeSpan TimeLeft;

            public TimeSpan TimeElapsed;

            public DateTime StartTime;

            public double Percentage;
        }

        public Command GetCommand(string input)
        {
            Command cmd = new Command();
            cmd.FullCommand = input;
            cmd.Params = cmd.FullCommand.Split(' ');
            cmd.Call = cmd.Params[0].ToLower();
            return cmd;
        }

        private int FindLastChar(char charToFind, string input)
        {
            int index = 0;
            if (input.Last() == '\\')
            {
                input.Remove(input.Length - 1);
            }
            for (int i = 0; i < input.Length; i++)
            {
                if (input[i] == charToFind)
                {
                    index = i;
                }
            }
            return index;
        }

        public ThreadProgress GetProgress(DateTime startTime, TimeSpan timeElapsed, double objCount, double currentObjIndex)
        {
            ThreadProgress tProg = new ThreadProgress();
            tProg.StartTime = startTime;
            tProg.TimeElapsed = timeElapsed;
            tProg.TimeLeft = TimeSpan.FromTicks(DateTime.Now.Subtract(startTime).Ticks * ((long)objCount - ((long)currentObjIndex + 1)) / ((long)currentObjIndex + 1));
            tProg.Percentage = (currentObjIndex + 1) / objCount * 100;
            return tProg;
        }

        public bool ExecuteCommand(Command cmd, bool fileInput, DiscOperatingSystem form, Bootloader bLoader)
        {
            discOS = form;
            try
            {
                if (!RegistryEditor)
                {
                    switch (cmd.Call)
                    {
                        case "cd":
                            if (cmd.FullCommand == "cd .." || cmd.FullCommand == "cd stdir" || cmd.FullCommand == "cd wdir")
                            {
                                CD(cmd.Params[1], form);
                            }
                            else
                            {
                                CD(cmd.FullCommand.Substring(3), form);
                            }
                            break;
                        case "dir":
                            if (cmd.FullCommand == "dir " || cmd.FullCommand == "dir")
                            {
                                DIR(null, form);
                            }
                            else
                            {
                                DIR(cmd.FullCommand.Substring(4), form);
                            }
                            break;
                        case "md":
                            MD(cmd.FullCommand.Substring(3));
                            break;
                        case "rd":
                            RD(cmd.FullCommand.Substring(3));
                            break;
                        case "mf":
                            MF(cmd.FullCommand.Substring(3));
                            break;
                        case "fc":
                            FC(cmd.FullCommand.Substring(3), form);
                            break;
                        case "open":
                            OPEN(cmd.FullCommand.Substring(5), form);
                            break;
                        case "copy":
                            COPY(cmd.Params[1], cmd.Params[2], form);
                            break;
                        case "move":
                            MOVE(cmd.Params[1], cmd.Params[2], form);
                            break;
                        case "del":
                            DEL(cmd.FullCommand.Substring(4), form);
                            break;
                        case "ren":
                            REN(cmd.Params[1], cmd.Params[1], form);
                            break;
                        case "forecolor":
                            FORECOLOR(cmd.Params[1], form);
                            break;
                        case "backcolor":
                            BACKCOLOR(cmd.Params[1], form);
                            break;
                        case "setfont":
                            SETFONT(cmd.FullCommand.Substring(8), form, bLoader);
                            break;
                        case "fullscreen":
                            FULLSCREEN(cmd.Params[1], form);
                            break;
                        case "emulation":
                            EMULATION(cmd.Params[1], form);
                            break;
                        case "regedit":
                            REGEDIT(form);
                            break;
                        case "wordgen":
                            if (!fileInput)
                            {
                                if (cmd.Params.Length == 4)
                                {
                                    if (cmd.Params[3].Contains(@"\"))
                                    {
                                        if (Directory.Exists(cmd.Params[3].Remove(FindLastChar('\\', cmd.Params[3]))))
                                        {
                                            form.OpenFilePath = cmd.Params[3];
                                            form.t = new Thread(() => WORDGEN(cmd.Params[1].Split(','), int.Parse(cmd.Params[2]), form));
                                            form.t.Start();
                                        }
                                        else if (Directory.Exists((CurrentDirectory + cmd.Params[3]).Remove(FindLastChar('\\', cmd.Params[3]))))
                                        {
                                            form.OpenFilePath = CurrentDirectory + cmd.Params[3];
                                            form.t = new Thread(() => WORDGEN(cmd.Params[1].Split(','), int.Parse(cmd.Params[2]), form));
                                            form.t.Start();
                                        }
                                        else
                                        {
                                            form.inputField.AppendText(Environment.NewLine + "The specified path is invalid!");
                                        }
                                    }
                                    else
                                    {
                                        if (cmd.Params[3].Length > 0)
                                        {
                                            form.OpenFilePath = CurrentDirectory + cmd.Params[3];
                                        }
                                        else
                                        {
                                            form.OpenFilePath = CurrentDirectory + "wordgen.txt";
                                        }
                                        form.t = new Thread(() => WORDGEN(cmd.Params[1].Split(','), int.Parse(cmd.Params[2]), form));
                                        form.t.Start();
                                    }
                                }
                                else if (cmd.Params.Length == 3)
                                {
                                    form.OpenFilePath = CurrentDirectory + "wordgen.txt";
                                    form.t = new Thread(() => WORDGEN(cmd.Params[1].Split(','), int.Parse(cmd.Params[2]), form));
                                    form.t.Start();
                                }
                                else
                                {
                                    form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                                }
                            }
                            break;
                        case "hashcrack":
                            if (!fileInput)
                            {
                                try
                                {
                                    switch (cmd.Params[3])
                                    {
                                        case "sha1":
                                            if (File.Exists(cmd.Params[2]) && cmd.Params[1].Length == 40 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(cmd.Params[2]), HashType.SHA1, form));
                                                form.t.Start();
                                            }
                                            else if (File.Exists(CurrentDirectory + cmd.Params[2]) && cmd.Params[1].Length == 40 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(CurrentDirectory + cmd.Params[2]), HashType.SHA1, form));
                                                form.t.Start();
                                            }
                                            else
                                            {
                                                throw new Exception();
                                            }
                                            break;
                                        case "sha256":
                                            if (File.Exists(cmd.Params[2]) && cmd.Params[1].Length == 64 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(cmd.Params[2]), HashType.SHA256, form));
                                                form.t.Start();
                                            }
                                            else if (File.Exists(CurrentDirectory + cmd.Params[2]) && cmd.Params[1].Length == 64 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(CurrentDirectory + cmd.Params[2]), HashType.SHA256, form));
                                                form.t.Start();
                                            }
                                            else
                                            {
                                                throw new Exception();
                                            }
                                            break;
                                        case "sha384":
                                            if (File.Exists(cmd.Params[2]) && cmd.Params[1].Length == 96 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(cmd.Params[2]), HashType.SHA384, form));
                                                form.t.Start();
                                            }
                                            else if (File.Exists(CurrentDirectory + cmd.Params[2]) && cmd.Params[1].Length == 96 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(CurrentDirectory + cmd.Params[2]), HashType.SHA384, form));
                                                form.t.Start();
                                            }
                                            else
                                            {
                                                throw new Exception();
                                            }
                                            break;
                                        case "sha512":
                                            if (File.Exists(cmd.Params[2]) && cmd.Params[1].Length == 128 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(cmd.Params[2]), HashType.SHA512, form));
                                                form.t.Start();
                                            }
                                            else if (File.Exists(CurrentDirectory + cmd.Params[2]) && cmd.Params[1].Length == 128 && hex.IsMatch(cmd.Params[1]))
                                            {
                                                form.t = new Thread(() => HASHCRACK(cmd.Params[1], File.ReadAllLines(CurrentDirectory + cmd.Params[2]), HashType.SHA512, form));
                                                form.t.Start();
                                            }
                                            else
                                            {
                                                throw new Exception();
                                            }
                                            break;
                                        default:
                                            form.inputField.AppendText(Environment.NewLine + "Invalid hash type!");
                                            break;
                                    }
                                }
                                catch (Exception)
                                {
                                    form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                                }
                            }
                            break;
                        case "mh":
                            try
                            {
                                switch (cmd.Params[2])
                                {
                                    case "sha1":
                                        MH(cmd.Params[1], HashType.SHA1, form);
                                        break;
                                    case "sha256":
                                        MH(cmd.Params[1], HashType.SHA256, form);
                                        break;
                                    case "sha384":
                                        MH(cmd.Params[1], HashType.SHA384, form);
                                        break;
                                    case "sha512":
                                        MH(cmd.Params[1], HashType.SHA512, form);
                                        break;
                                    default:
                                        form.inputField.AppendText(Environment.NewLine + "Invalid hash type!");
                                        break;
                                }
                            }
                            catch (Exception)
                            {
                                form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                            }
                            break;
                        case "biosinfo":
                            BIOSINFO(form);
                            break;
                        case "driver-info":
                            DRIVER_INFO(form);
                            break;
                        case "driver-check":
                            DRIVER_CHECK(form);
                            break;
                        case "getnics":
                            switch (cmd.Params.Length)
                            {
                                case 1:
                                    GETNICS(null, form);
                                    break;
                                case 2:
                                    if (cmd.Params[1] == "/all")
                                    {
                                        GETNICS(cmd.Params[1], form);
                                    }
                                    else
                                    {
                                        form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                                    }
                                    break;
                            }
                            break;
                        case "nmap":
                            if (!fileInput)
                            {
                                form.t = new Thread(() => NMAP(form));
                                form.t.Start();
                            }
                            break;
                        case "setmac":
                            SETMAC(cmd.Params[1].Replace("_", " "), cmd.Params[2], form);
                            break;
                        case "resetmac":
                            RESETMAC(cmd.Params[1].Replace("_", " "), form);
                            break;
                        case "startup-config":
                            STARTUP_CONFIG(form, bLoader);
                            break;
                        case "copy-current-config":
                            COPY_CURRENT_CONFIG(cmd.Params[1], form, bLoader);
                            break;
                        case "erase-ini-config":
                            ERASE_INI_CONFIG(form);
                            break;
                        case "erase-registry-config":
                            ERASE_REGISTRY_CONFIG(form);
                            break;
                        case "startup-dir":
                            STARTUP_DIR(cmd.FullCommand.Substring(12), form, bLoader);
                            break;
                        case "startup-fcolor":
                            STARTUP_FCOLOR(cmd.Params[1], form, bLoader);
                            break;
                        case "startup-bcolor":
                            STARTUP_BCOLOR(cmd.Params[1], form, bLoader);
                            break;
                        case "startup-font":
                            STARTUP_FONT(cmd.FullCommand.Substring(13), form, bLoader);
                            break;
                        case "startup-fullscreen":
                            STARTUP_FULLSCREEN(cmd.Params[1], form, bLoader);
                            break;
                        case "startup-emulation":
                            STARTUP_EMULATION(cmd.Params[1], form, bLoader);
                            break;
                        case "startup-shmotd":
                            STARTUP_SHMOTD(cmd.Params[1], form, bLoader);
                            break;
                        case "startup-motd":
                            STARTUP_MOTD(cmd.FullCommand.Substring(5), form, bLoader);
                            break;
                        case "startup-subkey":
                            STARTUP_SUBKEY(cmd.FullCommand.Substring(15), form, bLoader);
                            break;
                        case "exec":
                            if (!fileInput)
                            {
                                EXEC(cmd.FullCommand.Substring(5), form, bLoader);
                            }
                            break;
                        case "start":
                            START(cmd.FullCommand.Substring(5), form);
                            break;
                        case "cls":
                            form.inputField.Clear();
                            break;
                        case "help":
                            HELP(RegistryEditor, form);
                            break;
                        case "restart":
                            Application.Restart();
                            break;
                        case "exit":
                            EXIT(form);
                            break;
                        default:
                            break;
                    }

                    if (cmd.Call != "regedit" && cmd.Call != "open" && cmd.Call != "restart" && cmd.Call != "exit" && form.CanWrite && !form.FileIsOpen)
                    {
                        if (form.inputField.Text != "")
                        {
                            form.inputField.AppendText(Environment.NewLine);
                        }
                        form.inputField.AppendText(CurrentDirectory + "> ");
                    }
                    return true;
                }
                else
                {
                    switch (cmd.Call)
                    {
                        case "csk":
                            if (cmd.FullCommand == "csk .." || cmd.FullCommand == "csk stsk")
                            {
                                CSK(cmd.Params[1], form);
                            }
                            else
                            {
                                CSK(cmd.FullCommand.Substring(4), form);
                            }
                            break;
                        case "sk-get":
                            if (cmd.FullCommand == cmd.Call)
                            {
                                SK_GET(null, form);
                            }
                            else
                            {
                                SK_GET(cmd.FullCommand.Substring(7), form);
                            }
                            break;
                        case "sk-make":
                            SK_MAKE(cmd.FullCommand.Substring(8), form);
                            break;
                        case "sk-delete":
                            SK_DELETE(cmd.FullCommand.Substring(10), form);
                            break;
                        case "val-get":
                            if (cmd.FullCommand == cmd.Call)
                            {
                                VAL_GET(null, form);
                            }
                            else
                            {
                                VAL_GET(cmd.FullCommand.Substring(7), form);
                            }
                            break;
                        case "val-set":
                            VAL_SET(cmd.Params[1].Replace("#", " "), cmd.Params[2].Replace("#", " "), form);
                            break;
                        case "val-delete":
                            VAL_DELETE(cmd.FullCommand.Substring(11), form);
                            break;
                        case "val-make":
                            if (cmd.Params.Length == 4)
                            {
                                switch (cmd.Params[3].ToLower())
                                {
                                    case "none":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.None, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    case "unknown":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.Unknown, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    case "multistring":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.MultiString, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    case "expandstring":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.ExpandString, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    case "string":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.String, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    case "dword":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.DWord, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    case "qword":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.QWord, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    case "binary":
                                        VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.Binary, cmd.Params[2].Replace("#", " "), form);
                                        break;
                                    default:
                                        form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                                        break;
                                }
                            }
                            else
                            {
                                VAL_MAKE(cmd.Params[1].Replace("#", " "), RegistryValueKind.String, cmd.Params[2].Replace("#", " "), form);
                            }
                            break;
                        case "val-rename":
                            VAL_RENAME(cmd.Params[1].Replace("#", " "), cmd.Params[2].Replace("#", " "), form);
                            break;
                        case "cls":
                            form.inputField.Clear();
                            break;
                        case "help":
                            HELP(RegistryEditor, form);
                            break;
                        case "exit":
                            EXIT(form);
                            break;
                        default:
                            break;
                    }
                    if (cmd.Call != "exit")
                    {
                        if (form.inputField.Text != "")
                        {
                            form.inputField.AppendText(Environment.NewLine);
                        }
                        form.inputField.AppendText(CurrentSubkey + "> ");
                    }
                }
                return true;

            }
            catch (IndexOutOfRangeException)
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid arguments!" + Environment.NewLine + CurrentDirectory + "> ");
                return false;
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred: " + ex.Message + Environment.NewLine + CurrentDirectory + "> ");
                return false;
                throw;
            }
        }

        #region DEFAULT COMMANDS
        private void CD(string input, DiscOperatingSystem form)
        {
            switch (input)
            {
                case "..":
                    if (CurrentDirectory.Length != CurrentDirectory.Replace("\\", "").Length + 1)
                    {
                        CurrentDirectory = CurrentDirectory.Remove(CurrentDirectory.Length - 1);
                        CurrentDirectory = CurrentDirectory.Remove(FindLastChar('\\', CurrentDirectory) + 1);
                    }
                    break;
                case "stdir":
                    CurrentDirectory = StartupDirectory;
                    break;
                case "wdir":
                    CurrentDirectory = Environment.CurrentDirectory;
                    break;
                default:
                    if (Directory.Exists(input))
                    {
                        CurrentDirectory = input;
                    }
                    else if (Directory.Exists(CurrentDirectory + input))
                    {
                        CurrentDirectory += input;
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "Directory not found!");
                    }
                    if (CurrentDirectory.Last() != '\\')
                    {
                        CurrentDirectory += '\\';
                    }
                    break;
            }
        }

        private void DIR(string input, DiscOperatingSystem form)
        {
            string[] array = null;
            switch (input)
            {
                case null:
                    array = Directory.GetFileSystemEntries(CurrentDirectory);
                    break;
                default:
                    if (input.Last() != '\\')
                    {
                        input += '\\';
                    }
                    if (Directory.Exists(input))
                    {
                        array = Directory.GetFileSystemEntries(input);
                    }
                    else if (Directory.Exists(input + CurrentDirectory))
                    {
                        array = Directory.GetFileSystemEntries(input + CurrentDirectory);
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "Directory not found!");
                    }
                    break;
            }
            if (array != null)
            {
                for (int i = 0; i < array.Length; i++)
                {
                    form.inputField.AppendText(Environment.NewLine + "    " + array[i]);
                }
            }
        }

        private void MD(string input)
        {
            Directory.CreateDirectory(CurrentDirectory + input);
        }

        private void RD(string input)
        {
            if (Directory.Exists(CurrentDirectory + input))
            {
                Directory.Delete(CurrentDirectory + input, true);
            }
        }

        private void MF(string input)
        {
            File.Create(CurrentDirectory + input).Close();
        }

        private void FC(string input, DiscOperatingSystem form)
        {
            if (File.Exists(input))
            {
                form.inputField.AppendText(Environment.NewLine + File.ReadAllText(input));
            }
            else if (File.Exists(CurrentDirectory + input))
            {
                form.inputField.AppendText(Environment.NewLine + File.ReadAllText(CurrentDirectory + input));
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "File not found!");
            }
        }

        private void OPEN(string input, DiscOperatingSystem form)
        {
            form.OpenFilePath = null;
            if (File.Exists(input))
            {
                form.OpenFilePath = input;
            }
            else if (File.Exists(CurrentDirectory + input))
            {
                form.OpenFilePath = CurrentDirectory + input;
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "File not found!");
            }
            if (form.OpenFilePath != null)
            {
                form.FileIsOpen = true;
                form.inputField.Clear();
                form.inputField.AppendText(File.ReadAllText(form.OpenFilePath));
            }
        }

        private void COPY(string input, string output, DiscOperatingSystem form)
        {
            if (input.Contains('.'))
            {
                if (Directory.Exists(output.Substring(0, FindLastChar('\\', output))) && output.Contains(CurrentDirectory))
                {
                    if (File.Exists(input))
                    {
                        File.Copy(input, output);
                    }
                    else if (File.Exists(CurrentDirectory + input))
                    {
                        File.Copy(CurrentDirectory + input, output);
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "File not found!");
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Destination directory not found!");
                }
            }
            else
            {
                if (Directory.Exists(output.Substring(0, FindLastChar('\\', output))))
                {
                    if (Directory.Exists(input))
                    {
                        p = new Process();
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "xcopy.exe");
                        p.StartInfo.Arguments = input + " " + output + " /E /I /Y";
                        p.Start();
                        p.Exited += P_Exited; ;
                    }
                    else if (Directory.Exists(CurrentDirectory + input))
                    {
                        p = new Process();
                        p.StartInfo.CreateNoWindow = true;
                        p.StartInfo.UseShellExecute = false;
                        p.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "xcopy.exe");
                        p.StartInfo.Arguments = CurrentDirectory + input + " " + output + " /E /I /Y";
                        p.Start();
                        p.Exited += P_Exited;
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "Directory not found!");
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Destination directory not found!");
                }
            }
        }

        private void MOVE(string input, string output, DiscOperatingSystem form)
        {
            if (input.Contains('.'))
            {
                if (Directory.Exists(output.Substring(0, FindLastChar('\\', output))) && output.Contains(CurrentDirectory))
                {
                    if (File.Exists(input))
                    {
                        File.Move(input, output);
                    }
                    else if (File.Exists(CurrentDirectory + input))
                    {
                        File.Move(CurrentDirectory + input, output);
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "File not found!");
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Destination directory not found!");
                }
            }
            else
            {
                if (Directory.Exists(output.Substring(0, FindLastChar('\\', output))))
                {
                    if (Directory.Exists(input))
                    {
                        Directory.Move(input, output);
                    }
                    else if (Directory.Exists(CurrentDirectory + input))
                    {
                        Directory.Move(CurrentDirectory + input, output);
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "Directory not found!");
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Destination directory not found!");
                }
            }
        }

        private void P_Exited(object sender, EventArgs e)
        {
            p.Close();
        }

        private void DEL(string input, DiscOperatingSystem form)
        {
            if (File.Exists(input))
            {
                File.Delete(input);
            }
            else if (File.Exists(CurrentDirectory + input))
            {
                File.Delete(CurrentDirectory + input);
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "File not found!");
            }
        }

        private void REN(string input, string output, DiscOperatingSystem form)
        {
            if (File.Exists(input))
            {
                File.Move(input, input.Remove(FindLastChar('\\', input) + 1) + output);
            }
            else if (File.Exists(CurrentDirectory + input))
            {
                File.Move(CurrentDirectory + input, (CurrentDirectory + input).Remove(FindLastChar('\\', CurrentDirectory + input) + 1) + output);
            }
            else if (Directory.Exists(input))
            {
                Directory.Move(input, input.Remove(FindLastChar('\\', input) + 1) + output);
            }
            else if (Directory.Exists(CurrentDirectory + input))
            {
                Directory.Move(CurrentDirectory + input, (CurrentDirectory + input).Remove(FindLastChar('\\', CurrentDirectory + input) + 1) + output);
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "File or directory not found!");
            }
        }

        private void FORECOLOR(string input, DiscOperatingSystem form)
        {
            if (BackgroundColor != Color.FromName(input) && BackgroundColor.Name != input && Color.FromName(input).ToKnownColor() != 0)
            {
                ForegroundColor = Color.FromName(input);
                form.inputField.ForeColor = ForegroundColor;
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid color! Foreground color should not be the same as background color!");
            }
        }

        private void BACKCOLOR(string input, DiscOperatingSystem form)
        {
            if (ForegroundColor != Color.FromName(input) && ForegroundColor.Name != input && Color.FromName(input).ToKnownColor() != 0)
            {
                BackgroundColor = Color.FromName(input);
                form.inputField.BackColor = BackgroundColor;
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid color! Background color should not be the same as foreground color!");
            }
        }

        private void SETFONT(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            string[] fontParams = input.Split(' ');
            FontFamily fFamily = form.inputField.Font.FontFamily;
            FontStyle fStyle = form.inputField.Font.Style;
            float fSize = form.inputField.Font.Size;
            switch (fontParams.Length)
            {
                case 1:
                    if (fontParams[0].Replace("_", " ") == "CP437")
                    {
                        fFamily = bLoader.fonts.Families[0];
                    }
                    else
                    {
                        try
                        {

                            fFamily = new FontFamily(fontParams[0].Replace("_", " "));
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font family!");
                        }
                    }
                    break;
                case 2:
                    if (fontParams[0].Replace("_", " ") == "CP437")
                    {
                        fFamily = bLoader.fonts.Families[0];
                    }
                    else
                    {
                        try
                        {
                            fFamily = new FontFamily(fontParams[0].Replace("_", " "));
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font family!");
                        }
                    }

                    if (onlyDigit.IsMatch(fontParams[1]))
                    {
                        try
                        {
                            fSize = int.Parse(fontParams[1]);
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font size!");
                        }
                    }
                    else
                    {
                        switch (fontParams[1])
                        {
                            case "regular":
                                fStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fStyle = FontStyle.Italic;
                                break;
                            default:
                                form.inputField.AppendText(Environment.NewLine + "Invalid font style!");
                                break;
                        }
                    }
                    break;
                case 3:
                    if (fontParams[0].Replace("_", " ") == "CP437")
                    {
                        fFamily = bLoader.fonts.Families[0];
                    }
                    else
                    {
                        try
                        {
                            fFamily = new FontFamily(fontParams[0].Replace("_", " "));
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font family!");
                        }
                    }

                    if (onlyDigit.IsMatch(fontParams[1]))
                    {
                        try
                        {
                            fSize = int.Parse(fontParams[1]);
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font size!");
                        }

                        switch (fontParams[2])
                        {
                            case "regular":
                                fStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fStyle = FontStyle.Italic;
                                break;
                            default:
                                form.inputField.AppendText(Environment.NewLine + "Invalid font style!");
                                break;
                        }
                    }
                    else
                    {
                        switch (fontParams[1])
                        {
                            case "regular":
                                fStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fStyle = FontStyle.Italic;
                                break;
                            default:
                                form.inputField.AppendText(Environment.NewLine + "Invalid font style!");
                                break;
                        }

                        try
                        {
                            fSize = int.Parse(fontParams[2]);
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font size!");
                        }
                    }
                    break;
                default:
                    break;
            }
            form.inputField.Font = new Font(fFamily, fSize, fStyle);
            SystemFont = form.inputField.Font;
        }

        private void FULLSCREEN(string input, DiscOperatingSystem form)
        {
            try
            {
                switch (int.Parse(input))
                {
                    case 0:
                        form.FormBorderStyle = FormBorderStyle.Sizable;
                        form.Bounds = form.originalBonds;
                        form.inputField.Size = new Size(form.Width + 20, form.Height - 39);
                        FullscreenMode = false;
                        break;
                    case 1:
                        form.FormBorderStyle = FormBorderStyle.None;
                        form.Bounds = Screen.PrimaryScreen.Bounds;
                        form.inputField.Size = new Size(form.Width + 20, form.Height);
                        FullscreenMode = true;
                        break;
                    default:
                        form.inputField.AppendText(Environment.NewLine + "Invalid numeric value!");
                        break;
                }
            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid numeric value!");
            }
        }

        private void REGEDIT(DiscOperatingSystem form)
        {
            if (form.BootSource == BootOption.Registry)
            {
                RegistryEditor = true;
                form.inputField.Clear();
                form.inputField.AppendText(CurrentSubkey + "> ");
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Access denied!");
            }
        }

        private void EMULATION(string input, DiscOperatingSystem form)
        {
            switch (input)
            {
                case "0":
                    if (Emulation)
                    {
                        Process.Start("explorer.exe");
                        Emulation = false;
                    }
                    break;
                case "1":
                    if (!Emulation)
                    {
                        ProcessStartInfo taskKill = new ProcessStartInfo("taskkill", "/F /IM explorer.exe");
                        taskKill.WindowStyle = ProcessWindowStyle.Hidden;
                        p = new Process();
                        p.StartInfo = taskKill;
                        p.Start();
                        p.WaitForExit();
                        Emulation = true;
                    }
                    break;
                default:
                    form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                    break;
            }
        }

        private void WORDGEN(string[] chars, int wordLength, DiscOperatingSystem form)
        {
            try
            {
                form.WordgenIsRunning = true;
                form.ThreadStartTime = DateTime.Now;
                form.CanWrite = false;
                int[] locValue = new int[wordLength];
                for (int i = 0; i < locValue.Length; i++)
                {
                    locValue[i] = 0;
                }
                form.WordgenComboCount = Math.Pow(chars.Length, locValue.Length);
                for (int i = 0; i < form.WordgenComboCount; i++)
                {
                    string currentString = String.Empty;
                    for (int j = 0; j < locValue.Length; j++)
                    {
                        if (locValue[j] == chars.Length)
                        {
                            locValue[j] = 0;
                            try
                            {
                                locValue[j + 1]++;
                            }
                            catch (Exception)
                            {
                            }
                        }
                        currentString += chars[locValue[j]];
                    }
                    form.WordgenCurrentIndex = i;
                    locValue[0]++;
                    StreamWriter sw = new StreamWriter(form.OpenFilePath, true);
                    sw.WriteLine(currentString);
                    sw.Close();
                }
                form.WordgenIsRunning = false;
                form.CanWrite = true;
                form.inputField.Invoke(new MethodInvoker(() => form.inputField.AppendText(Environment.NewLine + CurrentDirectory + "> ")));
                form.t.Abort();
            }
            catch (ThreadAbortException)
            {
            }
            catch (UnauthorizedAccessException)
            {
                form.inputField.AppendText(Environment.NewLine + "Access denied!");
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void HASHCRACK(string hash, string[] wordList, HashType shaType, DiscOperatingSystem form)
        {
            form.CanWrite = false;
            bool found = false;
            switch (shaType)
            {
                case HashType.SHA1:
                    SHA1 sha1 = SHA1.Create();
                    try
                    {
                        string currentHash = String.Empty;
                        for (int i = 0; i < wordList.Length; i++)
                        {
                            currentHash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                            if (currentHash == hash)
                            {
                                form.inputField.Invoke(new MethodInvoker(() =>
                                {
                                    form.inputField.Text = "Password found: " + wordList[i];
                                    found = true;
                                }));
                                break;
                            }
                        }
                        try
                        {
                            form.inputField.Invoke(new MethodInvoker(() => {
                                if (!found)
                                {
                                    form.inputField.AppendText(Environment.NewLine + "Password not found!");
                                }
                                form.inputField.AppendText(Environment.NewLine + CurrentDirectory + "> ");
                            }));
                            form.CanWrite = true;
                            form.t.Abort();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    catch (ThreadAbortException)
                    {

                    }
                    catch (UnauthorizedAccessException)
                    {
                        form.inputField.AppendText(Environment.NewLine + "Access denied!");
                    }
                    catch (Exception ex)
                    {
                        form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
                    }
                    break;
                case HashType.SHA256:
                    SHA256 sha256 = SHA256.Create();
                    try
                    {
                        string currentHash = String.Empty;
                        for (int i = 0; i < wordList.Length; i++)
                        {
                            currentHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                            if (currentHash == hash)
                            {
                                form.inputField.Invoke(new MethodInvoker(() =>
                                {
                                    form.inputField.Text = "Password found: " + wordList[i];
                                    found = true;
                                }));
                                break;
                            }
                        }
                        try
                        {
                            form.inputField.Invoke(new MethodInvoker(() => {
                                if (!found)
                                {
                                    form.inputField.AppendText(Environment.NewLine + "Password not found!");
                                }
                                form.inputField.AppendText(Environment.NewLine + CurrentDirectory + "> ");
                            }));
                            form.CanWrite = true;
                            form.t.Abort();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    catch (ThreadAbortException)
                    {

                    }
                    catch (UnauthorizedAccessException)
                    {
                        form.inputField.AppendText(Environment.NewLine + "Access denied!");
                    }
                    catch (Exception ex)
                    {
                        form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
                    }
                    break;
                case HashType.SHA384:
                    SHA384 sha384 = SHA384.Create();
                    try
                    {
                        string currentHash = String.Empty;
                        for (int i = 0; i < wordList.Length; i++)
                        {
                            currentHash = BitConverter.ToString(sha384.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                            if (currentHash == hash)
                            {
                                form.inputField.Invoke(new MethodInvoker(() =>
                                {
                                    form.inputField.Text = "Password found: " + wordList[i];
                                    found = true;
                                }));
                                break;
                            }
                        }
                        try
                        {
                            form.inputField.Invoke(new MethodInvoker(() => {
                                if (!found)
                                {
                                    form.inputField.AppendText(Environment.NewLine + "Password not found!");
                                }
                                form.inputField.AppendText(Environment.NewLine + CurrentDirectory + "> ");
                            }));
                            form.CanWrite = true;
                            form.t.Abort();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    catch (ThreadAbortException)
                    {

                    }
                    catch (UnauthorizedAccessException)
                    {
                        form.inputField.AppendText(Environment.NewLine + "Access denied!");
                    }
                    catch (Exception ex)
                    {
                        form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
                    }
                    break;
                case HashType.SHA512:
                    SHA512 sha512 = SHA512.Create();
                    try
                    {
                        string currentHash = String.Empty;
                        for (int i = 0; i < wordList.Length; i++)
                        {
                            currentHash = BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                            if (currentHash == hash)
                            {
                                form.inputField.Invoke(new MethodInvoker(() =>
                                {
                                    form.inputField.Text = "Password found: " + wordList[i];
                                    found = true;
                                }));
                                break;
                            }
                        }
                        try
                        {
                            form.inputField.Invoke(new MethodInvoker(() => {
                                if (!found)
                                {
                                    form.inputField.AppendText(Environment.NewLine + "Password not found!");
                                }
                                form.inputField.AppendText(Environment.NewLine + CurrentDirectory + "> ");
                            }));
                            form.CanWrite = true;
                            form.t.Abort();
                        }
                        catch (Exception)
                        {
                        }
                    }
                    catch (ThreadAbortException)
                    {

                    }
                    catch (UnauthorizedAccessException)
                    {
                        form.inputField.AppendText(Environment.NewLine + "Access denied!");
                    }
                    catch (Exception ex)
                    {
                        form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
                    }
                    break;
            }
        }

        private void MH(string input, HashType shaType, DiscOperatingSystem form)
        {
            switch (shaType)
            {
                case HashType.SHA512:
                    SHA512 sha512 = SHA512.Create();
                    form.inputField.AppendText(Environment.NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    Clipboard.SetText(BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    break;
                case HashType.SHA384:
                    SHA384 sha384 = SHA384.Create();
                    form.inputField.AppendText(Environment.NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha384.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    Clipboard.SetText(BitConverter.ToString(sha384.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    break;
                case HashType.SHA256:
                    SHA256 sha256 = SHA256.Create();
                    form.inputField.AppendText(Environment.NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    Clipboard.SetText(BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    break;
                case HashType.SHA1:
                    SHA1 sha1 = SHA1.Create();
                    form.inputField.AppendText(Environment.NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    Clipboard.SetText(BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(input))).Replace("-", ""));
                    break;
            }
        }

        private void BIOSINFO(DiscOperatingSystem form)
        {
            try
            {
                BIOS bios = new BIOS();
                bios.GetBIOSInformation();
                form.inputField.AppendText(String.Format(Environment.NewLine +
                    "Build Number: {2}" + Environment.NewLine +
                    "Caption: {3}" + Environment.NewLine +
                    "Code Set: {4}" + Environment.NewLine +
                    "Current Language: {5}" + Environment.NewLine +
                    "Description: {6}" + Environment.NewLine +
                    "Embedded Controller Major Version: {7}" + Environment.NewLine +
                    "Embedded Controller Minor Version: {8}" + Environment.NewLine +
                    "Identification Code: {9}" + Environment.NewLine +
                    "Installable Languages: {10}" + Environment.NewLine +
                    "Install Date: {11}" + Environment.NewLine +
                    "Language Edition: {12}" + Environment.NewLine +
                    "Manufacturer: {14}" + Environment.NewLine +
                    "Name: {15}" + Environment.NewLine +
                    "Other Target OS: {16}" + Environment.NewLine +
                    "Primary BIOS: {17}" + Environment.NewLine +
                    "Release Date: {18}" + Environment.NewLine +
                    "Serial Number: {19}" + Environment.NewLine +
                    "SMBIOS Version: {20}" + Environment.NewLine +
                    "SMBIOS Major Version: {21}" + Environment.NewLine +
                    "SMBIOS Minor Version: {22}" + Environment.NewLine +
                    "SMBIOS Present: {23}" + Environment.NewLine +
                    "Software Element ID: {24}" + Environment.NewLine +
                    "Software Element State: {25}" + Environment.NewLine +
                    "Status: {26}" + Environment.NewLine +
                    "System BIOS Major Version: {27}" + Environment.NewLine +
                    "System BIOS Minor Version: {28}" + Environment.NewLine +
                    "Target Operating System: {29}" + Environment.NewLine +
                    "Version: {30}" + Environment.NewLine, bios.BiosCharacteristics, bios.BIOSVersion, bios.BuildNumber, bios.Caption, bios.CodeSet, bios.CurrentLanguage, bios.Description, bios.EmbeddedControllerMajorVersion, bios.EmbeddedControllerMinorVersion, bios.IdentificationCode, bios.InstallableLanguages, bios.InstallDate, bios.LanguageEdition, bios.ListOfLanguages, bios.Manufacturer, bios.Name, bios.OtherTargetOS, bios.PrimaryBIOS, bios.ReleaseDate, bios.SerialNumber, bios.SMBIOSBIOSVersion, bios.SMBIOSMajorVersion, bios.SMBIOSMinorVersion, bios.SMBIOSPresent, bios.SoftwareElementID, bios.SoftwareElementState, bios.Status, bios.SystemBiosMajorVersion, bios.SystemBiosMinorVersion, bios.TargetOperatingSystem, bios.Version));
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void DRIVER_INFO(DiscOperatingSystem form)
        {
            try
            {
                SystemDrivers systemDrivers = new SystemDrivers();
                DriverInfo[] driverInfos = systemDrivers.GetDriverInfo();
                if (driverInfos != null)
                {
                    for (int i = 0; i < driverInfos.Length; i++)
                    {
                        form.inputField.AppendText(String.Format(
                            Environment.NewLine + Environment.NewLine + "Accept Pause: {0}" + Environment.NewLine +
                            "Accept Stop: {1}" + Environment.NewLine +
                            "Caption: {2}" + Environment.NewLine +
                            "Creation Class Name: {3}" + Environment.NewLine +
                            "Description: {4}" + Environment.NewLine +
                            "Desktop Interact: {5}" + Environment.NewLine +
                            "Display Name: {6}" + Environment.NewLine +
                            "Error Control: {7}" + Environment.NewLine +
                            "Exit Code: {8}" + Environment.NewLine +
                            "Install Date: {9}" + Environment.NewLine +
                            "Name: {10}" + Environment.NewLine +
                            "Path Name: {11}" + Environment.NewLine +
                            "Service Specific Exit Code: {12}" + Environment.NewLine +
                            "Service Type: {13}" + Environment.NewLine +
                            "Started: {14}" + Environment.NewLine +
                            "Start Mode: {15}" + Environment.NewLine +
                            "Start Name: {16}" + Environment.NewLine +
                            "State: {17}" + Environment.NewLine +
                            "Status: {18}" + Environment.NewLine +
                            "System Creation Class Name: {19}" + Environment.NewLine +
                            "System Name: {20}" + Environment.NewLine +
                            "Tag ID: {21}" + Environment.NewLine, driverInfos[i].AcceptPause, driverInfos[i].AcceptStop, driverInfos[i].Caption, driverInfos[i].CreationClassName, driverInfos[i].Description, driverInfos[i].DesktopInteract, driverInfos[i].DisplayName, driverInfos[i].ErrorControl, driverInfos[i].ExitCode, driverInfos[i].InstallDate, driverInfos[i].Name, driverInfos[i].PathName, driverInfos[i].ServiceSpecificExitCode, driverInfos[i].ServiceType, driverInfos[i].Started, driverInfos[i].StartMode, driverInfos[i].StartName, driverInfos[i].State, driverInfos[i].Status, driverInfos[i].SystemCreationClassName, driverInfos[i].SystemName, driverInfos[i].TagId));
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Couldn't find any driver.");
                }
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void DRIVER_CHECK(DiscOperatingSystem form)
        {
            try
            {
                SystemDrivers systemDrivers = new SystemDrivers();
                DriverInfo[] driverInfos = systemDrivers.GetDriverIssues();
                if (driverInfos != null)
                {
                    for (int i = 0; i < driverInfos.Length; i++)
                    {
                        form.inputField.AppendText(String.Format(
                            Environment.NewLine + Environment.NewLine + "Accept Pause: {0}" + Environment.NewLine +
                            "Accept Stop: {1}" + Environment.NewLine +
                            "Caption: {2}" + Environment.NewLine +
                            "Creation Class Name: {3}" + Environment.NewLine +
                            "Description: {4}" + Environment.NewLine +
                            "Desktop Interact: {5}" + Environment.NewLine +
                            "Display Name: {6}" + Environment.NewLine +
                            "Error Control: {7}" + Environment.NewLine +
                            "Exit Code: {8}" + Environment.NewLine +
                            "Install Date: {9}" + Environment.NewLine +
                            "Name: {10}" + Environment.NewLine +
                            "Path Name: {11}" + Environment.NewLine +
                            "Service Specific Exit Code: {12}" + Environment.NewLine +
                            "Service Type: {13}" + Environment.NewLine +
                            "Started: {14}" + Environment.NewLine +
                            "Start Mode: {15}" + Environment.NewLine +
                            "Start Name: {16}" + Environment.NewLine +
                            "State: {17}" + Environment.NewLine +
                            "Status: {18}" + Environment.NewLine +
                            "System Creation Class Name: {19}" + Environment.NewLine +
                            "System Name: {20}" + Environment.NewLine +
                            "Tag ID: {21}" + Environment.NewLine, driverInfos[i].AcceptPause, driverInfos[i].AcceptStop, driverInfos[i].Caption, driverInfos[i].CreationClassName, driverInfos[i].Description, driverInfos[i].DesktopInteract, driverInfos[i].DisplayName, driverInfos[i].ErrorControl, driverInfos[i].ExitCode, driverInfos[i].InstallDate, driverInfos[i].Name, driverInfos[i].PathName, driverInfos[i].ServiceSpecificExitCode, driverInfos[i].ServiceType, driverInfos[i].Started, driverInfos[i].StartMode, driverInfos[i].StartName, driverInfos[i].State, driverInfos[i].Status, driverInfos[i].SystemCreationClassName, driverInfos[i].SystemName, driverInfos[i].TagId));
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Couldn't find any driver issues.");
                }
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void GETNICS(string input, DiscOperatingSystem form)
        {
            try
            {
                InterfaceDiscovery interfaceDisc = new InterfaceDiscovery();
                form.inputField.Clear();
                form.inputField.AppendText("Searching for network interfaces...");
                switch (input)
                {
                    case null:

                        foreach (Interface nic in interfaceDisc.GetInterfaces())
                        {
                            form.inputField.AppendText(Environment.NewLine + Environment.NewLine + "Name: " + nic.Name +
                                Environment.NewLine + "Type: " + nic.Type +
                                Environment.NewLine + "IP Address: " + nic.IPAddress +
                                Environment.NewLine + "Subnet Mask: " + nic.SubnetMask +
                                Environment.NewLine + "Default Gateway: " + nic.DefaultGateway);
                        }
                        break;
                    case "/all":
                        foreach (Interface nic in interfaceDisc.GetInterfaces())
                        {
                            form.inputField.AppendText(Environment.NewLine + Environment.NewLine + "Name: " + nic.Name +
                                Environment.NewLine + "Description: " + nic.Description +
                                Environment.NewLine + "Instance ID: " + nic.InstanceID +
                                Environment.NewLine + "Type: " + nic.Type +
                                Environment.NewLine + "IP Address: " + nic.IPAddress +
                                Environment.NewLine + "Subnet Mask: " + nic.SubnetMask +
                                Environment.NewLine + "Default Gateway: " + nic.DefaultGateway +
                                Environment.NewLine + "MAC Address: " + nic.MACAddress +
                                Environment.NewLine + "Speed: " + nic.Speed +
                                Environment.NewLine + "Status: " + nic.Status);
                        }
                        break;
                }
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void NMAP(DiscOperatingSystem form)
        {
            form.CanWrite = false;
            form.inputField.Invoke(new MethodInvoker(() =>
            {
                form.inputField.Clear();
                form.inputField.AppendText("Searching for local hosts...");
            }));
            try
            {
                NetworkInfo networkInfo = new NetworkInfo();
                string[] addresses = networkInfo.GetAddresses();
                DeviceDiscovery deviceDisc = new DeviceDiscovery();
                deviceDisc.CurrentDevice += ProgressEvent_CurrentDevice;
                try
                {
                    deviceDisc.GetDevices(addresses);
                }
                catch (ThreadAbortException) { }
                catch (Exception ex)
                {
                    form.inputField.Invoke(new MethodInvoker(() => form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message)));
                }
                form.inputField.Invoke(new MethodInvoker(() =>
                {
                    form.inputField.AppendText(Environment.NewLine + CurrentDirectory + "> ");
                    form.CanWrite = true;
                }));
                try
                {
                    form.t.Abort();
                }
                catch (Exception) { }
            }
            catch (ThreadAbortException) { }
            catch (Exception ex)
            {
                form.inputField.Invoke(new MethodInvoker(() => form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message)));
            }
        }

        private void ProgressEvent_CurrentDevice(object sender, CurrentDeviceArgs e)
        {
            discOS.inputField.Invoke(new MethodInvoker(() =>
            {
                discOS.inputField.AppendText(String.Format("{0}Name: {1}{2}IP Address: {3}{4}MAC Address: {5}", Environment.NewLine + Environment.NewLine, e.CurrentDevice.Name, Environment.NewLine, e.CurrentDevice.IPAddress, Environment.NewLine, e.CurrentDevice.MACAddress.ToUpper().Replace("-", ":")));
            }));
        }

        private void SETMAC(string nicName, string MACAddress, DiscOperatingSystem form)
        {
            try
            {
                if (form.BootSource == BootOption.Registry)
                {
                    InterfaceDiscovery interfaceDisc = new InterfaceDiscovery();
                    Interface[] nics = interfaceDisc.GetInterfaces();

                    try
                    {
                        foreach (Interface nic in nics)
                        {
                            if (nic.Name == nicName)
                            {
                                PhysicalAddress physicalAddr = new PhysicalAddress();
                                physicalAddr.SetAddress(nic, MACAddress);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                    }
                }
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void RESETMAC(string nicName, DiscOperatingSystem form)
        {
            try
            {
                if (form.BootSource == BootOption.Registry)
                {
                    InterfaceDiscovery interfaceDisc = new InterfaceDiscovery();
                    Interface[] nics = interfaceDisc.GetInterfaces();

                    try
                    {
                        foreach (Interface nic in nics)
                        {
                            if (nic.Name == nicName)
                            {
                                PhysicalAddress physicalAddr = new PhysicalAddress();
                                physicalAddr.ResetAddress(nic);
                            }
                        }
                    }
                    catch (Exception)
                    {
                        form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                    }
                }
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void STARTUP_CONFIG(DiscOperatingSystem form, Bootloader bLoader)
        {
            try
            {
                switch (form.BootSource)
                {
                    case BootOption.Registry:
                        form.inputField.AppendText(Environment.NewLine + "STARTUP CONFIG INFORMATION READ FROM REGISTRY" + Environment.NewLine +
                        "Startup directory: " + bLoader.startupConfigKey.GetValue("StartupDirectory").ToString() + Environment.NewLine +
                        "Foreground color: " + bLoader.startupConfigKey.GetValue("ForegroundColor").ToString() + Environment.NewLine +
                        "Background color: " + bLoader.startupConfigKey.GetValue("BackgroundColor").ToString() + Environment.NewLine +
                        "System font: " + bLoader.startupConfigKey.GetValue("SystemFont").ToString() + Environment.NewLine +
                        "Fullscreen mode: " + bLoader.startupConfigKey.GetValue("FullscreenMode").ToString() + Environment.NewLine +
                        "Emulation: " + bLoader.startupConfigKey.GetValue("Emulation").ToString() + Environment.NewLine +
                        "Show Message of The Day: " + bLoader.startupConfigKey.GetValue("ShowMessageOfTheDay").ToString() + Environment.NewLine +
                        "Message of The Day: " + bLoader.startupConfigKey.GetValue("MessageOfTheDay").ToString() + Environment.NewLine +
                        "Startup subkey: " + bLoader.startupConfigKey.GetValue("StartupSubkey").ToString());
                        break;
                    case BootOption.INI:
                        string[] bootInfo = File.ReadAllLines("BOOT.ini");
                        form.inputField.AppendText(Environment.NewLine + "STARTUP CONFIG INFORMATION READ FROM BOOT.ini" + Environment.NewLine +
                        "Startup directory: " + bootInfo[0] + Environment.NewLine +
                        "Foreground color: " + bootInfo[1] + Environment.NewLine +
                        "Background color: " + bootInfo[2] + Environment.NewLine +
                        "System font: " + bootInfo[3] + Environment.NewLine +
                        "Fullscreen mode: " + bootInfo[4] + Environment.NewLine +
                        "Emulation: " + bootInfo[5] + Environment.NewLine +
                        "Show Message of The Day: " + bootInfo[6] + Environment.NewLine +
                        "Message of The Day: " + bootInfo[7]);
                        break;
                    case BootOption.Basic:
                        form.inputField.AppendText(Environment.NewLine + "BASIC STARTUP CONFIG INFORMATION" + Environment.NewLine +
                        "Startup directory: C:\\" + Environment.NewLine +
                        "Foreground color: Silver" + Environment.NewLine +
                        "Background color: Black" + Environment.NewLine +
                        "System font: CP437, 16, Regular" + Environment.NewLine +
                        "Fullscreen mode: 1" + Environment.NewLine +
                        "Emulation: 1" + Environment.NewLine +
                        "Show Message of The Day: 1" + Environment.NewLine +
                        @"Message of The Day: Type ""help"" for more information!");
                        break;
                }
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void COPY_CURRENT_CONFIG(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            switch (input.ToLower())
            {
                case "ini":
                    StreamWriter sw = new StreamWriter("BOOT.ini");
                    if (SystemFont.FontFamily.Name == "Perfect DOS VGA 437 Win")
                    {
                        sw.Write("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", Environment.NewLine, CurrentDirectory, ForegroundColor.Name, BackgroundColor.Name, "CP437 " + SystemFont.Size + " " + SystemFont.Style.ToString().ToLower(), FullscreenMode, Emulation, ShowMessageOfTheDay, MessageOfTheDay);
                    }
                    else
                    {
                        sw.Write("{1}{0}{2}{0}{3}{0}{4}{0}{5}{0}{6}{0}{7}{0}{8}", Environment.NewLine, CurrentDirectory, ForegroundColor.Name, BackgroundColor.Name, SystemFont.FontFamily.Name + " " + SystemFont.Size + " " + SystemFont.Style.ToString().ToLower(), FullscreenMode, Emulation, ShowMessageOfTheDay, MessageOfTheDay);
                    }

                    sw.Close();
                    break;
                case "registry":

                    if (form.BootSource == BootOption.Registry)
                    {
                        bLoader.startupConfigKey.SetValue("StartupDirectory", CurrentDirectory);
                        bLoader.startupConfigKey.SetValue("ForegroundColor", ForegroundColor.Name);
                        bLoader.startupConfigKey.SetValue("BackgroundColor", BackgroundColor.Name);
                        if (SystemFont.FontFamily.Name == "Perfect DOS VGA 437 Win")
                        {
                            bLoader.startupConfigKey.SetValue("SystemFont", "CP437 " + SystemFont.Size + " " + SystemFont.Style);
                        }
                        else
                        {
                            bLoader.startupConfigKey.SetValue("SystemFont", SystemFont.FontFamily.Name + " " + SystemFont.Size + " " + SystemFont.Style);
                        }

                        bLoader.startupConfigKey.SetValue("FullscreenMode", FullscreenMode, RegistryValueKind.DWord);
                        bLoader.startupConfigKey.SetValue("Emulation", Emulation, RegistryValueKind.DWord);
                        bLoader.startupConfigKey.SetValue("ShowMessageOfTheDay", ShowMessageOfTheDay, RegistryValueKind.DWord);
                        bLoader.startupConfigKey.SetValue("MessageOfTheDay", MessageOfTheDay);
                        bLoader.startupConfigKey.SetValue("StartupSubkey", CurrentSubkey);
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "Access denied!");
                    }
                    break;
                default:
                    form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
                    break;
            }
        }

        private void ERASE_INI_CONFIG(DiscOperatingSystem form)
        {
            if (File.Exists("BOOT.ini"))
            {
                File.Delete("BOOT.ini");
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "INI config not found!");
            }
        }

        private void ERASE_REGISTRY_CONFIG(DiscOperatingSystem form)
        {
            if (form.BootSource == BootOption.Registry)
            {
                Registry.LocalMachine.DeleteSubKey("SOFTWARE\\WinDOS");
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Access denied!");
            }
        }

        private void STARTUP_DIR(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            if (form.BootSource == BootOption.Registry || form.BootSource == BootOption.INI)
            {
                string newStdir = null;
                if (Directory.Exists(input))
                {
                    newStdir = input;
                }
                else if (Directory.Exists(CurrentDirectory + input))
                {
                    newStdir = CurrentDirectory + input;
                }
                else if (input == "wdir")
                {
                    newStdir = Environment.CurrentDirectory;
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Directory not found!");
                }

                if (newStdir != null)
                {
                    if (newStdir.Last() != '\\')
                    {
                        newStdir += '\\';
                    }
                    switch (form.BootSource)
                    {
                        case BootOption.Registry:
                            bLoader.startupConfigKey.SetValue("StartupDirectory", newStdir);
                            break;
                        case BootOption.INI:
                            string[] INILines = File.ReadAllLines("BOOT.ini");
                            INILines[0] = newStdir;
                            File.WriteAllLines("BOOT.ini", INILines);
                            break;
                    }
                }
                StartupDirectory = newStdir;
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
            }
        }

        private void STARTUP_FCOLOR(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            if (Color.FromName(input).ToKnownColor() != 0)
            {
                switch (form.BootSource)
                {
                    case BootOption.Registry:
                        bLoader.startupConfigKey.SetValue("ForegroundColor", input);
                        break;
                    case BootOption.INI:
                        string[] INILines = File.ReadAllLines("BOOT.ini");
                        INILines[1] = input;
                        File.WriteAllLines("BOOT.ini", INILines);
                        break;
                    case BootOption.Basic:
                        form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
                        break;
                }
            }
        }

        private void STARTUP_BCOLOR(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            if (Color.FromName(input).ToKnownColor() != 0)
            {
                switch (form.BootSource)
                {
                    case BootOption.Registry:
                        bLoader.startupConfigKey.SetValue("BackgroundColor", input);
                        break;
                    case BootOption.INI:
                        string[] INILines = File.ReadAllLines("BOOT.ini");
                        INILines[2] = input;
                        File.WriteAllLines("BOOT.ini", INILines);
                        break;
                    case BootOption.Basic:
                        form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
                        break;
                }
            }
        }

        private void STARTUP_FONT(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            string[] fontParams = input.Split(' ');
            FontFamily fFamily = form.inputField.Font.FontFamily;
            FontStyle fStyle = form.inputField.Font.Style;
            float fSize = form.inputField.Font.Size;
            switch (fontParams.Length)
            {
                case 1:
                    if (fontParams[0] == "CP437")
                    {
                        fFamily = null;
                    }
                    else
                    {
                        try
                        {

                            fFamily = new FontFamily(fontParams[0].Replace("_", " "));
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font family!");
                        }
                    }
                    break;
                case 2:
                    if (fontParams[0] == "CP437")
                    {
                        fFamily = null;
                    }
                    else
                    {
                        try
                        {
                            fFamily = new FontFamily(fontParams[0].Replace("_", " "));
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font family!");
                        }
                    }

                    if (onlyDigit.IsMatch(fontParams[1]))
                    {
                        try
                        {
                            fSize = int.Parse(fontParams[1]);
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font size!");
                        }
                    }
                    else
                    {
                        switch (fontParams[1])
                        {
                            case "regular":
                                fStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fStyle = FontStyle.Italic;
                                break;
                            default:
                                form.inputField.AppendText(Environment.NewLine + "Invalid font style!");
                                break;
                        }
                    }
                    break;
                case 3:
                    if (fontParams[0]== "CP437")
                    {
                        fFamily = bLoader.fonts.Families[0];
                    }
                    else
                    {
                        try
                        {
                            fFamily = new FontFamily(fontParams[0].Replace("_", " "));
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font family!");
                        }
                    }

                    if (onlyDigit.IsMatch(fontParams[1]))
                    {
                        try
                        {
                            fSize = int.Parse(fontParams[1]);
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font size!");
                        }

                        switch (fontParams[2])
                        {
                            case "regular":
                                fStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fStyle = FontStyle.Italic;
                                break;
                            default:
                                form.inputField.AppendText(Environment.NewLine + "Invalid font style!");
                                break;
                        }
                    }
                    else
                    {
                        switch (fontParams[1])
                        {
                            case "regular":
                                fStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fStyle = FontStyle.Italic;
                                break;
                            default:
                                form.inputField.AppendText(Environment.NewLine + "Invalid font style!");
                                break;
                        }

                        try
                        {
                            fSize = int.Parse(fontParams[2]);
                        }
                        catch (Exception)
                        {
                            form.inputField.AppendText(Environment.NewLine + "Invalid font size!");
                        }
                    }
                    break;
                default:
                    break;
            }
            switch (form.BootSource)
            {
                case BootOption.Registry:
                    if (fFamily == null)
                    {
                        bLoader.startupConfigKey.SetValue("SystemFont", "CP437 " + fSize + " " + fStyle.ToString().ToLower());
                    }
                    else
                    {
                        bLoader.startupConfigKey.SetValue("SystemFont", fFamily.Name.Replace(" ", "_") + " " + fSize + " " + fStyle.ToString().ToLower());
                    }
                    break;
                case BootOption.INI:
                    string[] INILines = File.ReadAllLines("BOOT.ini");
                    if (fFamily == null)
                    {
                        INILines[3] = "CP437 " + fSize + " " + fStyle.ToString().ToLower();
                    }
                    else
                    {
                        INILines[3] = fFamily.Name.Replace(" ", "_") + " " + fSize + " " + fStyle.ToString().ToLower();
                    }
                    File.WriteAllLines("BOOT.ini", INILines);
                    break;
                case BootOption.Basic:
                    form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
                    break;
            }

        }

        private void STARTUP_FULLSCREEN(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            switch (input)
            {
                case "0":
                    break;
                case "1":
                    break;
                default:
                    input = null;
                    break;
            }
            if (input != null)
            {
                switch (form.BootSource)
                {
                    case BootOption.Registry:
                        bLoader.startupConfigKey.SetValue("FullscreenMode", input);
                        break;
                    case BootOption.INI:
                        string[] INILines = File.ReadAllLines("BOOT.ini");
                        INILines[4] = input;
                        File.WriteAllLines("BOOT.ini", INILines);
                        break;
                    case BootOption.Basic:
                        form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
                        break;
                }
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
            }
        }

        private void STARTUP_EMULATION(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            switch (input)
            {
                case "0":
                    break;
                case "1":
                    break;
                default:
                    input = null;
                    break;
            }
            if (input != null)
            {
                switch (form.BootSource)
                {
                    case BootOption.Registry:
                        bLoader.startupConfigKey.SetValue("Emulation", input);
                        break;
                    case BootOption.INI:
                        string[] INILines = File.ReadAllLines("BOOT.ini");
                        INILines[5] = input;
                        File.WriteAllLines("BOOT.ini", INILines);
                        break;
                    case BootOption.Basic:
                        form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
                        break;
                }
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
            }
        }

        private void STARTUP_SHMOTD(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            switch (input)
            {
                case "0":
                    break;
                case "1":
                    break;
                default:
                    input = null;
                    break;
            }
            if (input != null)
            {
                switch (form.BootSource)
                {
                    case BootOption.Registry:
                        bLoader.startupConfigKey.SetValue("ShowMessageOfTheDay", input);
                        break;
                    case BootOption.INI:
                        string[] INILines = File.ReadAllLines("BOOT.ini");
                        INILines[6] = input;
                        File.WriteAllLines("BOOT.ini", INILines);
                        break;
                    case BootOption.Basic:
                        form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
                        break;
                }
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid arguments!");
            }
        }

        private void STARTUP_MOTD(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            switch (form.BootSource)
            {
                case BootOption.Registry:
                    bLoader.startupConfigKey.SetValue("MessageOfTheDay", input);
                    break;
                case  BootOption.INI:
                    string[] INILines = File.ReadAllLines("BOOT.ini");
                    INILines[7] = input;
                    File.WriteAllLines("BOOT.ini", INILines);
                    break;
                case BootOption.Basic:
                    form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode!");
                    break;
            }
        }

        private void STARTUP_SUBKEY(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            if (form.BootSource == BootOption.Registry)
            {
                string newStsk = null;
                if (input.StartsWith("HKEY_CLASSES_ROOT") && input.Replace("HKEY_CLASSES_ROOT", "").Length > 0)
                {
                    if (Registry.ClassesRoot.OpenSubKey(input.Replace("HKEY_CLASSES_ROOT\\", ""), true) != null)
                    {
                        newStsk = input;
                    }
                }
                else if (input.StartsWith("HKEY_CURRENT_CONFIG") && input.Replace("HKEY_CURRENT_CONFIG", "").Length > 0)
                {
                    if (Registry.CurrentConfig.OpenSubKey(input.Replace("HKEY_CURRENT_CONFIG\\", ""), true) != null)
                    {
                        newStsk = input;
                    }
                }
                else if (input.StartsWith("HKEY_CURRENT_USER") && input.Replace("HKEY_CURRENT_USER", "").Length > 0)
                {
                    if (Registry.CurrentUser.OpenSubKey(input.Replace("HKEY_CURRENT_USER\\", ""), true) != null)
                    {
                        newStsk = input;
                    }
                }
                else if (input.StartsWith("HKEY_LOCAL_MACHINE") && input.Replace("HKEY_LOCAL_MACHINE", "").Length > 0)
                {
                    if (Registry.LocalMachine.OpenSubKey(input.Replace("HKEY_LOCAL_MACHINE\\", ""), true) != null)
                    {
                        newStsk = input;
                    }
                }
                else if (input.StartsWith("HKEY_USERS") && input.Replace("HKEY_USERS", "").Length > 0)
                {
                    if (Registry.Users.OpenSubKey(input.Replace("HKEY_USERS\\", ""), true) != null)
                    {
                        newStsk = input;
                    }
                }
                else if (input == "HKEY_CLASSES_ROOT" || input == "HKEY_CURRENT_CONFIG" || input == "HKEY_CURRENT_USER" || input == "HKEY_LOCAL_MACHINE" || input == "HKEY_USERS" || input == "HKEY_CLASSES_ROOT" || input == "HKEY_CURRENT_CONFIG\\" || input == "HKEY_CURRENT_USER\\" || input == "HKEY_LOCAL_MACHINE\\" || input == "HKEY_USERS\\")
                {
                    newStsk = input;
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Subkey not found!");
                }

                if (newStsk != null)
                {
                    if (newStsk.Last() != '\\')
                    {
                        newStsk += '\\';
                    }
                    bLoader.startupConfigKey.SetValue("StartupSubkey", newStsk);
                    StartupSubkey = newStsk;
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "Subkey not found!");
                }
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "Cannot modify startup settings in basic mode! This startup option is unavailable ini INI config!");
            }
        }

        private void EXEC(string input, DiscOperatingSystem form, Bootloader bLoader)
        {
            string[] cmdList = null;
            if (File.Exists(input) && Path.GetExtension(input) == ".doscmd")
            {
                cmdList = File.ReadAllLines(input);
                for (int i = 0; i < cmdList.Length; i++)
                {
                    ExecuteCommand(GetCommand(cmdList[i]), true, form, bLoader);
                }
            }
            else if (File.Exists(CurrentDirectory + input) && Path.GetExtension(CurrentDirectory + input) == ".doscmd")
            {
                cmdList = File.ReadAllLines(CurrentDirectory + input);
                for (int i = 0; i < cmdList.Length; i++)
                {
                    ExecuteCommand(GetCommand(cmdList[i]), true, form, bLoader);
                }
            }
            else
            {
                form.inputField.AppendText(Environment.NewLine + "File not found or invalid file type!");
            }
        }

        private void START(string input, DiscOperatingSystem form)
        {
            try
            {
                p = new Process();
                p.StartInfo.CreateNoWindow = false;
                p.StartInfo.UseShellExecute = false;
                if (File.Exists(input))
                {
                    p.StartInfo.FileName = input;
                    p.StartInfo.WorkingDirectory = input.Replace(Path.GetFileName(input), "");
                    try
                    {
                        p.Start();
                    }
                    catch (Exception ex)
                    {
                        form.inputField.AppendText(Environment.NewLine + "An error pervented the application from starting!" + Environment.NewLine + "Error message: " + ex.Message);
                    }
                }
                else if (File.Exists(CurrentDirectory + input))
                {
                    p.StartInfo.FileName = CurrentDirectory + input;
                    p.StartInfo.WorkingDirectory = CurrentDirectory + input.Replace(Path.GetFileName(CurrentDirectory + input), "");
                    try
                    {
                        p.Start();
                    }
                    catch (Exception ex)
                    {
                        form.inputField.AppendText(Environment.NewLine + "An error pervented the application from starting!" + Environment.NewLine + "Error message: " + ex.Message);
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "File not found!");
                }
            }
            catch (UnauthorizedAccessException)
            {
                form.inputField.AppendText(Environment.NewLine + "Access denied!");
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + "An error occurred!     Error code: " + ex.Message);
            }
        }

        private void HELP(bool regedit, DiscOperatingSystem form)
        {
            if (!regedit)
            {
                form.inputField.AppendText(
                Environment.NewLine + Environment.NewLine + "[] = USER SPECIFIED PARAMETERS" +
                Environment.NewLine + Environment.NewLine + "{} = OPTIONAL PARAMETERS" +
                Environment.NewLine + Environment.NewLine + "/  = OR" +
                Environment.NewLine + Environment.NewLine + "() = ADDITIONAL INFORMATION" +

                Environment.NewLine + Environment.NewLine + "CD                           Changes current directory;  " +
                Environment.NewLine + "                             Usage: cd [folderPath/wdir(working directory)/..(the directory that contains current directory)/stdir(startup directory)];" +
                Environment.NewLine + "                             Example: cd C:\\Users\\Public" +

                Environment.NewLine + Environment.NewLine + "DIR                          Returns the content of a specified directory;" +
                Environment.NewLine + "                             Usage: dir [folderPath/wdir(working directory)/stdir(startup directory)];" +
                Environment.NewLine + "                             Example: dir C:\\Windows" +

                Environment.NewLine + Environment.NewLine + "MD                           Creates a new directory;" +
                Environment.NewLine + "                             Usage: md [folderPath];" +
                Environment.NewLine + "                             Example: md C:\\NewFolder" +

                Environment.NewLine + Environment.NewLine + "RD                           Removes the specified directory and all of it's contents;" +
                Environment.NewLine + "                             Usage: rd [folderPath];" +
                Environment.NewLine + "                             Example: rd C:\\Users\\Guest\\Desktop\\FolderToRemove" +

                Environment.NewLine + Environment.NewLine + "MF                           Creates a new file;" +
                Environment.NewLine + "                             Usage: mf [filePath] {[fileConent]};" +
                Environment.NewLine + "                             Example: mf file.txt This is a new file." +

                Environment.NewLine + Environment.NewLine + "FC                           Returns the contents of the specified file, it is read-only;" +
                Environment.NewLine + "                             Usage: fc [filePath];" +
                Environment.NewLine + "                             Example: fc file.txt" +

                Environment.NewLine + Environment.NewLine + "MOVE                         Moves the specified directory or file to a specified location;" +
                Environment.NewLine + "                             Usage: move [sourcePath] [destinationPath];" +
                Environment.NewLine + "                             Example: move F:\\DirToMove C:\\DirectoryNewName " +

                Environment.NewLine + Environment.NewLine + "COPY                         Copies the specified directory or file to a specified location;" +
                Environment.NewLine + "                             Usage: copy [sourcePath] [destinationPath];" +
                Environment.NewLine + "                             Example: copy F:\\FileToCopy C:\\FileNewName" +

                Environment.NewLine + Environment.NewLine + "OPEN                         Returns the content of the specified file, it can be edited, save with F2, cancel with ESC;" +
                Environment.NewLine + "                             Usage: open [filePath];" +
                Environment.NewLine + "                             Example: open F:\\file.txt" +

                Environment.NewLine + Environment.NewLine + "DEL                          Deletes the specified file;" +
                Environment.NewLine + "                             Usage: del [filePath];" +
                Environment.NewLine + "                             Example: del F:\\file.txt" +

                Environment.NewLine + Environment.NewLine + "REN                          Renames the specified file or directory; " +
                Environment.NewLine + "                             Warning: Uppercase letters will not be detected when renaming directory;" +
                Environment.NewLine + "                             Usage: del [filePath];" +
                Environment.NewLine + "                             Example: del F:\\file.txt" +

                Environment.NewLine + Environment.NewLine + "FORECOLOR                    Sets the text color;" +
                Environment.NewLine + "                             Usage: forecolor [colorName] where: colorName not equal to backgroundColorName;" +
                Environment.NewLine + "                             Example: forecolor silver" +

                Environment.NewLine + Environment.NewLine + "BACKCOLOR                    Sets the background color;" +
                Environment.NewLine + "                             Usage: backcolor [colorName] where: colorName not equal to foregroundColorName;" +
                Environment.NewLine + "                             Example: backcolor black" +

                Environment.NewLine + Environment.NewLine + "SETFONT                      Sets the font family size and style;" +
                Environment.NewLine + "                             Usage: setfont [fontFamily(use underscores instead of spaces)] {[fontSize]} {[fontStyle]}; " +
                Environment.NewLine + "                             Example: setfont Courier_New 12 bold" +

                Environment.NewLine + Environment.NewLine + "FULLSCREEN                   Sets the window layout;" +
                Environment.NewLine + "                             Usage: fullscreen [0/1];" +
                Environment.NewLine + "                             Example: fullscreen 0" +

                Environment.NewLine + Environment.NewLine + "EMULATION                    Kills/starts Windows Explorer;" +
                Environment.NewLine + "                             Usage: emulation [0/1];" +
                Environment.NewLine + "                             Example: emulation 0" +

                Environment.NewLine + Environment.NewLine + "WORDGEN                      Returns all possible combinations of the specified characters at a specified length and writes the generated words into a specified file; " +
                Environment.NewLine + "                             Usage: wordgen [commaSeparatedCharacters] [wordLength] {[filePath]};" +
                Environment.NewLine + "                             Example: wordgen a,s,d,f,g,h 8 wordgen.txt" +

                Environment.NewLine + Environment.NewLine + "HASHCRACK                    Attempts to reverse the specified SHA(512/384/256/1) hash to plaintext;" +
                Environment.NewLine + "                             Usage: hashcrack [hash] [filePath] [sha1/sha256/sha384/sha512]; " +
                Environment.NewLine + "                             Example: hashcrack 2C909DB24C974545F5B4956C88C2991E9DF98E67DC39DB6252C2D54AE9D01132463FE1939F7256ABD619D98BA9A96870175C69C232C985A0D3523554FF813B0D wordlist.txt sha512" +

                Environment.NewLine + Environment.NewLine + "MH                           Converts the specified text to SHA512(512/384/256/1) hash;" +
                Environment.NewLine + "                             Usage: mh [plaintext] [sha1/sha256/sha384/sha512];" +
                Environment.NewLine + "                             Example: mh textToHash sha1" +

                Environment.NewLine + Environment.NewLine + "BIOSINFO                     Returns BIOS information;" +

                Environment.NewLine + Environment.NewLine + "DRIVER-INFO                  Returns all driver information;" +

                Environment.NewLine + Environment.NewLine + "DRIVER-CHECK                 Checks for driver issues;" +

                Environment.NewLine + Environment.NewLine + "GETNICS                      Returns the queryable information of all network interfaces;" +
                Environment.NewLine + "                             Usage: getnics {/all};" +
                Environment.NewLine + "                             Example: getnics /all" +

                Environment.NewLine + Environment.NewLine + "NMAP                         Returns all hosts on all connected networks; " +

                Environment.NewLine + Environment.NewLine + "SETMAC                       Modifies the MAC address of the specified device;" +
                Environment.NewLine + "                             Usage: setmac [interfaceName(use underscores instead of spaces)] [newMACAddress];" +
                Environment.NewLine + "                             Example: setmac Ethernet A1b2C3d4E5f6" +

                Environment.NewLine + Environment.NewLine + "RESETMAC                     Sets the MAC address of the specified device to the original MAC address;" +
                Environment.NewLine + "                             Usage: resetmac [interfaceName(use underscores instead of spaces)];" +
                Environment.NewLine + "                             Example: resetmac Ethernet" +

                Environment.NewLine + Environment.NewLine + "STARTUP-CONFIG               Returns startup config information;" +

                Environment.NewLine + Environment.NewLine + "COPY-CURRENT-CONFIG          Overrides startup config with the current config at the specified location;" +
                Environment.NewLine + "                             Usage: copy-current-config [ini/registry];" +
                Environment.NewLine + "                             Example: copy-current-config registry" +

                Environment.NewLine + Environment.NewLine + "ERASE-INI-CONFIG             Deletes BOOT.ini startup config file;" +

                Environment.NewLine + Environment.NewLine + "ERASE-REGISTRY-CONFIG        Clears startup config registry;" +

                Environment.NewLine + Environment.NewLine + "STARTUP-DIR                   Sets the startup directory;" +
                Environment.NewLine + "                             Usage: startup-dir [folderPath];" +
                Environment.NewLine + "                             Example: startup-dir C:\\Users\\Public" +

                Environment.NewLine + Environment.NewLine + "STARTUP-FCOLOR               Sets the startup text color;" +
                Environment.NewLine + "                             Usage: startup-fcolor [colorName];" +
                Environment.NewLine + "                             Example startup-fcolor Silver" +

                Environment.NewLine + Environment.NewLine + "STARTUP-BCOLOR               Sets the startup background color;" +
                Environment.NewLine + "                             Usage: startup-bcolor [colorName];" +
                Environment.NewLine + "                             Example startup-bcolor Black" +

                Environment.NewLine + Environment.NewLine + "STARTUP-FONT                 Sets the startup font family, size and style;" +
                Environment.NewLine + "                             Usage: startup-font [fontFamily(use underscores instead of spaces)] {[fontSize]} {[fontStyle]};" +
                Environment.NewLine + "                             Example: startup-font Courier_New 12 bold" +

                Environment.NewLine + Environment.NewLine + "STARTUP-FULLSCREEN           Sets the startup display mode;" +
                Environment.NewLine + "                             Usage: startup-fullscreen [0/1];" +
                Environment.NewLine + "                             Example: startup-fullscreen 0" +

                Environment.NewLine + Environment.NewLine + "STARTUP-EMULATION           Kills/starts Windows Explorer at startup;" +
                Environment.NewLine + "                             Usage: startup-emulation [0/1];" +
                Environment.NewLine + "                             Example: startup-emulation 0" +

                Environment.NewLine + Environment.NewLine + "SHMOTD                       Shows or hides the message of the day at startup;" +
                Environment.NewLine + "                             Usage: shmotd [0/1];" +
                Environment.NewLine + "                             Example: shmotd 1" +

                Environment.NewLine + Environment.NewLine + "MOTD                         Sets the message of the day text;" +
                Environment.NewLine + "                             Usage: motd [motdContent];" +
                Environment.NewLine + "                             Example: motd Hello there!" +

                Environment.NewLine + Environment.NewLine + "STARTUP-SUBKEY               Sets the regedit startup subkey;" +
                Environment.NewLine + "                             Usage: startup-subkey [subkeyPath];" +
                Environment.NewLine + "                             Example: startup-subkey HKEY_LOCAL_MACHINE\\SOFTWARE\\WinDOS" +

                Environment.NewLine + Environment.NewLine + "EXEC                         Executes commands from a .doscmd file; " +
                Environment.NewLine + "                             Unsupported commands: nmap, exec, wordgen, hashcrack;" +
                Environment.NewLine + "                             Usage: exec [filePath];" +
                Environment.NewLine + "                             Example: exec command.doscmd" +

                Environment.NewLine + Environment.NewLine + "START                        Opens a file at the specified path;" +
                Environment.NewLine + "                             Usage: start [filePath];" +
                Environment.NewLine + "                             Example: start C:\\Users\\You\\Desktop\\WinDOS.exe" +

                Environment.NewLine + Environment.NewLine + "CLS                          Clears the screen;" +

                Environment.NewLine + Environment.NewLine + "RESTART                      Restarts the application;" +

                Environment.NewLine + Environment.NewLine + "EXIT                         Closes the application.");
            }
            else
            {
                form.inputField.AppendText(
                Environment.NewLine + Environment.NewLine + "[] = USER SPECIFIED PARAMETERS" +
                Environment.NewLine + Environment.NewLine + "{} = OPTIONAL PARAMETERS" +
                Environment.NewLine + Environment.NewLine + "/  = OR" +
                Environment.NewLine + Environment.NewLine + "() = ADDITIONAL INFORMATION" +

                Environment.NewLine + Environment.NewLine + "CSK                          Changes the current subkey;  " +
                Environment.NewLine + "                             Usage: csk [subkeyPath/..(the subkey that contains current subkey)/stsk(startup subkey)];" +
                Environment.NewLine + "                             Example: csk HKEY_LOCAL_MACHINE\\SYSTEM" +

                Environment.NewLine + Environment.NewLine + "SK-GET                       Returns a subkey list of the specified subkey;" +
                Environment.NewLine + "                             Usage: sk-get {[subkeyPath/stsk(startup subkey)]};" +
                Environment.NewLine + "                             Example: sk-get HKEY_CLASSES_ROOT" +

                Environment.NewLine + Environment.NewLine + "SK-MAKE                      Creates a new subkey;" +
                Environment.NewLine + "                             Usage: sk-make [subkeyPath];" +
                Environment.NewLine + "                             Example: sk-make HKEY_CURRENT_USER\\NewSubkey" +

                Environment.NewLine + Environment.NewLine + "SK-DELETE                    Deletes the specified subkey and its subkey tree;" +
                Environment.NewLine + "                             Usage: sk-make [subkeyPath];" +
                Environment.NewLine + "                             Example: sk-make HKEY_CURRENT_USER\\SubKeyToDelete" +

                Environment.NewLine + Environment.NewLine + "VAL-GET                      Returns all values in the specified subkey;" +
                Environment.NewLine + "                             Usage: val-get [{subkeyPath]};" +
                Environment.NewLine + "                             Example: sk-make HKEY_LOCAL_MACHINE\\SOFTWARE" +

                Environment.NewLine + Environment.NewLine + "VAL-SET                      Modifies an existing value in the current subkey;" +
                Environment.NewLine + "                             Usage: val-set [valueName(Use hashmarks instead of spaces!)] [value(Use hashmarks instead of spaces!)];" +
                Environment.NewLine + "                             Example: val-set valueToModify 21" +

                Environment.NewLine + Environment.NewLine + "VAL-MAKE                     Creates a new value in the current subkey;" +
                Environment.NewLine + "                             Usage: val-make [valueName(Use hashmarks instead of spaces!)] [value(Use hashmarks instead of spaces!)] " +
                Environment.NewLine + "                                    {[valueKind(none/unknown/string/multistring/expandstring/dword/qword/binary)]};" +
                Environment.NewLine + "                             Example: val-make NewValue This#is#a#string#value string" +

                Environment.NewLine + Environment.NewLine + "VAL-DELETE                   Deletes the specified value in the current subkey;" +
                Environment.NewLine + "                             Usage: val-delete [valueName];" +
                Environment.NewLine + "                             Example: val-delete valueToDelete" +

                Environment.NewLine + Environment.NewLine + "VAL-RENAME                   Renames the specified value in the current subkey;" +
                Environment.NewLine + "                             Usage: val-rename [valueName(Use hashmarks instead of spaces!)] [valueNewName(Use hashmarks instead of spaces!)];" +
                Environment.NewLine + "                             Example: val-rename Value#to#rename newName" +

                Environment.NewLine + Environment.NewLine + "CLS                          Clears the screen." +

                Environment.NewLine + Environment.NewLine + "EXIT                         Closes regedit and returns to the default DOS Command Line Interface.");
            }    
        }

        private void EXIT(DiscOperatingSystem form)
        {
            if (!RegistryEditor)
            {
                try
                {
                    form.t.Abort();
                }
                catch (Exception)
                {
                }
                if (Emulation)
                {
                    Emulation = false;
                    Process.Start("explorer.exe");
                }
                Environment.Exit(0);
            }
            else
            {
                RegistryEditor = false;
                CD("stdir", form);
                form.inputField.Clear();
                form.inputField.AppendText(CurrentDirectory + "> ");
            }  
        }
        #endregion

        #region REGEDIT COMMANDS
        private void CSK(string input, DiscOperatingSystem form)
        {
            try
            {
                if (input == "..")
                {
                    if (CurrentSubkey.Length != CurrentSubkey.Replace("\\", "").Length + 1)
                    {
                        CurrentSubkey = CurrentSubkey.Remove(CurrentSubkey.Length - 1);
                        CurrentSubkey = CurrentSubkey.Remove(FindLastChar('\\', CurrentSubkey) + 1);
                    }
                }
                else if (input == "stsk")
                {
                    CurrentSubkey = StartupSubkey;
                }
                else
                {
                    if (input.StartsWith(CurrentSubkey))
                    {

                        if (CurrentHiveKey.OpenSubKey(input, true) != null)
                        {
                            CurrentSubkey += input.Remove(0, CurrentSubkey.Length);
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                    else if (input.StartsWith("HKEY_LOCAL_MACHINE"))
                    {
                        if (input.Replace("HKEY_LOCAL_MACHINE", "").Length > 0)
                        {
                            if (Registry.LocalMachine.OpenSubKey(input.Replace(@"HKEY_LOCAL_MACHINE\", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.LocalMachine;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            if (Registry.LocalMachine.OpenSubKey(input.Replace("HKEY_LOCAL_MACHINE", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.LocalMachine;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    else if (input.StartsWith("HKEY_CLASSES_ROOT"))
                    {
                        if (input.Replace("HKEY_CLASSES_ROOT", "").Length > 0)
                        {
                            if (Registry.ClassesRoot.OpenSubKey(input.Replace(@"HKEY_CLASSES_ROOT\", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.ClassesRoot;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            if (Registry.ClassesRoot.OpenSubKey(input.Replace("HKEY_CLASSES_ROOT", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.ClassesRoot;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    else if (input.StartsWith("HKEY_CURRENT_CONFIG"))
                    {
                        if (input.Replace("HKEY_CURRENT_CONFIG", "").Length > 0)
                        {
                            if (Registry.CurrentConfig.OpenSubKey(input.Replace(@"HKEY_CURRENT_CONFIG\", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.CurrentConfig;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            if (Registry.CurrentConfig.OpenSubKey(input.Replace("HKEY_CURRENT_CONFIG", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.CurrentConfig;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    else if (input.StartsWith("HKEY_CURRENT_USER"))
                    {
                        if (input.Replace("HKEY_CURRENT_USER", "").Length > 0)
                        {
                            if (Registry.CurrentUser.OpenSubKey(input.Replace(@"HKEY_CURRENT_USER\", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.CurrentUser;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            if (Registry.CurrentUser.OpenSubKey(input.Replace("HKEY_CURRENT_USER", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.CurrentUser;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    else if (input.StartsWith("HKEY_USERS"))
                    {
                        if (input.Replace("HKEY_USERS", "").Length > 0)
                        {
                            if (Registry.Users.OpenSubKey(input.Replace(@"HKEY_USERS\", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.Users;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                        else
                        {
                            if (Registry.Users.OpenSubKey(input.Replace("HKEY_USERS", ""), true) != null)
                            {
                                CurrentHiveKey = Registry.Users;
                                CurrentSubkey = input;
                            }
                            else
                            {
                                throw new Exception();
                            }
                        }
                    }
                    else
                    {
                        if (CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", "") + input, true) != null)
                        {
                            CurrentSubkey += input;
                        }
                        else
                        {
                            throw new Exception();
                        }
                    }
                }
                if (CurrentSubkey.Last() != '\\')
                {
                    CurrentSubkey += '\\';
                }
            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Subkey not found!");
            }
        }

        private void SK_GET(string input, DiscOperatingSystem form)
        {
            try
            {
                string[] skNames = null;
                if (input != null)
                {
                    if (input.StartsWith("HKEY_LOCAL_MACHINE"))
                    {
                        if (input.Replace("HKEY_LOCAL_MACHINE", "").Length > 0)
                        {
                            skNames = Registry.LocalMachine.OpenSubKey(input.Replace(@"HKEY_LOCAL_MACHINE\", ""), true).GetSubKeyNames();
                        }
                        else
                        {
                            skNames = Registry.LocalMachine.GetSubKeyNames();
                        }
                    }
                    else if (input.StartsWith("HKEY_CLASSES_ROOT"))
                    {
                        if (input.Replace("HKEY_CLASSES_ROOT", "").Length > 0)
                        {
                            skNames = Registry.ClassesRoot.OpenSubKey(input.Replace(@"HKEY_CLASSES_ROOT\", ""), true).GetSubKeyNames();
                        }
                        else
                        {
                            skNames = Registry.ClassesRoot.GetSubKeyNames();
                        }
                    }
                    else if (input.StartsWith("HKEY_CURRENT_CONFIG"))
                    {
                        if (input.Replace("HKEY_CURRENT_CONFIG", "").Length > 0)
                        {
                            skNames = Registry.CurrentConfig.OpenSubKey(input.Replace(@"HKEY_CURRENT_CONFIG\", ""), true).GetSubKeyNames();
                        }
                        else
                        {
                            skNames = Registry.CurrentConfig.GetSubKeyNames();
                        }
                    }
                    else if (input.StartsWith("HKEY_CURRENT_USER"))
                    {
                        if (input.Replace("HKEY_CURRENT_USER", "").Length > 0)
                        {
                            skNames = Registry.CurrentUser.OpenSubKey(input.Replace(@"HKEY_CURRENT_USER\", ""), true).GetSubKeyNames();
                        }
                        else
                        {
                            skNames = Registry.CurrentUser.GetSubKeyNames();
                        }
                    }
                    else if (input.StartsWith("HKEY_USERS"))
                    {
                        if (input.Replace("HKEY_USERS", "").Length > 0)
                        {
                            skNames = Registry.Users.OpenSubKey(input.Replace(@"HKEY_USERS\", ""), true).GetSubKeyNames();
                        }
                        else
                        {
                            skNames = Registry.Users.GetSubKeyNames();
                        }
                    }
                    else
                    {
                        skNames = CurrentHiveKey.OpenSubKey(input, true).GetSubKeyNames();
                    }
                }
                else
                {
                    if (CurrentSubkey == CurrentHiveKey.Name + "\\")
                    {
                        skNames = CurrentHiveKey.GetSubKeyNames();
                    }
                    else
                    {
                        skNames = CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).GetSubKeyNames();
                    }
                }

                if (skNames.Length > 0)
                {
                    for (int i = 0; i < skNames.Length; i++)
                    {
                        form.inputField.AppendText(Environment.NewLine + "    " + skNames[i]);
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "The specified subkey contains no subkeys!");
                }

            }
            catch (Exception)

            {
                form.inputField.AppendText(Environment.NewLine + "Subkey not found!");
            }

        }

        private void SK_MAKE(string input, DiscOperatingSystem form)
        {
            try
            {
                if (input.StartsWith(CurrentSubkey))
                {
                    CurrentHiveKey.CreateSubKey(input.Remove(0, CurrentSubkey.Length));
                }
                else
                {
                    if (input.StartsWith("HKEY_LOCAL_MACHINE"))
                    {
                        Registry.LocalMachine.CreateSubKey(input.Replace("HKEY_LOCAL_MACHINE", ""), true);
                    }
                    else if (input.StartsWith("HKEY_CLASSES_ROOT"))
                    {
                        Registry.ClassesRoot.CreateSubKey(input.Replace("HKEY_CLASSES_ROOT", ""), true);
                    }
                    else if (input.StartsWith("HKEY_CURRENT_CONFIG"))
                    {
                        Registry.CurrentConfig.CreateSubKey(input.Replace("HKEY_CURRENT_CONFIG", ""), true);
                    }
                    else if (input.StartsWith("HKEY_CURRENT_USER"))
                    {
                        Registry.CurrentUser.CreateSubKey(input.Replace("HKEY_CURRENT_USER", ""), true);
                    }
                    else if (input.StartsWith("HKEY_USERS"))
                    {
                        Registry.Users.CreateSubKey(input.Replace("HKEY_USERS", ""), true);
                    }
                    else
                    {
                        CurrentHiveKey.CreateSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", "") + input);
                    }
                }
            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Invalid path!");
            }
        }

        private void SK_DELETE(string input, DiscOperatingSystem form)
        {
            try
            {
                if (input.StartsWith(CurrentSubkey))
                {
                    CurrentHiveKey.DeleteSubKeyTree(input.Remove(0, CurrentSubkey.Length));
                }
                else
                {
                    if (input.StartsWith("HKEY_LOCAL_MACHINE"))
                    {
                        Registry.LocalMachine.DeleteSubKeyTree(input.Replace("HKEY_LOCAL_MACHINE", ""), true);
                    }
                    else if (input.StartsWith("HKEY_CLASSES_ROOT"))
                    {
                        Registry.ClassesRoot.DeleteSubKeyTree(input.Replace("HKEY_CLASSES_ROOT", ""), true);
                    }
                    else if (input.StartsWith("HKEY_CURRENT_CONFIG"))
                    {
                        Registry.CurrentConfig.DeleteSubKeyTree(input.Replace("HKEY_CURRENT_CONFIG", ""), true);
                    }
                    else if (input.StartsWith("HKEY_CURRENT_USER"))
                    {
                        Registry.CurrentUser.DeleteSubKeyTree(input.Replace("HKEY_CURRENT_USER", ""), true);
                    }
                    else if (input.StartsWith("HKEY_USERS"))
                    {
                        Registry.Users.DeleteSubKeyTree(input.Replace("HKEY_USERS", ""), true);
                    }
                    else
                    {
                        CurrentHiveKey.DeleteSubKeyTree(input);
                    }
                }
            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Innvalid path!");
            }
        }

        private void VAL_GET(string input, DiscOperatingSystem form)
        {
            try
            {
                RegistryKey hiveKey = null;
                string[] valueNames = null;
                if (input != null)
                {
                    if (input.StartsWith("HKEY_LOCAL_MACHINE"))
                    {
                        if (input.Replace("HKEY_LOCAL_MACHINE", "").Length > 0)
                        {
                            valueNames = Registry.LocalMachine.OpenSubKey(input.Replace(@"HKEY_LOCAL_MACHINE\", ""), true).GetValueNames();
                            hiveKey = Registry.LocalMachine.OpenSubKey(input.Replace(@"HKEY_LOCAL_MACHINE\", ""), true);
                        }
                        else
                        {
                            valueNames = Registry.LocalMachine.GetValueNames();
                            hiveKey = Registry.LocalMachine;
                        }
                    }
                    else if (input.StartsWith("HKEY_CLASSES_ROOT"))
                    {
                        if (input.Replace("HKEY_CLASSES_ROOT", "").Length > 0)
                        {
                            valueNames = Registry.ClassesRoot.OpenSubKey(input.Replace(@"HKEY_CLASSES_ROOT\", ""), true).GetValueNames();
                            hiveKey = Registry.ClassesRoot.OpenSubKey(input.Replace(@"HKEY_CLASSES_ROOT\", ""), true);
                        }
                        else
                        {
                            valueNames = Registry.ClassesRoot.GetValueNames();
                            hiveKey = Registry.ClassesRoot;
                        }
                    }
                    else if (input.StartsWith("HKEY_CURRENT_CONFIG"))
                    {
                        if (input.Replace("HKEY_CURRENT_CONFIG", "").Length > 0)
                        {
                            valueNames = Registry.CurrentConfig.OpenSubKey(input.Replace(@"HKEY_CURRENT_CONFIG\", ""), true).GetValueNames();
                            hiveKey = Registry.CurrentConfig.OpenSubKey(input.Replace(@"HKEY_CURRENT_CONFIG\", ""), true);
                        }
                        else
                        {
                            valueNames = Registry.CurrentConfig.GetValueNames();
                            hiveKey = Registry.CurrentConfig;
                        }
                    }
                    else if (input.StartsWith("HKEY_CURRENT_USER"))
                    {
                        if (input.Replace("HKEY_CURRENT_USER", "").Length > 0)
                        {
                            valueNames = Registry.CurrentUser.OpenSubKey(input.Replace(@"HKEY_CURRENT_USER\", ""), true).GetValueNames();
                            hiveKey = Registry.CurrentUser.OpenSubKey(input.Replace(@"HKEY_CURRENT_USER\", ""), true);
                        }
                        else
                        {
                            valueNames = Registry.CurrentUser.GetValueNames();
                            hiveKey = Registry.CurrentUser;
                        }
                    }
                    else if (input.StartsWith("HKEY_USERS"))
                    {
                        if (input.Replace("HKEY_USERS", "").Length > 0)
                        {
                            valueNames = Registry.Users.OpenSubKey(input.Replace(@"HKEY_USERS\", ""), true).GetValueNames();
                            hiveKey = Registry.Users.OpenSubKey(input.Replace(@"HKEY_USERS\", ""), true);
                        }
                        else
                        {
                            valueNames = Registry.Users.GetValueNames();
                            hiveKey = Registry.Users;
                        }
                    }
                    else
                    {
                        valueNames = CurrentHiveKey.OpenSubKey(input, true).GetValueNames();
                        hiveKey = CurrentHiveKey.OpenSubKey(input, true);
                    }
                }
                else
                {
                    if (CurrentSubkey == CurrentHiveKey.Name + "\\")
                    {
                        valueNames = CurrentHiveKey.GetValueNames();
                        hiveKey = CurrentHiveKey;
                    }
                    else
                    {
                        valueNames = CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).GetValueNames();
                        hiveKey = CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true);
                    }
                }

                if (valueNames.Length > 0)
                {
                    for (int i = 0; i < valueNames.Length; i++)
                    {
                        form.inputField.AppendText(String.Format("{0}   |||   {1}   |||   {2}   |||   {3}", Environment.NewLine, valueNames[i], hiveKey.GetValueKind(valueNames[i]), hiveKey.GetValue(valueNames[i])));
                    }
                }
                else
                {
                    form.inputField.AppendText(Environment.NewLine + "The subkey does not contain values!");
                }

            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Subkey not found!");
            }
        }

        private void VAL_SET(string name, string value, DiscOperatingSystem form)
        {
            try
            {
                if (CurrentSubkey != CurrentHiveKey.Name + "\\")
                {
                    RegistryValueKind vk = CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).GetValueKind(name);
                    if (vk == RegistryValueKind.DWord || vk == RegistryValueKind.QWord || vk == RegistryValueKind.Binary)
                    {
                        int result;
                        if (int.TryParse(value, out result))
                        {
                            CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).SetValue(name, result);
                        }
                        else
                        {
                            form.inputField.AppendText(Environment.NewLine + "Cannot assign string value to DWORD, QWORD and BINARY registry values!");
                        }
                    }
                    else
                    {
                        CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).SetValue(name, value);
                    }
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Value not found!");
            }
        }

        private void VAL_DELETE(string input, DiscOperatingSystem form)
        {
            try
            {
                if (CurrentSubkey != CurrentHiveKey.Name + "\\")
                {
                    CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).DeleteValue(input);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Value not found!");
            }
        }

        private void VAL_MAKE(string name, RegistryValueKind valueKind, string value, DiscOperatingSystem form)
        {
            try
            {
                int result;
                if (valueKind == RegistryValueKind.Binary ||  valueKind == RegistryValueKind.QWord || valueKind == RegistryValueKind.DWord)
                {
                    if (int.TryParse(value, out result))
                    {
                        if (CurrentSubkey != CurrentHiveKey.Name + "\\")
                        {
                            CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).SetValue(name, result, valueKind);
                        }
                        else
                        {
                            CurrentHiveKey.SetValue(name, value, valueKind);
                        }
                    }
                    else
                    {
                        form.inputField.AppendText(Environment.NewLine + "Cannot assign string value to DWORD, QWORD and BINARY registry values!");
                    }
                }
                else
                {
                    CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).SetValue(name, value, valueKind);
                }
            }
            catch (Exception ex)
            {
                form.inputField.AppendText(Environment.NewLine + ex.Message/*"Invalid arguments!"*/);
            }
        }

        private void VAL_RENAME(string oldName, string newName, DiscOperatingSystem form)
        {
            try
            {
                if (CurrentSubkey != CurrentHiveKey.Name + "\\")
                {
                    CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).SetValue(newName, CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).GetValue(oldName), CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).GetValueKind(oldName));
                    CurrentHiveKey.OpenSubKey(CurrentSubkey.Replace(CurrentHiveKey.Name + "\\", ""), true).DeleteValue(oldName);
                }
                else
                {
                    throw new Exception();
                }
            }
            catch (Exception)
            {
                form.inputField.AppendText(Environment.NewLine + "Value not found!");
            }
        }
        #endregion
    }
}
