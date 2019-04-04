using CLIShell;
using CLIShell.Interpreter;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ToolShed.Networking;
using ToolShed.ThisPC;
using static System.Environment;
using static WinDOS.Bootloader;

namespace WinDOS
{
    public partial class MainForm : Form
    {
        public MainForm()
        {
            InitializeComponent();
            CenterToScreen();
            LoadCP437();
            SYS_FONT = CP437;
            IOField.KeyDown += IOField_KeyDown;
            IOField.PreviewKeyDown += IOField_PreviewKeyDown;
            IOField.MouseDoubleClick += IOField_MouseInput;
            IOField.MouseClick += IOField_MouseInput;
            IOField.MouseEnter += IOField_MouseEnter;
            IOField.MouseLeave += IOField_MouseLeave;
            FormClosing += MainForm_FormClosing;
            SizeChanged += MainForm_SizeChanged;
            Shown += MainForm_Shown;
            CT = CT_SOURCE.Token;
            CMD_ADD();
        }

        #region SYSTEM LAYOUT FIELDS
        [DllImport("user32.dll")]
        public static extern bool SetSysColors(int cElements, int[] lpaElements, uint[] lpaRgbValues);

        public async Task ChangeSelectColour(Color highlightColor, Color textColor)
        {
            await Task.Delay(0);
            const int COLOR_HIGHLIGHT = 13;
            const int COLOR_HIGHLIGHTTEXT = 14;

            int[] elements = { COLOR_HIGHLIGHT, COLOR_HIGHLIGHTTEXT };


            List<uint> colours = new List<uint>();
            colours.Add((uint)ColorTranslator.ToWin32(highlightColor));
            colours.Add((uint)ColorTranslator.ToWin32(textColor));
            SetSysColors(elements.Length, elements, colours.ToArray());
        }

        public static string CURRENT_DIR;

        public Color FORE_COLOR;

        public Color BACK_COLOR;

        public Font SYS_FONT;

        public bool FSCR;

        public bool TXT_EFFECT;

        public int TXT_INTERVAL;

        public int DEF_HIST_SIZE;

        public int REG_HIST_SIZE;

        public static bool EMU;

        public string CURRENT_SKEY;

        public Rectangle DEF_BOUNDS;
        #endregion

        #region STARTUP FIELDS
        public BootImage STRT_IMG;

        public BootImageProperties STRT_PROPS;
        #endregion

        #region SYSTEM COMPONENTS
        private enum HashType { SHA1, SHA256, SHA384, SHA512 };

        public class CommandHistory
        {
            public int DefaultIndex { get; set; }

            public int RegeditIndex { get; set; }

            public string[] DefaultHistory { get; private set; }

            public string[] RegeditHistory { get; private set; }

            public CommandHistory(int defaultsize, int regeditsize)
            {
                DefaultIndex = 0;
                DefaultHistory = new string[defaultsize];
                RegeditIndex = 0;
                RegeditHistory = new string[regeditsize];
                for (int i = 0; i < DefaultHistory.Length; i++)
                {
                    DefaultHistory[i] = "";
                }
                for (int i = 0; i < RegeditHistory.Length; i++)
                {
                    RegeditHistory[i] = "";
                }
            }

            public void AddToDefaultHistory(string strToAdd)
            {
                if (strToAdd != string.Empty)
                {
                    for (int i = DefaultHistory.Length - 1; i > 1; i--)
                    {
                        DefaultHistory[i] = DefaultHistory[i - 1];
                    }
                    DefaultHistory[1] = strToAdd;
                }
            }

            public void AddToRegeditHistory(string strToAdd)
            {
                if (strToAdd != string.Empty)
                {
                    for (int i = RegeditHistory.Length - 1; i > 1; i--)
                    {
                        RegeditHistory[i] = RegeditHistory[i - 1];
                    }
                    RegeditHistory[1] = strToAdd;
                }
            }

            public void ResizeDefaultHistory(int newSize)
            {
                List<string> hList = new List<string>();
                if (newSize < DefaultHistory.Length)
                {
                    for (int i = 0; i < newSize; i++)
                    {
                        hList.Add(DefaultHistory[i]);
                    }
                    DefaultHistory = hList.ToArray();
                }
                else if (newSize > DefaultHistory.Length)
                {
                    hList = DefaultHistory.ToList();
                    DefaultHistory = new string[newSize];
                    for (int i = 0; i < DefaultHistory.Length; i++)
                    {
                        if (i < hList.Count())
                        {
                            DefaultHistory[i] = hList[i];
                        }
                        else
                        {
                            DefaultHistory[i] = "";
                        }
                    }
                }
            }

            public void ResizeRegeditHistory(int newSize)
            {
                List<string> hList = new List<string>();
                if (newSize < RegeditHistory.Length)
                {
                    for (int i = 0; i < newSize; i++)
                    {
                        hList.Add(RegeditHistory[i]);
                    }
                    RegeditHistory = hList.ToArray();
                }
                else if (newSize > RegeditHistory.Length)
                {
                    hList = RegeditHistory.ToList();
                    RegeditHistory = new string[newSize];
                    for (int i = 0; i < RegeditHistory.Length; i++)
                    {
                        if (i < hList.Count())
                        {
                            RegeditHistory[i] = hList[i];
                        }
                        else
                        {
                            RegeditHistory[i] = "";
                        }
                    }
                }
            }

            public void SetDefaultZeroEntry(string entryStr)
            {
                DefaultHistory[0] = entryStr;
            }

            public void SetRegeditZeroEntry(string entryStr)
            {
                RegeditHistory[0] = entryStr;
            }

            public string ReturnDefaultEntryAtIndex()
            {
                if (DefaultIndex < DefaultHistory.Length && DefaultIndex >= 0)
                {
                    return DefaultHistory[DefaultIndex];
                }
                else
                {
                    return "";
                }
            }

            public string ReturnRegeditEntryAtIndex()
            {
                if (RegeditIndex < RegeditHistory.Length && RegeditIndex >= 0)
                {
                    return RegeditHistory[RegeditIndex];
                }
                else
                {
                    return "";
                }
            }

            public void DefaultScrollUp()
            {
                if (DefaultHistory.Length - 2 >= DefaultIndex)
                {
                    if (DefaultHistory[DefaultIndex + 1].Length > 0)
                    {
                        DefaultIndex++;
                    }
                }
            }

            public void DefaultScrollDown()
            {
                if (DefaultIndex > 0)
                {
                    if (DefaultHistory[DefaultIndex - 1].Length > 0)
                    {
                        DefaultIndex--;
                    }
                    else if (DefaultIndex - 1 == 0)
                    {
                        DefaultIndex--;
                    }
                }
            }

            public void RegeditScrollUp()
            {
                if (RegeditHistory.Length - 2 >= RegeditIndex)
                {
                    if (RegeditHistory[RegeditIndex + 1].Length > 0)
                    {
                        RegeditIndex++;
                    }
                }
            }

            public void RegeditScrollDown()
            {
                if (RegeditIndex > 0)
                {
                    if (RegeditHistory[RegeditIndex - 1].Length > 0)
                    {
                        RegeditIndex--;
                    }
                    else if (RegeditIndex - 1 == 0)
                    {
                        RegeditIndex--;
                    }
                }
            }
        }

        public struct AutoComplete
        {
            public string[] DefaultPool { get; }

            public string[] StartupPool { get; }

            public string[] DriverPool { get; }

            public string[] HistoryPool { get; }

            public string[] SysfontPool { get; }

            public string[] ValPool { get; }

            public string[] SkPool { get; }

            public string[] MiscPool { get; }

            public bool DummyParam { get; }

            public AutoComplete(bool dummy)
            {
                DummyParam = dummy;
                string[] defP = { CD.Call, DIR.Call, MD.Call, RD.Call, MF.Call, FC.Call, OPEN.Call, COPY.Call, MOVE.Call, DEL.Call, FSEINF.Call, RENAME.Call, FORECOLOR.Call, BACKCOLOR.Call, SETFONT.Call, FULLSCREEN.Call, EMULATION.Call, TEXTEFFECT.Call, REGEDIT.Call, WORDGEN.Call, HASHCRACK.Call, MH.Call, BIOSINFO.Call,
                                          GETNICS.Call, NMAP.Call, SETMAC.Call, RESETMAC.Call, PING.Call, ARP.Call, COPY_CURRENT_CONFIG.Call, ERASE_STARTUP_CONFIG.Call, EXEC.Call, START.Call, RESTART.Call, CLS.Call, HELP.Call, EXIT.Call};
                string[] sp = { STARTUP_CONFIG.Call, STARTUP_DIR.Call, STARTUP_MOTD.Call, STARTUP_SHMOTD.Call, STARTUP_SUBKEY.Call, STARTUP_VERBOSE.Call };
                string[] dp = { DRIVER_CHECK.Call, DRIVER_INFO.Call, DRIVER_NAMES.Call };
                string[] hp = { HISTORY_SETSIZE.Call, HISTORY_GETENTRY.Call, HISTORY_CLEAR.Call, HISTORY_SHLIST.Call };
                string[] sysp = { SYSFONT_FAMILY.Call, SYSFONT_SIZE.Call, SYSFONT_STYLE.Call };
                string[] vp = { VAL_DELETE.Call, VAL_GET.Call, VAL_MAKE.Call, VAL_RENAME.Call, VAL_SET.Call };
                string[] skp = { SK_DELETE.Call, SK_GET.Call, SK_MAKE.Call };
                string[] mp = { CLS.Call, HELP.Call, EXIT.Call };
                DefaultPool = defP;
                StartupPool = sp;
                DriverPool = dp;
                HistoryPool = hp;
                SysfontPool = sysp;
                ValPool = vp;
                SkPool = skp;
                MiscPool = mp;
            }

            private void ReplaceCommand(string commandToReplace, string newCommand, TextBox inputField)
            {
                string lastLine = inputField.Lines.Last();
                if (inputField.Lines.Length > 1)
                {
                    inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                    inputField.AppendText(NewLine + lastLine.Remove(lastLine.Length - commandToReplace.Length) + newCommand);
                }
                else
                {
                    inputField.Lines = inputField.Lines.Take(inputField.Lines.Length - 1).ToArray();
                    inputField.Clear();
                    inputField.AppendText(lastLine.Remove(lastLine.Length - commandToReplace.Length) + newCommand);
                }
            }

            private void ReplaceCommandFromPool(string command, string[] commandArray, TextBox inputField)
            {
                for (int i = 0; i < commandArray.Length; i++)
                {
                    if (commandArray[i].StartsWith(command.ToLower()))
                    {
                        ReplaceCommand(command, commandArray[i], inputField);
                        break;
                    }
                }
            }

            public void CompleteCommand(string input, bool regedit, TextBox inputField)
            {
                if (regedit)
                {
                    if ("val-".StartsWith(input))
                    {
                        ReplaceCommand(input, "val-", inputField);
                    }
                    else if ("sk-".StartsWith(input))
                    {
                        ReplaceCommand(input, "sk-", inputField);
                    }
                    else if ("history-".StartsWith(input))
                    {
                        ReplaceCommand(input, "history-", inputField);
                    }
                    else if (input.StartsWith("val-"))
                    {
                        ReplaceCommandFromPool(input, ValPool, inputField);
                    }
                    else if (input.StartsWith("sk-"))
                    {
                        ReplaceCommandFromPool(input, SkPool, inputField);
                    }
                    else if (input.StartsWith("history-"))
                    {
                        ReplaceCommandFromPool(input, HistoryPool, inputField);
                    }
                    else if ("csk".StartsWith(input))
                    {
                        ReplaceCommand(input, "csk", inputField);
                    }
                    else
                    {
                        ReplaceCommandFromPool(input, MiscPool, inputField);
                    }
                }
                else
                {
                    if ("startup-".StartsWith(input))
                    {
                        ReplaceCommand(input, "startup-", inputField);
                    }
                    else if ("driver-".StartsWith(input))
                    {
                        ReplaceCommand(input, "driver-", inputField);
                    }
                    else if ("history-".StartsWith(input))
                    {
                        ReplaceCommand(input, "history-", inputField);
                    }
                    else if ("sysfont-".StartsWith(input))
                    {
                        ReplaceCommand(input, "sysfont-", inputField);
                    }
                    else if (input.StartsWith("startup-"))
                    {
                        ReplaceCommandFromPool(input, StartupPool, inputField);
                    }
                    else if (input.StartsWith("driver-"))
                    {
                        ReplaceCommandFromPool(input, DriverPool, inputField);
                    }
                    else if (input.StartsWith("history-"))
                    {
                        ReplaceCommandFromPool(input, HistoryPool, inputField);
                    }
                    else if (input.StartsWith("sysfont-"))
                    {
                        ReplaceCommandFromPool(input, SysfontPool, inputField);
                    }
                    else
                    {
                        ReplaceCommandFromPool(input, DefaultPool, inputField);
                    }
                }
            }
        }

        public struct ThreadProgress
        {
            public double ObjectsProcessed;

            public double ObjectsRemaining;

            public double ObjectCount;

            public TimeSpan TimeLeft;

            public TimeSpan TimeElapsed;

            public DateTime StartTime;

            public double Percentage;
        }

        public Bootloader BOOTLOADER;

        private CommandHistory HIST;

        private AutoComplete COMP = new AutoComplete(false);

        private static TextEngine TEXT_ENGINE;

        public CommandManagement CMD_MGMT = new CommandManagement(DEF_CMD_INIT);

        public static CommandPool DEF_CMD_INIT = new CommandPool();

        public static CommandPool REG_CMD_INIT = new CommandPool();

        public MultilineOutputBuffer MULTI_OUT_BUFFER;

        public SingleLineOutputBuffer SINGLE_OUT_BUFFER;

        public void CMD_ADD()
        {
            List<Command> cmdList = new List<Command>();
            /*DEFAULT COMMANDS*/
            DEF_CMD_INIT.Add(CD, async () => {
                try
                {
                    WRITABLE = false;
                    STR_BUILDER = new StringBuilder(CURRENT_DIR);
                    string arg = CD.Args[0].InterpreterParameters.InterpretedString;
                    switch (arg)
                    {
                        case "stdir":
                            STR_BUILDER.Replace(CURRENT_DIR, STRT_PROPS.StartupDirectory);
                            break;
                        case "wdir":
                            STR_BUILDER.Replace(CURRENT_DIR, CurrentDirectory);
                            break;
                        case "..":
                            if (CURRENT_DIR.Length != CURRENT_DIR.Replace("\\", "").Length + 1)
                            {
                                STR_BUILDER = STR_BUILDER.Remove(CURRENT_DIR.Length - 1, 1);
                                STR_BUILDER = STR_BUILDER.Replace(STR_BUILDER.ToString(), STR_BUILDER.ToString().Remove(FindLastCharacterIndex(STR_BUILDER.ToString(), '\\')));
                            }
                            break;
                        default:
                            if (Directory.Exists(arg))
                            {
                                STR_BUILDER.Replace(CURRENT_DIR, arg);
                            }
                            else if (Directory.Exists(CURRENT_DIR + arg))
                            {
                                STR_BUILDER.Append(arg);
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Directory not found!");
                            }
                            break;
                    }
                    if (STR_BUILDER.ToString().Last() != '\\')
                    {
                        STR_BUILDER.Append("\\");
                    }
                    CURRENT_DIR = STR_BUILDER.ToString();
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(DIR, async () => {
                try
                {
                    WRITABLE = false;
                    List<string> bufferList = new List<string>();
                    List<string> dirList = new List<string>();
                    InterpreterResult res = null;
                    switch (DIR.Args.Length)
                    {
                        case 0:
                            dirList = Directory.GetFileSystemEntries(CURRENT_DIR).ToList();
                            for (int i = 0; i < dirList.Count(); i++)
                            {
                                bufferList.Add(NewLine + "     " + dirList[i]);
                            }
                            break;
                        case 1:
                            if (DIR.Args[0].ContainsGlobs)
                            {
                                dirList = Directory.GetFileSystemEntries(CURRENT_DIR).ToList();
                                for (int i = 0; i < dirList.Count(); i++)
                                {
                                    res = new InterpreterResult(dirList[i]);
                                    res.GetResult(DIR.Args[0].InterpreterParameters);
                                    if (res.Result)
                                    {
                                        bufferList.Add(NewLine + "     " + dirList[i]);
                                    }
                                }
                            }
                            else
                            {
                                switch (DIR.Args[0].InterpreterParameters.InterpretedString)
                                {
                                    case "stdir":
                                        dirList = Directory.GetFileSystemEntries(STRT_PROPS.StartupDirectory).ToList();
                                        break;
                                    case "wdir":
                                        dirList = Directory.GetFileSystemEntries(Environment.CurrentDirectory).ToList();
                                        break;
                                    default:
                                        if (Directory.Exists(DIR.Args[0].InterpreterParameters.InterpretedString))
                                        {
                                            dirList = Directory.GetFileSystemEntries(DIR.Args[0].InterpreterParameters.InterpretedString).ToList();
                                        }
                                        else if (Directory.Exists(CURRENT_DIR + DIR.Args[0].InterpreterParameters.InterpretedString))
                                        {
                                            dirList = Directory.GetFileSystemEntries(CURRENT_DIR + DIR.Args[0].InterpreterParameters.InterpretedString).ToList();
                                        }
                                        else
                                        {
                                            await TEXT_ENGINE.Write(NewLine + "Directory not found!");
                                            ERROR_OCCURRED = true;
                                        }
                                        break;
                                }
                                if (!ERROR_OCCURRED)
                                {
                                    for (int i = 0; i < dirList.Count(); i++)
                                    {
                                        bufferList.Add(NewLine + "     " + dirList[i]);
                                    }
                                }
                            }
                            break;
                        case 2:
                            switch (DIR.Args[0].InterpreterParameters.InterpretedString)
                            {
                                case "stdir":
                                    dirList = Directory.GetFileSystemEntries(STRT_IMG.StartupDirectory).ToList();
                                    break;
                                case "wdir":
                                    dirList = Directory.GetFileSystemEntries(Environment.CurrentDirectory).ToList();
                                    break;
                                default:
                                    if (Directory.Exists(DIR.Args[0].InterpreterParameters.InterpretedString))
                                    {
                                        dirList = Directory.GetFileSystemEntries(DIR.Args[0].InterpreterParameters.InterpretedString).ToList();
                                    }
                                    else if (Directory.Exists(CURRENT_DIR + DIR.Args[0].InterpreterParameters.InterpretedString))
                                    {
                                        dirList = Directory.GetFileSystemEntries(CURRENT_DIR + DIR.Args[0].InterpreterParameters.InterpretedString).ToList();
                                    }
                                    else
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "Directory not found!");
                                        ERROR_OCCURRED = true;
                                    }
                                    break;
                            }
                            if (!ERROR_OCCURRED)
                            {
                                for (int i = 0; i < dirList.Count(); i++)
                                {
                                    res = new InterpreterResult(dirList[i]);
                                    res.GetResult(DIR.Args[1].InterpreterParameters);
                                    if (res.Result)
                                    {
                                        bufferList.Add(NewLine + "     " + dirList[i]);
                                    }
                                }
                            }
                            break;
                    }
                    await TEXT_ENGINE.WriteArray(bufferList.ToArray());
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    ERROR_OCCURRED = false;
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(MD, async () => {
                try
                {
                    WRITABLE = false;
                    string path = MD.Args[0].InterpreterParameters.InterpretedString;
                    if (!Directory.Exists(path) && !Directory.Exists(CURRENT_DIR + path) && !File.Exists(path) && !File.Exists(CURRENT_DIR + path))
                    {
                        Directory.CreateDirectory(path);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "A file and/or directory with the same name already exists!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(RD, async () => {
                try
                {
                    WRITABLE = false;
                    string path = RD.Args[0].InterpreterParameters.InterpretedString;
                    if (path == CURRENT_DIR || path.Remove(path.Length - 1) == CURRENT_DIR)
                    {
                        await TEXT_ENGINE.Write(NewLine + "The current directory cannot be removed!");
                        ERROR_OCCURRED = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        if (Directory.Exists(path))
                        {
                            Directory.Delete(path);
                        }
                        else if (Directory.Exists(CURRENT_DIR + path))
                        {
                            Directory.Delete(CURRENT_DIR + path);
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Directory not found!");
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(MF, async () => {
                try
                {
                    WRITABLE = false;
                    string path = MF.Args[0].InterpreterParameters.InterpretedString;
                    if (path == CURRENT_DIR)
                    {
                        await TEXT_ENGINE.Write(NewLine + "The current directory cannot be moved!");
                        ERROR_OCCURRED = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        bool contentToWrite = false;
                        if (!File.Exists(path))
                        {
                            if (path.Last() == '\\')
                            {
                                path.Remove(path.Length - 1);
                            }

                            if (Directory.Exists(path.Remove(FindLastCharacterIndex(path, '\\'))))
                            {
                                if (MF.Args.Length == 2)
                                {
                                    contentToWrite = true;
                                }
                            }
                            else if (Directory.Exists(CURRENT_DIR + path.Remove(FindLastCharacterIndex(path, '\\'))))
                            {
                                path = CURRENT_DIR + path;
                                if (MF.Args.Length == 2)
                                {
                                    contentToWrite = true;
                                }
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Destination directory not found!");
                                ERROR_OCCURRED = true;
                            }
                        }
                        if (!ERROR_OCCURRED)
                        {
                            if (contentToWrite)
                            {
                                StreamWriter sw = new StreamWriter(path);
                                sw.Write(MF.Args[1].Arg);
                            }
                            else
                            {
                                File.Create(path).Close();
                            }
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(FC, async () => {
                try
                {
                    WRITABLE = false;
                    string path = FC.Args[0].InterpreterParameters.InterpretedString;
                    string text = null;
                    if (File.Exists(path))
                    {
                        text = File.ReadAllText(path);
                    }
                    else if (File.Exists(CURRENT_DIR + path))
                    {
                        text = File.ReadAllText(CURRENT_DIR + path);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "File not found!");
                        ERROR_OCCURRED = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        if (text.Length <= 800000)
                        {
                            IOField.AppendText(NewLine + text);
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Text was too long!");
                            ERROR_OCCURRED = true;
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(OPEN, async () => {
                try
                {
                    WRITABLE = false;
                    string path = OPEN.Args[0].InterpreterParameters.InterpretedString;
                    string text = null;
                    if (File.Exists(path))
                    {
                        text = File.ReadAllText(path);
                    }
                    else if (File.Exists(CURRENT_DIR + path))
                    {
                        text = File.ReadAllText(CURRENT_DIR + path);
                        path = CURRENT_DIR + path;
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "File not found!");
                        ERROR_OCCURRED = true;
                    }

                    if (!ERROR_OCCURRED)
                    {
                        if (text.Length <= 800000)
                        {
                            IOField.Clear();
                            FILE_OPEN = true;
                            OPEN_PATH = path;
                            IOField.AppendText(text);
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Text was too long!");
                            ERROR_OCCURRED = true;
                        }
                    }
                    else
                    {
                        ERROR_OCCURRED = false;
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(COPY, async () => {
                try
                {
                    WRITABLE = false;
                    string path = COPY.Args[0].InterpreterParameters.InterpretedString;
                    bool entryExists = false;
                    if (Directory.Exists(path) || File.Exists(path))
                    {
                        entryExists = true;
                    }
                    else if (Directory.Exists(CURRENT_DIR + path) || File.Exists(CURRENT_DIR + path))
                    {
                        entryExists = true;
                        path = CURRENT_DIR + path;
                    }
                    if (entryExists)
                    {
                        FileAttributes attrib = File.GetAttributes(path);
                        string dst = COPY.Args[1].InterpreterParameters.InterpretedString;
                        if (dst == CURRENT_DIR || dst.Remove(dst.Length - 1) == CURRENT_DIR)
                        {
                            await TEXT_ENGINE.Write(NewLine + "The current directory cannot be copied to!");
                            ERROR_OCCURRED = true;
                        }
                        if (!ERROR_OCCURRED)
                        {
                            bool overwrite = false;
                            if (COPY.Args.Length == 3)
                            {
                                switch (COPY.Args[2].Arg)
                                {
                                    case "-o":
                                        overwrite = true;
                                        break;
                                    default:
                                        await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(3): " + COPY.Args[2].Arg);
                                        ERROR_OCCURRED = true;
                                        break;
                                }
                            }
                            if (!ERROR_OCCURRED)
                            {
                                if (dst.Last() == '\\')
                                {
                                    dst = dst.Remove(dst.Length - 1);
                                }
                                Process p = null;
                                if (Directory.Exists(dst.Remove(FindLastCharacterIndex(dst, '\\'))))
                                {
                                    if (attrib.HasFlag(FileAttributes.Directory))
                                    {
                                        p = new Process();
                                        p.StartInfo.CreateNoWindow = true;
                                        p.StartInfo.UseShellExecute = false;
                                        p.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "xcopy.exe");
                                        p.StartInfo.Arguments = path + " " + dst + " /E /I";
                                        if (overwrite)
                                        {
                                            p.StartInfo.Arguments += " /Y";
                                        }
                                        p.Start();
                                        p.WaitForExit();
                                    }
                                    else
                                    {
                                        try
                                        {
                                            File.Copy(path, dst, overwrite);
                                        }
                                        catch (Exception ex)
                                        {
                                            await TEXT_ENGINE.Write(NewLine + ex.Message);
                                        }
                                    }
                                }
                                else if (Directory.Exists(CURRENT_DIR + dst.Remove(FindLastCharacterIndex(dst, '\\'))))
                                {
                                    if (attrib.HasFlag(FileAttributes.Directory))
                                    {
                                        p = new Process();
                                        p.StartInfo.CreateNoWindow = true;
                                        p.StartInfo.UseShellExecute = false;
                                        p.StartInfo.FileName = Path.Combine(Environment.SystemDirectory, "xcopy.exe");
                                        p.StartInfo.Arguments = path + " " + CURRENT_DIR + dst + " /E /I";
                                        if (overwrite)
                                        {
                                            p.StartInfo.Arguments += " /Y";
                                        }
                                        p.Start();
                                        p.WaitForExit();
                                    }
                                    else
                                    {
                                        try
                                        {
                                            File.Copy(path, CURRENT_DIR + dst, overwrite);
                                        }
                                        catch (Exception ex)
                                        {
                                            await TEXT_ENGINE.Write(NewLine + ex.Message);
                                        }
                                    }
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write(NewLine + "Destination directory not found!");
                                    ERROR_OCCURRED = true;
                                }
                            }
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "File system entry not found!");
                            ERROR_OCCURRED = true;
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(MOVE, async () => {
                try
                {
                    WRITABLE = false;
                    string path = MOVE.Args[0].InterpreterParameters.InterpretedString;
                    bool entryExists = false;
                    if (Directory.Exists(path) || File.Exists(path))
                    {
                        entryExists = true;
                    }
                    else if (Directory.Exists(CURRENT_DIR + path) || File.Exists(CURRENT_DIR + path))
                    {
                        entryExists = true;
                        path = CURRENT_DIR + path;
                    }
                    if (path == CURRENT_DIR || path.Remove(path.Length - 1) == CURRENT_DIR)
                    {
                        await TEXT_ENGINE.Write(NewLine + "The current directory cannot be copied to!");
                        ERROR_OCCURRED = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        if (entryExists)
                        {
                            FileAttributes attrib = File.GetAttributes(path);
                            string dst = MOVE.Args[1].InterpreterParameters.InterpretedString;
                            if (dst.Last() == '\\')
                            {
                                dst = dst.Remove(dst.Length - 1);
                            }
                            if (Directory.Exists(dst.Remove(FindLastCharacterIndex(dst, '\\'))))
                            {
                                if (attrib.HasFlag(FileAttributes.Directory))
                                {
                                    Directory.Move(path, dst);
                                }
                                else
                                {
                                    File.Move(path, dst);
                                }

                            }
                            else if (Directory.Exists(CURRENT_DIR + dst.Remove(FindLastCharacterIndex(dst, '\\'))))
                            {
                                if (attrib.HasFlag(FileAttributes.Directory))
                                {
                                    Directory.Move(path, CURRENT_DIR + dst);
                                }
                                else
                                {
                                    File.Move(path, CURRENT_DIR + dst);
                                }
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Destination directory not found!");
                                ERROR_OCCURRED = true;
                            }
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "File system entry not found!");
                            ERROR_OCCURRED = true;
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(DEL, async () => {
                try
                {
                    WRITABLE = false;
                    string path = DEL.Args[0].InterpreterParameters.InterpretedString;
                    if (File.Exists(path))
                    {
                        File.Delete(path);
                    }
                    else if (File.Exists(CURRENT_DIR + path))
                    {
                        File.Delete(CURRENT_DIR + path);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "File not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(FSEINF, async () => {
                try
                {
                    WRITABLE = false;
                    FileInfo file_inf = null;
                    DirectoryInfo dir_inf = null;
                    bool isFile = false;
                    List<string> bufferList = new List<string>();
                    if (File.Exists(FSEINF.Args[0].InterpreterParameters.InterpretedString))
                    {
                        file_inf = new FileInfo(FSEINF.Args[0].InterpreterParameters.InterpretedString);
                        isFile = true;
                    }
                    else if (File.Exists(CURRENT_DIR + FSEINF.Args[0].InterpreterParameters.InterpretedString))
                    {
                        file_inf = new FileInfo(CURRENT_DIR + FSEINF.Args[0].InterpreterParameters.InterpretedString);
                        isFile = true;
                    }
                    else if (Directory.Exists(FSEINF.Args[0].InterpreterParameters.InterpretedString))
                    {
                        dir_inf = new DirectoryInfo(FSEINF.Args[0].InterpreterParameters.InterpretedString);
                    }
                    else if (Directory.Exists(CURRENT_DIR + FSEINF.Args[0].InterpreterParameters.InterpretedString))
                    {
                        dir_inf = new DirectoryInfo(CURRENT_DIR + FSEINF.Args[0].InterpreterParameters.InterpretedString);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "The specified file system entry could not be found!");
                        ERROR_OCCURRED = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        if (isFile)
                        {
                            bufferList.Add(NewLine);
                            bufferList.Add(NewLine + "Name: " + file_inf.Name);
                            bufferList.Add(NewLine + "Date created: " + file_inf.CreationTime);
                            bufferList.Add(NewLine + "Date modified: " + file_inf.LastWriteTime);
                            bufferList.Add(NewLine + "Parent directory: " + file_inf.DirectoryName);
                            bufferList.Add(NewLine + "Attributes: " + file_inf.Attributes);
                            bufferList.Add(NewLine + "Extension: " + file_inf.Extension);
                            bufferList.Add(NewLine + "Read only: " + file_inf.IsReadOnly);
                            double size = file_inf.Length;
                            string sizeMarker = " Bytes";
                            if (size / 1024 >= 1)
                            {
                                sizeMarker = " KBytes";
                                size /= 1024;
                            }
                            if (size / 1024 >= 1)
                            {
                                sizeMarker = " MBytes";
                                size /= 1024;
                            }
                            if (size / 1024 >= 1)
                            {
                                sizeMarker = "GBytes";
                                size /= 1024;
                            }
                            if (size / 1024 >= 1)
                            {
                                sizeMarker = "TBytes";
                                size /= 1024;
                            }
                            bufferList.Add(NewLine + string.Format("Size: {0:0.000}", size) + sizeMarker);
                        }
                        else
                        {
                            bufferList.Add(NewLine);
                            bufferList.Add(NewLine + "Name: " + dir_inf.Name);
                            bufferList.Add(NewLine + "Date created: " + dir_inf.CreationTime);
                            bufferList.Add(NewLine + "Date modified: " + dir_inf.LastWriteTime);
                            bufferList.Add(NewLine + "Parent directory: " + dir_inf.Parent);
                            bufferList.Add(NewLine + "Attributes: " + dir_inf.Attributes);
                            bufferList.Add(NewLine + "Root: " + dir_inf.Root);
                        }
                        await TEXT_ENGINE.WriteArray(bufferList.ToArray());
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(RENAME, async () => {
                try
                {
                    WRITABLE = false;
                    string oldName = RENAME.Args[0].InterpreterParameters.InterpretedString;
                    string newName = RENAME.Args[1].InterpreterParameters.InterpretedString;
                    if (File.Exists(oldName))
                    {
                        File.Move(oldName, oldName.Remove(FindLastCharacterIndex(oldName, '\\') + 1) + newName);
                    }
                    else if (File.Exists(CURRENT_DIR + oldName))
                    {
                        File.Move(CURRENT_DIR + oldName, (CURRENT_DIR + oldName).Remove(FindLastCharacterIndex(oldName, '\\') + 1) + newName);
                    }
                    else if (Directory.Exists(oldName))
                    {
                        Directory.Move(oldName, oldName.Remove(FindLastCharacterIndex(oldName, '\\') + 1) + newName);
                    }
                    else if (Directory.Exists(CURRENT_DIR + oldName))
                    {
                        Directory.Move(CURRENT_DIR + oldName, (CURRENT_DIR + oldName).Remove(FindLastCharacterIndex(oldName, '\\') + 1) + newName);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "File or directory not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = false;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(FORECOLOR, async () => {
                try
                {
                    WRITABLE = false;
                    switch (FORECOLOR.Args.Length)
                    {
                        case 1:
                            Color newColor = Color.FromName(FORECOLOR.Args[0].InterpreterParameters.InterpretedString);
                            if (BACK_COLOR != newColor && newColor.ToKnownColor() != 0)
                            {
                                FORE_COLOR = newColor;
                                IOField.ForeColor = newColor;
                                await ChangeSelectColour(BACK_COLOR, FORE_COLOR);
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid color (maybe equals foreground color)!");
                            }
                            break;
                        case 2:
                            switch (FORECOLOR.Args[1].Arg)
                            {
                                case "-s":
                                    newColor = Color.FromName(FORECOLOR.Args[0].InterpreterParameters.InterpretedString);
                                    if (newColor.ToKnownColor() != 0)
                                    {
                                        FORE_COLOR = newColor;
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("ForegroundColor", FORE_COLOR.Name);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("Color", FORE_COLOR.Name + ">" + STRT_IMG.StartupBackgroundColor.Name, "BOOT.ini");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "Invalid color!");
                                        ERROR_OCCURRED = true;
                                    }
                                    if (!ERROR_OCCURRED)
                                    {
                                        STRT_PROPS.StartupForegroundColor = FORE_COLOR;
                                    }
                                    break;
                                case "-b":
                                    newColor = Color.FromName(FORECOLOR.Args[0].InterpreterParameters.InterpretedString);
                                    if (BACK_COLOR != newColor && newColor.ToKnownColor() != 0)
                                    {
                                        FORE_COLOR = newColor;
                                        IOField.ForeColor = newColor;
                                        await ChangeSelectColour(BACK_COLOR, FORE_COLOR);
                                    }
                                    else
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "Invalid color (maybe equals foreground color)!");
                                        ERROR_OCCURRED = true;
                                    }
                                    if (!ERROR_OCCURRED)
                                    {
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("ForegroundColor", FORE_COLOR.Name);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("Color", FORE_COLOR.Name + ">" + STRT_IMG.StartupBackgroundColor.Name, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.StartupForegroundColor = FORE_COLOR;
                                    }
                                    break;
                                default:
                                    await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + FORECOLOR.Args[1].Arg);
                                    break;
                            }
                            break;
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(BACKCOLOR, async () => {
                try
                {
                    WRITABLE = false;
                    switch (BACKCOLOR.Args.Length)
                    {
                        case 1:
                            Color newColor = Color.FromName(BACKCOLOR.Args[0].InterpreterParameters.InterpretedString);
                            if (FORE_COLOR != newColor && newColor.ToKnownColor() != 0)
                            {
                                BACK_COLOR = newColor;
                                IOField.BackColor = newColor;
                                await ChangeSelectColour(BACK_COLOR, FORE_COLOR);
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid color (maybe equals foreground color)!");
                            }
                            break;
                        case 2:
                            switch (BACKCOLOR.Args[1].Arg)
                            {
                                case "-s":
                                    newColor = Color.FromName(BACKCOLOR.Args[0].InterpreterParameters.InterpretedString);
                                    if (newColor.ToKnownColor() != 0)
                                    {
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("BackgroundColor", BACK_COLOR.Name);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("Color", STRT_IMG.StartupForegroundColor.Name + ">" + BACK_COLOR.Name, "BOOT.ini");
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "Invalid color!");
                                        ERROR_OCCURRED = true;
                                    }
                                    if (!ERROR_OCCURRED)
                                    {
                                        STRT_PROPS.StartupBackgroundColor = BACK_COLOR;
                                    }
                                    break;
                                case "-b":
                                    newColor = Color.FromName(BACKCOLOR.Args[0].InterpreterParameters.InterpretedString);
                                    if (FORE_COLOR != newColor && newColor.ToKnownColor() != 0)
                                    {
                                        BACK_COLOR = newColor;
                                        IOField.BackColor = newColor;
                                        await ChangeSelectColour(BACK_COLOR, FORE_COLOR);
                                    }
                                    else
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "Invalid color (maybe equals foreground color)!");
                                        ERROR_OCCURRED = true;
                                    }
                                    if (!ERROR_OCCURRED)
                                    {
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("BackgroundColor", BACK_COLOR.Name);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("Color", STRT_IMG.StartupForegroundColor.Name + ">" + BACK_COLOR.Name, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.StartupBackgroundColor = BACK_COLOR;
                                    }

                                    break;
                                default:
                                    await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + BACKCOLOR.Args[1].Arg);
                                    break;
                            }
                            break;
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(SETFONT, async () => {
                try
                {
                    WRITABLE = false;
                    bool startupOnly = false;
                    bool startupAndCurrent = false;
                    string familyStr = SETFONT.Args[0].InterpreterParameters.InterpretedString;
                    switch (SETFONT.Args.Last().Arg)
                    {
                        case "-s":
                            startupOnly = true;
                            break;
                        case "-b":
                            startupAndCurrent = true;
                            break;
                        default:
                            if (SETFONT.Args.Length == 4)
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(4): " + SETFONT.Args[3].Arg);
                                ERROR_OCCURRED = true;
                            }
                            break;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        if (startupOnly)
                        {
                            SetfontHelper(familyStr, false, true);
                        }
                        else if (startupAndCurrent)
                        {
                            SetfontHelper(familyStr, setStartup: true);
                        }
                        else
                        {
                            SetfontHelper(familyStr);
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(SYSFONT_FAMILY, async () => {
                try
                {
                    WRITABLE = false;
                    FontFamily newFamily = IOField.Font.FontFamily;
                    if (SYSFONT_FAMILY.Args[0].InterpreterParameters.InterpretedString == "CP437")
                    {
                        newFamily = CP437.FontFamily;
                    }
                    else
                    {
                        try
                        {
                            newFamily = new FontFamily(SYSFONT_FAMILY.Args[0].InterpreterParameters.InterpretedString);
                        }
                        catch (Exception)
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid font family!");
                            ERROR_OCCURRED = true;
                        }
                    }
                    if (!ERROR_OCCURRED)
                    {
                        switch (SYSFONT_FAMILY.Args.Length)
                        {
                            case 1:
                                SYS_FONT = new Font(newFamily, SYS_FONT.Size, SYS_FONT.Style);
                                IOField.Font = SYS_FONT;
                                break;
                            case 2:
                                switch (SYSFONT_FAMILY.Args[1].Arg)
                                {
                                    case "-s":
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("FontFamily", newFamily.Name);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("StartupSystemFont", newFamily.Name + ">" + STRT_IMG.SystemFontSize + ">" + STRT_IMG.SystemFontStyle, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.SystemFontFamily = newFamily;
                                        STRT_PROPS.StartupSystemFont = new Font(newFamily, SYS_FONT.Size, SYS_FONT.Style);
                                        break;
                                    case "-b":
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("FontFamily", newFamily.Name);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("StartupSystemFont", newFamily.Name + ">" + STRT_IMG.SystemFontSize + ">" + STRT_IMG.SystemFontStyle, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.SystemFontFamily = newFamily;
                                        SYS_FONT = new Font(newFamily, SYS_FONT.Size, SYS_FONT.Style);
                                        STRT_PROPS.StartupSystemFont = SYS_FONT;
                                        IOField.Font = SYS_FONT;
                                        break;
                                    default:
                                        await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + SYSFONT_FAMILY.Args[1].Arg);
                                        break;
                                }
                                break;
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(SYSFONT_SIZE, async () => {
                try
                {
                    WRITABLE = false;
                    int newSize;
                    if (int.TryParse(SYSFONT_SIZE.Args[0].Arg, out newSize) && newSize > 0)
                    {
                        switch (SYSFONT_SIZE.Args.Length)
                        {
                            case 1:
                                SYS_FONT = new Font(SYS_FONT.FontFamily, newSize, SYS_FONT.Style);
                                IOField.Font = SYS_FONT;
                                break;
                            case 2:
                                switch (SYSFONT_SIZE.Args[1].Arg)
                                {
                                    case "-s":
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("FontSize", newSize, RegistryValueKind.DWord);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("StartupSystemFont", STRT_IMG.SystemFontFamily + ">" + newSize + ">" + STRT_IMG.SystemFontStyle, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.SystemFontSize = newSize;
                                        STRT_PROPS.StartupSystemFont = new Font(SYS_FONT.FontFamily, newSize, SYS_FONT.Style);
                                        break;
                                    case "-b":
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("FontSize", newSize, RegistryValueKind.DWord);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("StartupSystemFont", STRT_IMG.SystemFontFamily + ">" + newSize + ">" + STRT_IMG.SystemFontStyle, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.SystemFontSize = newSize;
                                        SYS_FONT = new Font(SYS_FONT.FontFamily, newSize, SYS_FONT.Style);
                                        STRT_PROPS.StartupSystemFont = SYS_FONT;
                                        IOField.Font = SYS_FONT;
                                        break;
                                    default:
                                        await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + SYSFONT_SIZE.Args[1].Arg);
                                        break;
                                }
                                break;
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Invalid font size!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(SYSFONT_STYLE, async () => {
                try
                {
                    WRITABLE = false;
                    FontStyle newStyle = IOField.Font.Style;
                    switch (SYSFONT_STYLE.Args[0].Arg)
                    {
                        case "regular":
                            newStyle = FontStyle.Regular;
                            break;
                        case "bold":
                            newStyle = FontStyle.Bold;
                            break;
                        case "italic":
                            newStyle = FontStyle.Italic;
                            break;
                        default:
                            await TEXT_ENGINE.Write(NewLine + "Invalid font style!");
                            ERROR_OCCURRED = true;
                            break;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        switch (SYSFONT_STYLE.Args.Length)
                        {
                            case 1:
                                SYS_FONT = new Font(SYS_FONT.FontFamily, SYS_FONT.Size, newStyle);
                                IOField.Font = SYS_FONT;
                                break;
                            case 2:
                                switch (SYSFONT_STYLE.Args[1].Arg)
                                {
                                    case "-s":
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("FontStyle", newStyle);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("StartupSystemFont", STRT_IMG.SystemFontFamily.Name + ">" + STRT_IMG.SystemFontSize + ">" + newStyle, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.SystemFontStyle = newStyle;
                                        STRT_PROPS.StartupSystemFont = new Font(STRT_PROPS.SystemFontFamily, STRT_PROPS.SystemFontSize, STRT_PROPS.SystemFontStyle);
                                        break;
                                    case "-b":
                                        switch (STRT_IMG.ExecutionLevel)
                                        {
                                            case ExecutionLevel.Administrator:
                                                REG_KEY.SetValue("FontStyle", newStyle);
                                                break;
                                            case ExecutionLevel.User:
                                                SetINIEntry("StartupSystemFont", STRT_IMG.SystemFontFamily.Name + ">" + STRT_IMG.SystemFontSize + ">" + newStyle, "BOOT.ini");
                                                break;
                                        }
                                        STRT_PROPS.SystemFontStyle = newStyle;
                                        SYS_FONT = new Font(SYS_FONT.FontFamily, SYS_FONT.Size, newStyle);
                                        STRT_PROPS.StartupSystemFont = SYS_FONT;
                                        IOField.Font = SYS_FONT;
                                        break;
                                    default:
                                        await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + SYSFONT_STYLE.Args[1].Arg);
                                        break;
                                }
                                break;
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(FULLSCREEN, async () => {
                try
                {
                    WRITABLE = false;
                    switch (FULLSCREEN.Args.Length)
                    {
                        case 0:
                            FullscreenHelper();
                            break;
                        default:
                            switch (FULLSCREEN.Args[0].Arg)
                            {
                                case "-w":
                                    if (FULLSCREEN.Args.Length == 2)
                                    {
                                        switch (FULLSCREEN.Args[1].Arg)
                                        {
                                            case "-s":
                                                FullscreenHelper(true, true, false);
                                                break;
                                            case "-b":
                                                FullscreenHelper(true, true, true);
                                                break;
                                            default:
                                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + FULLSCREEN.Args[1].Arg);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        FullscreenHelper(true);
                                    }
                                    break;
                                case "-s":
                                    if (FULLSCREEN.Args.Length == 2)
                                    {
                                        if (FULLSCREEN.Args[1].Arg == "-w")
                                        {
                                            FullscreenHelper(true, true, false);
                                        }
                                        else
                                        {
                                            await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + FULLSCREEN.Args[1].Arg);
                                        }
                                    }
                                    else
                                    {
                                        FullscreenHelper(setStartup: true, setFullsceen: false);
                                    }
                                    break;
                                case "-b":
                                    if (FULLSCREEN.Args.Length == 2)
                                    {
                                        if (FULLSCREEN.Args[1].Arg == "-w")
                                        {
                                            FullscreenHelper(true, true, true);
                                        }
                                        else
                                        {
                                            await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + FULLSCREEN.Args[1].Arg);
                                        }
                                    }
                                    else
                                    {
                                        FullscreenHelper(setStartup: true);
                                    }
                                    break;
                                default:
                                    await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(1): " + FULLSCREEN.Args[0].Arg);
                                    break;
                            }
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(EMULATION, async () => {
                try
                {
                    WRITABLE = false;
                    switch (EMULATION.Args.Length)
                    {
                        case 0:
                            EmulationHelper();
                            break;
                        default:
                            switch (EMULATION.Args[0].Arg)
                            {
                                case "-n":
                                    if (EMULATION.Args.Length == 2)
                                    {
                                        switch (EMULATION.Args[1].Arg)
                                        {
                                            case "-s":
                                                EmulationHelper(true, true, false);
                                                break;
                                            case "-b":
                                                EmulationHelper(true, true, true);
                                                break;
                                            default:
                                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + EMULATION.Args[1].Arg);
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        EmulationHelper(true);
                                    }
                                    break;
                                case "-s":
                                    if (EMULATION.Args.Length == 2)
                                    {
                                        if (EMULATION.Args[1].Arg == "-n")
                                        {
                                            EmulationHelper(true, true, false);
                                        }
                                        else
                                        {
                                            await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + EMULATION.Args[1].Arg);
                                        }
                                    }
                                    else
                                    {
                                        EmulationHelper(setStartup: true, setEmulation: false);
                                    }
                                    break;
                                case "-b":
                                    if (EMULATION.Args.Length == 2)
                                    {
                                        if (EMULATION.Args[1].Arg == "-n")
                                        {
                                            EmulationHelper(true, true, true);
                                        }
                                        else
                                        {
                                            await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + EMULATION.Args[1].Arg);
                                        }
                                    }
                                    else
                                    {
                                        EmulationHelper(setStartup: true);
                                    }
                                    break;
                                default:
                                    await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(1): " + EMULATION.Args[0].Arg);
                                    break;
                            }
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(TEXTEFFECT, async () => {
                try
                {
                    WRITABLE = false;
                    int result;
                    switch (TEXTEFFECT.Args.Length)
                    {
                        case 0:
                            TextEffecHelper(newInterval: TXT_INTERVAL);
                            break;
                        case 3:
                            if ((TEXTEFFECT.Args[0].Arg == "-s" && TEXTEFFECT.Args[1].Arg == "-n") || (TEXTEFFECT.Args[0].Arg == "-n" && TEXTEFFECT.Args[1].Arg == "-s"))
                            {
                                if (int.TryParse(TEXTEFFECT.Args.Last().Arg, out result) && result >= 1 && result <= 100)
                                {
                                    TextEffecHelper(true, true, false, result);
                                }
                            }
                            else if ((TEXTEFFECT.Args[0].Arg == "-b" && TEXTEFFECT.Args[1].Arg == "-n") || (TEXTEFFECT.Args[0].Arg == "-n" && TEXTEFFECT.Args[1].Arg == "-b"))
                            {
                                if (int.TryParse(TEXTEFFECT.Args.Last().Arg, out result) && result >= 1 && result <= 100)
                                {
                                    TextEffecHelper(true, true, true, result);
                                }
                            }
                            break;
                        default:
                            switch (TEXTEFFECT.Args[0].Arg)
                            {
                                case "-n":
                                    if (TEXTEFFECT.Args.Length == 2)
                                    {
                                        switch (TEXTEFFECT.Args[1].Arg)
                                        {
                                            case "-s":
                                                TextEffecHelper(true, true, false, TXT_INTERVAL);
                                                break;
                                            case "-b":
                                                TextEffecHelper(true, true, true, TXT_INTERVAL);
                                                break;
                                            default:
                                                if (int.TryParse(TEXTEFFECT.Args.Last().Arg, out result) && result >= 1 && result <= 100)
                                                {
                                                    TextEffecHelper(true, false, false, result);
                                                }
                                                else
                                                {
                                                    await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + TEXTEFFECT.Args[1].Arg);
                                                }
                                                break;
                                        }
                                    }
                                    else
                                    {
                                        TextEffecHelper(true, newInterval: TXT_INTERVAL);
                                    }
                                    break;
                                case "-s":
                                    if (TEXTEFFECT.Args.Length == 2)
                                    {
                                        if (TEXTEFFECT.Args[1].Arg == "-n")
                                        {
                                            TextEffecHelper(true, true, false, TXT_INTERVAL);
                                        }
                                        else
                                        {
                                            if (int.TryParse(TEXTEFFECT.Args.Last().Arg, out result) && result >= 1 && result <= 100)
                                            {
                                                TextEffecHelper(false, true, false, result);
                                            }
                                            else
                                            {
                                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + TEXTEFFECT.Args[1].Arg);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TextEffecHelper(setStartup: true, setTextEffect: false, newInterval: TXT_INTERVAL);
                                    }
                                    break;
                                case "-b":
                                    if (TEXTEFFECT.Args.Length == 2)
                                    {
                                        if (TEXTEFFECT.Args[1].Arg == "-n")
                                        {
                                            TextEffecHelper(true, true, true, TXT_INTERVAL);
                                        }
                                        else
                                        {
                                            if (int.TryParse(TEXTEFFECT.Args.Last().Arg, out result) && result >= 1 && result <= 100)
                                            {
                                                TextEffecHelper(false, true, true, result);
                                            }
                                            else
                                            {
                                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + TEXTEFFECT.Args[1].Arg);
                                            }
                                        }
                                    }
                                    else
                                    {
                                        TextEffecHelper(setStartup: true, newInterval: TXT_INTERVAL);
                                    }
                                    break;
                                default:
                                    if (int.TryParse(TEXTEFFECT.Args.Last().Arg, out result) && result >= 1 && result <= 100)
                                    {
                                        TextEffecHelper(newInterval: result);
                                    }
                                    else
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(1): " + TEXTEFFECT.Args[0].Arg);
                                    }
                                    break;
                            }
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(REGEDIT, async () => {
                try
                {
                    WRITABLE = false;
                    if (STRT_IMG.ExecutionLevel == ExecutionLevel.Administrator)
                    {
                        if (CURRENT_SKEY.StartsWith(Registry.CurrentUser.Name))
                        {
                            CURRENT_HKEY = Registry.CurrentUser;
                        }
                        else if (CURRENT_SKEY.StartsWith(Registry.ClassesRoot.Name))
                        {
                            CURRENT_HKEY = Registry.ClassesRoot;
                        }
                        else if (CURRENT_SKEY.StartsWith(Registry.CurrentConfig.Name))
                        {
                            CURRENT_HKEY = Registry.CurrentConfig;
                        }
                        else if (CURRENT_SKEY.StartsWith(Registry.LocalMachine.Name))
                        {
                            CURRENT_HKEY = Registry.LocalMachine;
                        }
                        else if (CURRENT_SKEY.StartsWith(Registry.Users.Name))
                        {
                            CURRENT_HKEY = Registry.Users;
                        }
                        CMD_MGMT = new CommandManagement(REG_CMD_INIT);
                        REGEDIT_MODE = true;
                        IOField.Clear();
                        await TEXT_ENGINE.Write(CURRENT_SKEY + "> ");
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Access denied!");
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }      
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(WORDGEN, async () => {
                try
                {
                    WRITABLE = false;
                    char[] chars = null;
                    string path = null;
                    int wordLength = 0;
                    if (WORDGEN.Args.Length == 2)
                    {
                        path = CURRENT_DIR + "wordgen.txt";
                    }
                    else
                    {
                        string pathArg = WORDGEN.Args[2].InterpreterParameters.InterpretedString;
                        if (pathArg.Last() == '\\')
                        {
                            pathArg = pathArg.Remove(pathArg.Length - 1);
                        }
                        if (Directory.Exists(pathArg))
                        {
                            path = pathArg;
                        }
                        else if (Directory.Exists(CURRENT_DIR + pathArg))
                        {
                            path = CURRENT_DIR + pathArg;
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid output path!");
                            ERROR_OCCURRED = true;
                        }
                    }

                    if (!ERROR_OCCURRED)
                    {
                        chars = WORDGEN.Args[0].Arg.ToCharArray();
                        chars.Distinct();
                        if (!int.TryParse(WORDGEN.Args[1].Arg, out wordLength))
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid word length!");
                            ERROR_OCCURRED = true;
                        }
                        if (!ERROR_OCCURRED)
                        {
                            IOField.AppendText(NewLine);
                            OPEN_PATH = path;
                            await Task.Run(async () => await WordgenWorker(chars, wordLength), CT);
                            CT_SOURCE.Dispose();
                            CT_SOURCE = new CancellationTokenSource();
                            CT = CT_SOURCE.Token;
                            WORDGEN_RUNNING = false;
                            WRITABLE = true;
                            OPEN_PATH = null;
                        }
                    }
                    if (ERROR_OCCURRED)
                    {
                        ERROR_OCCURRED = false;
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(HASHCRACK, async () => {
                try
                {
                    WRITABLE = false;
                    Regex hexCheck = new Regex("^[a-fA-F0-9]*$");
                    HashType shaType = new HashType();
                    bool hashTypeFound = false;
                    string pathStr = HASHCRACK.Args[0].InterpreterParameters.InterpretedString;
                    string hashStr = HASHCRACK.Args[1].InterpreterParameters.InterpretedString;
                    if (File.Exists(CURRENT_DIR + pathStr))
                    {
                        pathStr = CURRENT_DIR + pathStr;
                    }
                    else
                    {
                        if (!File.Exists(pathStr))
                        {
                            await TEXT_ENGINE.Write("Invalid wordlist path!");
                            ERROR_OCCURRED = true;
                        }
                    }
                    if (!ERROR_OCCURRED)
                    {
                        switch (hashStr.Length)
                        {
                            case 40:
                                if (hexCheck.IsMatch(hashStr))
                                {
                                    shaType = HashType.SHA1;
                                    hashTypeFound = true;
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write("Invalid hash value!");
                                }
                                break;
                            case 64:
                                if (hexCheck.IsMatch(hashStr))
                                {
                                    shaType = HashType.SHA256;
                                    hashTypeFound = true;
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write("Invalid hash value!");
                                }
                                break;
                            case 96:
                                if (hexCheck.IsMatch(hashStr))
                                {
                                    shaType = HashType.SHA384;
                                    hashTypeFound = true;
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write("Invalid hash value!");
                                }
                                break;
                            case 128:
                                if (hexCheck.IsMatch(hashStr))
                                {
                                    shaType = HashType.SHA512;
                                    hashTypeFound = true;
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write("Invalid hash value!");
                                }
                                break;
                            default:
                                await TEXT_ENGINE.Write("Invalid hash value!");
                                break;
                        }
                        if (hashTypeFound)
                        {
                            IOField.AppendText(NewLine);
                            await Task.Run(async () => await HashcrackWorker(File.ReadAllLines(pathStr), hashStr, shaType), CT);
                            CT_SOURCE.Dispose();
                            CT_SOURCE = new CancellationTokenSource();
                            CT = CT_SOURCE.Token;
                            WRITABLE = true;
                            HASHCRACK_RUNNING = false;
                        }
                    }
                    else
                    {
                        ERROR_OCCURRED = false;
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(MH, async () => {
                try
                {
                    WRITABLE = false;
                    switch (MH.Args[1].Arg)
                    {
                        case "-sha512":
                            SHA512 sha512 = SHA512.Create();
                            await TEXT_ENGINE.Write(NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            Clipboard.SetText(BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            break;
                        case "-sha384":
                            SHA384 sha384 = SHA384.Create();
                            await TEXT_ENGINE.Write(NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha384.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            Clipboard.SetText(BitConverter.ToString(sha384.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            break;
                        case "-sha256":
                            SHA256 sha256 = SHA256.Create();
                            await TEXT_ENGINE.Write(NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            Clipboard.SetText(BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            break;
                        case "-sha1":
                            SHA1 sha1 = SHA1.Create();
                            await TEXT_ENGINE.Write(NewLine + "Output hash(copied to clipboard): " + BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            Clipboard.SetText(BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(MH.Args[0].InterpreterParameters.InterpretedString))).Replace("-", ""));
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(BIOSINFO, async () => {
                try
                {
                    WRITABLE = false;
                    BIOS bios = new BIOS();
                    bios.GetBIOSInformation();
                    string[] writerBufferArray = new string[28];
                    writerBufferArray[0] = "Build Number: " + bios.BuildNumber + NewLine;
                    writerBufferArray[1] = "Caption: " + bios.Caption + NewLine;
                    writerBufferArray[2] = "Code Set: " + bios.CodeSet + NewLine;
                    writerBufferArray[3] = "Current Language: " + bios.CurrentLanguage + NewLine;
                    writerBufferArray[4] = "Description: " + bios.Description + NewLine;
                    writerBufferArray[5] = "Embedded Controller Major Version: " + bios.EmbeddedControllerMajorVersion + NewLine;
                    writerBufferArray[6] = "Embedded Controller Minor Version: " + bios.EmbeddedControllerMinorVersion + NewLine;
                    writerBufferArray[7] = "Identification Code: " + bios.IdentificationCode + NewLine;
                    writerBufferArray[8] = "Installable Languages: " + bios.InstallableLanguages + NewLine;
                    writerBufferArray[9] = "Install Date: " + bios.InstallDate + NewLine;
                    writerBufferArray[10] = "Language Edition: " + bios.LanguageEdition + NewLine;
                    writerBufferArray[11] = "Manufacturer: " + bios.Manufacturer + NewLine;
                    writerBufferArray[12] = "Name: " + bios.Name + NewLine;
                    writerBufferArray[13] = "Other Target OS: " + bios.OtherTargetOS + NewLine;
                    writerBufferArray[14] = "Primary BIOS: " + bios.PrimaryBIOS + NewLine;
                    writerBufferArray[15] = "Release Date: " + bios.ReleaseDate + NewLine;
                    writerBufferArray[16] = "Serial Number: " + bios.SerialNumber + NewLine;
                    writerBufferArray[17] = "SMBIOS Version: " + bios.SMBIOSBIOSVersion + NewLine;
                    writerBufferArray[18] = "SMBIOS Major Version: " + bios.SMBIOSMajorVersion + NewLine;
                    writerBufferArray[19] = "SMBIOS Minor Version: " + bios.SMBIOSMinorVersion + NewLine;
                    writerBufferArray[20] = "SMBIOS Present: " + bios.SMBIOSPresent + NewLine;
                    writerBufferArray[21] = "Software Element ID: " + bios.SoftwareElementID + NewLine;
                    writerBufferArray[22] = "Software Element State: " + bios.SoftwareElementState + NewLine;
                    writerBufferArray[23] = "Status: " + bios.Status + NewLine;
                    writerBufferArray[24] = "System BIOS Major Version: " + bios.SystemBiosMajorVersion + NewLine;
                    writerBufferArray[25] = "System BIOS Minor Version: " + bios.SystemBiosMinorVersion + NewLine;
                    writerBufferArray[26] = "Target Operating System: " + bios.TargetOperatingSystem + NewLine;
                    writerBufferArray[27] = "Version: " + bios.Version;
                    IOField.AppendText(NewLine);
                    await TEXT_ENGINE.WriteArray(writerBufferArray);
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(DRIVER_NAMES, async () => {
                try
                {
                    WRITABLE = false;
                    DriverInfo systemDrivers = new DriverInfo();
                    DriverInfo[] driverInfos = systemDrivers.GetDriverInfo();
                    if (driverInfos != null)
                    {
                        switch (DRIVER_NAMES.Args.Length)
                        {
                            case 0:
                                for (int i = 0; i < driverInfos.Length; i++)
                                {
                                    await TEXT_ENGINE.Write(NewLine + driverInfos[i].Name);
                                }
                                break;
                            case 1:
                                InterpreterResult res = null;
                                for (int i = 0; i < driverInfos.Length; i++)
                                {
                                    res = new InterpreterResult(driverInfos[i].Name);
                                    res.GetResult(DRIVER_NAMES.Args[0].InterpreterParameters);
                                    if (res.Result)
                                    {
                                        await TEXT_ENGINE.Write(NewLine + driverInfos[i].Name);
                                    }
                                }
                                break;
                        }
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(DRIVER_INFO, async () => {
                try
                {
                    WRITABLE = false;
                    DriverInfo systemDrivers = new DriverInfo();
                    DriverInfo[] driverInfos = systemDrivers.GetDriverInfo();
                    if (driverInfos != null)
                    {
                        string[] writerBufferArray = null;
                        for (int i = 0; i < driverInfos.Length; i++)
                        {
                            if (driverInfos[i].Name == DRIVER_INFO.Args[0].InterpreterParameters.InterpretedString)
                            {
                                writerBufferArray = new string[22];
                                writerBufferArray[0] = "Accept Pause: " + driverInfos[i].AcceptPause + NewLine;
                                writerBufferArray[1] = "Accept Stop: " + driverInfos[i].AcceptStop + NewLine;
                                writerBufferArray[2] = "Caption: " + driverInfos[i].Caption + NewLine;
                                writerBufferArray[3] = "Creation Class Name: " + driverInfos[i].CreationClassName + NewLine;
                                writerBufferArray[4] = "Description: " + driverInfos[i].Description + NewLine;
                                writerBufferArray[5] = "Desktop Interact: " + driverInfos[i].DesktopInteract + NewLine;
                                writerBufferArray[6] = "Display Name: " + driverInfos[i].DisplayName + NewLine;
                                writerBufferArray[7] = "Error Control: " + driverInfos[i].ErrorControl + NewLine;
                                writerBufferArray[8] = "Exit Code: " + driverInfos[i].ExitCode + NewLine;
                                writerBufferArray[9] = "Install Date: " + driverInfos[i].InstallDate + NewLine;
                                writerBufferArray[10] = "Name: " + driverInfos[i].Name + NewLine;
                                writerBufferArray[11] = "Path Name: " + driverInfos[i].PathName + NewLine;
                                writerBufferArray[12] = "Service Specific Exit Code: " + driverInfos[i].ServiceSpecificExitCode + NewLine;
                                writerBufferArray[13] = "Service Type: " + driverInfos[i].ServiceType + NewLine;
                                writerBufferArray[14] = "Started: " + driverInfos[i].Started + NewLine;
                                writerBufferArray[15] = "Start Mode: " + driverInfos[i].StartMode + NewLine;
                                writerBufferArray[16] = "Start Name: " + driverInfos[i].StartName + NewLine;
                                writerBufferArray[17] = "State: " + driverInfos[i].State + NewLine;
                                writerBufferArray[18] = "Status: " + driverInfos[i].Status + NewLine;
                                writerBufferArray[19] = "System Creation Class Name: " + driverInfos[i].SystemCreationClassName + NewLine;
                                writerBufferArray[20] = "System Name: " + driverInfos[i].SystemName + NewLine;
                                writerBufferArray[21] = "Tag ID: " + driverInfos[i].TagId + NewLine;
                                IOField.AppendText(NewLine);
                                await TEXT_ENGINE.WriteArray(writerBufferArray);
                                break;
                            }
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Drivers not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(DRIVER_CHECK, async () => {
                try
                {
                    WRITABLE = false;
                    DriverInfo driverInfo = new DriverInfo();
                    DriverInfo[] driverInfos = driverInfo.GetDriverIssues();
                    if (driverInfos != null)
                    {
                        string[] writerBufferArray = null;
                        for (int i = 0; i < driverInfos.Length; i++)
                        {
                            if (driverInfos[i].Name == DRIVER_INFO.Args[0].InterpreterParameters.InterpretedString)
                            {
                                writerBufferArray = new string[22];
                                writerBufferArray[0] = "Accept Pause: " + driverInfos[i].AcceptPause + NewLine;
                                writerBufferArray[1] = "Accept Stop: " + driverInfos[i].AcceptStop + NewLine;
                                writerBufferArray[2] = "Caption: " + driverInfos[i].Caption + NewLine;
                                writerBufferArray[3] = "Creation Class Name: " + driverInfos[i].CreationClassName + NewLine;
                                writerBufferArray[4] = "Description: " + driverInfos[i].Description + NewLine;
                                writerBufferArray[5] = "Desktop Interact: " + driverInfos[i].DesktopInteract + NewLine;
                                writerBufferArray[6] = "Display Name: " + driverInfos[i].DisplayName + NewLine;
                                writerBufferArray[7] = "Error Control: " + driverInfos[i].ErrorControl + NewLine;
                                writerBufferArray[8] = "Exit Code: " + driverInfos[i].ExitCode + NewLine;
                                writerBufferArray[9] = "Install Date: " + driverInfos[i].InstallDate + NewLine;
                                writerBufferArray[10] = "Name: " + driverInfos[i].Name + NewLine;
                                writerBufferArray[11] = "Path Name: " + driverInfos[i].PathName + NewLine;
                                writerBufferArray[12] = "Service Specific Exit Code: " + driverInfos[i].ServiceSpecificExitCode + NewLine;
                                writerBufferArray[13] = "Service Type: " + driverInfos[i].ServiceType + NewLine;
                                writerBufferArray[14] = "Started: " + driverInfos[i].Started + NewLine;
                                writerBufferArray[15] = "Start Mode: " + driverInfos[i].StartMode + NewLine;
                                writerBufferArray[16] = "Start Name: " + driverInfos[i].StartName + NewLine;
                                writerBufferArray[17] = "State: " + driverInfos[i].State + NewLine;
                                writerBufferArray[18] = "Status: " + driverInfos[i].Status + NewLine;
                                writerBufferArray[19] = "System Creation Class Name: " + driverInfos[i].SystemCreationClassName + NewLine;
                                writerBufferArray[20] = "System Name: " + driverInfos[i].SystemName + NewLine;
                                writerBufferArray[21] = "Tag ID: " + driverInfos[i].TagId + NewLine;
                                IOField.AppendText(NewLine);
                                await TEXT_ENGINE.WriteArray(writerBufferArray);
                                break;
                            }
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Driver issues not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(GETNICS, async () => {
                try
                {
                    WRITABLE = false;
                    NetworkInterfaceDiscovery interfaceDisc = new NetworkInterfaceDiscovery();
                    ToolShed.ThisPC.NetworkInterface[] intArray = interfaceDisc.GetInterfaces().Result;
                    List<string> bufferList = new List<string>();
                    bufferList.Add(NewLine);
                    bufferList.Add("Searching for network interfaces...");
                    bufferList.Add(NewLine);
                    switch (GETNICS.Args.Length)
                    {
                        case 0:
                            for (int i = 0; i < intArray.Length; i++)
                            {
                                bufferList.Add(NewLine);
                                bufferList.Add("Name: " + intArray[i].Name + NewLine);
                                bufferList.Add("Type: " + intArray[i].Type + NewLine);
                                bufferList.Add("IP Address: " + intArray[i].IPAddress + NewLine);
                                bufferList.Add("Subnet Mask: " + intArray[i].SubnetMask + NewLine);
                                bufferList.Add("Default Gateway: " + intArray[i].DefaultGateway + NewLine);
                            }
                            break;
                        case 1:
                            switch (GETNICS.Args[0].Arg)
                            {
                                case "-a":
                                    for (int i = 0; i < intArray.Length; i++)
                                    {
                                        bufferList.Add(NewLine);
                                        bufferList.Add("Name: " + intArray[i].Name + NewLine);
                                        bufferList.Add("Description: " + intArray[i].Description + NewLine);
                                        bufferList.Add("Instance ID: " + intArray[i].InstanceID + NewLine);
                                        bufferList.Add("Type: " + intArray[i].Type + NewLine);
                                        bufferList.Add("IP Address: " + intArray[i].IPAddress + NewLine);
                                        bufferList.Add("Subnet Mask: " + intArray[i].SubnetMask + NewLine);
                                        bufferList.Add("Default Gateway: " + intArray[i].DefaultGateway + NewLine);
                                        bufferList.Add("MAC Address: " + intArray[i].MACAddress + NewLine);
                                        bufferList.Add("Speed: " + intArray[i].Speed + NewLine);
                                        bufferList.Add("Status: " + intArray[i].Status + NewLine);
                                    }
                                    break;
                                default:
                                    InterpreterResult res = null;
                                    for (int i = 0; i < intArray.Length; i++)
                                    {
                                        res = new InterpreterResult(intArray[i].Name);
                                        res.GetResult(GETNICS.Args[0].InterpreterParameters);
                                        if (res.Result)
                                        {
                                            bufferList.Add(NewLine);
                                            bufferList.Add("Name: " + intArray[i].Name + NewLine);
                                            bufferList.Add("Type: " + intArray[i].Type + NewLine);
                                            bufferList.Add("IP Address: " + intArray[i].IPAddress + NewLine);
                                            bufferList.Add("Subnet Mask: " + intArray[i].SubnetMask + NewLine);
                                            bufferList.Add("Default Gateway: " + intArray[i].DefaultGateway + NewLine);
                                        }
                                    }
                                    break;
                            }
                            break;
                        case 2:
                            bool argCheck = false;
                            Argument filterArg = new Argument();
                            if (GETNICS.Args[0].Arg == "-a")
                            {
                                argCheck = true;
                                filterArg = GETNICS.Args[1];
                            }
                            else if (GETNICS.Args[1].Arg == "-a")
                            {
                                argCheck = true;
                                filterArg = GETNICS.Args[0];
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + GETNICS.Args[1].Arg);
                                ERROR_OCCURRED = true;
                            }
                            if (argCheck && !ERROR_OCCURRED)
                            {
                                InterpreterResult res = null;
                                for (int i = 0; i < intArray.Length; i++)
                                {
                                    res = new InterpreterResult(intArray[i].Name);
                                    res.GetResult(filterArg.InterpreterParameters);
                                    if (res.Result)
                                    {
                                        bufferList.Add(NewLine);
                                        bufferList.Add("Name: " + intArray[i].Name + NewLine);
                                        bufferList.Add("Description: " + intArray[i].Description + NewLine);
                                        bufferList.Add("Instance ID: " + intArray[i].InstanceID + NewLine);
                                        bufferList.Add("Type: " + intArray[i].Type + NewLine);
                                        bufferList.Add("IP Address: " + intArray[i].IPAddress + NewLine);
                                        bufferList.Add("Subnet Mask: " + intArray[i].SubnetMask + NewLine);
                                        bufferList.Add("Default Gateway: " + intArray[i].DefaultGateway + NewLine);
                                        bufferList.Add("MAC Address: " + intArray[i].MACAddress + NewLine);
                                        bufferList.Add("Speed: " + intArray[i].Speed + NewLine);
                                        bufferList.Add("Status: " + intArray[i].Status + NewLine);
                                    }
                                }
                            }
                            break;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        await TEXT_ENGINE.WriteArray(bufferList.ToArray());
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(PING, async () => {
                try
                {
                    await Task.Run(async () => await PingWorker(PING.Args[0].InterpreterParameters.InterpretedString), CT);
                    CT_SOURCE.Dispose();
                    CT_SOURCE = new CancellationTokenSource();
                    CT = CT_SOURCE.Token;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(ARP, async () => {
                try
                {
                    WRITABLE = false;
                    int dstIP = 0;
                    int srcIP = 0;
                    byte[] macAddr = null;
                    int macAddrLength = 0;
                    IPAddress res;
                    if (IPAddress.TryParse(ARP.Args[1].Arg, out res))
                    {
                        dstIP = ConvertIPToInt32(res);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Invalid destination IP address!");
                        ERROR_OCCURRED = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        if (IPAddress.TryParse(ARP.Args[0].Arg, out res))
                        {
                            srcIP = ConvertIPToInt32(res);
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid source IP address!");
                            ERROR_OCCURRED = true;
                        }
                        if (!ERROR_OCCURRED)
                        {
                            try
                            {
                                macAddr = ConvertHexStringToByteArray(ARP.Args[2].Arg);
                                macAddrLength = macAddr.Length;
                            }
                            catch (Exception)
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid MAC address!");
                                ERROR_OCCURRED = true;
                            }
                            if (!ERROR_OCCURRED)
                            {
                                try
                                {
                                    SendArp(dstIP, srcIP, macAddr, ref macAddrLength);
                                    await TEXT_ENGINE.Write(NewLine + "ARP packet sent to: " + ARP.Args[1].Arg);
                                }
                                catch (Exception ex)
                                {
                                    await TEXT_ENGINE.Write(NewLine + "Failed to send ARP packet! Error: " + ex.Message);
                                }
                                ERROR_OCCURRED = false;
                                await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                            }
                        }
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(NMAP, async () => {
                try
                {
                    await Task.Run(NmapWorker, CT);
                    CT_SOURCE.Dispose();
                    CT_SOURCE = new CancellationTokenSource();
                    CT = CT_SOURCE.Token;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(SETMAC, async () => {
                try
                {
                    WRITABLE = false;
                    if (STRT_IMG.ExecutionLevel == ExecutionLevel.Administrator)
                    {
                        string nicName = SETMAC.Args[0].InterpreterParameters.InterpretedString;
                        string MACAddress = string.Empty;
                        if (SETMAC.Args[1].Arg.Contains(":"))
                        {
                            MACAddress = SETMAC.Args[1].Arg.Replace(":", "");
                        }
                        else
                        {
                            MACAddress = SETMAC.Args[1].Arg;
                        }
                        NetworkInterfaceDiscovery interfaceDisc = new NetworkInterfaceDiscovery();
                        ToolShed.ThisPC.NetworkInterface[] intArray = interfaceDisc.GetInterfaces().Result;
                        PhysicalAddressManagement MACMgmt;
                        for (int i = 0; i < intArray.Length; i++)
                        {
                            if (intArray[i].Name == nicName)
                            {
                                MACMgmt = new PhysicalAddressManagement(intArray[i]);
                                MACMgmt.SetAddress(MACAddress);
                            }
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Access denied!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(RESETMAC, async () => {
                try
                {
                    WRITABLE = false;
                    if (STRT_IMG.ExecutionLevel == ExecutionLevel.Administrator)
                    {
                        string nicName = RESETMAC.Args[0].InterpreterParameters.InterpretedString;
                        NetworkInterfaceDiscovery interfaceDisc = new NetworkInterfaceDiscovery();
                        ToolShed.ThisPC.NetworkInterface[] intArray = interfaceDisc.GetInterfaces().Result;
                        PhysicalAddressManagement MACMgmt;
                        for (int i = 0; i < intArray.Length; i++)
                        {
                            if (intArray[i].Name == nicName)
                            {
                                MACMgmt = new PhysicalAddressManagement(intArray[i]);
                                MACMgmt.ResetAddress();
                            }
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Access denied!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(STARTUP_CONFIG, async () => {
                try
                {
                    WRITABLE = false;
                    List<string> bufferList = new List<string>();
                    switch (STRT_IMG.ExecutionLevel)
                    {
                        case ExecutionLevel.Administrator:
                            bufferList.Add(NewLine + "STARTUP CONFIG INFORMATION LOADED FROM REGISTRY");
                            break;
                        case ExecutionLevel.User:
                            bufferList.Add(NewLine + "STARTUP CONFIG INFORMATION LOADED FROM BOOT.ini");
                            break;
                    }
                    bufferList.Add(NewLine + "Execution level: " + STRT_PROPS.ExecutionLevel);
                    bufferList.Add(NewLine + "Startup directory: " + STRT_PROPS.StartupDirectory);
                    bufferList.Add(NewLine + "Foreground color: " + STRT_PROPS.StartupForegroundColor.Name);
                    bufferList.Add(NewLine + "Background color: " + STRT_PROPS.StartupBackgroundColor.Name);
                    if (STRT_PROPS.StartupSystemFont.Name == "Perfect DOS VGA 437 Win")
                    {
                        bufferList.Add(NewLine + "System font: CP437 " + STRT_IMG.StartupSystemFont.Size + " " + STRT_IMG.StartupSystemFont.Style);
                    }
                    else
                    {
                        bufferList.Add(NewLine + "System font: " + STRT_PROPS.StartupSystemFont.Name + " " + STRT_IMG.StartupSystemFont.Size + " " + STRT_IMG.StartupSystemFont.Style);
                    }
                    bufferList.Add(NewLine + "Fullscreen: " + STRT_PROPS.StartupFullscreen);
                    bufferList.Add(NewLine + "Emulation: " + STRT_PROPS.StartupEmulation);
                    bufferList.Add(NewLine + "Text effect: " + STRT_PROPS.TextEffect);
                    bufferList.Add(NewLine + "Text effect interval: " + STRT_PROPS.TextEffectInterval);
                    bufferList.Add(NewLine + "Default history size: " + STRT_PROPS.DefaultHistorySize);
                    bufferList.Add(NewLine + "Regedit history size: " + STRT_PROPS.RegeditHistorySize);
                    bufferList.Add(NewLine + "Verbose startup: " + STRT_PROPS.VerboseStartup);
                    bufferList.Add(NewLine + "SHMOTD: " + STRT_PROPS.StartupShowMOTD);
                    bufferList.Add(NewLine + "MOTD: " + STRT_PROPS.StartupMOTD);
                    bufferList.Add(NewLine + "Startup subkey: " + STRT_PROPS.StartupSubkey);
                    await TEXT_ENGINE.WriteArray(bufferList.ToArray());
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(COPY_CURRENT_CONFIG, async () => {
                try
                {
                    WRITABLE = false;
                    switch (COPY_CURRENT_CONFIG.Args[0].Arg)
                    {
                        case "-ini":
                            File.WriteAllText("BOOT.ini", string.Format(@"StartupDirectory>{0}|Color>{1}>{2}|StartupSystemFont>{3}>{4}>{5}|Fullscreen>{6}|Emulation>{7}|VerboseStartup>{8}|TextEffect>{9}|TextEffectInterval>{10}|DefaultHistorySize>{11}|RegeditHistorySize>{12}|SHMOTD>{13}|MOTD>{14}", CURRENT_DIR, FORE_COLOR.Name, BACK_COLOR.Name, SYS_FONT.FontFamily.Name, SYS_FONT.Size, SYS_FONT.Style, Convert.ToInt16(FSCR), Convert.ToInt16(EMU), Convert.ToInt16(STRT_PROPS.VerboseStartup), Convert.ToInt16(STRT_PROPS.TextEffect), STRT_PROPS.TextEffectInterval, STRT_PROPS.DefaultHistorySize, STRT_PROPS.RegeditHistorySize, Convert.ToInt16(STRT_PROPS.StartupShowMOTD), STRT_PROPS.StartupMOTD));
                            break;
                        case "-reg":
                            REG_KEY.SetValue("StartupDirectory", CURRENT_DIR);
                            REG_KEY.SetValue("ForegroundColor", FORE_COLOR.Name);
                            REG_KEY.SetValue("BackgroundColor", BACK_COLOR.Name);
                            REG_KEY.SetValue("FontFamily", SYS_FONT.FontFamily.Name);
                            REG_KEY.SetValue("FontSize", SYS_FONT.Size, RegistryValueKind.DWord);
                            REG_KEY.SetValue("FontStyle", SYS_FONT.Style);
                            REG_KEY.SetValue("Fullscreen", Convert.ToInt16(FSCR), RegistryValueKind.DWord);
                            REG_KEY.SetValue("Emulation", Convert.ToInt16(EMU), RegistryValueKind.DWord);
                            REG_KEY.SetValue("SHMOTD", Convert.ToInt16(STRT_PROPS.StartupShowMOTD), RegistryValueKind.DWord);
                            REG_KEY.SetValue("VerboseStartup", Convert.ToInt16(STRT_PROPS.VerboseStartup), RegistryValueKind.DWord);
                            REG_KEY.SetValue("TextEffect", Convert.ToInt16(STRT_PROPS.TextEffect), RegistryValueKind.DWord);
                            REG_KEY.SetValue("TextEffectInterval", STRT_PROPS.TextEffectInterval, RegistryValueKind.DWord);
                            REG_KEY.SetValue("DefaultHistorySize", STRT_PROPS.DefaultHistorySize, RegistryValueKind.DWord);
                            REG_KEY.SetValue("RegeditHistorySize", STRT_PROPS.RegeditHistorySize, RegistryValueKind.DWord);
                            REG_KEY.SetValue("MOTD", STRT_PROPS.StartupMOTD);
                            REG_KEY.SetValue("StartupSubkey", CURRENT_SKEY);
                            break;
                        default:
                            await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(1): " + COPY_CURRENT_CONFIG.Args[0].Arg);
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(ERASE_STARTUP_CONFIG, async () => {
                try
                {
                    WRITABLE = false;
                    switch (ERASE_STARTUP_CONFIG.Args[0].Arg)
                    {
                        case "-ini":
                            if (File.Exists("BOOT.ini"))
                            {
                                File.Delete("BOOT.ini");
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "BOOT.ini not found!");
                            }
                            break;
                        case "-reg":
                            if (STRT_IMG.ExecutionLevel == ExecutionLevel.Administrator)
                            {
                                Registry.LocalMachine.DeleteSubKey(@"SOFTWARE\WinDOS");
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Access denied!");
                            }
                            break;
                        default:
                            await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(1): " + ERASE_STARTUP_CONFIG.Args[0].Arg);
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(STARTUP_DIR, async () => {
                try
                {
                    WRITABLE = false;
                    string newStdir = STARTUP_DIR.Args[0].InterpreterParameters.InterpretedString;
                    STR_BUILDER = new StringBuilder();
                    if (Directory.Exists(newStdir))
                    {
                        STR_BUILDER.Append(newStdir);
                    }
                    else if (Directory.Exists(CURRENT_DIR + newStdir))
                    {
                        STR_BUILDER.Append(CURRENT_DIR + newStdir);
                    }
                    else if (newStdir == "wdir")
                    {
                        STR_BUILDER.Append(CurrentDirectory);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Directory not found!");
                        ERROR_OCCURRED = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        if (STR_BUILDER.ToString() != null)
                        {
                            if (STR_BUILDER.ToString().Last() != '\\')
                            {
                                STR_BUILDER.Append('\\');
                            }
                            switch (STRT_IMG.ExecutionLevel)
                            {
                                case ExecutionLevel.Administrator:
                                    REG_KEY.SetValue("StartupDirectory", STR_BUILDER.ToString());
                                    break;
                                case ExecutionLevel.User:
                                    SetINIEntry("StartupDirectory", STR_BUILDER.ToString(), "BOOT.ini");
                                    break;
                            }
                        }

                        if (STARTUP_DIR.Args.Length == 2)
                        {
                            if (STARTUP_DIR.Args[1].Arg == "-b")
                            {
                                CURRENT_DIR = STR_BUILDER.ToString();
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + STARTUP_DIR.Args[1].Arg);
                                ERROR_OCCURRED = true;
                            }
                        }
                        if (!ERROR_OCCURRED)
                        {
                            STRT_PROPS.StartupDirectory = newStdir;
                        }
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(STARTUP_SHMOTD, async () => {
                try
                {
                    WRITABLE = false;
                    bool shmotd = false;
                    switch (STARTUP_SHMOTD.Args.Length)
                    {
                        case 0:
                            shmotd = true;
                            break;
                        case 1:
                            if (STARTUP_SHMOTD.Args[0].Arg != "-n")
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(1): " + STARTUP_SHMOTD.Args[0].Arg);
                                ERROR_OCCURRED = true;
                            }
                            break;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        switch (STRT_PROPS.ExecutionLevel)
                        {
                            case ExecutionLevel.Administrator:
                                REG_KEY.SetValue("SHMOTD", Convert.ToInt16(shmotd), RegistryValueKind.DWord);
                                break;
                            case ExecutionLevel.User:
                                SetINIEntry("SHMOTD", Convert.ToInt16(shmotd).ToString(), "BOOT.ini");
                                break;
                        }
                        STRT_PROPS.StartupShowMOTD = shmotd;
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(STARTUP_MOTD, async () => {
                WRITABLE = false;
                switch (STRT_IMG.ExecutionLevel)
                {
                    case ExecutionLevel.Administrator:
                        REG_KEY.SetValue("MOTD", STARTUP_MOTD.Args[0].InterpreterParameters.InterpretedString);
                        break;
                    case ExecutionLevel.User:
                        SetINIEntry("MOTD", STARTUP_MOTD.Args[0].InterpreterParameters.InterpretedString, "BOOT.ini");
                        break;
                }
                STRT_PROPS.StartupMOTD = STARTUP_MOTD.Args[0].InterpreterParameters.InterpretedString;
                try
                {
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                }
                catch (InvalidOperationException) { }
                WRITABLE = true;
            });
            DEF_CMD_INIT.Add(STARTUP_SUBKEY, async () => {
                try
                {
                    WRITABLE = false;
                    if (STRT_IMG.ExecutionLevel == ExecutionLevel.Administrator)
                    {
                        bool stskIsValid = false;
                        string newStsk = STARTUP_SUBKEY.Args[0].InterpreterParameters.InterpretedString;
                        if (newStsk.StartsWith("HKEY_CLASSES_ROOT") && newStsk.Replace("HKEY_CLASSES_ROOT", "").Length > 0)
                        {
                            if (Registry.ClassesRoot.OpenSubKey(newStsk.Replace("HKEY_CLASSES_ROOT\\", ""), true) != null)
                            {
                                stskIsValid = true;
                            }
                        }
                        else if (newStsk.StartsWith("HKEY_CURRENT_CONFIG") && newStsk.Replace("HKEY_CURRENT_CONFIG", "").Length > 0)
                        {
                            if (Registry.CurrentConfig.OpenSubKey(newStsk.Replace("HKEY_CURRENT_CONFIG\\", ""), true) != null)
                            {
                                stskIsValid = true;
                            }
                        }
                        else if (newStsk.StartsWith("HKEY_CURRENT_USER") && newStsk.Replace("HKEY_CURRENT_USER", "").Length > 0)
                        {
                            if (Registry.CurrentUser.OpenSubKey(newStsk.Replace("HKEY_CURRENT_USER\\", ""), true) != null)
                            {
                                stskIsValid = true;
                            }
                        }
                        else if (newStsk.StartsWith("HKEY_LOCAL_MACHINE") && newStsk.Replace("HKEY_LOCAL_MACHINE", "").Length > 0)
                        {
                            if (Registry.LocalMachine.OpenSubKey(newStsk.Replace("HKEY_LOCAL_MACHINE\\", ""), true) != null)
                            {
                                stskIsValid = true;
                            }
                        }
                        else if (newStsk.StartsWith("HKEY_USERS") && newStsk.Replace("HKEY_USERS", "").Length > 0)
                        {
                            if (Registry.Users.OpenSubKey(newStsk.Replace("HKEY_USERS\\", ""), true) != null)
                            {
                                stskIsValid = true;
                            }
                        }
                        else if (newStsk == "HKEY_CLASSES_ROOT" || newStsk == "HKEY_CURRENT_CONFIG" || newStsk == "HKEY_CURRENT_USER" || newStsk == "HKEY_LOCAL_MACHINE" || newStsk == "HKEY_USERS" || newStsk == "HKEY_CLASSES_ROOT" || newStsk == "HKEY_CURRENT_CONFIG\\" || newStsk == "HKEY_CURRENT_USER\\" || newStsk == "HKEY_LOCAL_MACHINE\\" || newStsk == "HKEY_USERS\\")
                        {
                            stskIsValid = true;
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Subkey not found!");
                        }


                        if (stskIsValid)
                        {
                            if (newStsk.Last() != '\\')
                            {
                                newStsk += '\\';
                            }
                            REG_KEY.SetValue("StartupSubkey", newStsk);
                            STRT_PROPS.StartupSubkey = newStsk;
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Access denied!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(STARTUP_VERBOSE, async () => {
                try
                {
                    WRITABLE = false;
                    bool vstart = false;
                    switch (STARTUP_VERBOSE.Args.Length)
                    {
                        case 0:
                            vstart = true;
                            break;
                        case 1:
                            if (STARTUP_VERBOSE.Args[0].Arg != "-n")
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(1): " + STARTUP_VERBOSE.Args[0].Arg);
                                ERROR_OCCURRED = true;
                            }
                            break;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        switch (STRT_IMG.ExecutionLevel)
                        {
                            case ExecutionLevel.Administrator:
                                REG_KEY.SetValue("VerboseStartup", Convert.ToInt16(vstart), RegistryValueKind.DWord);
                                break;
                            case ExecutionLevel.User:
                                SetINIEntry("VerboseStartup", Convert.ToInt16(vstart).ToString(), "BOOT.ini");
                                break;
                        }
                        STRT_PROPS.VerboseStartup = vstart;
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(EXEC, async () => {
                try
                {
                    WRITABLE = false;
                    bool sourceIsValid = false;
                    STR_BUILDER = new StringBuilder(EXEC.Args[0].InterpreterParameters.InterpretedString);
                    if (File.Exists(STR_BUILDER.ToString()))
                    {
                        if (Path.GetExtension(STR_BUILDER.ToString()) == ".doscmd")
                        {
                            sourceIsValid = true;
                        }
                    }
                    else if (File.Exists(CURRENT_DIR + STR_BUILDER.ToString()))
                    {
                        STR_BUILDER.Insert(0, CURRENT_DIR);
                        if (Path.GetExtension(STR_BUILDER.ToString()) == ".doscmd")
                        {
                            sourceIsValid = true;
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "File not found!");
                        ERROR_OCCURRED = true;
                    }

                    if (!ERROR_OCCURRED)
                    {
                        if (sourceIsValid)
                        {
                            WRITABLE = false;
                            string[] lineArray = File.ReadAllLines(STR_BUILDER.ToString());
                            for (int i = 0; i < lineArray.Length; i++)
                            {
                                if (!lineArray[i].StartsWith("hashcrack ") && !lineArray[i].StartsWith("wordgen ") && !lineArray[i].StartsWith("nmap") && !lineArray[i].StartsWith("exec "))
                                {
                                    CMD_MGMT.ExecuteCommand(CMD_MGMT.GetCommandFromPool(lineArray[i]));
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write(NewLine + "Unsupported command: " + lineArray[i]);
                                }
                            }
                            WRITABLE = true;
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid file extension!");
                        }
                    }
                    else
                    {
                        ERROR_OCCURRED = false;
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(START, async () => {
                try
                {
                    WRITABLE = false;
                    Process p = new Process();
                    p.StartInfo.CreateNoWindow = false;
                    p.StartInfo.UseShellExecute = false;
                    if (File.Exists(START.Args[0].InterpreterParameters.InterpretedString))
                    {
                        p.StartInfo.FileName = START.Args[0].InterpreterParameters.InterpretedString;
                        p.StartInfo.WorkingDirectory = START.Args[0].InterpreterParameters.InterpretedString.Replace(Path.GetFileName(START.Args[0].InterpreterParameters.InterpretedString), "");
                        try
                        {
                            p.Start();
                        }
                        catch (Exception ex)
                        {
                            await TEXT_ENGINE.Write(NewLine + "An error pervented the application from starting!");
                            await TEXT_ENGINE.Write(NewLine + "Error message: " + ex.Message);
                        }
                    }
                    else if (File.Exists(CURRENT_DIR + START.Args[0].InterpreterParameters.InterpretedString))
                    {
                        p.StartInfo.FileName = CURRENT_DIR + START.Args[0].InterpreterParameters.InterpretedString;
                        p.StartInfo.WorkingDirectory = CURRENT_DIR + START.Args[0].InterpreterParameters.InterpretedString.Replace(Path.GetFileName(CURRENT_DIR + START.Args[0].InterpreterParameters.InterpretedString), "");
                        try
                        {
                            p.Start();
                        }
                        catch (Exception ex)
                        {
                            await TEXT_ENGINE.Write(NewLine + "An error pervented the application from starting!");
                            await TEXT_ENGINE.Write(NewLine + "Error message: " + ex.Message);
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "File not found!");
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            DEF_CMD_INIT.Add(RESTART);

            /*UNIVERSAL COMMANDS*/
            HISTORY_SETSIZE.Function = async () =>
            {
                try
                {
                    WRITABLE = false;
                    bool setStartup = false;
                    bool setHistory = false;
                    if (HISTORY_SETSIZE.Args.Length == 2)
                    {
                        switch (HISTORY_SETSIZE.Args[1].Arg)
                        {
                            case "-s":
                                setStartup = true;
                                break;
                            case "-b":
                                setStartup = true;
                                setHistory = true;
                                break;
                            default:
                                await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + HISTORY_SETSIZE.Args[1].Arg);
                                ERROR_OCCURRED = true;
                                break;
                        }
                    }
                    else
                    {
                        setHistory = true;
                    }
                    if (!ERROR_OCCURRED)
                    {
                        int newSize;
                        if (int.TryParse(HISTORY_SETSIZE.Args[0].Arg, out newSize) && newSize >= 2 && newSize < int.MaxValue)
                        {
                            if (REGEDIT_MODE)
                            {
                                if (setHistory)
                                {
                                    HIST.ResizeRegeditHistory(newSize);
                                }
                                if (setStartup)
                                {
                                    switch (STRT_IMG.ExecutionLevel)
                                    {
                                        case ExecutionLevel.Administrator:
                                            REG_KEY.SetValue("RegeditHistorySize", newSize, RegistryValueKind.DWord);
                                            break;
                                        case ExecutionLevel.User:
                                            SetINIEntry("RegeditHistorySize", newSize.ToString(), "BOOT.ini");
                                            break;
                                    }
                                    STRT_PROPS.RegeditHistorySize = newSize;
                                }
                            }
                            else
                            {
                                if (setHistory)
                                {
                                    HIST.ResizeDefaultHistory(newSize);
                                }
                                if (setStartup)
                                {
                                    switch (STRT_IMG.ExecutionLevel)
                                    {
                                        case ExecutionLevel.Administrator:
                                            REG_KEY.SetValue("DefaultHistorySize", newSize, RegistryValueKind.DWord);
                                            break;
                                        case ExecutionLevel.User:
                                            SetINIEntry("DefaultHistorySize", newSize.ToString(), "BOOT.ini");
                                            break;
                                    }
                                    STRT_PROPS.DefaultHistorySize = newSize;
                                }
                            }
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid history size!");
                        }
                    }
                    ERROR_OCCURRED = false;
                    if (REGEDIT_MODE)
                    {
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            };
            REG_CMD_INIT.Add(HISTORY_SETSIZE);
            DEF_CMD_INIT.Add(HISTORY_SETSIZE);
            HISTORY_GETENTRY.Function = async () =>
            {
                try
                {
                    WRITABLE = false;
                    int index;
                    if (REGEDIT_MODE)
                    {
                        if (HISTORY_GETENTRY.Args[0].Arg.StartsWith("!"))
                        {
                            int lastNonEmptyIndex = 0;
                            for (int i = 0; i < HIST.RegeditHistory.Length; i++)
                            {
                                if (HIST.RegeditHistory[i] != "")
                                {
                                    lastNonEmptyIndex = i;
                                }
                            }
                            if (int.TryParse(HISTORY_GETENTRY.Args[0].Arg.Replace("!", ""), out index) && index >= 0 && index < lastNonEmptyIndex)
                            {
                                await TEXT_ENGINE.Write(NewLine + "     " + HIST.RegeditHistory[lastNonEmptyIndex - (index + 1)]);
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid history entry index!");
                            }
                        }
                        else if (int.TryParse(HISTORY_GETENTRY.Args[0].Arg, out index) && index > 0 && index < HIST.RegeditHistory.Length)
                        {
                            await TEXT_ENGINE.Write(NewLine + "     " + HIST.RegeditHistory[index]);
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid history entry index!");
                        }
                        await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    }
                    else
                    {
                        if (HISTORY_GETENTRY.Args[0].Arg.StartsWith("!"))
                        {
                            int lastNonEmptyIndex = 0;
                            for (int i = 0; i < HIST.DefaultHistory.Length; i++)
                            {
                                if (HIST.DefaultHistory[i] != "")
                                {
                                    lastNonEmptyIndex = i;
                                }
                            }
                            if (int.TryParse(HISTORY_GETENTRY.Args[0].Arg.Replace("!", ""), out index) && index >= 0 && index < lastNonEmptyIndex)
                            {
                                await TEXT_ENGINE.Write(NewLine + "     " + HIST.DefaultHistory[lastNonEmptyIndex - (index + 1)]);
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid history entry index!");
                            }
                        }
                        else if (int.TryParse(HISTORY_GETENTRY.Args[0].Arg, out index) && index > 0 && index < HIST.DefaultHistory.Length)
                        {
                            await TEXT_ENGINE.Write(NewLine + "     " + HIST.DefaultHistory[index]);
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid history entry index!");
                        }
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                        WRITABLE = true;
                    }
                }
                catch (InvalidOperationException) { }
            };
            DEF_CMD_INIT.Add(HISTORY_GETENTRY);
            REG_CMD_INIT.Add(HISTORY_GETENTRY);
            HISTORY_CLEAR.Function = async () =>
            {
                try
                {
                    WRITABLE = false;
                    if (REGEDIT_MODE)
                    {
                        for (int i = 0; i < HIST.RegeditHistory.Length; i++)
                        {
                            HIST.RegeditHistory[i] = "";
                        }
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }
                    else
                    {
                        for (int i = 0; i < HIST.DefaultHistory.Length; i++)
                        {
                            HIST.DefaultHistory[i] = "";
                        }
                        await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            };
            DEF_CMD_INIT.Add(HISTORY_CLEAR);
            REG_CMD_INIT.Add(HISTORY_CLEAR);
            HISTORY_SHLIST.Function = async () =>
            {
                try
                {
                    WRITABLE = false;
                    string[] histArray = null;
                    if (REGEDIT_MODE)
                    {
                        histArray = HIST.RegeditHistory;
                    }
                    else
                    {
                        histArray = HIST.DefaultHistory;
                    }
                    switch (HISTORY_SHLIST.Args.Length)
                    {
                        case 0:
                            for (int i = 0; i < histArray.Length; i++)
                            {
                                if (histArray[i].Length > 0)
                                {
                                    await TEXT_ENGINE.Write(NewLine + "    " + histArray[i]);
                                }
                            }
                            break;
                        case 1:
                            InterpreterResult res = null;
                            for (int i = 0; i < histArray.Length; i++)
                            {
                                res = new InterpreterResult(histArray[i]);
                                res.GetResult(HISTORY_SHLIST.Args[0].InterpreterParameters);
                                if (res.Result)
                                {
                                    if (histArray[i].Length > 0)
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "    " + histArray[i]);
                                    }
                                }
                            }
                            break;
                    }
                    if (REGEDIT_MODE)
                    {
                        await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }            
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            };
            DEF_CMD_INIT.Add(HISTORY_SHLIST);
            REG_CMD_INIT.Add(HISTORY_SHLIST);
            CLS.Function = async () =>
            {
                try
                {
                    if (REGEDIT_MODE)
                    {
                        await TEXT_ENGINE.Write(CURRENT_SKEY + "> ", true);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(CURRENT_DIR + "> ", true);
                    }
                }
                catch (InvalidOperationException) { }
            };
            DEF_CMD_INIT.Add(CLS);
            REG_CMD_INIT.Add(CLS);
            HELP.Function = async () =>
            {
                try
                {
                    List<string> bufferList = new List<string>();
                    bufferList.Add(NewLine + "[]  = USER SPECIFIED ARGUMENTS");
                    bufferList.Add(NewLine + "{}  = OPTIONAL ARGUMENTS");
                    bufferList.Add(NewLine + "/   = OR");
                    bufferList.Add(NewLine + "()  = ADDITIONAL INFORMATION");
                    bufferList.Add(NewLine + "<=> = INTERCHANGEABLE ORDER BETWEEN TWO ARGUMENTS");
                    Command[] cmdArray = CMD_MGMT.CommandPool.GetCommands();
                    if (HELP.Args.Length > 0)
                    {
                        InterpreterResult res = null;
                        for (int i = 0; i < cmdArray.Length; i++)
                        {
                            res = new InterpreterResult(cmdArray[i].Call);
                            res.GetResult(HELP.Args[0].InterpreterParameters);
                            if (res.Result)
                            {
                                bufferList.Add(NewLine);
                                string[] splitArray = cmdArray[i].Description.Replace(NewLine, "˝").Split('˝');
                                for (int j = 0; j < splitArray.Length; j++)
                                {
                                    bufferList.Add(NewLine + splitArray[j]);
                                }
                            }
                        }
                    }
                    else
                    {
                        for (int i = 0; i < cmdArray.Length; i++)
                        {
                            bufferList.Add(NewLine);
                            string[] splitArray = cmdArray[i].Description.Replace(NewLine, "˝").Split('˝');
                            for (int j = 0; j < splitArray.Length; j++)
                            {
                                bufferList.Add(NewLine + splitArray[j]);
                            }
                        }
                    }
                    WRITABLE = false;
                    await TEXT_ENGINE.WriteArray(bufferList.ToArray());
                    if (REGEDIT_MODE)
                    {
                        await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            };
            DEF_CMD_INIT.Add(HELP);
            REG_CMD_INIT.Add(HELP);
            EXIT.Function = async () =>
            {
                try
                {
                    if (!REGEDIT_MODE)
                    {
                        await ChangeSelectColour(Color.FromArgb(0, 120, 215), Color.FromArgb(255, 255, 255));
                        WRITABLE = false;
                        await TEXT_ENGINE.Write("Closing WinDOS...", true);
                        if (NMAP_RUNNING)
                        {
                            DeviceDiscovery.CancelDiscovery();
                            NMAP_RUNNING = false;
                        }
                        CancelWorker();
                        await TextEngine.CancelWrite();
                        if (EMU)
                        {
                            Process.Start("C:\\Windows\\explorer.exe");
                        }
                        Exit(0);
                    }
                    else
                    {
                        CMD_MGMT = new CommandManagement(DEF_CMD_INIT);
                        REGEDIT_MODE = false;
                        await TEXT_ENGINE.Write(CURRENT_DIR + "> ", true);
                    }
                }
                catch (InvalidOperationException) { }
            };
            DEF_CMD_INIT.Add(EXIT);
            REG_CMD_INIT.Add(EXIT);

            /*REGEDIT COMMANDS*/
            REG_CMD_INIT.Add(CSK, async () => {
                try
                {
                    WRITABLE = false;
                    string input = CSK.Args[0].InterpreterParameters.InterpretedString;
                    try
                    {
                        if (input == "..")
                        {
                            if (CURRENT_SKEY.Length != CURRENT_SKEY.Replace("\\", "").Length + 1)
                            {
                                CURRENT_SKEY = CURRENT_SKEY.Remove(CURRENT_SKEY.Length - 1);
                                CURRENT_SKEY = CURRENT_SKEY.Remove(FindLastCharacterIndex(CURRENT_SKEY, '\\') + 1);
                            }
                        }
                        else if (input == "stsk")
                        {
                            CURRENT_SKEY = STRT_PROPS.StartupSubkey;
                        }
                        else
                        {
                            if (input.StartsWith(CURRENT_SKEY))
                            {

                                if (CURRENT_HKEY.OpenSubKey(input, true) != null)
                                {
                                    CURRENT_SKEY += input.Remove(0, CURRENT_SKEY.Length);
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
                                        CURRENT_HKEY = Registry.LocalMachine;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.LocalMachine;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.ClassesRoot;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.ClassesRoot;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.CurrentConfig;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.CurrentConfig;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.CurrentUser;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.CurrentUser;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.Users;
                                        CURRENT_SKEY = input;
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
                                        CURRENT_HKEY = Registry.Users;
                                        CURRENT_SKEY = input;
                                    }
                                    else
                                    {
                                        throw new Exception();
                                    }
                                }
                            }
                            else
                            {
                                if (CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", "") + input, true) != null)
                                {
                                    CURRENT_SKEY += input;
                                }
                                else
                                {
                                    throw new Exception();
                                }
                            }
                        }
                        if (CURRENT_SKEY.Last() != '\\')
                        {
                            CURRENT_SKEY += '\\';
                        }
                    }
                    catch (Exception)
                    {
                        await TEXT_ENGINE.Write(NewLine + "Subkey not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(SK_GET, async () => {
                try
                {
                    WRITABLE = false;
                    string[] skArray = null;
                    InterpreterResult res = null;
                    switch (SK_GET.Args.Length)
                    {
                        case 0:
                            skArray = Task.Run(async () => await GetSubkeyHelper(CURRENT_SKEY), CT).Result;
                            if (skArray.Length > 0)
                            {
                                for (int i = 0; i < skArray.Length; i++)
                                {
                                    await TEXT_ENGINE.Write(NewLine + "    " + skArray[i]);
                                }
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "The specified subkey contains no subkeys!");
                            }
                            break;
                        case 1:
                            if (SK_GET.Args[0].ContainsGlobs)
                            {
                                skArray = Task.Run(async () => await GetSubkeyHelper(CURRENT_SKEY), CT).Result;
                                if (skArray.Length > 0)
                                {
                                    for (int i = 0; i < skArray.Length; i++)
                                    {
                                        res = new InterpreterResult(skArray[i]);
                                        res.GetResult(SK_GET.Args[0].InterpreterParameters);
                                        if (res.Result)
                                        {
                                            await TEXT_ENGINE.Write(NewLine + "    " + skArray[i]);
                                        }
                                    }
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write(NewLine + "The specified subkey contains no subkeys!");
                                }
                            }
                            else
                            {
                                skArray = Task.Run(async () => await GetSubkeyHelper(SK_GET.Args[0].InterpreterParameters.InterpretedString), CT).Result;
                                if (skArray.Length > 0)
                                {
                                    for (int i = 0; i < skArray.Length; i++)
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "    " + skArray[i]);
                                    }
                                }
                                else
                                {
                                    await TEXT_ENGINE.Write(NewLine + "The specified subkey contains no subkeys!");
                                }
                            }
                            break;
                        case 2:
                            skArray = Task.Run(async () => await GetSubkeyHelper(SK_GET.Args[0].InterpreterParameters.InterpretedString), CT).Result;
                            if (skArray.Length > 0)
                            {
                                for (int i = 0; i < skArray.Length; i++)
                                {
                                    res = new InterpreterResult(skArray[i]);
                                    res.GetResult(SK_GET.Args[1].InterpreterParameters);
                                    if (res.Result)
                                    {
                                        await TEXT_ENGINE.Write(NewLine + "    " + skArray[i]);
                                    }
                                }
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "The specified subkey contains no subkeys!");
                            }
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(SK_MAKE, async () => {
                try
                {
                    WRITABLE = false;
                    try
                    {
                        string input = SK_MAKE.Args[0].InterpreterParameters.InterpretedString;
                        if (input.StartsWith(CURRENT_SKEY))
                        {
                            CURRENT_HKEY.CreateSubKey(input.Remove(0, CURRENT_SKEY.Length));
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
                                CURRENT_HKEY.CreateSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", "") + input);
                            }
                        }
                    }

                    catch (Exception ex)
                    {
                        if (ex.Message != "Sequence contains no elements.")
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid path!");
                        }
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(SK_DELETE, async () => {
                try
                {
                    WRITABLE = false;
                    try
                    {
                        string input = SK_DELETE.Args[0].InterpreterParameters.InterpretedString;
                        if (input.StartsWith(CURRENT_SKEY))
                        {
                            CURRENT_HKEY.DeleteSubKeyTree(input.Remove(0, CURRENT_SKEY.Length));
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
                                CURRENT_HKEY.DeleteSubKeyTree(input);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ex.Message != "Sequence contains no elements.")
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid path!");
                        }
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(VAL_GET, async () => {
                try
                {
                    WRITABLE = false;
                    switch (VAL_GET.Args.Length)
                    {
                        case 0:
                            GetValueHelper();
                            break;
                        case 1:
                            if (VAL_GET.Args[0].ContainsGlobs)
                            {
                                GetValueHelper("", true, VAL_GET.Args[0].InterpreterParameters);
                            }
                            else
                            {
                                GetValueHelper(VAL_GET.Args[0].InterpreterParameters.InterpretedString);
                            }
                            break;
                        case 2:
                            GetValueHelper(VAL_GET.Args[0].InterpreterParameters.InterpretedString, true, VAL_GET.Args[1].InterpreterParameters);
                            break;
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(VAL_SET, async () => {
                try
                {
                    WRITABLE = false;
                    if (CURRENT_SKEY != CURRENT_HKEY.Name + "\\")
                    {
                        string name = VAL_SET.Args[0].InterpreterParameters.InterpretedString;
                        string value = VAL_SET.Args[1].Arg;
                        RegistryValueKind vk = CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).GetValueKind(name);
                        if (vk == RegistryValueKind.DWord || vk == RegistryValueKind.QWord || vk == RegistryValueKind.Binary)
                        {
                            int result;
                            if (int.TryParse(value, out result))
                            {
                                CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).SetValue(name, result, vk);
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Cannot assign string value to DWORD, QWORD and BINARY registry values!");
                            }
                        }
                        else
                        {
                            CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).SetValue(name, value);
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Value not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(VAL_DELETE, async () => {
                try
                {
                    WRITABLE = false;
                    if (CURRENT_SKEY != CURRENT_HKEY.Name + "\\")
                    {
                        CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).DeleteValue(VAL_DELETE.Args[0].InterpreterParameters.InterpretedString);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Value not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(VAL_MAKE, async () => {
                try
                {
                    WRITABLE = false;
                    switch (VAL_MAKE.Args.Length)
                    {
                        case 1:
                            MakeValueHelper(VAL_MAKE.Args[0].InterpreterParameters.InterpretedString);
                            break;
                        case 2:
                            MakeValueHelper(VAL_MAKE.Args[0].InterpreterParameters.InterpretedString, VAL_MAKE.Args[1].Arg);
                            break;
                        case 3:
                            RegistryValueKind kind = RegistryValueKind.None;
                            switch (VAL_MAKE.Args[2].Arg.ToLower())
                            {
                                case "-dw":
                                    kind = RegistryValueKind.DWord;
                                    break;
                                case "-bin":
                                    kind = RegistryValueKind.Binary;
                                    break;
                                case "-es":
                                    kind = RegistryValueKind.ExpandString;
                                    break;
                                case "-ms":
                                    kind = RegistryValueKind.MultiString;
                                    break;
                                case "-n":
                                    kind = RegistryValueKind.None;
                                    break;
                                case "-qw":
                                    kind = RegistryValueKind.QWord;
                                    break;
                                case "-s":
                                    kind = RegistryValueKind.String;
                                    break;
                                case "-u":
                                    kind = RegistryValueKind.Unknown;
                                    break;
                                default:
                                    await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(3): " + VAL_MAKE.Args[2].Arg);
                                    ERROR_OCCURRED = true;
                                    break;
                            }
                            if (!ERROR_OCCURRED)
                            {
                                MakeValueHelper(VAL_MAKE.Args[0].InterpreterParameters.InterpretedString, VAL_MAKE.Args[1].Arg, kind);
                            }
                            break;
                    }
                    ERROR_OCCURRED = false;
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
            REG_CMD_INIT.Add(VAL_RENAME, async () => {
                try
                {
                    WRITABLE = false;
                    if (CURRENT_SKEY != CURRENT_HKEY.Name + "\\")
                    {
                        string oldName = VAL_RENAME.Args[0].InterpreterParameters.InterpretedString;
                        string newName = VAL_RENAME.Args[1].InterpreterParameters.InterpretedString;
                        CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).SetValue(newName, CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).GetValue(oldName), CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).GetValueKind(oldName));
                        CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).DeleteValue(oldName);
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Value not found!");
                    }
                    await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                    WRITABLE = true;
                }
                catch (InvalidOperationException) { }
            });
        }
        #endregion

        #region STARTUP METHODS
        private async Task SimulateLoading()
        {
            IOField.AppendText(NewLine + "Starting WinDOS - v" + Assembly.GetExecutingAssembly().GetName().Version);
            for (int i = 0; i < 5; i++)
            {
                IOField.AppendText(".");
                await Task.Delay(500);
            }
            IOField.Clear();
        }

        private async void LoadImage(BootImage imageToLoad)
        {
            Text = "WinDOS (" + imageToLoad.ExecutionLevel.ToString() + ")";
            DEF_BOUNDS = Bounds;
            CURRENT_DIR = imageToLoad.StartupDirectory;
            FORE_COLOR = imageToLoad.StartupForegroundColor;
            BACK_COLOR = imageToLoad.StartupBackgroundColor;
            await ChangeSelectColour(BACK_COLOR, FORE_COLOR);
            SYS_FONT = imageToLoad.StartupSystemFont;
            FSCR = imageToLoad.StartupFullscreen;
            EMU = imageToLoad.StartupEmulation;
            CURRENT_SKEY = imageToLoad.StartupSubkey;
            IOField.ForeColor = FORE_COLOR;
            IOField.BackColor = BACK_COLOR;
            IOField.Font = SYS_FONT;
            TXT_EFFECT = imageToLoad.OutputEffect;
            TXT_INTERVAL = imageToLoad.OutputEffectInterval;
            DEF_HIST_SIZE = imageToLoad.BasicHistorySize;
            REG_HIST_SIZE = imageToLoad.RegeditHistorySize;
            TEXT_ENGINE = new TextEngine(TXT_INTERVAL, TXT_EFFECT, IOField);
            HIST = new CommandHistory(DEF_HIST_SIZE, REG_HIST_SIZE);
            if (FSCR)
            {
                FormBorderStyle = FormBorderStyle.None;
                Bounds = Screen.PrimaryScreen.Bounds;
                IOField.Size = new Size(Width + 20, Height);
            }
            else
            {
                FormBorderStyle = FormBorderStyle.Sizable;
                Bounds = DEF_BOUNDS;
                IOField.Size = new Size(Width + 1, Height - 39);
                CenterToScreen();
            }
            if (EMU)
            {
                ProcessStartInfo taskKill = new ProcessStartInfo("taskkill", "/F /IM explorer.exe");
                taskKill.WindowStyle = ProcessWindowStyle.Hidden;
                Process p = new Process();
                p.StartInfo = taskKill;
                p.Start();
                p.WaitForExit();
            }
        }
        #endregion

        #region WORKERS
        private void CancelWorker()
        {
            CT_SOURCE.Cancel();
        }

        private async Task NmapWorker()
        {
            try
            {
                NMAP_RUNNING = true;
                WRITABLE = false;
                IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write("Searching for local hosts...", true)));
                NetworkInfo networkInfo = new NetworkInfo();
                string[] addresses = networkInfo.GetAddresses(Task.Run(async () => await new NetworkInterfaceDiscovery().GetInterfaces(), CT).Result);
                DeviceDiscovery deviceDisc = new DeviceDiscovery();
                deviceDisc.DeviceFound += DeviceDisc_DeviceFound;
                await Task.Run(async () => await deviceDisc.GetDevices(addresses, Task.Run(async () => await new NetworkInterfaceDiscovery().GetInterfaces(), CT).Result), CT);
                IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ")));
                WRITABLE = true;
                NMAP_RUNNING = false;
            }
            catch (InvalidOperationException) { }
        }

        private async Task PingWorker(string hostNameOrIPAddress)
        {
            try
            {
                bool endlessPing = false;
                if (PING.Args.Length > 1)
                {
                    if (PING.Args[1].Arg == "-t")
                    {
                        endlessPing = true;
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Invalid argument! Arg(2): " + PING.Args[1].Arg);
                    }
                }
                WRITABLE = false;
                Ping ping = new Ping();
                PingReply pingReply = null;
                if (endlessPing)
                {
                    while (!CT.IsCancellationRequested)
                    {
                        try
                        {
                            pingReply = ping.Send(hostNameOrIPAddress);
                            IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "Reply from: " + hostNameOrIPAddress + "     time = " + pingReply.RoundtripTime + "ms")));
                        }
                        catch (PingException ex)
                        {
                            IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + ex.Message)));
                        }
                        await Task.Delay(1000);
                    }
                }
                else
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (!CT.IsCancellationRequested)
                        {
                            try
                            {
                                pingReply = ping.Send(hostNameOrIPAddress);
                                IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "Reply from: " + hostNameOrIPAddress + "     time = " + pingReply.RoundtripTime + "ms")));
                            }
                            catch (PingException ex)
                            {
                                IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + ex.Message)));
                            }
                            await Task.Delay(1000);
                        }
                    }
                }
                IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ")));
                WRITABLE = true;
            }
            catch (InvalidOperationException) { }
        }

        private async Task WordgenWorker(char[] chars, int wordLength)
        {
            try
            {
                await Task.Delay(0);
                if (!CT.IsCancellationRequested)
                {
                    FileInfo inf = null;
                    StringBuilder sb = null;
                    WORDGEN_RUNNING = true;
                    TASK_START_TIME = DateTime.Now;
                    WGEN_CURRENT_INDEX = 0;
                    int generated = 0;
                    WRITABLE = false;
                    int[] locValue = new int[wordLength];
                    for (int i = 0; i < locValue.Length; i++)
                    {
                        if (!CT.IsCancellationRequested)
                        {
                            locValue[i] = 0;
                        }
                    }
                    WGEN_COMBO_COUNT = Math.Pow(chars.Length, locValue.Length);
                    for (int i = 0; i < WGEN_COMBO_COUNT; i++)
                    {
                        if (!CT.IsCancellationRequested)
                        {
                            sb = new StringBuilder();
                            for (int j = 0; j < locValue.Length; j++)
                            {
                                if (!CT.IsCancellationRequested)
                                {
                                    if (locValue[j] == chars.Length)
                                    {
                                        locValue[j] = 0;
                                        if (j + 1 < locValue.Length)
                                        {
                                            locValue[j + 1]++;
                                        }
                                    }
                                    sb.Append(chars[locValue[j]]);
                                }
                            }
                            WGEN_CURRENT_INDEX = i;
                            locValue[0]++;
                            StreamWriter sw = new StreamWriter(OPEN_PATH, true);
                            sw.WriteLine(sb.ToString());
                            sw.Close();
                            generated++;
                        }
                    }
                    IOField.Invoke(new MethodInvoker(async () =>
                    {
                        inf = new FileInfo(OPEN_PATH);
                        await TEXT_ENGINE.Write(NewLine + "Output file path: " + OPEN_PATH);
                        await TEXT_ENGINE.Write(NewLine + "File size: " + inf.Length / 1024 + "KB");
                        await TEXT_ENGINE.Write(NewLine + "Words generated: " + generated);
                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                    }));
                }
            }
            catch (ThreadAbortException) { }
            catch (InvalidOperationException) { }
            catch (UnauthorizedAccessException)
            {
                IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "Access denied!")));
            }
            catch (Exception ex)
            {
                IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "An error occurred!     Error code: " + ex.Message)));
            }
        }

        private async Task HashcrackWorker(string[] wordList, string hash, HashType shaType)
        {
            try
            {
                await Task.Delay(0);
                TASK_START_TIME = DateTime.Now;
                HCRACK_PROCESSED = 0;
                HCRACK_REMAINING = 0;
                HASHCRACK_RUNNING = true;
                WRITABLE = false;
                string[] writerBufferArray = null;
                string wordFound = string.Empty;
                string currentHash = string.Empty;
                if (!CT.IsCancellationRequested)
                {
                    switch (shaType)
                    {
                        case HashType.SHA1:
                            SHA1 sha1 = SHA1.Create();
                            try
                            {
                                if (!CT.IsCancellationRequested)
                                {
                                    for (int i = 0; i < wordList.Length; i++)
                                    {
                                        if (!CT.IsCancellationRequested)
                                        {
                                            currentHash = BitConverter.ToString(sha1.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                                            Thread.Sleep(1);
                                            HCRACK_PROCESSED = i + 1;
                                            HCRACK_REMAINING = wordList.Length - HCRACK_PROCESSED;
                                            if (currentHash == hash || currentHash.ToLower() == hash)
                                            {
                                                wordFound = wordList[i];
                                                break;
                                            }
                                        }
                                    }
                                    writerBufferArray = new string[3];
                                    writerBufferArray[0] = "Words checked: " + HCRACK_REMAINING + NewLine;
                                    writerBufferArray[1] = "Word found: " + wordFound + NewLine;
                                    writerBufferArray[2] = CURRENT_DIR + "> ";
                                    IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.WriteArray(writerBufferArray)));
                                }
                            }
                            catch (ThreadAbortException) { }
                            catch (UnauthorizedAccessException) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "Access denied!"))); }
                            catch (Exception ex) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write("An error occurred!     Error code: " + ex.Message))); }
                            break;
                        case HashType.SHA256:
                            SHA256 sha256 = SHA256.Create();
                            try
                            {
                                if (!CT.IsCancellationRequested)
                                {
                                    for (int i = 0; i < wordList.Length; i++)
                                    {
                                        if (!CT.IsCancellationRequested)
                                        {
                                            currentHash = BitConverter.ToString(sha256.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                                            Thread.Sleep(1);
                                            HCRACK_PROCESSED = i + 1;
                                            HCRACK_REMAINING = wordList.Length - HCRACK_PROCESSED;
                                            if (currentHash == hash || currentHash.ToLower() == hash)
                                            {
                                                wordFound = wordList[i];
                                                break;
                                            }
                                        }
                                    }
                                    writerBufferArray = new string[3];
                                    writerBufferArray[0] = "Words checked: " + HCRACK_REMAINING + NewLine;
                                    writerBufferArray[1] = "Word found: " + wordFound + NewLine;
                                    writerBufferArray[2] = CURRENT_DIR + "> ";
                                    IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.WriteArray(writerBufferArray)));
                                }
                            }
                            catch (ThreadAbortException) { }
                            catch (UnauthorizedAccessException) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "Access denied!"))); }
                            catch (Exception ex) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write("An error occurred!     Error code: " + ex.Message))); }
                            break;
                        case HashType.SHA384:
                            SHA384 sha384 = SHA384.Create();
                            try
                            {
                                if (!CT.IsCancellationRequested)
                                {
                                    for (int i = 0; i < wordList.Length; i++)
                                    {
                                        if (!CT.IsCancellationRequested)
                                        {
                                            currentHash = BitConverter.ToString(sha384.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                                            Thread.Sleep(1);
                                            HCRACK_PROCESSED = i + 1;
                                            HCRACK_REMAINING = wordList.Length - HCRACK_PROCESSED;
                                            if (currentHash == hash || currentHash.ToLower() == hash)
                                            {
                                                wordFound = wordList[i];
                                                break;
                                            }
                                        }
                                    }
                                    writerBufferArray = new string[3];
                                    writerBufferArray[0] = "Words checked: " + HCRACK_REMAINING + NewLine;
                                    writerBufferArray[1] = "Word found: " + wordFound + NewLine;
                                    writerBufferArray[2] = CURRENT_DIR + "> ";
                                    IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.WriteArray(writerBufferArray)));
                                }
                            }
                            catch (ThreadAbortException) { }
                            catch (UnauthorizedAccessException) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "Access denied!"))); }
                            catch (Exception ex) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write("An error occurred!     Error code: " + ex.Message))); }
                            break;
                        case HashType.SHA512:
                            SHA512 sha512 = SHA512.Create();
                            try
                            {
                                if (!CT.IsCancellationRequested)
                                {
                                    for (int i = 0; i < wordList.Length; i++)
                                    {
                                        if (!CT.IsCancellationRequested)
                                        {
                                            currentHash = BitConverter.ToString(sha512.ComputeHash(Encoding.UTF8.GetBytes(wordList[i]))).Replace("-", "");
                                            Thread.Sleep(1);
                                            HCRACK_PROCESSED = i + 1;
                                            HCRACK_REMAINING = wordList.Length - HCRACK_PROCESSED;
                                            if (currentHash == hash || currentHash.ToLower() == hash)
                                            {
                                                wordFound = wordList[i];
                                                break;
                                            }
                                        }
                                    }
                                    writerBufferArray = new string[3];
                                    writerBufferArray[0] = "Words checked: " + HCRACK_REMAINING + NewLine;
                                    writerBufferArray[1] = "Word found: " + wordFound + NewLine;
                                    writerBufferArray[2] = CURRENT_DIR + "> ";
                                    IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.WriteArray(writerBufferArray)));
                                }
                            }
                            catch (ThreadAbortException) { }
                            catch (UnauthorizedAccessException) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(NewLine + "Access denied!"))); }
                            catch (Exception ex) { IOField.Invoke(new MethodInvoker(async () => await TEXT_ENGINE.Write("An error occurred!     Error code: " + ex.Message))); }
                            break;
                    }
                }
            }
            catch (InvalidOperationException) { }
        }

        public ThreadProgress GetWordgenProgress(DateTime startTime, TimeSpan timeElapsed, double objCount, double currentObjIndex)
        {
            ThreadProgress tProg = new ThreadProgress();
            tProg.StartTime = startTime;
            tProg.TimeElapsed = timeElapsed;
            tProg.TimeLeft = TimeSpan.FromTicks(DateTime.Now.Subtract(startTime).Ticks * ((long)objCount - ((long)currentObjIndex + 1)) / ((long)currentObjIndex + 1));
            tProg.Percentage = (currentObjIndex + 1) / objCount * 100;
            return tProg;
        }

        public ThreadProgress GetHashcrackProgress(DateTime startTime, TimeSpan timeElapsed, double objProcessed, double objRemaining)
        {
            ThreadProgress tProg = new ThreadProgress();
            tProg.StartTime = startTime;
            tProg.TimeElapsed = timeElapsed;
            tProg.ObjectsProcessed = objProcessed;
            tProg.ObjectsRemaining = objRemaining;
            tProg.ObjectCount = objProcessed + objRemaining;
            return tProg;
        }

        private void DeviceDisc_DeviceFound(object sender, CurrentDeviceArgs e)
        {
            STR_BUILDER = new StringBuilder();
            IOField.BeginInvoke(new MethodInvoker(async () => await TEXT_ENGINE.Write(STR_BUILDER.Append(string.Format(NewLine + "Hostname: {0} ----- IP Address: {1} ----- MAC Address: {2}", e.CurrentDevice.Name, e.CurrentDevice.IPAddress, e.CurrentDevice.MACAddress.ToUpper().Replace("-", ":"))).ToString())));
        }
        #endregion

        #region HELPERS
        [DllImport("Iphlpapi.dll", EntryPoint = "SendARP")]
        internal extern static Int32 SendArp(Int32 destIpAddress, Int32 srcIpAddress, byte[] macAddress, ref Int32 macAddressLength);

        private static Int32 ConvertIPToInt32(IPAddress pIPAddr)
        {
            byte[] lByteAddress = pIPAddr.GetAddressBytes();
            return BitConverter.ToInt32(lByteAddress, 0);
        }

        public static byte[] ConvertHexStringToByteArray(string hexString)
        {
            if (hexString.Contains(":"))
            {
                hexString.Replace(":", "");
            }
            if (hexString.Length % 2 != 0)
            {
                throw new ArgumentException(string.Format(CultureInfo.InvariantCulture, "The binary key cannot have an odd number of digits: {0}", hexString));
            }

            byte[] data = new byte[hexString.Length / 2];
            for (int index = 0; index < data.Length; index++)
            {
                string byteValue = hexString.Substring(index * 2, 2);
                data[index] = byte.Parse(byteValue, NumberStyles.HexNumber, CultureInfo.InvariantCulture);
            }

            return data;
        }

        private async void TextEffecHelper(bool turnOff = false, bool setStartup = false, bool setTextEffect = true, int newInterval = 1)
        {
            try
            {
                if (newInterval < 1)
                {
                    await TEXT_ENGINE.Write(NewLine + "Invalid text effect interval!");
                }
                if (newInterval >= 1)
                {
                    if (turnOff)
                    {
                        TXT_EFFECT = false;
                    }
                    else
                    {
                        TXT_EFFECT = true;
                    }
                    if (setTextEffect)
                    {
                        TEXT_ENGINE = new TextEngine(newInterval, TXT_EFFECT, IOField);
                    }
                    if (setStartup)
                    {
                        switch (STRT_IMG.ExecutionLevel)
                        {
                            case ExecutionLevel.Administrator:
                                REG_KEY.SetValue("TextEffect", Convert.ToInt16(TXT_EFFECT), RegistryValueKind.DWord);
                                REG_KEY.SetValue("TextEffectInterval", newInterval, RegistryValueKind.DWord);
                                break;
                            case ExecutionLevel.User:
                                SetINIEntry("TextEffect", Convert.ToInt16(TXT_EFFECT).ToString(), "BOOT.ini");
                                SetINIEntry("TextEffectInterval", newInterval.ToString(), "BOOT.ini");
                                break;
                        }
                        STRT_PROPS.TextEffect = TXT_EFFECT;
                        STRT_PROPS.TextEffectInterval = newInterval;
                    }
                }
            }
            catch (InvalidOperationException) { }
        }

        private async void MakeValueHelper(string name, string value = null, RegistryValueKind valueKind = RegistryValueKind.String)
        {
            try
            {
                int result;
                if (valueKind == RegistryValueKind.Binary || valueKind == RegistryValueKind.QWord || valueKind == RegistryValueKind.DWord)
                {
                    if (int.TryParse(value, out result))
                    {
                        if (CURRENT_SKEY != CURRENT_HKEY.Name + "\\")
                        {
                            CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).SetValue(name, result, valueKind);
                        }
                        else
                        {
                            CURRENT_HKEY.SetValue(name, value, valueKind);
                        }
                    }
                    else
                    {
                        await TEXT_ENGINE.Write(NewLine + "Cannot assign string value to DWORD, QWORD and BINARY registry values!");
                    }
                }
                else
                {
                    CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).SetValue(name, value, valueKind);
                }
            }
            catch (InvalidOperationException) { }
        }

        private void HistoryScrollUpHelper()
        {
            try
            {
                if (REGEDIT_MODE)
                {
                    HIST.RegeditScrollUp();
                    if (HIST.RegeditHistory[HIST.RegeditIndex].Length > 0)
                    {
                        string newLine = null;
                        if (HIST.RegeditIndex == 1)
                        {
                            HIST.SetRegeditZeroEntry(IOField.Lines.Last().Replace(CURRENT_SKEY + "> ", ""));
                        }
                        if (IOField.Lines.Length > 1)
                        {
                            newLine = NewLine;
                        }
                        else
                        {
                            newLine = string.Empty;
                        }
                        IOField.Lines = IOField.Lines.Take(IOField.Lines.Length - 1).ToArray();
                        IOField.AppendText(newLine + CURRENT_SKEY + "> " + HIST.ReturnRegeditEntryAtIndex());
                    }
                }
                else
                {
                    HIST.DefaultScrollUp();
                    if (HIST.DefaultHistory[HIST.DefaultIndex].Length > 0)
                    {
                        string newLine = null;
                        if (HIST.DefaultIndex == 1)
                        {
                            HIST.SetDefaultZeroEntry(IOField.Lines.Last().Replace(CURRENT_DIR + "> ", ""));
                        }
                        if (IOField.Lines.Length > 1)
                        {
                            newLine = NewLine;
                        }
                        else
                        {
                            newLine = string.Empty;
                        }
                        IOField.Lines = IOField.Lines.Take(IOField.Lines.Length - 1).ToArray();
                        IOField.AppendText(newLine + CURRENT_DIR + "> " + HIST.ReturnDefaultEntryAtIndex());
                    }
                }
            }
            catch (InvalidOperationException) { }
        }

        private void HistoryScrollDownHelper()
        {
            try
            {
                if (REGEDIT_MODE)
                {
                    string newLine = null;
                    HIST.RegeditScrollDown();
                    if (HIST.RegeditHistory[HIST.RegeditIndex].Length > 0)
                    {
                        if (IOField.Lines.Length > 1)
                        {
                            newLine = NewLine;
                        }
                        else
                        {
                            newLine = string.Empty;
                        }
                        IOField.Lines = IOField.Lines.Take(IOField.Lines.Length - 1).ToArray();
                        IOField.AppendText(newLine + CURRENT_SKEY + "> " + HIST.ReturnRegeditEntryAtIndex());
                    }
                    else if (HIST.RegeditIndex == 0)
                    {
                        if (IOField.Lines.Length > 1)
                        {
                            newLine = NewLine;
                        }
                        else
                        {
                            newLine = string.Empty;
                        }
                        IOField.Lines = IOField.Lines.Take(IOField.Lines.Length - 1).ToArray();
                        IOField.AppendText(newLine + CURRENT_SKEY + "> " + HIST.RegeditHistory[0]);
                    }
                }
                else
                {
                    string newLine = null;
                    HIST.DefaultScrollDown();
                    if (HIST.DefaultHistory[HIST.DefaultIndex].Length > 0)
                    {
                        if (IOField.Lines.Length > 1)
                        {
                            newLine = NewLine;
                        }
                        else
                        {
                            newLine = string.Empty;
                        }
                        IOField.Lines = IOField.Lines.Take(IOField.Lines.Length - 1).ToArray();
                        IOField.AppendText(newLine + CURRENT_DIR + "> " + HIST.ReturnDefaultEntryAtIndex());
                    }
                    else if (HIST.DefaultIndex == 0)
                    {
                        if (IOField.Lines.Length > 1)
                        {
                            newLine = NewLine;
                        }
                        else
                        {
                            newLine = string.Empty;
                        }
                        IOField.Lines = IOField.Lines.Take(IOField.Lines.Length - 1).ToArray();
                        IOField.AppendText(newLine + CURRENT_DIR + "> " + HIST.DefaultHistory[0]);
                    }
                }
            }
            catch (InvalidOperationException) { }
        }

        private async void GetValueHelper(string input = null, bool filter = false, InterpreterParameters filterParams = null)
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
                        valueNames = CURRENT_HKEY.OpenSubKey(input, true).GetValueNames();
                        hiveKey = CURRENT_HKEY.OpenSubKey(input, true);
                    }
                }
                else
                {
                    if (CURRENT_SKEY == CURRENT_HKEY.Name + "\\")
                    {
                        valueNames = CURRENT_HKEY.GetValueNames();
                        hiveKey = CURRENT_HKEY;
                    }
                    else
                    {
                        valueNames = CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).GetValueNames();
                        hiveKey = CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true);
                    }
                }

                if (valueNames.Length > 0)
                {
                    if (filter)
                    {
                        InterpreterResult res = null;
                        for (int i = 0; i < valueNames.Length; i++)
                        {
                            res = new InterpreterResult(valueNames[i]);
                            res.GetResult(filterParams);
                            await TEXT_ENGINE.Write(string.Format("{0}   |||   {1}   |||   {2}   |||   {3}", NewLine, valueNames[i], hiveKey.GetValueKind(valueNames[i]), hiveKey.GetValue(valueNames[i])));
                        }
                    }
                    else
                    {
                        for (int i = 0; i < valueNames.Length; i++)
                        {
                            await TEXT_ENGINE.Write(string.Format("{0}   |||   {1}   |||   {2}   |||   {3}", NewLine, valueNames[i], hiveKey.GetValueKind(valueNames[i]), hiveKey.GetValue(valueNames[i])));
                        }
                    }
                }
                else
                {
                    IOField.AppendText(NewLine + "Values not found!");
                }

            }
            catch (InvalidOperationException) { }
            catch (Exception)
            {
                try
                {
                    await TEXT_ENGINE.Write(NewLine + "Subkey not found!");
                }
                catch (InvalidOperationException) { }
            }
        }

        private async Task<string[]> GetSubkeyHelper(string input)
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
                        skNames = CURRENT_HKEY.OpenSubKey(input, true).GetSubKeyNames();
                    }
                }
                else
                {
                    if (CURRENT_SKEY == CURRENT_HKEY.Name + "\\")
                    {
                        skNames = CURRENT_HKEY.GetSubKeyNames();
                    }
                    else
                    {
                        skNames = CURRENT_HKEY.OpenSubKey(CURRENT_SKEY.Replace(CURRENT_HKEY.Name + "\\", ""), true).GetSubKeyNames();
                    }
                }
                return skNames;
            }
            catch (InvalidOperationException) { return null; }
            catch (Exception)
            {
                try
                {
                    await TEXT_ENGINE.Write(NewLine + "Subkey not found!");
                    return null;
                }
                catch (InvalidOperationException) { return null; }
            }
        }

        private void EmulationHelper(bool turnOff = false, bool setStartup = false, bool setEmulation = true)
        {
            if (setEmulation)
            {
                if (turnOff)
                {
                    if (EMU)
                    {
                        Process.Start(@"C:\Windows\explorer.exe");
                        EMU = false;
                    }
                }
                else
                {
                    if (!EMU)
                    {
                        ProcessStartInfo taskKill = new ProcessStartInfo("taskkill", "/F /IM explorer.exe");
                        taskKill.WindowStyle = ProcessWindowStyle.Hidden;
                        Process p = new Process();
                        p.StartInfo = taskKill;
                        p.Start();
                        p.WaitForExit();
                        EMU = true;
                    }
                }
            }
            if (setStartup)
            {
                switch (STRT_IMG.ExecutionLevel)
                {
                    case ExecutionLevel.Administrator:
                        if (turnOff)
                        {
                            REG_KEY.SetValue("Emulation", 0, RegistryValueKind.DWord);
                            STRT_PROPS.StartupEmulation = false;
                        }
                        else
                        {
                            REG_KEY.SetValue("Emulation", 1, RegistryValueKind.DWord);
                            STRT_PROPS.StartupEmulation = true;
                        }

                        break;
                    case ExecutionLevel.User:
                        if (turnOff)
                        {
                            SetINIEntry("Emulation", "0", "BOOT.ini");
                            STRT_PROPS.StartupEmulation = false;
                        }
                        else
                        {
                            SetINIEntry("Emulation", "1", "BOOT.ini");
                            STRT_PROPS.StartupEmulation = true;
                        }
                        break;
                }
            }
        }

        private void FullscreenHelper(bool windowed = false, bool setStartup = false, bool setFullsceen = true)
        {
            if (setFullsceen)
            {
                if (windowed)
                {
                    FormBorderStyle = FormBorderStyle.Sizable;
                    Bounds = DEF_BOUNDS;
                    IOField.Size = new Size(Width + 20, Height - 39);
                    CenterToScreen();
                    FSCR = false;
                }
                else
                {
                    FormBorderStyle = FormBorderStyle.None;
                    Bounds = Screen.PrimaryScreen.Bounds;
                    IOField.Size = new Size(Width + 20, Height);
                    FSCR = true;
                }
            }
            if (setStartup)
            {
                switch (STRT_IMG.ExecutionLevel)
                {
                    case ExecutionLevel.Administrator:
                        if (windowed)
                        {
                            REG_KEY.SetValue("Fullscreen", 0, RegistryValueKind.DWord);
                            STRT_PROPS.StartupFullscreen = false;
                        }
                        else
                        {
                            REG_KEY.SetValue("Fullscreen", 1, RegistryValueKind.DWord);
                            STRT_PROPS.StartupFullscreen = true;
                        }

                        break;
                    case ExecutionLevel.User:
                        if (windowed)
                        {
                            SetINIEntry("Fullscreen", "0", "BOOT.ini");
                            STRT_PROPS.StartupFullscreen = false;
                        }
                        else
                        {
                            SetINIEntry("Fullscreen", "1", "BOOT.ini");
                            STRT_PROPS.StartupFullscreen = true;
                        }
                        break;
                }
            }
        }

        private async void SetfontHelper(string familyStr, bool setFont = true, bool setStartup = false)
        {
            try
            {
                bool fSizeFilter = false;
                bool fStyleFilter = false;
                int fontSize = (int)IOField.Font.Size;
                FontStyle fontStyle = IOField.Font.Style;
                Font f = null;
                FontFamily ff = null;
                if (familyStr == "CP437")
                {
                    ff = CP437.FontFamily;
                }
                else
                {
                    try
                    {
                        ff = new FontFamily(familyStr);
                    }
                    catch (Exception)
                    {
                        await TEXT_ENGINE.Write(NewLine + "Invalid font family!");
                    }
                }
                if (SETFONT.Args.Length >= 2)
                {
                    int result;
                    if (int.TryParse(SETFONT.Args[1].Arg, out result))
                    {
                        if (result > 0)
                        {
                            fontSize = result;
                        }
                        else
                        {
                            await TEXT_ENGINE.Write(NewLine + "Invalid font size!");
                        }
                        fSizeFilter = true;
                    }
                    else
                    {
                        switch (SETFONT.Args[1].Arg)
                        {
                            case "regular":
                                fontStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fontStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fontStyle = FontStyle.Bold;
                                break;
                            default:
                                if (SETFONT.Args[1].Arg != "-s" && SETFONT.Args[1].Arg != "-b")
                                {
                                    await TEXT_ENGINE.Write(NewLine + "Invalid font style!");
                                }
                                break;
                        }
                        fStyleFilter = true;
                    }
                }
                if (SETFONT.Args.Length >= 3)
                {
                    if (fSizeFilter)
                    {
                        switch (SETFONT.Args[2].Arg)
                        {
                            case "regular":
                                fontStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                fontStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                fontStyle = FontStyle.Bold;
                                break;
                            default:
                                if (SETFONT.Args[2].Arg != "-s" && SETFONT.Args[2].Arg != "-b")
                                {
                                    await TEXT_ENGINE.Write(NewLine + "Invalid font style!");
                                }
                                break;
                        }
                    }
                    else if (fStyleFilter)
                    {
                        int result;
                        if (int.TryParse(SETFONT.Args[2].Arg, out result))
                        {
                            if (result > 0)
                            {
                                fontSize = result;
                            }
                            else
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid font size!");
                            }
                        }
                        else
                        {
                            if (SETFONT.Args[2].Arg != "-s" && SETFONT.Args[2].Arg != "-b")
                            {
                                await TEXT_ENGINE.Write(NewLine + "Invalid font size!");
                            }
                        }
                    }
                }
                f = new Font(ff, fontSize, fontStyle);
                if (setFont)
                {
                    SYS_FONT = f;
                    IOField.Font = SYS_FONT;
                }
                if (setStartup)
                {
                    switch (STRT_IMG.ExecutionLevel)
                    {
                        case ExecutionLevel.Administrator:
                            REG_KEY.SetValue("FontFamily", familyStr);
                            REG_KEY.SetValue("FontStyle", fontStyle);
                            REG_KEY.SetValue("FontSize", fontSize, RegistryValueKind.DWord);
                            break;
                        case ExecutionLevel.User:
                            SetINIEntry("StartupSystemFont", familyStr + ">" + fontSize + ">" + fontStyle, "BOOT.ini");
                            break;
                    }
                    STRT_PROPS.StartupSystemFont = f;
                }
            }
            catch (InvalidOperationException) { }
        }

        private void SetINIEntry(string entryName, string entryValue, string pathStr)
        {
            if (File.Exists(pathStr))
            {
                StreamWriter sw = null;
                StringBuilder sb = new StringBuilder();
                string[] INILines = File.ReadAllText(pathStr).Split('|');
                bool entryFound = false;
                sw = new StreamWriter(pathStr);
                for (int i = 0; i < INILines.Length; i++)
                {
                    string[] lineSplit = INILines[i].Split('>');
                    if (lineSplit[0] == entryName)
                    {
                        INILines[i] = sb.Append(entryName + ">" + entryValue).ToString();
                        entryFound = true;
                    }
                    sw.Write(INILines[i]);
                    if (i + 1 < INILines.Length)
                    {
                        sw.Write("|");
                    }
                }
                sw.Close();
                if (!entryFound)
                {
                    sw = new StreamWriter(pathStr, true);
                    sw.Write('|' + entryName + ">" + entryValue);
                    sw.Close();
                }
            }
        }

        private void CaretBlocker(KeyEventArgs arg)
        {
            if (REGEDIT_MODE)
            {
                if (IOField.Lines.Last().Length - (IOField.Text.Length - IOField.SelectionStart) == CURRENT_SKEY.Length + 2)
                {
                    arg.SuppressKeyPress = true;
                }
            }
            else
            {
                if (IOField.Lines.Last().Length - (IOField.Text.Length - IOField.SelectionStart) == CURRENT_DIR.Length + 2)
                {
                    arg.SuppressKeyPress = true;
                }
            }
        }

        private int FindLastCharacterIndex(string inputStr, char charToFind)
        {
            int index = 0;
            for (int i = 0; i < inputStr.Length; i++)
            {
                if (inputStr[i] == charToFind)
                {
                    index = i;
                }
            }
            return index;
        }
        #endregion

        #region IO EVENTS
        private void IOField_MouseLeave(object sender, EventArgs e)
        {
            Cursor.Show();
        }

        private void IOField_MouseEnter(object sender, EventArgs e)
        {
            Cursor.Hide();
        }

        private void IOField_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
        {
            if (e.KeyCode == Keys.Tab)
            {
                if (!FILE_OPEN && WRITABLE)
                {
                    if (IOField.Lines.Last() != CURRENT_DIR + "> " && IOField.Lines.Last() != CURRENT_SKEY + "> ")
                    {
                        e.IsInputKey = true;
                        if (REGEDIT_MODE)
                        {
                            COMP.CompleteCommand(IOField.Lines.Last().Replace(CURRENT_SKEY + "> ", ""), true, IOField);
                        }
                        else
                        {
                            COMP.CompleteCommand(IOField.Lines.Last().Replace(CURRENT_DIR + "> ", ""), false, IOField);
                        }
                        SendKeys.Send("{BACKSPACE}");
                    }
                }
            }
        }

        private void IOField_MouseInput(object sender, MouseEventArgs e)
        {
            IOField.SelectionStart = IOField.Text.Length;
        }

        private async void IOField_KeyDown(object sender, KeyEventArgs e)
        {
            try
            {
                if (FILE_OPEN)
                {
                    switch (e.KeyCode)
                    {
                        case Keys.F2:
                            e.SuppressKeyPress = true;
                            File.WriteAllText(OPEN_PATH, IOField.Text);
                            IOField.Clear();
                            await TEXT_ENGINE.Write(CURRENT_DIR + "> ");
                            OPEN_PATH = null;
                            FILE_OPEN = false;
                            break;
                        case Keys.Escape:
                            e.SuppressKeyPress = true;
                            IOField.Clear();
                            await TEXT_ENGINE.Write(CURRENT_DIR + "> ");
                            OPEN_PATH = null;
                            FILE_OPEN = false;
                            break;
                    }
                }
                else
                {
                    string userInput = null;
                    string[] threadOutputBuffer = null;
                    Command cmd = null;
                    if (WRITABLE)
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Enter:
                                e.SuppressKeyPress = true;
                                try
                                {
                                    if (REGEDIT_MODE)
                                    {
                                        if (!IOField.Lines.Last().StartsWith(CURRENT_SKEY + "> "))
                                        {
                                            IOField.AppendText(NewLine + CURRENT_SKEY + "> ");
                                        }
                                        else
                                        {
                                            userInput = IOField.Lines.Last().Replace(CURRENT_SKEY + "> ", "");
                                        }

                                    }
                                    else
                                    {
                                        if (!IOField.Lines.Last().StartsWith(CURRENT_DIR + "> "))
                                        {
                                            IOField.AppendText(NewLine + CURRENT_DIR + "> ");
                                        }
                                        else
                                        {
                                            userInput = IOField.Lines.Last().Replace(CURRENT_DIR + "> ", "");
                                        }
                                    }
                                    if (userInput.Length > 0)
                                    {
                                        if (REGEDIT_MODE)
                                        {
                                            HIST.AddToRegeditHistory(userInput);
                                            HIST.RegeditIndex = 0;
                                        }
                                        else
                                        {
                                            HIST.AddToDefaultHistory(userInput);
                                            HIST.DefaultIndex = 0;
                                        }
                                        cmd = CMD_MGMT.GetCommandFromPool(userInput);
                                        if (CMD_MGMT.CommandFound)
                                        {
                                            try
                                            {
                                                CMD_MGMT.ExecuteCommand(cmd);
                                                if (SINGLE_OUT_BUFFER.OutputText != null)
                                                {
                                                    await TEXT_ENGINE.WriteOutputBuffer(SINGLE_OUT_BUFFER, WRITE_FILE);
                                                    SINGLE_OUT_BUFFER.ReplaceBuffer(null);
                                                }
                                                else if (MULTI_OUT_BUFFER.OutputLines != null)
                                                {
                                                    await TEXT_ENGINE.WriteOutputBuffer(MULTI_OUT_BUFFER, WRITE_FILE);
                                                    MULTI_OUT_BUFFER.ReplaceBuffer(null);
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                if (ex.Message != "Command to execute returend null!" && ex.Message != "Object reference not set to an instance of an object.")
                                                {
                                                    await TEXT_ENGINE.Write(NewLine + ex.Message);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            if (REGEDIT_MODE)
                                            {
                                                if (IOField.Lines.Length > 0)
                                                {
                                                    await TEXT_ENGINE.Write(NewLine);
                                                }
                                                await TEXT_ENGINE.Write(CURRENT_SKEY + "> ");
                                            }
                                            else
                                            {
                                                if (IOField.Lines.Length > 0)
                                                {
                                                    await TEXT_ENGINE.Write(NewLine);
                                                }
                                                await TEXT_ENGINE.Write(CURRENT_DIR + "> ");
                                            }
                                        }
                                    }
                                    else
                                    {
                                        if (REGEDIT_MODE)
                                        {
                                            if (IOField.Lines.Length > 0)
                                            {
                                                await TEXT_ENGINE.Write(NewLine);
                                            }
                                            await TEXT_ENGINE.Write(CURRENT_SKEY + "> ");
                                        }
                                        else
                                        {
                                            if (IOField.Lines.Length > 0)
                                            {
                                                await TEXT_ENGINE.Write(NewLine);
                                            }
                                            await TEXT_ENGINE.Write(CURRENT_DIR + "> ");
                                        }
                                    }
                                }
                                catch (NullReferenceException) { }
                                catch (Exception ex)
                                {
                                    if (ex.Message != "Command to execute returend null!")
                                    {
                                        await TEXT_ENGINE.Write(NewLine + ex.Message);
                                    }
                                    if (REGEDIT_MODE)
                                    {
                                        await TEXT_ENGINE.Write(NewLine + CURRENT_SKEY + "> ");
                                    }
                                    else
                                    {
                                        await TEXT_ENGINE.Write(NewLine + CURRENT_DIR + "> ");
                                    }
                                }
                                break;
                            case Keys.Left:
                                CaretBlocker(e);
                                break;
                            case Keys.Back:
                                CaretBlocker(e);
                                break;
                            case Keys.Up:
                                e.SuppressKeyPress = true;
                                HistoryScrollUpHelper();
                                break;
                            case Keys.Down:
                                e.SuppressKeyPress = true;
                                HistoryScrollDownHelper();
                                break;
                        }
                    }
                    else
                    {
                        switch (e.KeyCode)
                        {
                            case Keys.Enter:
                                e.SuppressKeyPress = true;
                                if (WORDGEN_RUNNING)
                                {
                                    ThreadProgress tProg = GetWordgenProgress(TASK_START_TIME, DateTime.Now - TASK_START_TIME, WGEN_COMBO_COUNT, WGEN_CURRENT_INDEX);
                                    threadOutputBuffer = new string[4];
                                    threadOutputBuffer[0] = "Start time: " + tProg.StartTime.ToString() + NewLine;
                                    threadOutputBuffer[1] = string.Format("Time elapsed: Time elapsed: {0:00}:{1:00}:{2:00}:{3:00}{4}", tProg.TimeElapsed.Days, tProg.TimeElapsed.Hours, tProg.TimeElapsed.Minutes, tProg.TimeElapsed.Seconds, NewLine);
                                    threadOutputBuffer[2] = string.Format("Time left: {0:00}:{1:00}:{2:00}:{3:00}{4}", tProg.TimeLeft.Days, tProg.TimeLeft.Hours, tProg.TimeLeft.Minutes, tProg.TimeLeft.Seconds, NewLine);
                                    threadOutputBuffer[3] = string.Format("Progress: {0:0.00}%{1}", tProg.Percentage, NewLine);
                                    await TEXT_ENGINE.WriteArray(threadOutputBuffer);
                                    IOField.AppendText(NewLine);
                                }
                                else if (HASHCRACK_RUNNING)
                                {
                                    ThreadProgress tProg = GetHashcrackProgress(TASK_START_TIME, DateTime.Now - TASK_START_TIME, HCRACK_PROCESSED, HCRACK_REMAINING);
                                    threadOutputBuffer = new string[4];
                                    threadOutputBuffer[0] = "Start time: " + tProg.StartTime.ToString() + NewLine;
                                    threadOutputBuffer[1] = string.Format("Time elapsed: {0:00}:{1:00}:{2:00}:{3:00}{4}", tProg.TimeElapsed.Days, tProg.TimeElapsed.Hours, tProg.TimeElapsed.Minutes, tProg.TimeElapsed.Seconds, NewLine);
                                    threadOutputBuffer[2] = "Words processed: " + tProg.ObjectsProcessed + NewLine;
                                    threadOutputBuffer[3] = "Words remaining: " + tProg.ObjectsRemaining + NewLine;
                                    await TEXT_ENGINE.WriteArray(threadOutputBuffer);
                                    IOField.AppendText(NewLine);
                                }
                                break;
                            case Keys.Escape:
                                if (NMAP_RUNNING)
                                {
                                    DeviceDiscovery.CancelDiscovery();
                                    NMAP_RUNNING = false;
                                }
                                CancelWorker();
                                await TextEngine.CancelWrite();
                                WRITABLE = true;
                                CT_SOURCE.Dispose();
                                CT_SOURCE = new CancellationTokenSource();
                                CT = CT_SOURCE.Token;
                                break;
                            default:
                                e.SuppressKeyPress = true;
                                break;
                        }
                    }
                }
            }
            catch (InvalidOperationException) { }
        }
        #endregion

        #region FORM EVENTS
        private async void MainForm_Shown(object sender, EventArgs e)
        {
            LoadCP437();
            IOField.Font = CP437;
            BOOTLOADER = new Bootloader(this);
            switch (GetExecutionLevel())
            {
                case ExecutionLevel.Administrator:
                    STRT_PROPS = BOOTLOADER.REG_SEQ();
                    break;
                case ExecutionLevel.User:
                    STRT_PROPS = BOOTLOADER.INI_SEQ();
                    break;
            }
            STRT_IMG = new BootImage(STRT_PROPS);
            LoadImage(STRT_IMG);
            if (STRT_PROPS.VerboseStartup)
            {
                await SimulateLoading();
            }
            if (STRT_IMG.StartupShowMOTD)
            {
                await TEXT_ENGINE.Write(STRT_IMG.StartupMOTD + NewLine, false);
            }
            await TEXT_ENGINE.Write(CURRENT_DIR + "> ", false);
            WRITABLE = true;
        }

        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            if (!FSCR)
            {
                IOField.Size = new Size(Width + 1, Height - 39);
            }
        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            try
            {
                await TEXT_ENGINE.Write("Closing WinDOS...", true);
                await ChangeSelectColour(Color.FromArgb(0, 120, 215), Color.FromArgb(255, 255, 255));
                if (EMU)
                {
                    Process.Start("C:\\Windows\\explorer.exe");
                }
                WRITABLE = false;
                if (NMAP_RUNNING)
                {
                    DeviceDiscovery.CancelDiscovery();
                    NMAP_RUNNING = false;
                }
                CancelWorker();
                await TextEngine.CancelWrite();
            }
            catch (InvalidOperationException) { }
        }
        #endregion

        #region HELPER FIELDS
        private DateTime TASK_START_TIME;

        private double WGEN_COMBO_COUNT;

        private double WGEN_CURRENT_INDEX;

        private double HCRACK_PROCESSED;

        private double HCRACK_REMAINING;

        public string OPEN_PATH;
        #endregion

        #region CLI STATUS FIELDS
        private bool FILE_OPEN;

        private bool NMAP_RUNNING;

        private bool HASHCRACK_RUNNING;

        private bool WORDGEN_RUNNING;

        private bool REGEDIT_MODE;

        private bool ERROR_OCCURRED;

        private bool WRITABLE;
        #endregion

        #region GLOBAL FIELDS
        public RegistryKey REG_KEY;

        private StringBuilder STR_BUILDER = null;

        private RegistryKey CURRENT_HKEY = Registry.CurrentUser;

        private CancellationTokenSource CT_SOURCE = new CancellationTokenSource();

        private CancellationToken CT;

        private bool WRITE_FILE;
        #endregion

        /*COMMANDS*/
        #region DEFAULT COMMANDS
        public static Command CD = new Command("cd", 1, 1, false, "CD ------------------------------ Changes current directory." +
            NewLine + "                                  Usage: CD [folder path]" +
            NewLine + "                                  Example: CD C:\\Users" +
            NewLine + "                                  Glob support: double quotes");


        public static Command DIR = new Command("dir", 0, 2, true, "DIR ----------------------------- Displays all file system entries in the specified directory that matches the filter if any." +
            NewLine + "                                  Usage: DIR {[folder path]} {[filter]}" +
            NewLine + "                                  Example: DIR C:\\ !\"C:\\Program \"*" +
            NewLine + "                                  Glob support: all");

        public static Command MD = new Command("md", 1, 1, false, "MD ------------------------------ Creates a new folder at the specified path." +
            NewLine + "                                  Usage: MD [folder path]" +
            NewLine + "                                  Example: MD C:\\Users\\John\\NewFolder" +
            NewLine + "                                  Glob support: double quotes");

        public static Command RD = new Command("rd", 1, 1, false, "RD ------------------------------ Removes the folder at the specified path." +
            NewLine + "                                  Usage: RD [folder path]" +
            NewLine + "                                  Example: RD C:\\Users\\John\\NewFolder" +
            NewLine + "                                  Glob support: double quotes");

        public static Command MF = new Command("mf", 1, 2, false, "MF ------------------------------ Creates a new file at the specified path." +
            NewLine + "                                  Usage: MF [file path]" +
            NewLine + "                                  Example: MF C:\\Users\\John\\NewFolder\\note.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command FC = new Command("fc", 1, 1, false, "FC ------------------------------ Display the read only content of the specified file. Maximum character count: 800.000" +
            NewLine + "                                  Usage: FC [file path]" +
            NewLine + "                                  Example: FC C:\\Users\\John\\NewFolder\\note.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command OPEN = new Command("open", 1, 1, false, "OPEN ---------------------------- Display the editable content of the specified file." +
            NewLine + "                                  Press ESC to discard changes and F2 to save them. Maximum character count: 800.000" +
            NewLine + "                                  Usage: OPEN [file path]" +
            NewLine + "                                  Example: OPEN C:\\Users\\John\\NewFolder\\note.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command COPY = new Command("copy", 2, 3, false, "COPY ---------------------------- Copies the specified file system entry to the specified destination." +
            NewLine + "                                  Args: If the file already exists in the destionation folder, it is overwritten." +
            NewLine + "                                  Usage: COPY [source file/folder path] [destination folder path] {-o}" +
            NewLine + "                                  Example: COPY C:\\Users\\John\\NewFolder\\note.txt C:\\Windows\\note.txt -o" +
            NewLine + "                                  Glob support: double quotes");

        public static Command MOVE = new Command("move", 2, 2, false, "MOVE ---------------------------- Moves the specified file system entry to the specified destination." +
            NewLine + "                                  If the file/folder already exists at the destinatin, it is overwritten." +
            NewLine + "                                  Usage: MOVE [file/folder path] [folder path]" +
            NewLine + "                                  Example: MOVE C:\\Users\\John\\NewFolder\\note.txt C:\\Windows\\note.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command DEL = new Command("del", 1, 1, false, "DEL ----------------------------- Removes the specified file at the specified destination." +
            NewLine + "                                  Usage: DEL [file path]" +
            NewLine + "                                  Example: DEL C:\\Users\\John\\NewFolder\\note.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command FSEINF = new Command("fseinf", 1, 1, false, "FSEINF -------------------------- Returns information about the specified file system entry." +
            NewLine + "                                  Usage: FSEINF [file system entry path]" +
            NewLine + "                                  Example: FSEINF C:\\Users\\John\\NewFolder\\note.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command RENAME = new Command("rename", 2, 2, false, "RENAME -------------------------- Renames the specified file system entry." +
            NewLine + "                                  Usage: RENAME [file/folder path] [file/folder name]" +
            NewLine + "                                  Example: RENAME C:\\Users\\John\\NewFolder\\note.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command FORECOLOR = new Command("forecolor", 1, 2, false, "FORECOLOR ----------------------- Sets the foreground color." +
            NewLine + "                                  Args: -s sets startup foreground color; -b sets current AND startup foregfround color." +
            NewLine + "                                  Usage: FORECOLOR [color name] {-s/-b}" +
            NewLine + "                                  Example: FORECOLOR limegreen -b" +
            NewLine + "                                  Glob support: double quotes");

        public static Command BACKCOLOR = new Command("backcolor", 1, 2, false, "BACKCOLOR ----------------------- Sets the background color." +
            NewLine + "                                  Args: -s sets startup background color; -b sets current AND startup background color." +
            NewLine + "                                  Usage: BACKCOLOR [color name] {-s/-b}" +
            NewLine + "                                  Example: BACKCOLOR limegreen -b" +
            NewLine + "                                  Glob support: double quotes");

        public static Command SETFONT = new Command("setfont", 1, 4, false, "SETFONT ------------------------- Sets the system font." +
            NewLine + "                                  Args: -s sets startup system font; -b sets current AND startup system font." +
            NewLine + "                                  Usage: SETFONT [family name(default character set is CP437)] {[font size]} <=> {[font style(regular/bold/italic)]} {-s/-b}" +
            NewLine + "                                  Example: SETFONT \"Courier New\" regular 12 -b" +
            NewLine + "                                  Glob support: double quotes");

        public static Command SYSFONT_FAMILY = new Command("sysfont-family", 1, 2, false, "SYSFONT-FAMILY ------------------- Sets system font family." + 
            NewLine + "                                  Args: -s sets startup system font family; -b sets current AND startup system font family." +
            NewLine + "                                  Usage: SYSFONT-FAMILY [family name] {-s/-b}" +
            NewLine + "                                  Example: SYSFONT-FAMILY CP437 -b" +
            NewLine + "                                  Glob support: double quotes");

        public static Command SYSFONT_SIZE = new Command("sysfont-size", 1, 2, false, "SYSFONT-SIZE --------------------- Sets system font size." +
            NewLine + "                                  Args: -s sets startup system font size; -b sets current AND startup system font size." +
            NewLine + "                                  Usage: SYSFONT-SIZE [size] {-s/-b}" +
            NewLine + "                                  Example: SYSFONT-SIZE 12 -s" +
            NewLine + "                                  Glob support: none");

        public static Command SYSFONT_STYLE = new Command("sysfont-style", 1, 2, false, "SYSFONT-STYLE -------------------- Sets system font style." +
            NewLine + "                                  Args: -s sets startup system font style; -b sets current AND startup system font style." +
            NewLine + "                                  Usage: SYSFONT-STYLE [regular/bold/italic] {-s/-b}" +
            NewLine + "                                  Example: SYSFONT-STYLE bold -s" +
            NewLine + "                                  Glob support: none");

        public static Command FULLSCREEN = new Command("fullscreen", 0, 2, false, "FULLSCREEN ---------------------- Sets the application's fullscreen mode." +
            NewLine + "                                  Args: -s sets startup fullscreen mode; -b sets current AND startup fullscreen mode; -w turns off fullscreen mode." +
            NewLine + "                                  Usage: FULLSCREEN {-w} <=> {-s/-b}" +
            NewLine + "                                  Example: FULLSCREEN -w -s" +
            NewLine + "                                  Glob support: none");

        public static Command EMULATION = new Command("emulation", 0, 2, false, "EMULATION ----------------------- Kills/starts explorer.exe." +
            NewLine + "                                  Args: -s sets startup emulation; -b sets current AND startup emulation; -n turns off emulation." +
            NewLine + "                                  Usage: EMULATION {-n} <=> {-s/-b}" +
            NewLine + "                                  Example: EMULATION -n -s" +
            NewLine + "                                  Glob support: none");

        public static Command TEXTEFFECT = new Command("texteffect", 0, 3, false, "TEXTEFFECT ---------------------- Sets output text effect." +
            NewLine + "                                  Args: -s sets startup text effect; -b sets current AND startup text effect; -n turns off text effect." +
            NewLine + "                                  Usage: TEXTEFFECT {-n} <=> {-s/-b} {[effect delay in milliseconds]}" +
            NewLine + "                                  Example: TEXTEFFECT -s 10" +
            NewLine + "                                  Glob support: none");

        public static Command REGEDIT = new Command("regedit", 0, 0, false, "REGEDIT ------------------------- Elevates the command line to \"regedit\" mode. This allows registry browsing/editing. Requires administrator execution level!");

        public static Command WORDGEN = new Command("wordgen", 2, 3, false, "WORDGEN ------------------------- Generates all possible combinatons of the specified charactes at he specified length and writes them to the specified file." +
            NewLine + "                                  Usage: WORDGEN [characters(duplicate characters will be removed)] [word length] {[output file path]}" +
            NewLine + "                                  Example: WORDGEN asdfghj 6 wordgen.txt" +
            NewLine + "                                  Glob support: double quotes");

        public static Command HASHCRACK = new Command("hashcrack", 2, 2, false, "HASHCRACK ----------------------- Attempts to revert the original text of the specified hash from the specified word list." +
            NewLine + "                                  Supported hash types: SHA1; SHA256; SHA384; SHA512." +
            NewLine + "                                  Usage: HASHCRACK [word list path] [hash]" +
            NewLine + "                                  Example: HASHCRACK wordgen.txt 20A90DD8E99D50533DAA12C79228A186B2CA2860" +
            NewLine + "                                  Glob support: double quotes");

        public static Command MH = new Command("mh", 2, 2, false, "MH ------------------------------ Convert the specified text to the specified hash type." +
            NewLine + "                                  Usage: MH [text] [hashType(-sha1/-sha256/-sha384/-sha512)]" +
            NewLine + "                                  Example: MH asdfghj -sha512" +
            NewLine + "                                  Glob support: double quotes");

        public static Command BIOSINFO = new Command("biosinfo", 0, 0, false, "BIOSINFO ------------------------ Displays BIOS information.");

        public static Command DRIVER_NAMES = new Command("driver-names", 0, 1, false, "DRIVER-NAMES -------------------- Returns all driver's names that matches the filter if any." +
            NewLine + "                                  Usage: DRIVER-NAMES {[filter]}" +
            NewLine + "                                  Example: DRIVER-NAMES !\"Microsoft \"*" +
            NewLine + "                                  Glob support: all");

        public static Command DRIVER_INFO = new Command("driver-info", 1, 1, false, "DRIVER-INFO --------------------- Returns all driver information of the s, pecified driver." +
            NewLine + "                                  Usage: DRIVER-INFO [driver name]" +
            NewLine + "                                  Example: DRIVER-INFO UEFI" +
            NewLine + "                                  Glob support: double quotes");

        public static Command DRIVER_CHECK = new Command("driver-check", 0, 0, false, "DRIVER-CHECK -------------------- Checks for driver issues and displays them if any.");

        public static Command GETNICS = new Command("getnics", 0, 2, false, "GETNICS ------------------------- Returns network interface information." +
            NewLine + "                                  Args: -a returns all network interface information." +
            NewLine + "                                  Usage: GETNICS {-a}" +
            NewLine + "                                  Example: GETNICS -a" +
            NewLine + "                                  Glob support: none");

        public static Command PING = new Command("ping", 1, 2, false, "PING ---------------------------- Sends ICMP packets and displays the result." +
            NewLine + "                                  Args: -t sends ICMP packets until the user terminates the process." +
            NewLine + "                                  Usage: PING {-t}" +
            NewLine + "                                  Example: PING -t" +
            NewLine + "                                  Glob support: none");

        public static Command ARP = new Command("arp", 3, 3, false, "ARP ----------------------------- Sends ARP packet from the specified source to the specified destination." + 
            NewLine + "                                  Usage: ARP [source IP] [destination IP] [destionation MAC Address]" + 
            NewLine + "                                  Example: ARP 192.168.1.10 192.168.1.1 A1:b2:C3:d4:E5:f6" +
            NewLine + "                                  Glob support: none");

        public static Command NMAP = new Command("nmap", 0, 0, false, "NAMP ---------------------------- Attempts to discover all devices(hostname, IP address, MAC address) on all connected networks.");

        public static Command SETMAC = new Command("setmac", 2, 2, false, "SETMAC -------------------------- Attempts to set the MAC address of the specified network interface." +
            NewLine + "                                  Usage: SETMAC [interface name] [MAC address]" +
            NewLine + "                                  Example: SETMAC Ethernet A1b2C3d4E5f6" +
            NewLine + "                                  Glob support: none");

        public static Command RESETMAC = new Command("resetmac", 1, 1, false, "RESETMAC ------------------------ Attempts to reset the MAC address of the specified network interface." +
            NewLine + "                                  Usage: RESETMAC [interface name]" +
            NewLine + "                                  Example: RESETMAC Ethernet" +
            NewLine + "                                  Glob support: none");

        public static Command STARTUP_CONFIG = new Command("startup-config", 0, 0, false, "STARTUP-CONFIG ------------------ Displays startup system configuration.");

        public static Command COPY_CURRENT_CONFIG = new Command("copy-current-config", 1, 1, false, "COPY-CURRENT-CONFIG ------------- Copies the current system configuration to the specified boot source location." +
            NewLine + "                                  Args: -reg(registry)/-ini(BOOT.ini). Saving to registry requires administrator execution level." +
            NewLine + "                                  Usage: COPY-CURRENT-CONFIG [-reg/-ini]" +
            NewLine + "                                  Example: COPY-CURRENT-CONFIG [-reg]" +
            NewLine + "                                  Glob support: none");

        public static Command ERASE_STARTUP_CONFIG = new Command("erase-startup-config", 1, 1, false, "ERASE-STARTUP-CONFIG ------------ Erases the startup system configuration at the specified boot source location." +
            NewLine + "                                  Args: -reg(registry)/-ini(BOOT.ini). Erasing registry configuration requires administrator execution level." +
            NewLine + "                                  Usage: ERASE-STARTUP-CONFIG [-reg/-ini]" +
            NewLine + "                                  Example: ERASE-STARTUP-CONFIG [-reg]" +
            NewLine + "                                  Glob support: none");

        public static Command STARTUP_DIR = new Command("startup-dir", 1, 2, false, "STARTUP-DIR --------------------- Sets startup directory." +
            NewLine + "                                  Args: -b sets current AND startup directory" +
            NewLine + "                                  Usage: STARTUP-DIR [folder path] {-b}" +
            NewLine + "                                  Example: STARTUP-DIR C:\\Windows -b" +
            NewLine + "                                  Glob support: double quotes");

        public static Command STARTUP_SHMOTD = new Command("startup-shmotd", 0, 1, false, "STARTUP-SHMOTD ------------------ Shows/hides message of the day text at startup." +
            NewLine + "                                  Args: -n hides MOTD at startup." +
            NewLine + "                                  Usage: STARTUP-SHMOTD {-n}" +
            NewLine + "                                  Example: STARTUP-SHMOTD -n" +
            NewLine + "                                  Glob support: double quotes");

        public static Command STARTUP_MOTD = new Command("startup-motd", 1, 1, false, "STARTUP-MOTD -------------------- Sets message of the day text." +
            NewLine + "                                  Usage: STARTUP-MOTD [MOTD text]" +
            NewLine + "                                  Example: STARTUP-MOTD Hello there!" +
            NewLine + "                                  Glob support: double quotes");

        public static Command STARTUP_SUBKEY = new Command("startup-subkey", 1, 1, false, "STARTUP-SUBKEY ------------------ Sets startup registry subkey of \"regedit\" mode." +
            NewLine + "                                  Usage: STARTUP-SUBKEY [registry subkey]" +
            NewLine + "                                  Example: STARTUP-SUBKEY HKEY_LOCAL_MACHINE\\SYSTEM" +
            NewLine + "                                  Glob support: double quotes");

        public static Command STARTUP_VERBOSE = new Command("startup-verbose", 0, 1, false, "STARTUP-VERBOSE ----------------- Disables/enables loading screen at startup." +
            NewLine + "                                  Args: -n disables verbose startup." +
            NewLine + "                                  Usage: STARTUP-VERBOSE {-n}" +
            NewLine + "                                  Example: STARTUP-VERBOSE -n" +
            NewLine + "                                  Glob support: none");

        public static Command EXEC = new Command("exec", 1, 1, false, "EXEC ---------------------------- Executes commands from the specified .doscmd file." +
            NewLine + "                                  Unsupported commands: WORDGEN; HASHCRACK; NMAP; PING -t; EXEC." +
            NewLine + "                                  Usage: EXEC [file path]" +
            NewLine + "                                  Example: EXEC C:\\Path\\to\\file.doscmd" +
            NewLine + "                                  Glob support: double quote");

        public static Command START = new Command("start", 1, 1, false, "START --------------------------- Attempt to starts the specified application." +
            NewLine + "                                  Usage: START [file path]" +
            NewLine + "                                  Example: START C:\\Path\\to\\file.exe" +
            NewLine + "                                  Glob support: double quote");

        public static Command RESTART = new Command("restart", 0, 0, false, "RESTART ------------------------- Restarts WinDOS.", function: () => { Application.Restart(); });
        #endregion

        #region UNIVERSAL COMMANDS
        public static Command HISTORY_SETSIZE = new Command("history-setsize", 1, 2, false, "HISTORY-SETSIZE ----------------- Sets the history size of the current command line mode (default/regedit)." +
            NewLine + "                                  Args: -s sets startup history size" +
            NewLine + "                                  Usage: HISTORY-SETSIZE [new history size] {-n}" +
            NewLine + "                                  Example: HISTORY-SETSIZE 100 -n" +
            NewLine + "                                  Glob support: none");

        public static Command HISTORY_GETENTRY = new Command("history-getentry", 1, 1, false, "HISTORY-GETENTRY ---------------- Returns the specified history entry of the current command line mode (default/regedit)." +
            NewLine + "                                  Usage: HISTORY-GETENTRY {[entry index/!entry index]}" +
            NewLine + "                                  If \"!\" is used, entry search starts from the last entry in history, otherwise the search starts from the first." +
            NewLine + "                                  Example: HISTORY-GETENTRY !10" +
            NewLine + "                                  Glob support: none");

        public static Command HISTORY_CLEAR = new Command("history-clear", 0, 0, false, "HISTORY-CLEAR ------------------- Clears the history of current command line mode (default/regedit).");

        public static Command HISTORY_SHLIST = new Command("history-shlist", 0, 1, true, "HISTORY-SHLIST ------------------ Returns the current CLI mode's command history." +
            NewLine + "                                  Usage: HISTORY-SHLIST {[filter]}" +
            NewLine + "                                  Example: HISTORY-SHLIST !history-*" +
            NewLine + "                                  Glob support: all");


        public static Command CLS = new Command("cls", 0, 0, false, "CLS ----------------------------- Clears the screen.");

        public static Command HELP = new Command("help", 0, 1, false, "HELP ---------------------------- Displays all information about all commands." +
            NewLine + "                                  Usage: HELP {[filter]}" +
            NewLine + "                                  Example: HELP startup-*" +
            NewLine + "                                  Glob support: all");

        public static Command EXIT = new Command("exit", 0, 0, false, "EXIT ---------------------------- Closes WinDOS.");
        #endregion

        #region REGEDIT COMMANDS
        public static Command CSK = new Command("csk", 1, 1, false, "CSK ----------------------------- Changes current subkey." +
            NewLine + "                                  Usage: CSK [subkey path]" +
            NewLine + "                                  Example: CSK HKEY_LOCAL_MACHINE\\SYSTEM" +
            NewLine + "                                  Glob support: double quotes");

        public static Command SK_GET = new Command("sk-get", 0, 2, true, "SK-GET -------------------------- Returns all subkey in the specified subkey that matches the filter if any." +
            NewLine + "                                  Usage: SK-GET {[subkey path]} {[filter]}" +
            NewLine + "                                  Example: SK-GET HKEY_LOCAL_MACHINE\\SYSTEM !HKEY_LOCAL_MACHINE\\SYSTEM*" +
            NewLine + "                                  Glob support: all");

        public static Command SK_MAKE = new Command("sk-make", 1, 1, false, "SK-MAKE ------------------------- Creates a new subkey in the specified subkey." +
            NewLine + "                                  Usage: SK-MAKE [subkey path]" +
            NewLine + "                                  Example: SK-MAKE HKEY_LOCAL_MACHINE\\SOFTWARE\\NewSubkey" +
            NewLine + "                                  Glob support: double quotes");

        public static Command SK_DELETE = new Command("sk-delete", 1, 1, false, "SK-DELETE ----------------------- Deletes the specified subkey." +
            NewLine + "                                  Usage: SK-DELETE [subkey path]" +
            NewLine + "                                  Example: SK-DELETE HKEY_LOCAL_MACHINE\\SOFTWARE\\NewSubkey" +
            NewLine + "                                  Glob support: double quotes");

        public static Command VAL_GET = new Command("val-get", 0, 2, true, "VAL-GET ------------------------- Returns all values in the current subkey that matches the filter if any." +
            NewLine + "                                  Usage: VAL-GET {[subkey path]} {[filter]}" +
            NewLine + "                                  Example: VAL-GET HKEY_LOCAL_MACHINE\\SOFTWARE\\NewSubkey !NewVal*" +
            NewLine + "                                  Glob support: double quotes");

        public static Command VAL_SET = new Command("val-set", 2, 2, false, "VAL-SET ------------------------- Modifies an existing value." +
            NewLine + "                                  Usage: VAL-SET [value name] [value]" +
            NewLine + "                                  Example: VAL-SET NewValue 12" +
            NewLine + "                                  Glob support: double quotes");

        public static Command VAL_DELETE = new Command("val-delete", 1, 1, false, "VAL-DELETE ---------------------- Modifies an existing value." +
            NewLine + "                                  Usage: VAL-DELETE [value name]" +
            NewLine + "                                  Example: VAL-DELETE NewValue" +
            NewLine + "                                  Glob support: double quotes");

        public static Command VAL_MAKE = new Command("val-make", 1, 3, false, "VAL-MAKE ------------------------ Modifies an existing value." +
            NewLine + "                                  Args: -bin binary; -dw dword; -es expandstring; -ms multistring; -n none; -qw qword; -s string; -u unknown." +
            NewLine + "                                  Usage: VAL-MAKE [value name] {[value]} {value kind(-bin/-dw/-es/-ms/-n/-qw/-s/-u)}" +
            NewLine + "                                  Example: VAL-MAKE NewValue 12 -dw" +
            NewLine + "                                  Glob support: double quotes");

        public static Command VAL_RENAME = new Command("val-rename", 2, 2, false, "VAL-RENAME ---------------------- Modifies an existing value." +
            NewLine + "                                  Usage: VAL-RENAME [value name] [new value name]" +
            NewLine + "                                  Example: VAL-RENAME ValueToRename NewName" +
            NewLine + "                                  Glob support: double quotes");
        #endregion
    }

    public struct MultilineOutputBuffer
    {
        public string[] OutputLines { get; private set; }

        public bool GenerateEffect { get; }

        public MultilineOutputBuffer(string[] outLines, bool genEffect)
        {
            OutputLines = outLines;
            GenerateEffect = genEffect;
        }

        public void AddLine(string text)
        {
            List<string> mod = new List<string>();
            if (OutputLines != null)
            {
               mod = OutputLines.ToList();
            }
            mod.Add(text);
            OutputLines = mod.ToArray();
        }

        public void ReplaceBuffer(string[] outLines)
        {
            OutputLines = outLines;
        }
    }

    public struct SingleLineOutputBuffer
    {
        public string OutputText { get; private set; }

        public bool GenerateEffect { get; }

        public SingleLineOutputBuffer(string outText, bool genEffect)
        {
            OutputText = outText;
            GenerateEffect = genEffect;
        }

        public void ReplaceBuffer(string outText)
        {
            OutputText = outText;
        }
    }

    public struct TextEngine
    {
        public TextEngine(int alter, bool generateEffect, TextBox io)
        {
            CTS = new CancellationTokenSource();
            CT = CTS.Token;
            GEN_EFFECT = generateEffect;
            RAND_GEN = new Random();
            STR_BUILDER = null;
            STR_LENGTH = 0;
            FILE_PATH = "";
            EFFECT_OUTPUT = io;
            ALTER_INTERVAL = alter;
            char[] cp = { '!', '\"', '#', '$', '%', '&', '\'', '(', ')', '*', '+', '`', '-', '.', '/', '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', ':', ';', '<', '=', '>', '?', '@', 'A', 'B', 'C', 'D', 'E', 'F', 'G', 'H', 'I', 'J', 'K', 'L', 'M', 'N', 'O', 'P', 'Q', 'R', 'S', 'T', 'U', 'V', 'W', 'X', 'Y', 'Z', '[', '\\', ']', '^', '_', 'a', 'b', 'c', 'd', 'e', 'f', 'g', 'h', 'i', 'j', 'k', 'l', 'm', 'n', 'o', 'p', 'q', 'r', 's', 't', 'u', 'v', 'w', 'x', 'y', 'z', '{', '|', '}', '~' };
            CHAR_POOL = cp;
        }

        #region FIELDS
        private static CancellationToken CT;

        private static CancellationTokenSource CTS;

        public TextBox EFFECT_OUTPUT { get; }

        public bool GEN_EFFECT { get; private set; }

        public int ALTER_INTERVAL { get; }

        private char[] CHAR_POOL { get; }

        private StringBuilder STR_BUILDER { get; set; }

        private int STR_LENGTH { get; set; }

        private Random RAND_GEN { get; set; }

        private string FILE_PATH { get; set; }
        #endregion

        #region PRIVATE METHODS
        private async Task EFFECT_GENERATOR(string inputStr, bool startWithNewLine)
        {
            if (startWithNewLine)
            {
                EFFECT_OUTPUT.AppendText(NewLine);
            }

            STR_LENGTH = inputStr.Length;
            if (ALTER_INTERVAL > 10)
            {
                for (int i = 0; i < STR_LENGTH; i++)
                {
                    EFFECT_OUTPUT.AppendText(inputStr[i].ToString());
                    EFFECT_OUTPUT.Update();
                    await Task.Delay(ALTER_INTERVAL);
                }
            }
            else
            {
                STR_BUILDER = new StringBuilder();
                for (int i = 0; i < STR_LENGTH; i++)
                {
                    EFFECT_OUTPUT.AppendText(CHAR_POOL[RAND_GEN.Next(0, CHAR_POOL.Length)].ToString());
                }
                for (int i = 0; i < STR_LENGTH; i++)
                {
                    STR_BUILDER.Clear();
                    for (int j = 0; j < STR_LENGTH - (i + 1); j++)
                    {
                        STR_BUILDER.Append(CHAR_POOL[RAND_GEN.Next(0, CHAR_POOL.Length)]);
                    }
                    STR_BUILDER.Insert(0, inputStr.Substring(0, i + 1));
                    EFFECT_OUTPUT.Select(EFFECT_OUTPUT.Text.Length - EFFECT_OUTPUT.Lines.Last().Length, EFFECT_OUTPUT.Lines.Last().Length);
                    EFFECT_OUTPUT.SelectedText = STR_BUILDER.ToString();
                    EFFECT_OUTPUT.Update();
                    await Task.Delay(ALTER_INTERVAL);
                }
                EFFECT_OUTPUT.SelectionStart = EFFECT_OUTPUT.Text.Length;
                EFFECT_OUTPUT.ScrollToCaret();
            }
        }
        #endregion

        #region PUBLIC METHODS
        public async Task Write(string textToWrite, bool clearBeforeWrite = false)
        {
            if (!CTS.IsCancellationRequested)
            {
                if (clearBeforeWrite)
                {
                    EFFECT_OUTPUT.Clear();
                }

                if (GEN_EFFECT)
                {
                    if (textToWrite.StartsWith(NewLine))
                    {
                        textToWrite = textToWrite.Replace(NewLine, "");
                        await EFFECT_GENERATOR(textToWrite, true);
                    }
                    else if (textToWrite.EndsWith(NewLine))
                    {
                        textToWrite = textToWrite.Replace(NewLine, "");
                        await EFFECT_GENERATOR(textToWrite, false);
                        EFFECT_OUTPUT.AppendText(NewLine);
                    }
                    else
                    {
                        await EFFECT_GENERATOR(textToWrite, false);
                    }
                }
                else
                {
                    EFFECT_OUTPUT.AppendText(textToWrite);
                }
            }
        }

        public async Task WriteArray(string[] arrayToWrite, bool clearBeforeWrite = false)
        {
            if (clearBeforeWrite)
            {
                EFFECT_OUTPUT.Clear();
            }

            for (int i = 0; i < arrayToWrite.Length; i++)
            {
                if (!CT.IsCancellationRequested)
                {
                    await Write(arrayToWrite[i]);
                }
                else
                {
                    CTS.Dispose();
                    CTS = new CancellationTokenSource();
                    CT = CTS.Token;
                    break;
                }
            }
        }

        public async Task WriteOutputBuffer(MultilineOutputBuffer buffer, bool writeFile)
        {
            if (writeFile)
            {
                File.WriteAllLines(FILE_PATH, buffer.OutputLines);
            }
            else
            {
                await WriteArray(buffer.OutputLines, false);
            }
        }

        public async Task WriteOutputBuffer(SingleLineOutputBuffer buffer, bool writeFile)
        {
            if (writeFile)
            {
                File.WriteAllText(FILE_PATH, buffer.OutputText);
            }
            else
            {
                await Write(buffer.OutputText, false);
            }
        }

        public async static Task CancelWrite()
        {
            CTS.Cancel();
            await Task.Delay(1000);
            CTS.Dispose();
            CTS = new CancellationTokenSource();
            CT = CTS.Token;
        }
        #endregion
    }

    public struct Bootloader
    {
        public Bootloader(MainForm form)
        {
            LOADER_FORM = form;
        }

        #region BOOTLOADER COMPONENTS
        public static MainForm LOADER_FORM { get; private set; }

        public enum ExecutionLevel { Administrator, User };

        public class BootImage
        {
            public ExecutionLevel ExecutionLevel { get; }

            public string StartupDirectory { get; }

            public Color StartupForegroundColor { get; }

            public Color StartupBackgroundColor { get; }

            public Font StartupSystemFont { get; }

            public FontFamily SystemFontFamily { get; }

            public int SystemFontSize { get; }

            public FontStyle SystemFontStyle { get; }

            public bool StartupFullscreen { get; }

            public bool StartupShowMOTD { get; }

            public string StartupMOTD { get; }

            public bool StartupEmulation { get; }

            public bool VerboseStartup { get; }

            public string StartupSubkey { get; }

            public bool OutputEffect { get; }

            public int OutputEffectInterval { get; }

            public int BasicHistorySize { get; }

            public int RegeditHistorySize { get; }

            public BootImage(BootImageProperties props)
            {
                ExecutionLevel = props.ExecutionLevel;
                StartupDirectory = props.StartupDirectory;
                StartupForegroundColor = props.StartupForegroundColor;
                StartupBackgroundColor = props.StartupBackgroundColor;
                StartupSystemFont = props.StartupSystemFont;
                SystemFontFamily = props.SystemFontFamily;
                SystemFontSize = props.SystemFontSize;
                SystemFontStyle = props.SystemFontStyle;
                StartupFullscreen = props.StartupFullscreen;
                StartupShowMOTD = props.StartupShowMOTD;
                StartupMOTD = props.StartupMOTD;
                StartupEmulation = props.StartupEmulation;
                VerboseStartup = props.VerboseStartup;
                StartupSubkey = props.StartupSubkey;
                OutputEffect = props.TextEffect;
                OutputEffectInterval = props.TextEffectInterval;
                BasicHistorySize = props.DefaultHistorySize;
                RegeditHistorySize = props.RegeditHistorySize;
            }
        }

        public class BootImageProperties
        {
            public ExecutionLevel ExecutionLevel { get; }

            public string StartupDirectory { get; set; }

            public Color StartupForegroundColor { get; set; }

            public Color StartupBackgroundColor { get; set; }

            public Font StartupSystemFont { get; set; }

            public FontFamily SystemFontFamily { get; set; }

            public int SystemFontSize { get; set; }

            public FontStyle SystemFontStyle { get; set; }

            public bool StartupFullscreen { get; set; }

            public bool StartupShowMOTD { get; set; }

            public string StartupMOTD { get; set; }

            public bool StartupEmulation { get; set; }

            public bool VerboseStartup { get; set; }

            public string StartupSubkey { get; set; }

            public bool TextEffect { get; set; }

            public int TextEffectInterval { get; set; }

            public int DefaultHistorySize { get; set; }

            public int RegeditHistorySize { get; set; }

            public BootImageProperties(string stdir = "C:\\", Color? stfc = null, Color? stbc = null, FontFamily sysfam = null, int syssize = 16, FontStyle sysstlye = FontStyle.Regular, bool stfu = true, bool outeffect = false, int outeffectInterval = 1, int bhistSize = 50, int rhistSize = 50, bool stshmotd = true, string motd = @"Type ""help"" for more information!", bool emu = false, string stsk = "HKEY_CURRENT_USER\\")
            {
                ExecutionLevel = GetExecutionLevel();
                StartupDirectory = stdir;
                StartupForegroundColor = stfc.GetValueOrDefault(Color.Silver);
                StartupBackgroundColor = stfc.GetValueOrDefault(Color.Black);
                if (sysfam == null)
                {
                    LoadCP437();
                    SystemFontFamily = CP437.FontFamily;
                }
                else
                {
                    SystemFontFamily = sysfam;
                }
                if (syssize == 0)
                {
                    SystemFontSize = 16;
                }
                else
                {
                    SystemFontSize = syssize;
                }
                SystemFontStyle = sysstlye;
                StartupSystemFont = new Font(SystemFontFamily, SystemFontSize, SystemFontStyle);
                StartupFullscreen = stfu;
                StartupShowMOTD = stshmotd;
                TextEffect = outeffect;
                TextEffectInterval = outeffectInterval;
                DefaultHistorySize = bhistSize;
                RegeditHistorySize = rhistSize;
                StartupMOTD = motd;
                StartupEmulation = emu;
                VerboseStartup = GetVerboseStartup(ExecutionLevel);
                StartupSubkey = stsk;
            }
        }
        #endregion

        #region DEFAULT FONT LOADER
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);

        private static PrivateFontCollection fonts = new PrivateFontCollection();

        public static Font CP437;

        public static void LoadCP437()
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
        #endregion

        #region BOOT METHODS
        public void VerboseStartupWriter(bool canWrite, string textToWrite)
        {
            if (canWrite)
            {
                LOADER_FORM.IOField.AppendText(textToWrite);
                LOADER_FORM.IOField.Update();
                Thread.Sleep(500);
            }
        }

        public BootImageProperties REG_SEQ()
        {
            BootImageProperties props = new BootImageProperties();
            string[] valueNames = LOADER_FORM.REG_KEY.GetValueNames();
            if (valueNames.Length > 0)
            {
                object value = null;
                /*STARTUP DIRECTORY*/
                /*Searching for valid directory path*/
                if (valueNames.Contains("StartupDirectory"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("StartupDirectory");
                        /*Searching for the specified directory*/
                        if (Directory.Exists(value.ToString()))
                        {
                            props.StartupDirectory = value.ToString();

                            /*Appending the last "\" character if not present(startup directory must end with "\")*/
                            if (props.StartupDirectory.Last() != '\\')
                            {
                                props.StartupDirectory += '\\';
                            }
                        }
                        else
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load startup directory(directory not found)... ");
                            LOADER_FORM.REG_KEY.SetValue("StartupDirectory", "C:\\");
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load startup directory(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("StartupDirectory");
                        LOADER_FORM.REG_KEY.SetValue("StartupDirectory", "C:\\");
                    }   
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load startup directory(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("StartupDirectory", "C:\\");
                }
                VerboseStartupWriter(props.VerboseStartup, "Startup directory is set to: " + props.StartupDirectory + NewLine);
                /*COLOR SETTINGS*/
                /*Searching for valid forecolor name*/
                if (valueNames.Contains("ForegroundColor"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("ForegroundColor");
                        if (Color.FromName(value.ToString()).ToKnownColor() != 0)
                        {
                            props.StartupForegroundColor = Color.FromName(value.ToString());
                        }
                        else
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load foreground color(invalid value)... " + NewLine);
                            LOADER_FORM.REG_KEY.SetValue("ForegroundColor", "Silver");
                            props.StartupForegroundColor = Color.Silver;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load foreground color(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("ForegroundColor");
                        LOADER_FORM.REG_KEY.SetValue("ForegroundColor", "Silver");
                        props.StartupForegroundColor = Color.Silver;
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load foreground color(value not found)... " + NewLine);
                    LOADER_FORM.REG_KEY.SetValue("ForegroundColor", "Silver");
                    props.StartupForegroundColor = Color.Silver;
                }
                /*Searching for valid backcolor name*/
                if (valueNames.Contains("BackgroundColor"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("BackgroundColor");
                        if (Color.FromName(value.ToString()).ToKnownColor() != 0)
                        {
                            props.StartupBackgroundColor = Color.FromName(value.ToString());
                        }
                        else
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load background color(invalid value)... " + NewLine);
                            LOADER_FORM.REG_KEY.SetValue("BackgroundColor", "Black");
                            props.StartupBackgroundColor = Color.Black;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load background color(invalid data type)... " + NewLine);
                        LOADER_FORM.REG_KEY.DeleteValue("BackgroundColor");
                        LOADER_FORM.REG_KEY.SetValue("BackgroundColor", "Black");
                        props.StartupBackgroundColor = Color.Black;
                    }

                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load background color(value not found)... " + NewLine);
                    LOADER_FORM.REG_KEY.SetValue("BackgroundColor", "Black");
                    props.StartupBackgroundColor = Color.Black;
                }
                /*Detecting foreground and background color contrariety*/
                if (props.StartupForegroundColor == props.StartupBackgroundColor)
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load system colors(foreground and background color cannot be the same)... ");
                    props.StartupForegroundColor = Color.Silver;
                    props.StartupBackgroundColor = Color.Black;
                }
                VerboseStartupWriter(props.VerboseStartup, "Foreground color is set to: " + props.StartupForegroundColor.Name + NewLine);
                VerboseStartupWriter(props.VerboseStartup, "Background color is set to: " + props.StartupBackgroundColor.Name + NewLine);
                /*FONT SETTINGS*/
                /*Searching for valid font family name*/
                if (valueNames.Contains("FontFamily"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("FontFamily");
                        if ((string)value == "CP437")
                        {
                            props.SystemFontFamily = CP437.FontFamily;
                        }
                        else
                        {
                            try
                            {
                                props.SystemFontFamily = new FontFamily(value.ToString());
                            }
                            catch (Exception)
                            {
                                VerboseStartupWriter(props.VerboseStartup, "Failed to load system font family(invalid value)... ");
                                LOADER_FORM.REG_KEY.SetValue("FontFamily", "CP437");
                                props.SystemFontFamily = CP437.FontFamily;
                            }
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load system font family(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("FontFamily");
                        LOADER_FORM.REG_KEY.SetValue("FontFamily", "CP437");
                        props.SystemFontFamily = CP437.FontFamily;
                    }      
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load system font family(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("FontFamily", "CP437");
                    props.SystemFontFamily = CP437.FontFamily;
                }
                if (props.SystemFontFamily.Name == "Perfect DOS VGA 437 Win")
                {
                    VerboseStartupWriter(props.VerboseStartup, "System font family is set to: CP437" + NewLine);
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "System font family is set to: " + props.SystemFontFamily.Name + NewLine);
                }
                /*Searching for valid font size*/
                if (valueNames.Contains("FontSize"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("FontSize");
                        int result;
                        if ((int)value == 0 || !int.TryParse(value.ToString(), out result))
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load system font size(invalid value)... ");
                            LOADER_FORM.REG_KEY.SetValue("FontSize", 16, RegistryValueKind.DWord);
                        }
                        else
                        {
                            props.SystemFontSize = (int)value;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load system font size(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("FontSize");
                        LOADER_FORM.REG_KEY.SetValue("FontSize", 16, RegistryValueKind.DWord);
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load system font size(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("FontSize", 16, RegistryValueKind.DWord);
                }
                VerboseStartupWriter(props.VerboseStartup, "System font size is set to: " + props.SystemFontSize + NewLine);
                /*Searching for valid font style*/
                if (valueNames.Contains("FontStyle"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("FontStyle");
                        switch (value.ToString().ToLower())
                        {
                            case "regular":
                                props.SystemFontStyle = FontStyle.Regular;
                                break;
                            case "bold":
                                props.SystemFontStyle = FontStyle.Bold;
                                break;
                            case "italic":
                                props.SystemFontStyle = FontStyle.Italic;
                                break;
                            default:
                                VerboseStartupWriter(props.VerboseStartup, "Failed to load system font family(invalid value)... ");
                                LOADER_FORM.REG_KEY.SetValue("FontStyle", "regular");
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load system font family(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("FontStyle");
                        LOADER_FORM.REG_KEY.SetValue("FontStyle", "regular");
                    }              
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load system font family(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("FontStyle", "regular");
                }
                VerboseStartupWriter(props.VerboseStartup, "System font style is set to: " + props.SystemFontStyle + NewLine);
                props.StartupSystemFont = new Font(props.SystemFontFamily, props.SystemFontSize, props.SystemFontStyle);
                /*FULLSCREEN*/
                /*Searching for valid fullscreen mode*/
                if (valueNames.Contains("Fullscreen"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("Fullscreen");
                        switch ((int)value)
                        {
                            case 0:
                                props.StartupFullscreen = false;
                                break;
                            case 1:
                                props.StartupFullscreen = true;
                                break;
                            default:
                                VerboseStartupWriter(props.VerboseStartup, "Failed to load fullscreen mode(invalid value)... ");
                                LOADER_FORM.REG_KEY.SetValue("Fullscreen", 1, RegistryValueKind.DWord);
                                props.StartupFullscreen = true;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load fullscreen mode(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("Fullscreen");
                        LOADER_FORM.REG_KEY.SetValue("Fullscreen", 1, RegistryValueKind.DWord);
                        props.StartupFullscreen = true;
                    }                  
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load fullscreen mode(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("Fullscreen", 1, RegistryValueKind.DWord);
                    props.StartupFullscreen = true;
                }
                VerboseStartupWriter(props.VerboseStartup, "Fullscreen mode is set to: " + props.StartupFullscreen + NewLine);
                /*EMULATION*/
                /*Searching for valid emulation value*/
                if (valueNames.Contains("Emulation"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("Emulation");
                        switch ((int)value)
                        {
                            case 0:
                                props.StartupEmulation = false;
                                break;
                            case 1:
                                props.StartupEmulation = true;
                                break;
                            default:
                                VerboseStartupWriter(props.VerboseStartup, "Failed to load emulation(invalid value)... ");
                                LOADER_FORM.REG_KEY.SetValue("Emulation", 0, RegistryValueKind.DWord);
                                props.StartupEmulation = false;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load emulation(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("Emulation");
                        LOADER_FORM.REG_KEY.SetValue("Emulation", 0, RegistryValueKind.DWord);
                        props.StartupEmulation = false;
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load emulation(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("Emulation", 0, RegistryValueKind.DWord);
                    props.StartupEmulation = false;
                }
                VerboseStartupWriter(props.VerboseStartup, "Emulation is set to: " + props.StartupEmulation + NewLine);
                /*TEXT EFFECT*/
                /*Searching for valid output effect value*/
                if (valueNames.Contains("TextEffect"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("TextEffect");
                        switch ((int)value)
                        {
                            case 0:
                                props.TextEffect = false;
                                break;
                            case 1:
                                props.TextEffect = true;
                                break;
                            default:
                                VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect(invalid value)... ");
                                LOADER_FORM.REG_KEY.SetValue("TextEffect", 0, RegistryValueKind.DWord);
                                props.TextEffect = false;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("TextEffect");
                        LOADER_FORM.REG_KEY.SetValue("TextEffect", 0, RegistryValueKind.DWord);
                        props.TextEffect = false;
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("TextEffect", 0, RegistryValueKind.DWord);
                    props.TextEffect = false;
                }
                VerboseStartupWriter(props.VerboseStartup, "Text effect is set to: " + props.TextEffect + NewLine);
                /*TEXT EFFECT INTERVAL*/
                /*Searching for valid output effect interval value*/
                if (valueNames.Contains("TextEffectInterval"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("TextEffectInterval");
                        if ((int)value >= 1 && (int)value <= 100)
                        {
                            props.TextEffectInterval = (int)value;
                        }
                        else
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect interval(invalid value)... ");
                            props.TextEffectInterval = 1;
                            LOADER_FORM.REG_KEY.SetValue("TextEffectInterval", props.TextEffectInterval, RegistryValueKind.DWord);
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect interval(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("TextEffectInterval");
                        LOADER_FORM.REG_KEY.SetValue("TextEffectInterval", props.TextEffectInterval, RegistryValueKind.DWord);
                        props.TextEffectInterval = 1;
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect interval(value not found)... ");
                    props.TextEffectInterval = 1;
                    LOADER_FORM.REG_KEY.SetValue("TextEffectInterval", props.TextEffectInterval, RegistryValueKind.DWord);
                }
                VerboseStartupWriter(props.VerboseStartup, "Text effect interval is set to: " + props.TextEffectInterval + NewLine);
                /*BASIC HISTORY SIZE*/
                /*Searching for valid basic history size value*/
                if (valueNames.Contains("DefaultHistorySize"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("DefaultHistorySize");
                        if ((int)value >= 2 && (int)value < int.MaxValue)
                        {
                            props.DefaultHistorySize = (int)value;
                        }
                        else
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load default history size(invalid value)... ");
                            props.DefaultHistorySize = 50;
                            LOADER_FORM.REG_KEY.SetValue("DefaultHistorySize", 50, RegistryValueKind.DWord);
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load default history size(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("DefaultHistorySize");
                        LOADER_FORM.REG_KEY.SetValue("DefaultHistorySize", 50, RegistryValueKind.DWord);
                        props.DefaultHistorySize = 50;
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load default history size(value not found)... ");
                    props.DefaultHistorySize = 50;
                    LOADER_FORM.REG_KEY.SetValue("DefaultHistorySize", 50, RegistryValueKind.DWord);
                }
                VerboseStartupWriter(props.VerboseStartup, "Default history size is set to: " + props.DefaultHistorySize + NewLine);
                /*REGEDIT HISTORY SIZE*/
                /*Searching for valid regedit history size value*/
                if (valueNames.Contains("RegeditHistorySize"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("RegeditHistorySize");
                        if ((int)value >= 2 && (int)value < int.MaxValue)
                        {
                            props.RegeditHistorySize = (int)value;
                        }
                        else
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load Regedit History Size(invalid value)... ");
                            props.RegeditHistorySize = 50;
                            LOADER_FORM.REG_KEY.SetValue("RegeditHistorySize", 50, RegistryValueKind.DWord);
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load Regedit History Size(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("RegeditHistorySize");
                        LOADER_FORM.REG_KEY.SetValue("RegeditHistorySize", 50, RegistryValueKind.DWord);
                        props.RegeditHistorySize = 50;
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load Regedit History Size(value not found)... ");
                    props.RegeditHistorySize = 50;
                    LOADER_FORM.REG_KEY.SetValue("RegeditHistorySize", 50, RegistryValueKind.DWord);
                }
                VerboseStartupWriter(props.VerboseStartup, "Regedit History Size is set to: " + props.RegeditHistorySize + NewLine);
                /*SHMOTD*/
                /*Searching for valid SHMOTD value*/
                if (valueNames.Contains("SHMOTD"))
                {
                    try
                    {
                        value = LOADER_FORM.REG_KEY.GetValue("SHMOTD");
                        switch ((int)value)
                        {
                            case 0:
                                props.StartupShowMOTD = false;
                                break;
                            case 1:
                                props.StartupShowMOTD = true;
                                break;
                            default:
                                VerboseStartupWriter(props.VerboseStartup, "Failed to load SHMOTD(invalid value)... ");
                                LOADER_FORM.REG_KEY.SetValue("SHMOTD", 1, RegistryValueKind.DWord);
                                props.StartupShowMOTD = true;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load SHMOTD(invalid data type)... ");
                        LOADER_FORM.REG_KEY.DeleteValue("SHMOTD");
                        LOADER_FORM.REG_KEY.SetValue("SHMOTD", 1, RegistryValueKind.DWord);
                        props.StartupShowMOTD = true;
                    }
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load SHMOTD(value not found)... ");
                    LOADER_FORM.REG_KEY.SetValue("SHMOTD", 1, RegistryValueKind.DWord);
                    props.StartupShowMOTD = true;
                }
                VerboseStartupWriter(props.VerboseStartup, "SHMOTD is set to: " + props.StartupShowMOTD + NewLine);
                /*MOTD*/
                /*Searching for valid MOTD message*/
                if (valueNames.Contains("MOTD"))
                {
                    try
                    {
                        props.StartupMOTD = LOADER_FORM.REG_KEY.GetValue("MOTD").ToString();
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load MOTD(invalid data type)... MOTD is set to: " + props.StartupMOTD + NewLine);
                        LOADER_FORM.REG_KEY.DeleteValue("MOTD");
                        LOADER_FORM.REG_KEY.SetValue("MOTD", @"Type ""help"" for more information!");
                    }             
                }
                else
                {
                    VerboseStartupWriter(props.VerboseStartup, "Failed to load MOTD(value not found)... MOTD is set to: " + props.StartupMOTD + NewLine);
                    LOADER_FORM.REG_KEY.SetValue("MOTD", @"Type ""help"" for more information!");
                }
                /*STARTUP SUBKEY*/
                /*Searching for valid subkey path*/
                if (valueNames.Contains("StartupSubkey"))
                {
                    /*Searching for the specified subkey*/
                    value = LOADER_FORM.REG_KEY.GetValue("StartupSubkey");
                    /*Verifying subkey*/
                    RegistryKey testKey = null;
                    try
                    {
                        if (value.ToString().StartsWith(Registry.ClassesRoot.Name))
                        {
                            testKey = Registry.ClassesRoot.OpenSubKey(value.ToString().Remove(0, Registry.ClassesRoot.Name.Length));
                            props.StartupSubkey = (string)value;
                        }
                        else if (value.ToString().StartsWith(Registry.CurrentConfig.Name))
                        {
                            testKey = Registry.CurrentConfig.OpenSubKey(value.ToString().Remove(0, Registry.CurrentConfig.Name.Length));
                            props.StartupSubkey = (string)value;
                        }
                        else if (value.ToString().StartsWith(Registry.CurrentUser.Name))
                        {
                            testKey = Registry.CurrentUser.OpenSubKey(value.ToString().Remove(0, Registry.CurrentUser.Name.Length));
                            props.StartupSubkey = (string)value;
                        }
                        else if (value.ToString().StartsWith(Registry.LocalMachine.Name))
                        {
                            testKey = Registry.LocalMachine.OpenSubKey(value.ToString().Remove(0, Registry.LocalMachine.Name.Length));
                            props.StartupSubkey = (string)value;
                        }
                        else if (value.ToString().StartsWith(Registry.Users.Name))
                        {
                            testKey = Registry.Users.OpenSubKey(value.ToString().Remove(0, Registry.Users.Name.Length));
                            props.StartupSubkey = (string)value;
                        }
                        else
                        {
                            VerboseStartupWriter(props.VerboseStartup, "Failed to load startup subkey(invalid hivekey)... ");
                            LOADER_FORM.REG_KEY.SetValue("StartupSubkey", "HKEY_CURRENT_USER\\");
                        }
                    }
                    catch (Exception)
                    {
                        VerboseStartupWriter(props.VerboseStartup, "Failed to load startup subkey(value not found)... ");
                        LOADER_FORM.REG_KEY.SetValue("StartupSubkey", "HKEY_CURRENT_USER\\");
                    }
                    /*Appending the last "\" character if not present(startup subkey must end with "\")*/
                    if (props.StartupSubkey.Last() != '\\')
                    {
                        props.StartupSubkey += '\\';
                    }
                    VerboseStartupWriter(props.VerboseStartup, "Startup subkey is set to: " + props.StartupSubkey + NewLine);
                }
                else
                {
                    LOADER_FORM.REG_KEY.SetValue("StartupSubkey", "HKEY_CURRENT_USER\\");
                }
            }
            else
            {
                VerboseStartupWriter(props.VerboseStartup, "Registry config not found... Creating registry config");
                LOADER_FORM.REG_KEY.SetValue("StartupDirectory", "C:\\");
                LOADER_FORM.REG_KEY.SetValue("ForegroundColor", "Silver");
                LOADER_FORM.REG_KEY.SetValue("BackgroundColor", "Black");
                LOADER_FORM.REG_KEY.SetValue("FontFamily", "CP437");
                LOADER_FORM.REG_KEY.SetValue("FontSize", 16, RegistryValueKind.DWord);
                LOADER_FORM.REG_KEY.SetValue("FontStyle", "regular");
                LOADER_FORM.REG_KEY.SetValue("Fullscreen", 1, RegistryValueKind.DWord);
                LOADER_FORM.REG_KEY.SetValue("SHMOTD", 1, RegistryValueKind.DWord);
                LOADER_FORM.REG_KEY.SetValue("MOTD", @"Type ""help"" for more information!");
                LOADER_FORM.REG_KEY.SetValue("Emulation", 0, RegistryValueKind.DWord);
                LOADER_FORM.REG_KEY.SetValue("StartupSubkey", "HKEY_CURRENT_USER\\");
                LOADER_FORM.REG_KEY.SetValue("TextEffect", 0, RegistryValueKind.DWord);
                LOADER_FORM.REG_KEY.SetValue("TextEffectInterval", 1, RegistryValueKind.DWord);
                LOADER_FORM.REG_KEY.SetValue("DefaultHistorySize", 50, RegistryValueKind.DWord);
                LOADER_FORM.REG_KEY.SetValue("RegeditHistorySize", 50, RegistryValueKind.DWord);
            }
            return props;
        }

        public BootImageProperties INI_SEQ()
        {
            BootImageProperties props = new BootImageProperties();
            StringBuilder sb = null;
            if (File.Exists("BOOT.ini"))
            {
                List<int> corruptedProperties = new List<int>();
                string[] INIProperties = File.ReadAllText("BOOT.ini").Split('|');
                for (int i = 0; i < INIProperties.Length; i++)
                {
                    string[] propSplit = INIProperties[i].Split('>');
                    if (propSplit.Length >= 2)
                    {
                        int result = int.MaxValue;
                        switch (propSplit[0])
                        {
                            case "StartupDirectory":
                                if (Directory.Exists(propSplit[1]))
                                {
                                    props.StartupDirectory = propSplit[1];
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Startup directory not found... ");
                                    INIProperties[i] = "StartupDirectory>C:\\";
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Startup directory is set to: " + props.StartupDirectory + NewLine);
                                break;
                            case "Color":
                                switch (propSplit.Length)
                                {
                                    case 2:
                                        props.StartupForegroundColor = Color.FromName(propSplit[1]);
                                        if (props.StartupForegroundColor.ToKnownColor() != 0)
                                        {
                                            if (props.StartupForegroundColor != Color.Black)
                                            {
                                                props.StartupBackgroundColor = Color.Black;
                                                INIProperties[i] = "Color>" + props.StartupForegroundColor.Name + ">Black";
                                            }
                                            else
                                            {
                                                props.StartupBackgroundColor = Color.Silver;
                                                INIProperties[i] = "Color>" + props.StartupForegroundColor.Name + ">Silver";
                                            }
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load background color(value not found)... " + NewLine);
                                        }
                                        else
                                        {
                                            INIProperties[i] = "Color>Silver>Black";
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load system colors(invalid values)... " + NewLine);
                                        }
                                        break;
                                    default:
                                        props.StartupForegroundColor = Color.FromName(propSplit[1]);
                                        props.StartupBackgroundColor = Color.FromName(propSplit[2]);
                                        bool foregroundIsValid = false;
                                        bool backgroundIsValid = false;
                                        if (props.StartupForegroundColor.ToKnownColor() != 0)
                                        {
                                            foregroundIsValid = true;
                                        }
                                        if (props.StartupBackgroundColor.ToKnownColor() != 0)
                                        {
                                            backgroundIsValid = true;
                                        }
                                        if (foregroundIsValid && backgroundIsValid)
                                        {
                                            if (props.StartupForegroundColor == props.StartupBackgroundColor)
                                            {
                                                VerboseStartupWriter(props.VerboseStartup, "Failed to load system colors(contrariety occurred)... " + NewLine);
                                                props.StartupForegroundColor = Color.Silver;
                                                props.StartupBackgroundColor = Color.Black;
                                            }
                                        }
                                        else
                                        {
                                            if (!foregroundIsValid)
                                            {
                                                VerboseStartupWriter(props.VerboseStartup, "Failed to load foreground color(invalid value)... " + NewLine);
                                                props.StartupForegroundColor = Color.Silver;
                                            }
                                            if (!backgroundIsValid)
                                            {
                                                VerboseStartupWriter(props.VerboseStartup, "Failed to load background color(invalid value)... " + NewLine);
                                                props.StartupBackgroundColor = Color.Black;
                                            }
                                        }
                                        INIProperties[i] = "Color>" + props.StartupForegroundColor.Name + ">" + props.StartupBackgroundColor.Name;
                                        break;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Foreground color is set to: " + props.StartupForegroundColor.Name + NewLine);
                                VerboseStartupWriter(props.VerboseStartup, "Background color is set to: " + props.StartupBackgroundColor.Name + NewLine);
                                break;
                            case "StartupSystemFont":
                                if (propSplit.Length >= 2)
                                {
                                    if (propSplit[1] == "CP437")
                                    {
                                        props.SystemFontFamily = CP437.FontFamily;
                                    }
                                    else
                                    {
                                        try
                                        {
                                            props.SystemFontFamily = new FontFamily(propSplit[1]);
                                        }
                                        catch (Exception)
                                        {
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load system font family(invalid value)... " + NewLine);
                                            props.SystemFontFamily = CP437.FontFamily;
                                        }
                                    }
                                }
                                if (propSplit.Length >= 3)
                                {
                                    if (int.TryParse(propSplit[2], out result) && result != 0)
                                    {
                                        props.SystemFontSize = result;
                                    }
                                    else
                                    {
                                        VerboseStartupWriter(props.VerboseStartup, "Failed to load system font size(invalid value)... " + NewLine);
                                    }
                                }
                                if (propSplit.Length >= 4)
                                {
                                    switch (propSplit[3].ToLower())
                                    {
                                        case "regular":
                                            props.SystemFontStyle = FontStyle.Regular;
                                            break;
                                        case "bold":
                                            props.SystemFontStyle = FontStyle.Bold;
                                            break;
                                        case "italic":
                                            props.SystemFontStyle = FontStyle.Italic;
                                            break;
                                        default:
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load system font style(invalid value)... " + NewLine);
                                            break;
                                    }
                                }
                                INIProperties[i] = "StartupSystemFont>" + props.SystemFontFamily.Name + ">" + props.SystemFontSize + ">" + props.SystemFontStyle;
                                if (props.SystemFontFamily.Name == "Perfect DOS VGA 437 Win")
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "System font family is set to: CP437" + NewLine);
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "System font family is set to: " + props.SystemFontFamily.Name + NewLine);
                                }
                                VerboseStartupWriter(props.VerboseStartup, "System font size is set to: " + props.SystemFontSize + NewLine);
                                VerboseStartupWriter(props.VerboseStartup, "System font style is set to: " + props.SystemFontStyle + NewLine);
                                props.StartupSystemFont = new Font(props.SystemFontFamily, props.SystemFontSize, props.SystemFontStyle);
                                break;
                            case "Fullscreen":
                                if (int.TryParse(propSplit[1], out result))
                                {
                                    switch (result)
                                    {
                                        case 0:
                                            props.StartupFullscreen = false;
                                            break;
                                        case 1:
                                            props.StartupFullscreen = true;
                                            break;
                                        default:
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load fullscreen mode(invalid value)... ");
                                            INIProperties[i] = "Fullscreen>1";
                                            props.StartupFullscreen = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Failed to load fullscreen mode(value not found)... ");
                                    INIProperties[i] = "Fullscreen>1";
                                    props.StartupFullscreen = true;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Fullscreen mode is set to: " + props.StartupFullscreen + NewLine);
                                break;
                            case "Emulation":
                                if (int.TryParse(propSplit[1], out result))
                                {
                                    switch (result)
                                    {
                                        case 0:
                                            props.StartupEmulation = false;
                                            break;
                                        case 1:
                                            props.StartupEmulation = true;
                                            break;
                                        default:
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load emulation(invalid value)... ");
                                            INIProperties[i] = "Emulation>0";
                                            props.StartupEmulation = false;
                                            break;
                                    }
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Failed to load fullscreen mode(value not found)... ");
                                    INIProperties[i] = "Emulation>0";
                                    props.StartupEmulation = false;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Emulation is set to: " + props.StartupEmulation + NewLine);
                                break;
                            case "TextEffect":
                                if (int.TryParse(propSplit[1], out result))
                                {
                                    switch (result)
                                    {
                                        case 0:
                                            props.TextEffect = false;
                                            break;
                                        case 1:
                                            props.TextEffect = true;
                                            break;
                                        default:
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect(invalid value)... ");
                                            INIProperties[i] = "TextEffect>0";
                                            props.TextEffect = false;
                                            break;
                                    }
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect(value not found)... ");
                                    INIProperties[i] = "TextEffect>0";
                                    props.StartupShowMOTD = false;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Text effect is set to: " + props.TextEffect + NewLine);
                                break;
                            case "TextEffectInterval":
                                if (int.TryParse(propSplit[1], out result))
                                {
                                    if (result >= 1 && result <= 100)
                                    {
                                        props.TextEffectInterval = result;
                                    }
                                    else
                                    {
                                        VerboseStartupWriter(props.VerboseStartup, "Failed to load text effect Interval(invalid value)... ");
                                        INIProperties[i] = "TextEffectInterval>1";
                                        props.TextEffectInterval = 1;
                                    }
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Failed to load Text effect interval(value not found)... ");
                                    INIProperties[i] = "TextEffectInterval>1";
                                    props.TextEffectInterval = 1;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Text effect interval is set to: " + props.TextEffectInterval + NewLine);
                                break;
                            case "DefaultHistorySize":
                                if (int.TryParse(propSplit[1], out result))
                                {
                                    if (result >= 2 && result < int.MaxValue)
                                    {
                                        props.DefaultHistorySize = result;
                                    }
                                    else
                                    {
                                        VerboseStartupWriter(props.VerboseStartup, "Failed to load default history size(invalid value)... ");
                                        INIProperties[i] = "DefaultHistorySize>50";
                                        props.DefaultHistorySize = 50;
                                    }
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Failed to load default history size(value not found)... ");
                                    INIProperties[i] = "DefaultHistorySize>50";
                                    props.DefaultHistorySize = 50;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Default history size is set to: " + props.DefaultHistorySize + NewLine);
                                break;
                            case "RegeditHistorySize":
                                if (int.TryParse(propSplit[1], out result))
                                {
                                    if (result >= 2 && result < int.MaxValue)
                                    {
                                        props.RegeditHistorySize = result;
                                    }
                                    else
                                    {
                                        VerboseStartupWriter(props.VerboseStartup, "Failed to load regedit history size(invalid value)... ");
                                        INIProperties[i] = "RegeditHistorySize>50";
                                        props.RegeditHistorySize = 50;
                                    }
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Failed to load regedit history size(value not found)... ");
                                    INIProperties[i] = "RegeditHistorySize>50";
                                    props.RegeditHistorySize = 50;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "Regedit history size is set to: " + props.RegeditHistorySize + NewLine);
                                break;
                            case "SHMOTD":
                                if (int.TryParse(propSplit[1], out result))
                                {
                                    switch (result)
                                    {
                                        case 0:
                                            props.StartupShowMOTD = false;
                                            break;
                                        case 1:
                                            props.StartupShowMOTD = true;
                                            break;
                                        default:
                                            VerboseStartupWriter(props.VerboseStartup, "Failed to load SHMOTD(invalid value)... ");
                                            INIProperties[i] = "SHMOTD>1";
                                            props.StartupShowMOTD = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    VerboseStartupWriter(props.VerboseStartup, "Failed to load SHMOTD(value not found)... ");
                                    INIProperties[i] = "SHMOTD>1";
                                    props.StartupShowMOTD = true;
                                }
                                VerboseStartupWriter(props.VerboseStartup, "SHMOTD is set to: " + props.StartupShowMOTD + NewLine);
                                break;
                            case "MOTD":
                                props.StartupMOTD = propSplit[1];
                                VerboseStartupWriter(props.VerboseStartup, "MOTD is set to: " + props.StartupMOTD + NewLine);
                                break;
                            default:
                                if (propSplit[0] != "VerboseStartup")
                                {
                                    corruptedProperties.Add(i);
                                }
                                break;
                        }
                    }
                    else
                    {
                        corruptedProperties.Add(i);
                    }
                }

                List<string> INIPropertiesList = INIProperties.ToList();
                for (int i = 0; i < corruptedProperties.Count(); i++)
                {
                    try
                    {
                        INIPropertiesList.RemoveAt(corruptedProperties[i]);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show(corruptedProperties[i] + "-" + INIPropertiesList.Count());
                    }
                }
                sb = new StringBuilder();
                for (int i = 0; i < INIPropertiesList.Count(); i++)
                {
                    sb.Append(INIPropertiesList[i]);
                    if (i < INIPropertiesList.Count() - 1)
                    {
                        sb.Append("|");
                    }
                }
                File.WriteAllText("BOOT.ini", sb.ToString());
            }
            else
            {
                VerboseStartupWriter(props.VerboseStartup, "INI config not found... Creating BOOT.ini");
                sb = new StringBuilder();
                sb.Append(@"StartupDirectory>C:\|Color>Silver>Black|StartupSystemFont>CP437>16>regular|Fullscreen>1|Emulation>0|VerboseStartup>1|TextEffect>0|TextEffectInterval>1|DefaultHistorySize>50|RegeditHistorySize>50|SHMOTD>1|MOTD>Type ""help"" for more information!");
                File.WriteAllText("BOOT.ini", sb.ToString());
            }
            return props;
        }
        #endregion

        #region PRE-BOOT METHODS
        public static ExecutionLevel GetExecutionLevel()
        {
            try
            {
                LOADER_FORM.REG_KEY = Registry.LocalMachine.CreateSubKey("SOFTWARE\\WinDOS");
                if (LOADER_FORM.REG_KEY != null)
                {
                    return ExecutionLevel.Administrator;
                }
                else
                {
                    return ExecutionLevel.User;
                }
            }
            catch (Exception)
            {
                return ExecutionLevel.User;
            }
        }

        public static bool GetVerboseStartup(ExecutionLevel execLevel)
        {
            bool vStart = false;
            StringBuilder sb = new StringBuilder();
            switch (execLevel)
            {
                case ExecutionLevel.Administrator:
                    try
                    {
                        switch (int.Parse(LOADER_FORM.REG_KEY.GetValue("VerboseStartup").ToString()))
                        {
                            case 0:
                                vStart = false;
                                break;
                            case 1:
                                vStart = true;
                                break;
                            default:
                                LOADER_FORM.REG_KEY.SetValue("VerboseStartup", 1, RegistryValueKind.DWord);
                                vStart = true;
                                break;
                        }
                    }
                    catch (Exception)
                    {
                        LOADER_FORM.REG_KEY.SetValue("VerboseStartup", 1, RegistryValueKind.DWord);
                        vStart = true;
                    }
                    break;
                case ExecutionLevel.User:
                    bool vsFound = false;
                    if (File.Exists("BOOT.ini"))
                    {
                        string[] propLines = File.ReadAllText("BOOT.ini").Split('|');
                        for (int i = 0; i < propLines.Length; i++)
                        {
                            string[] lineData = propLines[i].Split('>');
                            if (lineData[0] == "VerboseStartup")
                            {
                                vsFound = true;
                                if (lineData.Length >= 2)
                                {
                                    switch (lineData[1])
                                    {
                                        case "0":
                                            vStart = false;
                                            break;
                                        case "1":
                                            vStart = true;
                                            break;
                                        default:
                                            propLines[i] = "VerboseStartup>1";
                                            vStart = true;
                                            break;
                                    }
                                }
                                else
                                {
                                    propLines[i] = "VerboseStartup>1";
                                    vStart = true;
                                }
                                break;
                            }
                        }
                        if (!vsFound)
                        {
                            List<string> propLinesList = propLines.ToList();
                            propLinesList.Add("VerboseStartup>1");
                            propLines = propLinesList.ToArray();
                            vStart = true;
                        }
                        for (int i = 0; i < propLines.Length; i++)
                        {
                            sb.Append(propLines[i]);
                            if (i < propLines.Length - 1)
                            {
                                sb.Append("|");
                            }
                        }
                        File.WriteAllText("BOOT.ini", sb.ToString());
                    }
                    else
                    {
                        File.Create("BOOT.ini").Close();
                        File.WriteAllText("BOOT.ini", @"StartupDirectory>C:\|Color>Silver>Black|StartupSystemFont>CP437>16>regular|Fullscreen>1|Emulation>0|VerboseStartup>1|TextEffect>0|TextEffectInterval>1|DefaultHistorySize>50|RegeditHistorySize>50|SHMOTD>1|MOTD>Type ""help"" for more information!");
                        vStart = true;
                    }
                    break;
            }
            return vStart;
        }
        #endregion
    }
}
