using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using CLIShell;
using ICSharpCode.AvalonEdit;
using System.Windows.Media.Animation;

namespace WinDOS
{
    public partial class MainWindow : Window
    {
        #region SYSTEM COMPONENT DECLARATIONS
        public InputHistory DEF_HISTORY;

        public InputHistory REG_HISTORY;

        public AutoComplete DEF_AUTO_COMPLETE;

        public AutoComplete REG_AUTO_COMPLETE;

        public Caret SYSTEM_CARET;

        public SystemEffects SYSTEM_EFFECTS;

        public CommandManagement CMD_MGMT;

        public CommandPool DEF_CMD_POOL;

        public CommandPool REG_CMD_POOL;

        private double LAST_WINDOWED_WIDTH;

        private double LAST_WINDOWED_HEIGHT;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            STARTUP();
        }

        #region HELPER METHODS
        private void WRITE_CONFIG_INI(string type, string name, string newValue)
        {
            List<string> lines = File.ReadAllLines("config.ini").ToList();
            lines[lines.FindIndex(x =>  x.StartsWith(type + ":" + name + "="))] = type + ":" + name + "=" + newValue;
            File.WriteAllLines("config.ini", lines.ToArray());
        }

        private void WRITE_CURRENT_DIR()
        {
            IOField.AppendText(IOField.Text != "" ? Environment.NewLine + EnvironmentVariables.GetCurrentValue((string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default" ? "DIRECTORY" : "SUBKEY") + "> " : EnvironmentVariables.GetCurrentValue((string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default" ? "DIRECTORY" : "SUBKEY") + "> ");
        }

        private string EXTRACT_INPUT()
        {
            return IOField.Text.Split('\n').Last().Replace(EnvironmentVariables.Find((string)EnvironmentVariables.GetCurrentValue("CLI_MODE") == "Default" ? "DIRECTORY" : "SUBKEY").CurrentValue + "> ", "");
        }

        private void CONSTRUCT_CARET()
        {
            SYSTEM_CARET = new Caret(
                   IOField,
                   CaretCanvas,
                   Canvas,
                   grid,
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_HEIGHT")),
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_WIDTH")),
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_BORDER")),
                   new BrushConverter().ConvertFromString((string)EnvironmentVariables.GetCurrentValue("SYS_CARET_FILL_COLOR")) as Brush,
                   new BrushConverter().ConvertFromString((string)EnvironmentVariables.GetCurrentValue("SYS_CARET_BORDER_COLOR")) as Brush,
                   (bool)EnvironmentVariables.GetCurrentValue("SYS_CARET_FOOTPRINT"),
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_FADEIN_SPEED")),
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_FADEOUT_SPEED")),
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_FOOTPRINT_FADEOUT_SPEED")),
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_BLINK_INTERVAL")),
                   Convert.ToInt32(EnvironmentVariables.GetCurrentValue("SYS_CARET_BLINK_IDLE_TIME")),
                   Convert.ToDouble(EnvironmentVariables.GetCurrentValue("SYS_CARET_SHADOW_BLUR_RADIUS")),
                   Convert.ToDouble(EnvironmentVariables.GetCurrentValue("SYS_CARET_SHADOW_DEPTH")),
                   (new BrushConverter().ConvertFromString((string)EnvironmentVariables.GetCurrentValue("SYS_CARET_SHADOW_COLOR")) as SolidColorBrush).Color);
        }
        #endregion

        #region STARTUP METHODS
        private void LOAD_CONFIG()
        {
            IOField.TextArea.TextView.LinkTextUnderline = false;
            IOField.Background = Brushes.Transparent;
            IOField.TextArea.TextView.LinkTextBackgroundBrush = Brushes.Transparent;
            if (!File.Exists("config.ini"))
            {
                File.WriteAllLines("config.ini", new string[] {
                    typeof(string).FullName + ":CLI_MODE=Default",
                    typeof(string).FullName + ":DIRECTORY=C:\\",
                    typeof(string).FullName + ":FORECOLOR=Silver",
                    typeof(string).FullName + ":BACKCOLOR=Transparent",
                    typeof(string).FullName + ":FONT_FAMILY=EGA9",
                    typeof(int).FullName + ":FONT_SIZE=24",
                    typeof(string).FullName + ":FONT_STYLE=Regular",
                    typeof(bool).FullName + ":FULLSCREEN=False",
                    typeof(bool).FullName +":SHMOTD=True",
                    typeof(string).FullName + ":MOTD=Type \"help\" for more information!",
                    typeof(string).FullName + ":SUBKEY=LocalMachine\\",
                    typeof(int).FullName + ":DEF_HIST_SIZE=50",
                    typeof(int).FullName + ":REG_HIST_SIZE=50",
                    typeof(bool).FullName + ":SYS_CARET_BLINK=True",
                    typeof(int).FullName + ":SYS_CARET_BLINK_INTERVAL=1000",
                    typeof(bool).FullName + ":SYS_EFFECTS=True",
                    typeof(bool).FullName + ":SYS_CARET=True",
                    typeof(int).FullName + ":SYS_CARET_HEIGHT=0",
                    typeof(int).FullName + ":SYS_CARET_WIDTH=0",
                    typeof(int).FullName + ":SYS_CARET_BORDER=0",
                    typeof(bool).FullName + ":SYS_CARET_FOOTPRINT=True",
                    typeof(int).FullName + ":SYS_CARET_FADEIN_SPEED=1000",
                    typeof(int).FullName + ":SYS_CARET_FADEOUT_SPEED=1000",
                    typeof(int).FullName + ":SYS_CARET_FOOTPRINT_FADEOUT_SPEED=200",
                    typeof(int).FullName + ":SYS_CARET_BLINK_IDLE_TIME=2000",
                    typeof(string).FullName + ":SYS_CARET_FILL_COLOR=Silver",
                    typeof(double).FullName + ":SYS_CARET_SHADOW_BLUR_RADIUS=15",
                    typeof(double).FullName + ":SYS_CARET_SHADOW_DEPTH=0",
                    typeof(string).FullName + ":SYS_CARET_SHADOW_COLOR=Silver",
                    typeof(string).FullName + ":SYS_CARET_BORDER_COLOR=Transparent",
                    typeof(double).FullName + ":BLUR_RADIUS=2",
                    typeof(double).FullName + ":SHADOW_BLUR_RADIUS=15",
                    typeof(double).FullName + ":SHADOW_DEPTH=0",
                    typeof(string).FullName + ":SHADOW_COLOR=Silver",
                    typeof(string).FullName + ":BLUR_TYPE=Gaussian",
                    typeof(double).FullName + ":CRT_CURVE=0.03",
                    typeof(double).FullName + ":CRT_NOISE_INTENSITY=100",
                    typeof(double).FullName + ":CRT_SCANLINE_DENSITY=0",
                    typeof(double).FullName + ":CRT_SCANLINE_SPEED=0",
                    typeof(double).FullName + ":CRT_VIGNETTE_SIZE=1.9",
                    typeof(double).FullName + ":CRT_VIGNETTE_SMOOTHNESS=0.6",
                    typeof(double).FullName + ":CRT_VIGNETTE_EDGE_ROUNDING=8",});
            }
            List<string> lines = File.ReadAllLines("config.ini").ToList();
            Type currentType = null;
            lines.ForEach((x) =>
            {
                currentType = Type.GetType(x.Split(':')[0]);
                x = x.Remove(0, x.IndexOf(':') + 1);
                try
                {
                    EnvironmentVariables.Add(new EnvironmentVariable(x.Split('=')[0], currentType, x.Remove(0, x.Split('=')[0].Length + 1), x.Remove(0, x.Split('=')[0].Length + 1), VariableType.Constant));
                }
                catch (EnvironmentVariableException ex)
                {
                    IOField.AppendText(ex.Message + "\n");
                }
            });

            ExecutionLevel execLevel = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator) ? ExecutionLevel.Administrator : ExecutionLevel.User;
            try
            {
                EnvironmentVariables.Add(new EnvironmentVariable("EXEC_LEVEL", typeof(ExecutionLevel), execLevel, execLevel, VariableType.RuntimeConstant));
            }
            catch (EnvironmentVariableException ex)
            {
                IOField.AppendText(ex.Message + "\n");
            }

            CLIMode cmode;
            if (Enum.TryParse((string)EnvironmentVariables.GetCurrentValue("CLI_MODE"), true, out cmode) && cmode != CLIMode.Any && cmode != CLIMode.Text)
            {
                if (cmode == CLIMode.Regedit && execLevel != ExecutionLevel.Administrator)
                {
                    EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Default");
                    IOField.AppendText("CLI mode: Regedit requires Administrator execution level - var: CLI_MODE is set to Default\n");
                }
            }
            else
            {
                IOField.AppendText("Invalid CLI mode: " + (string)EnvironmentVariables.GetCurrentValue("CLI_MODE") + " - var: CLI_MODE is set to Default\n");
                EnvironmentVariables.ChangeCurrentValue("CLI_MODE", "Default");
            }

            try
            {
                IOField.Foreground = new BrushConverter().ConvertFromString((string)EnvironmentVariables.GetCurrentValue("FORECOLOR")) as SolidColorBrush;
            }
            catch (Exception)
            {
                IOField.Foreground = Brushes.Silver;
                IOField.AppendText("Invalid foreground color: " + (string)EnvironmentVariables.GetCurrentValue("FORECOLOR") + " - var: FORECOLOR is set to Silver");
                EnvironmentVariables.ChangeCurrentValue("FORECOLOR", "Silver");
            }

            try
            {
                Canvas.Background = new BrushConverter().ConvertFromString((string)EnvironmentVariables.GetCurrentValue("BACKCOLOR")) as SolidColorBrush;
            }
            catch (Exception)
            {
                Canvas.Background = Brushes.Black;
                IOField.AppendText("Invalid background color: " + (string)EnvironmentVariables.GetCurrentValue("BACKCOLOR") + " - var: BACKCOLOR is set to Black");
                EnvironmentVariables.ChangeCurrentValue("BACKCOLOR", "Black");
            }

            System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();

            if (Application.Current.Resources.Contains((string)EnvironmentVariables.GetCurrentValue("FONT_FAMILY")))
            {
                IOField.Style = (Style)Application.Current.Resources[(string)EnvironmentVariables.GetCurrentValue("FONT_FAMILY")];
            }
            else if (ifc.Families.ToList().Exists(x => x.Name == (string)EnvironmentVariables.GetCurrentValue("FONT_FAMILY")))
            {
                Style s = new Style();
                s.Setters.Add(new Setter(FontFamilyProperty, new FontFamily((string)EnvironmentVariables.GetCurrentValue("FONT_FAMILY"))));
                IOField.Style = s;
            }
            else
            {
                IOField.Style = (Style)Application.Current.Resources["EGA9"];
                IOField.AppendText("Invalid font family: " + (string)EnvironmentVariables.GetCurrentValue("FONT_FAMILY") + " - var: FONT_FAMILY is set to IBM_EGA9");
                EnvironmentVariables.ChangeCurrentValue("FONT_FAMILY", "EGA9");
            }

            if ((int)EnvironmentVariables.GetCurrentValue("FONT_SIZE") > 0)
            {
                IOField.FontSize = (int)EnvironmentVariables.GetCurrentValue("FONT_SIZE");
            }
            else
            {
                IOField.FontSize = 24;
                IOField.AppendText("Invalid font size: " + (string)EnvironmentVariables.GetCurrentValue("FONT_SIZE") + " - var: FONT_SIZE is set to 24");
                EnvironmentVariables.ChangeCurrentValue("FONT_SIZE", "EGA9");
            }

            IOField.FontStyle = (string)EnvironmentVariables.GetCurrentValue("FONT_STYLE") == "Regular" ? FontStyles.Normal : (string)EnvironmentVariables.GetCurrentValue("FONT_STYLE") == "Oblique" ? FontStyles.Oblique : (string)EnvironmentVariables.GetCurrentValue("FONT_STYLE") == "Italic" ? FontStyles.Italic : FontStyles.Normal;
            if (IOField.FontStyle == FontStyles.Normal && (string)EnvironmentVariables.GetCurrentValue("FONT_STYLE") != "Regular")
            {
                IOField.AppendText("Invalid font style: " + (string)EnvironmentVariables.GetCurrentValue("FONT_STYLE") + " - var: FONT_STYLE is set to Regular");
                EnvironmentVariables.ChangeCurrentValue("FONT_STYLE", "Regular");
            }


            LAST_WINDOWED_WIDTH = Width;
            LAST_WINDOWED_HEIGHT = Height;

            if ((bool)EnvironmentVariables.GetCurrentValue("FULLSCREEN"))
            {
                WindowStyle = WindowStyle.None;
                ResizeMode = ResizeMode.NoResize;
                Left = 0;
                Top = 0;
                Width = SystemParameters.PrimaryScreenWidth;
                Height = SystemParameters.PrimaryScreenHeight;
                Topmost = true;
            }

            if ((bool)EnvironmentVariables.GetCurrentValue("SYS_EFFECTS"))
            {
                SYSTEM_EFFECTS = new SystemEffects(grid, Canvas, CaretCanvas, IOField);
                double d_res;
                if (double.TryParse(EnvironmentVariables.GetCurrentValue("CRT_NOISE_INTENSITY").ToString(), out d_res))
                {
                    SYSTEM_EFFECTS.CRTEffect.NoiseIntensity = d_res;
                }
                else
                {
                    IOField.AppendText("Invalid noise intensity: " + EnvironmentVariables.GetCurrentValue("CRT_NOISE_INTENSITY").ToString() + " - var: CRT_NOISE_INTENSITY is set to " + SYSTEM_EFFECTS.CRTEffect.NoiseIntensity);
                    EnvironmentVariables.ChangeCurrentValue("CRT_NOISE_INTENSITY", SYSTEM_EFFECTS.CRTEffect.NoiseIntensity);
                }
                if (double.TryParse(EnvironmentVariables.GetCurrentValue("CRT_SCANLINE_DENSITY").ToString(), out d_res))
                {
                    SYSTEM_EFFECTS.NoiseEffect.ScanLine1Density = d_res;
                    SYSTEM_EFFECTS.NoiseEffect.ScanLine2Density = d_res;
                }
                else
                {
                    IOField.AppendText("Invalid scanline density: " + EnvironmentVariables.GetCurrentValue("CRT_SCANLINE_DENSITY").ToString() + " - var: CRT_SCANLINE_DENSITY is set to " + SYSTEM_EFFECTS.NoiseEffect.ScanLine1Density);
                    EnvironmentVariables.ChangeCurrentValue("CRT_SCANLINE_DENSITY", SYSTEM_EFFECTS.NoiseEffect.ScanLine1Density);
                }
                if (double.TryParse(EnvironmentVariables.GetCurrentValue("CRT_SCANLINE_SPEED").ToString(), out d_res))
                {
                    SYSTEM_EFFECTS.NoiseEffect.ScanLine1Speed = d_res;
                    SYSTEM_EFFECTS.NoiseEffect.ScanLine2Speed = d_res;
                }
                else
                {
                    IOField.AppendText("Invalid scanline speed: " + EnvironmentVariables.GetCurrentValue("CRT_SCANLINE_SPEED").ToString() + " - var: CRT_SCANLINE_SPEED is set to " + SYSTEM_EFFECTS.NoiseEffect.ScanLine1Speed);
                    EnvironmentVariables.ChangeCurrentValue("CRT_SCANLINE_SPEED", SYSTEM_EFFECTS.NoiseEffect.ScanLine1Speed);
                }

                if (double.TryParse(EnvironmentVariables.GetCurrentValue("CRT_VIGNETTE_SIZE").ToString(), out d_res))
                {
                    SYSTEM_EFFECTS.NoiseEffect.VignetteSize = d_res;
                }
                else
                {
                    IOField.AppendText("Invalid vignette size: " + EnvironmentVariables.GetCurrentValue("CRT_VIGNETTE_SIZE").ToString() + " - var: CRT_VIGNETTE_SIZE is set to " + SYSTEM_EFFECTS.NoiseEffect.VignetteSize);
                    EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_SIZE", SYSTEM_EFFECTS.NoiseEffect.VignetteSize);
                }

                if (double.TryParse(EnvironmentVariables.GetCurrentValue("CRT_VIGNETTE_SMOOTHNESS").ToString(), out d_res))
                {
                    SYSTEM_EFFECTS.NoiseEffect.VignetteSmoothness = d_res;
                }
                else
                {
                    IOField.AppendText("Invalid vignette smoothness: " + EnvironmentVariables.GetCurrentValue("CRT_VIGNETTE_SMOOTHNESS").ToString() + " - var: CRT_VIGNETTE_SMOOTHNESS is set to " + SYSTEM_EFFECTS.NoiseEffect.VignetteSmoothness);
                    EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_SMOOTHNESS", SYSTEM_EFFECTS.NoiseEffect.VignetteSmoothness);
                }

                if (double.TryParse(EnvironmentVariables.GetCurrentValue("CRT_VIGNETTE_EDGE_ROUNDING").ToString(), out d_res))
                {
                    SYSTEM_EFFECTS.NoiseEffect.VignetteEdgeRounding = d_res;
                }
                else
                {
                    IOField.AppendText("Invalid vignette edge rounding: " + EnvironmentVariables.GetCurrentValue("CRT_VIGNETTE_EDGE_ROUNDING").ToString() + " - var: CRT_VIGNETTE_EDGE_ROUNDING is set to " + SYSTEM_EFFECTS.NoiseEffect.VignetteEdgeRounding);
                    EnvironmentVariables.ChangeCurrentValue("CRT_VIGNETTE_EDGE_ROUNDING", SYSTEM_EFFECTS.NoiseEffect.VignetteEdgeRounding);
                }

                if (double.TryParse(EnvironmentVariables.GetCurrentValue("CRT_CURVE").ToString(), out d_res))
                {
                    SYSTEM_EFFECTS.NoiseEffect.Curve = d_res;
                }
                else
                {
                    IOField.AppendText("Invalid distortion curve: " + EnvironmentVariables.GetCurrentValue("CRT_CURVE").ToString() + " - var: CRT_CURVE is set to " + SYSTEM_EFFECTS.NoiseEffect.Curve);
                    EnvironmentVariables.ChangeCurrentValue("CRT_CURVE", SYSTEM_EFFECTS.NoiseEffect.Curve);
                }

                if (!double.TryParse(EnvironmentVariables.GetCurrentValue("BLUR_RADIUS").ToString(), out d_res))
                {
                    d_res = 2;
                    IOField.AppendText("Invalid blur radius: " + EnvironmentVariables.GetCurrentValue("BLUR_RADIUS").ToString() + " - var: BLUR_RADIUS is set to " + d_res);
                    EnvironmentVariables.ChangeCurrentValue("BLUR_RADIUS", d_res);
                }

                if (!double.TryParse(EnvironmentVariables.GetCurrentValue("SHADOW_BLUR_RADIUS").ToString(), out d_res))
                {
                    d_res = 15;
                    IOField.AppendText("Invalid shadow blur radius: " + EnvironmentVariables.GetCurrentValue("SHADOW_BLUR_RADIUS").ToString() + " - var: SHADOW_BLUR_RADIUS is set to " + d_res);
                    EnvironmentVariables.ChangeCurrentValue("SHADOW_BLUR_RADIUS", d_res);
                }

                if (!double.TryParse(EnvironmentVariables.GetCurrentValue("SHADOW_DEPTH").ToString(), out d_res))
                {
                    d_res = 0;
                    IOField.AppendText("Invalid shadow depth: " + EnvironmentVariables.GetCurrentValue("SHADOW_DEPTH").ToString() + " - var: SHADOW_DEPTH is set to " + d_res);
                    EnvironmentVariables.ChangeCurrentValue("SHADOW_DEPTH", d_res);
                }

                try
                {
                    ColorConverter.ConvertFromString((string)EnvironmentVariables.GetCurrentValue("SHADOW_COLOR"));
                }
                catch (FormatException)
                {
                    IOField.AppendText("Invalid shadow color: " + (string)EnvironmentVariables.GetCurrentValue("SHADOW_COLOR") + " - var: SHADOW_COLOR is set to Silver");
                    EnvironmentVariables.ChangeCurrentValue("SHADOW_COLOR", "Silver");
                }

                KernelType ktype;
                if (!Enum.TryParse((string)EnvironmentVariables.GetCurrentValue("BLUR_TYPE"), true, out ktype))
                {
                    IOField.AppendText("Invalid blur type: " + (string)EnvironmentVariables.GetCurrentValue("BLUR_TYPE") + " - var: BLUR_TYPE is set to Gaussian");
                    EnvironmentVariables.ChangeCurrentValue("BLUR_TYPE", "Gaussian");
                }
                SYSTEM_EFFECTS.ApplyEffects((double)EnvironmentVariables.GetCurrentValue("BLUR_RADIUS"),
                                            (double)EnvironmentVariables.GetCurrentValue("SHADOW_BLUR_RADIUS"),
                                            (double)EnvironmentVariables.GetCurrentValue("SHADOW_DEPTH"),
                                            (Color)ColorConverter.ConvertFromString((string)EnvironmentVariables.GetCurrentValue("SHADOW_COLOR")),
                                            (KernelType)Enum.Parse(typeof(KernelType),
                                            (string)EnvironmentVariables.GetCurrentValue("BLUR_TYPE")));
            }

            Title = $"WinDOS({execLevel})";
            int res;
            if (int.TryParse(EnvironmentVariables.GetCurrentValue("DEF_HIST_SIZE").ToString(), out res))
            {
                DEF_HISTORY = new InputHistory(res + 1);
            }
            else
            {
                DEF_HISTORY = new InputHistory(51);
                IOField.AppendText("Invalid regedit history style: " + EnvironmentVariables.GetCurrentValue("DEF_HIST_SIZE").ToString() + " - var: DEF_HIST_SIZE is set to 50");
                EnvironmentVariables.ChangeCurrentValue("DEF_HIST_SIZE", 50);
            }
            if (int.TryParse(EnvironmentVariables.GetCurrentValue("REG_HIST_SIZE").ToString(), out res))
            {
                REG_HISTORY = new InputHistory(res + 1);
            }
            else
            {
                REG_HISTORY = new InputHistory(51);
                IOField.AppendText("Invalid regedit history style: " + EnvironmentVariables.GetCurrentValue("REG_HIST_SIZE").ToString() + " - var: REG_HIST_SIZE is set to 50");
                EnvironmentVariables.ChangeCurrentValue("REG_HIST_SIZE", 50);
            }
            IOField.TextArea.TextView.LinkTextBackgroundBrush = IOField.Background;
            IOField.TextArea.TextView.LinkTextForegroundBrush = IOField.Foreground;
            IOField.Width = Width - 16;
            IOField.Height = Width - 39;
            IOField.TextArea.SelectionBrush = IOField.Foreground;
            IOField.TextArea.SelectionBorder = null;
            IOField.TextArea.SelectionForeground = Canvas.Background == Brushes.Transparent ? Brushes.Black : Canvas.Background;
            IOField.TextArea.SelectionCornerRadius = 0;
            IOField.VerticalScrollBarVisibility = ScrollBarVisibility.Hidden;
            IOField.HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled;
            IOField.Focus();
        }

        private void LOAD_CMD()
        {
            CommandPool CMD_POOL = new CommandPool(Environment.CurrentDirectory + "\\Commands");
            DEF_CMD_POOL = new CommandPool(CMD_POOL.FindAll(x => x.CLIMode == CLIMode.Default || x.CLIMode == CLIMode.Any).ToArray());
            REG_CMD_POOL = new CommandPool(CMD_POOL.FindAll(x => x.CLIMode == CLIMode.Regedit || x.CLIMode == CLIMode.Any).ToArray());
            CMD_POOL = null;
            CMD_MGMT = new CommandManagement((ExecutionLevel)EnvironmentVariables.GetCurrentValue("EXEC_LEVEL"));
            DEF_AUTO_COMPLETE = new AutoComplete();
            REG_AUTO_COMPLETE = new AutoComplete();
            DEF_CMD_POOL.GetPool().ForEach(x => 
            {
                if (x.Call.Contains("-") && DEF_CMD_POOL.GetPool().Count(y => y.Call.StartsWith(x.Call.Split('-')[0] + "-")) > 1)
                {
                    DEF_AUTO_COMPLETE.AddPool(new AutoCompletePool(x.Call.Split('-')[0] + "-", DEF_CMD_POOL.FindAll(y => y.Call.StartsWith(x.Call.Split('-')[0] + "-")).Select(y => y.Call).ToArray()));
                }
                else
                {
                    DEF_AUTO_COMPLETE.AddStandalone(x.Call);
                }
            });
            REG_CMD_POOL.GetPool().ForEach(x =>
            {
                if (x.Call.Contains("-") && REG_CMD_POOL.GetPool().Count(y => y.Call.StartsWith(x.Call.Split('-')[0] + "-")) > 1)
                {
                    REG_AUTO_COMPLETE.AddPool(new AutoCompletePool(x.Call.Split('-')[0] + "-", REG_CMD_POOL.FindAll(y => y.Call.StartsWith(x.Call.Split('-')[0] + "-")).Select(y => y.Call).ToArray()));
                }
                else
                {
                    REG_AUTO_COMPLETE.AddStandalone(x.Call);
                }
            });
            EnvironmentVariables.Add(new EnvironmentVariable("DEF_CMD_POOL", typeof(CommandPool), DEF_CMD_POOL, DEF_CMD_POOL, VariableType.Runtime));
            EnvironmentVariables.Add(new EnvironmentVariable("REG_CMD_POOL", typeof(CommandPool), REG_CMD_POOL, REG_CMD_POOL, VariableType.Runtime));
            EnvironmentVariables.Add(new EnvironmentVariable("CMD_MGMT", typeof(CommandManagement), CMD_MGMT, CMD_MGMT, VariableType.Runtime));
        }

        private void STARTUP()
        {
            IOField.PreviewKeyDown += IOField_PreviewKeyDown;
            IOField.PreviewMouseDown += IOField_PreviewMouseDown;
            IOField.TextArea.Caret.PositionChanged += Caret_PositionChanged;
            IOField.Loaded += IOField_Loaded;
            SizeChanged += MainWindow_SizeChanged;
            Loaded += MainWindow_Loaded;
            Closing += MainWindow_Closing;
            IOInteractLayer.StandardOutputReceived += IOInteractLayer_StandardOutputReceived;
            IOInteractLayer.StandardErrorReceived += IOInteractLayer_StandardErrorReceived;
            EnvironmentVariables.CurrentValueChanged += EnvironmentVariables_ValueChanged;
            EnvironmentVariables.DefaultValueChanged += EnvironmentVariables_DefaultValueChanged;
            LOAD_CONFIG();
            LOAD_CMD();
            if ((bool)EnvironmentVariables.GetCurrentValue("SHMOTD"))
            {
                IOField.AppendText((string)EnvironmentVariables.GetCurrentValue("MOTD"));
            }
            WRITE_CURRENT_DIR();
            EnvironmentVariables.Add(new EnvironmentVariable("IOFIELD", typeof(TextEditor), IOField, IOField, VariableType.Runtime));
            EnvironmentVariables.Add(new EnvironmentVariable("DEF_HISTORY", typeof(List<string>), DEF_HISTORY, DEF_HISTORY, VariableType.RuntimeConstant));
            EnvironmentVariables.Add(new EnvironmentVariable("REG_HISTORY", typeof(List<string>), REG_HISTORY, REG_HISTORY, VariableType.RuntimeConstant));
            CONSTRUCT_CARET();
            if (!(bool)EnvironmentVariables.GetCurrentValue("SYS_CARET"))
            {
                SYSTEM_CARET.Destroy();
            }
        }
        #endregion

        #region SHELL EVENTS
        private void EnvironmentVariables_DefaultValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (((EnvironmentVariable)sender).VarType == VariableType.Constant)
            {
                WRITE_CONFIG_INI(((EnvironmentVariable)sender).ValueType.FullName, ((EnvironmentVariable)sender).Name, e.NewValue.ToString());
            }  
        }

        private async void EnvironmentVariables_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            switch (((EnvironmentVariable)sender).Name)
            {
                case "DEF_HISTORY" when ((List<string>)e.NewValue).Count == 0:
                    DEF_HISTORY.Clear();
                    break;
                case "REG_HISTORY" when ((List<string>)e.NewValue).Count == 0:
                    DEF_HISTORY.Clear();
                    break;
                case "CRT_NOISE_INTENSITY":
                    try
                    {
                        SYSTEM_EFFECTS.CRTEffect.NoiseIntensity = Convert.ToDouble(e.NewValue);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "CRT_VIGNETTE_SIZE":
                    try
                    {
                        SYSTEM_EFFECTS.NoiseEffect.VignetteSize = Convert.ToDouble(e.NewValue);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "CRT_VIGNETTE_SMOOTHNESS":
                    try
                    {
                        SYSTEM_EFFECTS.NoiseEffect.VignetteSmoothness = Convert.ToDouble(e.NewValue);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "CRT_VIGNETTE_EDGE_ROUNDING":
                    try
                    {
                        SYSTEM_EFFECTS.NoiseEffect.VignetteEdgeRounding = Convert.ToDouble(e.NewValue);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "CRT_SCANLINE_SPEED":
                    try
                    {
                        SYSTEM_EFFECTS.NoiseEffect.ScanLine1Speed = Convert.ToDouble(e.NewValue);
                        SYSTEM_EFFECTS.NoiseEffect.ScanLine2Speed = Convert.ToDouble(e.NewValue);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "CRT_SCANLINE_DENSITY":
                    try
                    {
                        SYSTEM_EFFECTS.NoiseEffect.ScanLine1Density = Convert.ToDouble(e.NewValue);
                        SYSTEM_EFFECTS.NoiseEffect.ScanLine2Density = Convert.ToDouble(e.NewValue);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "CRT_CURVE":
                    try
                    {
                        SYSTEM_EFFECTS.NoiseEffect.Curve = Convert.ToDouble(e.NewValue);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "FORECOLOR":
                    try
                    {
                        IOField.Foreground = new BrushConverter().ConvertFromString((string)e.NewValue) as SolidColorBrush;
                        IOField.TextArea.SelectionBrush = IOField.Foreground;
                        IOField.TextArea.TextView.LinkTextForegroundBrush = IOField.Foreground;
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "BACKCOLOR":
                    try
                    {
                        Canvas.Background = new BrushConverter().ConvertFromString((string)e.NewValue) as SolidColorBrush;
                        IOField.TextArea.SelectionForeground = Canvas.Background == Brushes.Transparent ? Brushes.Black : Canvas.Background;
                        SYSTEM_CARET.SetTextColor(Canvas.Background);
                        SYSTEM_EFFECTS.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "FULLSCREEN":
                    try
                    {
                        if (Convert.ToBoolean(e.NewValue))
                        {
                            LAST_WINDOWED_WIDTH = Width;
                            LAST_WINDOWED_HEIGHT = Height;
                            WindowStyle = WindowStyle.None;
                            ResizeMode = ResizeMode.NoResize;
                            Left = 0;
                            Top = 0;
                            Width = SystemParameters.PrimaryScreenWidth;
                            Height = SystemParameters.PrimaryScreenHeight;
                            Topmost = true;
                        }
                        else
                        {
                            WindowState = WindowState.Normal;
                            WindowStyle = WindowStyle.SingleBorderWindow;
                            ResizeMode = ResizeMode.CanResize;
                            Left = (SystemParameters.PrimaryScreenWidth / 2) - (LAST_WINDOWED_WIDTH / 2);
                            Top = (SystemParameters.PrimaryScreenHeight / 2) - (LAST_WINDOWED_HEIGHT / 2);
                            Width = LAST_WINDOWED_WIDTH;
                            Height = LAST_WINDOWED_HEIGHT;
                        }
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET":
                    try
                    {
                        if (Convert.ToBoolean(e.NewValue))
                        {
                            if (SYSTEM_CARET.IsDestroyed)
                            {
                                SYSTEM_CARET.Create();
                            }
                        }
                        else
                        {
                            if (!SYSTEM_CARET.IsDestroyed)
                            {
                                SYSTEM_CARET.Destroy();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_FILL_COLOR":
                    try
                    {
                        SYSTEM_CARET.SetFillColor(new BrushConverter().ConvertFromString((string)e.NewValue) as SolidColorBrush);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_BORDER_COLOR":
                    try
                    {
                        SYSTEM_CARET.SetBorderColor(new BrushConverter().ConvertFromString((string)e.NewValue) as SolidColorBrush);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "DEF_HIST_SIZE":
                    try
                    {
                        DEF_HISTORY.SetHistorySize(Convert.ToInt32(e.NewValue) + 1);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "REG_HIST_SIZE":
                    try
                    {
                        REG_HISTORY.SetHistorySize(Convert.ToInt32(e.NewValue) + 1);
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "BLUR_RADIUS":
                    try
                    {
                        SYSTEM_EFFECTS.BlurRadius = Convert.ToDouble(e.NewValue);
                        SYSTEM_EFFECTS.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "BLUR_TYPE":
                    try
                    {
                        SYSTEM_EFFECTS.BlurType = (KernelType)Enum.Parse(typeof(KernelType), ((string)e.NewValue).ToUpper()[0] + ((string)e.NewValue).ToLower().Remove(0, 1));
                        SYSTEM_EFFECTS.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SHADOW_BLUR_RADIUS":
                    try
                    {
                        SYSTEM_EFFECTS.ShadowBlurRadius = Convert.ToDouble(e.NewValue);
                        SYSTEM_EFFECTS.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SHADOW_DEPTH":
                    try
                    {
                        SYSTEM_EFFECTS.ShadowDepth = Convert.ToDouble(e.NewValue);
                        SYSTEM_EFFECTS.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SHADOW_COLOR":
                    try
                    {
                        SYSTEM_EFFECTS.ShadowColor = (new BrushConverter().ConvertFromString((string)e.NewValue) as SolidColorBrush).Color;
                        SYSTEM_EFFECTS.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_SHADOW_BLUR_RADIUS":
                    try
                    {
                        SYSTEM_CARET.ShadowBlurRadius = Convert.ToDouble(e.NewValue);
                        SYSTEM_CARET.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_SHADOW_DEPTH":
                    try
                    {
                        SYSTEM_CARET.ShadowDepth = Convert.ToDouble(e.NewValue);
                        SYSTEM_CARET.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_SHADOW_COLOR":
                    try
                    {
                        SYSTEM_CARET.ShadowColor = (new BrushConverter().ConvertFromString((string)e.NewValue) as SolidColorBrush).Color;
                        SYSTEM_CARET.ReApplyEffects();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_BLINK_INTERVAL":
                    try
                    {
                        SYSTEM_CARET.SetBlinkInterval(Convert.ToInt32(e.NewValue));
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_BLINK_IDLE_TIME":
                    try
                    {
                        SYSTEM_CARET.SetBlinkIdleTime(Convert.ToInt32(e.NewValue));
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_FOOTPRINT_FADEOUT_SPEED":
                    try
                    {
                        SYSTEM_CARET.SetFootprintFadeOutSpeed(Convert.ToInt32(e.NewValue));
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_FADEIN_SPEED":
                    try
                    {
                        SYSTEM_CARET.EndBlink();
                        SYSTEM_CARET.SetFadeInSpeed(Convert.ToInt32(e.NewValue));
                        await SYSTEM_CARET.BeginBlink();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_FADEOUT_SPEED":
                    try
                    {
                        SYSTEM_CARET.EndBlink();
                        SYSTEM_CARET.SetFadeOutSpeed(Convert.ToInt32(e.NewValue));
                        await SYSTEM_CARET.BeginBlink();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_BLINK":
                    try
                    {
                        if (!Convert.ToBoolean(e.NewValue))
                        {
                            SYSTEM_CARET.EndBlink();
                        }
                        else
                        {
                            await SYSTEM_CARET.BeginBlink();
                        }
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_WIDTH":
                    try
                    {
                        SYSTEM_CARET.SetWidth(Convert.ToInt32(e.NewValue));
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_HEIGHT":
                    try
                    {
                        SYSTEM_CARET.SetHeight(Convert.ToInt32(e.NewValue));
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_BORDER":
                    try
                    {
                        SYSTEM_CARET.SetBorderThickness(Convert.ToInt32(e.NewValue));
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "FONT_FAMILY":
                    try
                    {
                        System.Drawing.Text.InstalledFontCollection ifc = new System.Drawing.Text.InstalledFontCollection();

                        if (Application.Current.Resources.Contains((string)e.NewValue))
                        {
                            IOField.Style = (Style)Application.Current.Resources[(string)e.NewValue];
                        }
                        else if (ifc.Families.ToList().Exists(x => x.Name == (string)e.NewValue))
                        {
                            Style s = new Style();
                            s.Setters.Add(new Setter(FontFamilyProperty, new FontFamily((string)e.NewValue)));
                            IOField.Style = s;
                        }
                        else
                        {
                            IOField.Style = (Style)Application.Current.Resources["IBM_EGA9"];
                        }

                        if (!SYSTEM_CARET.IsDestroyed)
                        {
                            if (SYSTEM_CARET.AutoHeight)
                            {
                                SYSTEM_CARET.SetHeight(0);
                            }
                            if (SYSTEM_CARET.AutoWidth)
                            {
                                SYSTEM_CARET.SetWidth(0);
                            }
                        }
                        SYSTEM_CARET.RefreshFont();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "FONT_STYLE":
                    try
                    {
                        IOField.FontStyle = ((string)e.NewValue).ToLower() == "regular" ? FontStyles.Normal : ((string)e.NewValue).ToLower() == "bold" ? FontStyles.Oblique : ((string)e.NewValue).ToLower() == "italic" ? FontStyles.Oblique : throw new Exception();
                    }
                    catch (Exception)
                    {
                        IOField.AppendText("\nInvalid font style: " + e.NewValue + "\n" + EnvironmentVariables.GetCurrentValue("DIRECTORY") + "> ");
                    }
                    break;
                case "FONT_SIZE":
                    try
                    {
                        IOField.FontSize = Convert.ToInt32(e.NewValue);
                        if (!SYSTEM_CARET.IsDestroyed)
                        {
                            if (SYSTEM_CARET.AutoHeight)
                            {
                                SYSTEM_CARET.SetHeight(0);
                            }
                            if (SYSTEM_CARET.AutoWidth)
                            {
                                SYSTEM_CARET.SetWidth(0);
                            }
                        }
                        SYSTEM_CARET.RefreshFont();
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
                case "SYS_CARET_FOOTPRINT":
                    try
                    {
                        if (Convert.ToBoolean(e.NewValue))
                        {
                            SYSTEM_CARET.LeaveFootprint = true;
                        }
                        else
                        {
                            SYSTEM_CARET.LeaveFootprint = false;
                        }
                    }
                    catch (Exception ex)
                    {
                        IOField.AppendText("\n" + ex.Message);
                    }
                    break;
            }
        }

        private void IOInteractLayer_StandardErrorReceived(object sender, IOInteractStdErrEventArgs e)
        {
            IOField.AppendText("\n" + e.Error.Message);
            IOField.ScrollToEnd();
        }

        private void IOInteractLayer_StandardOutputReceived(object sender, IOInteractStdOutEventArgs e)
        {
            IOField.AppendText(e.Output);
            IOField.ScrollToEnd();
        }

        #endregion

        #region IO EVENTS
        private async void Caret_PositionChanged(object sender, EventArgs e)
        {
            if (SYSTEM_CARET != null && !SYSTEM_CARET.IsDestroyed)
            {
                await SYSTEM_CARET.Update();
            }
        }

        private void MainWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            if ((bool)EnvironmentVariables.GetCurrentValue("FULLSCREEN"))
            {
                IOField.Width = ActualWidth;
                IOField.Height = ActualHeight;
                Canvas.Width = ActualWidth;
                Canvas.Height = ActualHeight;
                CaretCanvas.Width = ActualWidth;
                CaretCanvas.Height = ActualHeight;
            }
            else
            {
                IOField.Width = ActualWidth - 16;
                IOField.Height = ActualHeight - 39;
                Canvas.Width = ActualWidth - 16;
                Canvas.Height = ActualHeight - 39;
                CaretCanvas.Width = ActualWidth - 16;
                CaretCanvas.Height = ActualHeight - 39;
            }
            if (SYSTEM_CARET != null)
            {
                SYSTEM_CARET.Refresh();
            }
        }

        private void IOField_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;
        }

        private async void IOField_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (EnvironmentVariables.GetCurrentValue("CLI_MODE").ToString() != "Text" && EnvironmentVariables.GetCurrentValue("CLI_MODE").ToString() != "Any")
            {
                switch (e.Key)
                {
                    case Key.Enter when IOField.IsReadOnly == false:
                        e.Handled = true;
                        switch ((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")))
                        {
                            case CLIMode.Default:
                                DEF_HISTORY.SaveToHistory(EXTRACT_INPUT());
                                break;
                            case CLIMode.Regedit:
                                REG_HISTORY.SaveToHistory(EXTRACT_INPUT());
                                break;
                        }
                        
                        if (IOField.Text.Split('\n').Last().Length > (EnvironmentVariables.GetCurrentValue("DIRECTORY").ToString() + "> ").Length)
                        {
                            try
                            {
                                Command cmd = CMD_MGMT.GetCommand(EXTRACT_INPUT(), (CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")) == CLIMode.Default ? DEF_CMD_POOL : REG_CMD_POOL);
                                if (cmd.AsyncFunction == null)
                                {
                                    IOField.AppendText(CMD_MGMT.ExecuteCommand(cmd));
                                }
                                else
                                {
                                    string res = await Task.Run(async () => await CMD_MGMT.ExecuteAsyncCommand(cmd));
                                    IOField.AppendText(res);
                                }
                                WRITE_CURRENT_DIR();
                            }
                            catch (Exception ex)
                            {
                                IOField.AppendText($"\n{ex.Message}");
                                WRITE_CURRENT_DIR();
                            }
                        }
                        else
                        {
                            WRITE_CURRENT_DIR();
                        }
                        DEF_HISTORY.SetIndex(0);
                        REG_HISTORY.SetIndex(0);
                        IOField.CaretOffset = IOField.Text.Length;
                        IOField.ScrollToEnd();
                        break;
                    case Key.Left when IOField.IsReadOnly == false:
                        if (IOField.TextArea.Caret.Location.Column == (EnvironmentVariables.GetCurrentValue((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")) == CLIMode.Default ? "DIRECTORY" : "SUBKEY").ToString() + "> ").Length + 1)
                        {
                            e.Handled = true;
                        }
                        break;
                    case Key.Right when IOField.IsReadOnly == false:
                        break;
                    case Key.Back when IOField.IsReadOnly == false:
                        if (IOField.TextArea.Caret.Location.Column == (EnvironmentVariables.GetCurrentValue((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")) == CLIMode.Default ? "DIRECTORY" : "SUBKEY").ToString() + "> ").Length + 1)
                        {
                            e.Handled = true;
                        }
                        break;
                    case Key.Up when IOField.IsReadOnly == false:
                        e.Handled = true;
                        switch ((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")))
                        {
                            case CLIMode.Default:
                                DEF_HISTORY.ScrollTowardsOldest(IOField);
                                break;
                            case CLIMode.Regedit:
                                REG_HISTORY.ScrollTowardsOldest(IOField);
                                break;
                        }
                        break;
                    case Key.Down when IOField.IsReadOnly == false:
                        e.Handled = true;
                        switch ((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")))
                        {
                            case CLIMode.Default:
                                DEF_HISTORY.ScrollTowardsNewest(IOField);
                                break;
                            case CLIMode.Regedit:
                                REG_HISTORY.ScrollTowardsNewest(IOField);
                                break;
                        }
                        break;
                    case Key.Tab when IOField.IsReadOnly == false:
                        e.Handled = true;
                        switch ((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")))
                        {
                            case CLIMode.Default:
                                IOField.AppendText(DEF_AUTO_COMPLETE.Complete(IOField.Text.Split('\n').Last().Remove(0, IOField.Text.Split('\n').Last().IndexOf(' ') + 1)));
                                break;
                            case CLIMode.Regedit:
                                IOField.AppendText(REG_AUTO_COMPLETE.Complete(IOField.Text.Split('\n').Last().Remove(0, IOField.Text.Split('\n').Last().IndexOf(' ') + 1)));
                                break;
                        }
                        break;
                }
            } 
        }

        private async void IOField_Loaded(object sender, RoutedEventArgs e)
        {
            if (!SYSTEM_CARET.IsDestroyed)
            {
                await SYSTEM_CARET.Update();
            }
            if ((bool)EnvironmentVariables.GetCurrentValue("SYS_CARET_BLINK") && SYSTEM_CARET != null)
            {
                if (!SYSTEM_CARET.IsBlinking)
                {
                    await SYSTEM_CARET.BeginBlink();
                }
            }
        }

        private void MainWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            SYSTEM_EFFECTS.EndShaderUpdate();
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await SYSTEM_EFFECTS.BeginShaderUpdate();
        }
        #endregion
    }

    #region SYSTEM COMPONENTS
    public class InputHistory
    {
        public List<string> INPUT_LIST { get; private set; }

        public int CurrentIndex { get; private set; }

        public int HistorySize { get; private set; }

        public InputHistory(int size)
        {
            CurrentIndex = 0;
            HistorySize = size;
            INPUT_LIST = new List<string>();
            INPUT_LIST.Add("");
        }

        public void ScrollTowardsOldest(TextEditor target)
        {
            if (CurrentIndex < INPUT_LIST.Count - 1)
            {
                if (CurrentIndex == 0)
                {
                    INPUT_LIST[0] = target.Text.Split('\n').Last().Replace(EnvironmentVariables.GetCurrentValue((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")) == CLIMode.Default ? "DIRECTORY" : "SUBKEY").ToString() + "> ", "");
                }
                CurrentIndex++;
                try
                {
                    target.Text = target.Text.Remove(target.Text.LastIndexOf('\n') + 1) + EnvironmentVariables.GetCurrentValue((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")) == CLIMode.Default ? "DIRECTORY" : "SUBKEY").ToString() + "> " + INPUT_LIST[CurrentIndex];
                }
                catch (Exception) { }
                target.CaretOffset = target.Text.Length;
            }
        }

        public void ScrollTowardsNewest(TextEditor target)
        {
            if (CurrentIndex > 0)
            {
                CurrentIndex--;
                try
                {
                    target.Text = target.Text.Remove(target.Text.LastIndexOf('\n') + 1) + EnvironmentVariables.GetCurrentValue((CLIMode)Enum.Parse(typeof(CLIMode), (string)EnvironmentVariables.GetCurrentValue("CLI_MODE")) == CLIMode.Default ? "DIRECTORY" : "SUBKEY").ToString() + "> " + INPUT_LIST[CurrentIndex];
                }
                catch (Exception) { }
                target.CaretOffset = target.Text.Length;
            }
        }

        public void SetIndex(int index) => CurrentIndex = index;

        public void SetHistorySize(int newSize)
        {
            HistorySize = newSize;
            if (INPUT_LIST.Count > HistorySize)
            {
                INPUT_LIST.RemoveRange(HistorySize, INPUT_LIST.Count - HistorySize);
            }
        }

        public void SaveToHistory(string input)
        {
            if (INPUT_LIST.Count < HistorySize)
            {
                if (INPUT_LIST.Count > 1)
                {
                    INPUT_LIST.Insert(1, input);
                }
                else
                {
                    INPUT_LIST.Add(input);
                }
            }
        }

        public void Clear()
        {
            INPUT_LIST.Clear();
            INPUT_LIST.Add("");
        }
    }

    public class AutoComplete
    {
        private List<AutoCompletePool> POOL_LST;

        private List<string> STANDALONE_LST;

        public AutoComplete()
        {
            POOL_LST = new List<AutoCompletePool>();
            STANDALONE_LST = new List<string>();
        }

        public void AddPool(params AutoCompletePool[] pools)
        {
            POOL_LST.AddRange(pools);
        }

        public void AddStandalone(params string[] standalones)
        {
            STANDALONE_LST.AddRange(standalones);
        }

        public string Complete(string input)
        {
            string result = STANDALONE_LST.Find(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase));
            if (result != null)
            {
                return result.ToLower().Remove(0, input.Length);
            }
            if (POOL_LST.Count > 0)
            {
                result = POOL_LST.Exists(x => x.Name.StartsWith(input, StringComparison.OrdinalIgnoreCase)) ? POOL_LST.Find(x => x.Name.StartsWith(input, StringComparison.OrdinalIgnoreCase)).Name : "";
                if (result.ToLower() == input.ToLower())
                {
                    return "";
                }
                result = result == null ? "" : POOL_LST.Exists(x => input.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase)) ? POOL_LST.Find(x => input.StartsWith(x.Name, StringComparison.OrdinalIgnoreCase)).Results.Find(x => x.StartsWith(input, StringComparison.OrdinalIgnoreCase)) : result;
            }
            return result == null || result.Length == 0 ? "" : result.ToLower().Remove(0, input.Length);
        }
    }

    public class AutoCompletePool
    {
        public string Name;

        public List<string> Results;

        public AutoCompletePool(string name, params string[] results)
        {
            Name = name;
            Results = new List<string>();
            Results.AddRange(results);
        }

        public void AddResult(string res)
        {
            if (!Results.Contains(res))
            {
                Results.Add(res);
            }
        }

        public void AddRange(params string[] results)
        {
            Results.AddRange(results);
        }
    }

    public class SystemEffects
    {
        private CancellationTokenSource CTS;

        private CancellationToken CT;

        public DateTime start = DateTime.Now;

        private Grid SYS_GRID;

        private Canvas SYS_CANVAS;

        private Canvas CARET_CANVAS;

        private TextEditor SYS_IO;

        public NoiseEffect CRTEffect { get; private set; }

        public CRTEffect NoiseEffect { get; private set; }

        public double BlurRadius { get; set; }

        public double ShadowBlurRadius { get; set; }

        public double ShadowDepth { get; set; }

        public Color ShadowColor { get; set; }

        public KernelType BlurType { get; set; }

        public SystemEffects(Grid grid, Canvas canvas, Canvas caretCanvas, TextEditor io)
        {
            CTS = new CancellationTokenSource();
            CT = CTS.Token;
            SYS_GRID = grid;
            SYS_CANVAS = canvas;
            CARET_CANVAS = caretCanvas;
            SYS_IO = io;
            CRTEffect = new NoiseEffect();
            NoiseEffect = new CRTEffect();
        }

        public void ApplyEffects(double blurRadius, double shadowBlurRadius, double shadowDepth, Color shadowColor, KernelType blurType)
        {
            BlurRadius = blurRadius;
            ShadowBlurRadius = shadowBlurRadius;
            ShadowDepth = shadowDepth;
            ShadowColor = shadowColor;
            BlurType = blurType;
            SYS_GRID.Effect = new BlurEffect() { Radius = BlurRadius, KernelType = BlurType };
            if (SYS_CANVAS.Background == Brushes.Transparent)
            {
                SYS_CANVAS.Effect = new DropShadowEffect() { Color = ShadowColor, BlurRadius = ShadowBlurRadius, ShadowDepth = ShadowDepth };
            }
            else
            {
                SYS_IO.TextArea.Effect = new DropShadowEffect() { Color = ShadowColor, BlurRadius = ShadowBlurRadius, ShadowDepth = ShadowDepth };
            }

            SYS_IO.Effect = CRTEffect;
            CARET_CANVAS.Effect = NoiseEffect;
        }

        public void ReApplyEffects()
        {
            SYS_GRID.Effect = null;
            SYS_CANVAS.Effect = null;
            SYS_IO.TextArea.Effect = null;
            SYS_GRID.Effect = new BlurEffect() { Radius = BlurRadius, KernelType = BlurType };
            if (SYS_CANVAS.Background == Brushes.Transparent)
            {
                SYS_CANVAS.Effect = new DropShadowEffect() { Color = ShadowColor, BlurRadius = ShadowBlurRadius, ShadowDepth = ShadowDepth };
            }
            else
            {
                SYS_IO.TextArea.Effect = new DropShadowEffect() { Color = ShadowColor, BlurRadius = ShadowBlurRadius, ShadowDepth = ShadowDepth };
            }
        }

        public async Task BeginShaderUpdate()
        {
            await Task.Run(async () =>
            {
                while (!CT.IsCancellationRequested)
                {
                    await Application.Current.Dispatcher.Invoke(async () =>
                    {
                        CRTEffect.SystemTime = (DateTime.Now - CRTEffect.START_TIME).TotalSeconds;
                        NoiseEffect.Time = CRTEffect.SystemTime;
                        await Task.Delay(1);
                    });
                }
            });
        }

        public void EndShaderUpdate()
        {
            CTS.Cancel();
        }
    }

    public class Caret
    {
        private DoubleAnimation FOOTPRINT_FADEOUT;

        private DoubleAnimation BLINK_FADEOUT;

        private DoubleAnimation BLINK_FADEIN;

        private Storyboard BLINK_ANIMATION = new Storyboard();

        private CancellationTokenSource CTS = new CancellationTokenSource();

        private CancellationToken CT;

        private Border CARET;

        private TextEditor TARGET;

        private Grid GRID;

        private Canvas TARGET_CANVAS;

        private Canvas CANVAS;

        public bool IsDestroyed { get; private set; }

        public bool IsBlinking { get; private set; }

        public bool LeaveFootprint { get; set; }

        private int BLINK_IDLE_TIME;

        private int BLINK_INTERVAL;

        private int FADEIN_SPEED;

        private int FADEOUT_SPEED;

        private int FOOTPRINT_FADEOUT_SPEED;

        public double ShadowBlurRadius { get; set; }

        public double ShadowDepth { get; set; }

        public Color ShadowColor { get; set; }

        public bool AutoHeight { get; private set; }

        public bool AutoWidth { get; private set; }

        public Caret(TextEditor target, Canvas canvas, Canvas target_canvas, Grid grid, int height, int width, int borderThickness, Brush fill, Brush border, bool footprint, int fadeinSpeed, int fadeoutSpeed, int footprintFadeoutSpeed, int blinkInterval, int blinkIdleTime, double shadowBlurRadius, double shadowDepth, Color shadowColor)
        {
            GRID = grid;
            CARET = new Border();
            CARET.BorderThickness = new Thickness(borderThickness);
            CANVAS = canvas;
            TARGET_CANVAS = target_canvas;
            CANVAS.Children.Add(CARET);
            CARET.Height = height == 0 ? Math.Ceiling(target.FontSize * target.FontFamily.LineSpacing) : height;
            CARET.Width = width == 0 ? target.FontSize / 3 * 1.680 : width;
            AutoHeight = height == 0;
            AutoWidth = width == 0;
            CARET.Background = fill;
            CARET.BorderBrush = border;
            TARGET = target;
            CARET.Child = new TextBlock() { Foreground = TARGET.Background == Brushes.Transparent ? Brushes.Black : TARGET.Background, Width = CARET.Width, Height = CARET.Height, Background = Brushes.Transparent, TextAlignment = TextAlignment.Center, FontSize = TARGET.FontSize, Style = TARGET.Style };
            TARGET.TextArea.Caret.CaretBrush = Brushes.Transparent;
            LeaveFootprint = footprint;
            FADEIN_SPEED = fadeinSpeed;
            FADEOUT_SPEED = fadeoutSpeed;
            FOOTPRINT_FADEOUT_SPEED = footprintFadeoutSpeed;
            BLINK_INTERVAL = blinkInterval;
            BLINK_IDLE_TIME = blinkIdleTime;
            ShadowBlurRadius = shadowBlurRadius;
            ShadowDepth = shadowDepth;
            ShadowColor = shadowColor;
            ReApplyEffects();
            BLINK_FADEIN = new DoubleAnimation(0.0, 1.0, new Duration(TimeSpan.FromMilliseconds(FADEIN_SPEED)));
            BLINK_FADEOUT = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(FADEOUT_SPEED)));
            FOOTPRINT_FADEOUT = new DoubleAnimation(1.0, 0.0, new Duration(TimeSpan.FromMilliseconds(FOOTPRINT_FADEOUT_SPEED)));
            BLINK_FADEIN.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(UIElement.OpacityProperty));
            BLINK_FADEOUT.SetValue(Storyboard.TargetPropertyProperty, new PropertyPath(UIElement.OpacityProperty));
            BLINK_FADEOUT.BeginTime = TimeSpan.FromMilliseconds(FADEOUT_SPEED + BLINK_INTERVAL);
            BLINK_ANIMATION.Children.Add(BLINK_FADEIN);
            BLINK_ANIMATION.Children.Add(BLINK_FADEOUT);
            BLINK_ANIMATION.RepeatBehavior = RepeatBehavior.Forever;
        }

        public async Task Update(bool leaveFootprint = true)
        {
            if (!CANVAS.Children.Contains(CARET))
            {
                return;
            }
            Border newPrint = new Border();
            if (LeaveFootprint && leaveFootprint)
            {
                newPrint.Visibility = Visibility.Visible;
                newPrint.Background = CARET.Background;
                newPrint.BorderBrush = CARET.BorderBrush;
                newPrint.BorderThickness = CARET.BorderThickness;
                newPrint.Height = CARET.Height;
                newPrint.Width = CARET.Width;
                newPrint.Effect = CARET.Effect;
                CANVAS.Children.Add(newPrint);

                Canvas.SetLeft(newPrint, Canvas.GetLeft(CARET));
                Canvas.SetTop(newPrint, Canvas.GetTop(CARET));
            }

            Rect caretLocation = TARGET.TextArea.Caret.CalculateCaretRectangle();
            double y = caretLocation.Y - TARGET.TextArea.TextView.VerticalOffset;
            y = y < 0 ? 0 : y > TARGET.ActualHeight - CARET.Height ? TARGET.ActualHeight - CARET.Height : y;

            Canvas.SetLeft(CARET, caretLocation.X - 2);

            Canvas.SetTop(CARET, y + (TARGET.TextArea.TextView.DefaultLineHeight - CARET.Height));

            if (TARGET.CaretOffset <= TARGET.Text.Length - 1 && AutoWidth && AutoHeight)
            {
                ((TextBlock)CARET.Child).Text = TARGET.Text[TARGET.CaretOffset].ToString();
            }
            else
            {
                ((TextBlock)CARET.Child).Text = "";
            }

            if (LeaveFootprint && leaveFootprint)
            {
                FOOTPRINT_FADEOUT.Completed += (o, e) => { newPrint = null; };
                newPrint.BeginAnimation(UIElement.OpacityProperty, FOOTPRINT_FADEOUT);
                await Task.Delay(0);
            }
        }

        public void Refresh()
        {
            Destroy();
            TARGET_CANVAS.Children.Remove(TARGET);
            CANVAS.Children.Remove(TARGET);
            CANVAS.Children.Remove(TARGET_CANVAS);
            GRID.Children.Remove(CANVAS);
            CANVAS = new Canvas() { HorizontalAlignment = HorizontalAlignment.Left, VerticalAlignment = VerticalAlignment.Top, Effect = CANVAS.Effect, Height = 442, Width = 792 };
            GRID.Children.Add(CANVAS);
            CANVAS.Children.Add(TARGET_CANVAS);
            TARGET_CANVAS.Children.Add(TARGET);
            if (Math.Ceiling(TARGET.FontSize * TARGET.FontFamily.LineSpacing) * TARGET.LineCount > TARGET.ActualHeight)
            {
                Canvas.SetTop(CARET, TARGET.ActualHeight - CARET.Height - 1);
            }
            Create();
            TARGET.ScrollToEnd();
        }

        public void Destroy()
        {
            CANVAS.Children.Remove(CARET);
            TARGET.TextArea.Caret.CaretBrush = CARET.Background;
            IsDestroyed = true;
        }

        public void Create()
        {
            CANVAS.Children.Add(CARET);
            TARGET.TextArea.Caret.CaretBrush = Brushes.Transparent;
            IsDestroyed = false;
        }

        public void RefreshFont()
        {
            CARET.Child = new TextBlock() { Foreground = TARGET.Background == Brushes.Transparent ? Brushes.Black : TARGET.Background, Width = CARET.Width, Height = CARET.Height, Background = Brushes.Transparent, TextAlignment = TextAlignment.Center, FontSize = TARGET.FontSize, Style = TARGET.Style };
        }

        public void SetTextColor(Brush color)
        {
            ((TextBlock)CARET.Child).Foreground = color == Brushes.Transparent ? Brushes.Black : color;
        }

        public void SetFillColor(Brush color)
        {
            CARET.Background = color;
        }

        public void SetBorderColor(Brush color)
        {
            CARET.BorderBrush = color;
        }

        public void SetBlinkIdleTime(int blinkIdleTime)
        {
            BLINK_IDLE_TIME = blinkIdleTime;
        }

        public void SetBlinkInterval(int blinkInterval)
        {
            BLINK_INTERVAL = blinkInterval;
        }

        public void SetBorderThickness(int borderThickness)
        {
            CARET.BorderThickness = new Thickness(borderThickness);
        }

        public void SetHeight(int height)
        {
            AutoHeight = height == 0;
            CARET.Height = height == 0 ? Math.Ceiling(TARGET.FontSize * TARGET.FontFamily.LineSpacing) : height;
            ((TextBlock)CARET.Child).Height = CARET.Height;
        }

        public void SetWidth(int width)
        {
            AutoWidth = width == 0;
            CARET.Width = width == 0 ? TARGET.FontSize / 3 * 1.680 : width;
            ((TextBlock)CARET.Child).Height = CARET.Width;
        }

        public void SetVisibility(Visibility visibility)
        {
            CARET.Visibility = visibility;
        }

        public void SetFootprintFadeOutSpeed(int fadeoutSpeed)
        {
            FOOTPRINT_FADEOUT_SPEED = fadeoutSpeed;
            FOOTPRINT_FADEOUT.Duration = new Duration(TimeSpan.FromMilliseconds(FOOTPRINT_FADEOUT_SPEED));
        }

        public void SetFadeInSpeed(int fadeinSpeed)
        {
            FADEIN_SPEED = fadeinSpeed;
            BLINK_FADEIN.Duration = new Duration(TimeSpan.FromMilliseconds(FADEIN_SPEED));
        }

        public void SetFadeOutSpeed(int fadeoutSpeed)
        {
            FADEOUT_SPEED = fadeoutSpeed;
            BLINK_FADEOUT.Duration = new Duration(TimeSpan.FromMilliseconds(FADEOUT_SPEED));
        }

        public void SetPosition(Rect newPosition)
        {
            Canvas.SetLeft(CARET, newPosition.Left);
            Canvas.SetTop(CARET, newPosition.Top);
        }

        public void ApplyEffects(double shadowBlurRadius, double shadowDepth, Color shadowColor)
        {
            ShadowBlurRadius = shadowBlurRadius;
            ShadowDepth = shadowDepth;
            ShadowColor = shadowColor;
            CARET.Effect = new DropShadowEffect() { BlurRadius = ShadowBlurRadius, ShadowDepth = ShadowDepth, Color = ShadowColor };
        }

        public void ReApplyEffects()
        {
            CARET.Effect = new DropShadowEffect() { BlurRadius = ShadowBlurRadius, ShadowDepth = ShadowDepth, Color = ShadowColor };
        }

        public async Task BeginBlink()
        {
            IsBlinking = true;
            CTS = new CancellationTokenSource();
            CT = CTS.Token;
            BLINK_ANIMATION.Begin(CARET, HandoffBehavior.Compose, true);
            while (!CT.IsCancellationRequested)
            {
                if (IdleTimeDetector.GetIdleTimeInfo().IdleTime.TotalMilliseconds < BLINK_IDLE_TIME)
                {
                    CARET.SetCurrentValue(UIElement.OpacityProperty, 1.0);
                    BLINK_ANIMATION.Pause(CARET);
                }
                else
                {
                    BLINK_ANIMATION.Resume(CARET);
                }
                await Task.Delay(1);
            }
        }

        public void EndBlink()
        {
            CTS = new CancellationTokenSource();
            CT = CTS.Token;
            CTS.Cancel();
            IsBlinking = false;
        }

        private static class IdleTimeDetector
        {
            [DllImport("user32.dll")]
            static extern bool GetLastInputInfo(ref LASTINPUTINFO plii);

            public static IdleTimeInfo GetIdleTimeInfo()
            {
                int systemUptime = Environment.TickCount,
                    lastInputTicks = 0,
                    idleTicks = 0;

                LASTINPUTINFO lastInputInfo = new LASTINPUTINFO();
                lastInputInfo.cbSize = (uint)Marshal.SizeOf(lastInputInfo);
                lastInputInfo.dwTime = 0;

                if (GetLastInputInfo(ref lastInputInfo))
                {
                    lastInputTicks = (int)lastInputInfo.dwTime;

                    idleTicks = systemUptime - lastInputTicks;
                }

                return new IdleTimeInfo
                {
                    LastInputTime = DateTime.Now.AddMilliseconds(-1 * idleTicks),
                    IdleTime = new TimeSpan(0, 0, 0, 0, idleTicks),
                    SystemUptimeMilliseconds = systemUptime,
                };
            }

            public class IdleTimeInfo
            {
                public DateTime LastInputTime { get; internal set; }

                public TimeSpan IdleTime { get; internal set; }

                public int SystemUptimeMilliseconds { get; internal set; }
            }

            internal struct LASTINPUTINFO
            {
                public uint cbSize;
                public uint dwTime;
            }
        }
    }

    public class NoiseEffect : ShaderEffect
    {
        private static PixelShader SHADER = new PixelShader() { UriSource = MakePackUri("Shader\\Noise_Shader.fxc") };

        public DateTime START_TIME = DateTime.Now;

        public NoiseEffect()
        {
            PixelShader = SHADER;

            UpdateShaderValue(InputProperty);
            UpdateShaderValue(TextureSizeProperty);
            UpdateShaderValue(SystemTimeProperty);
            UpdateShaderValue(NoiseResolutionProperty);
        }

        public static Uri MakePackUri(string relativeFile)
        {
            Assembly a = typeof(NoiseEffect).Assembly;
            string assemblyShortName = a.ToString().Split(',')[0];
            string uriString = "pack://application:,,,/" + assemblyShortName + ";component/" + relativeFile;
            return new Uri(uriString);
        }

        #region Input dependency property

        public Brush Input
        {
            get { return (Brush)GetValue(InputProperty); }
            set { SetValue(InputProperty, value); }
        }

        public static readonly DependencyProperty InputProperty = RegisterPixelShaderSamplerProperty("Input", typeof(NoiseEffect), 0);

        #endregion

        #region TextureSize dependency property

        public double TextureSize
        {
            get { return (double)GetValue(TextureSizeProperty); }
            set { SetValue(TextureSizeProperty, value); }
        }

        public static readonly DependencyProperty TextureSizeProperty = DependencyProperty.Register("TextureSize", typeof(double), typeof(NoiseEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(0)));

        #endregion

        #region SystemTime dependency property

        public double SystemTime
        {
            get { return (double)GetValue(SystemTimeProperty); }
            set { SetValue(SystemTimeProperty, value); }
        }

        public static readonly DependencyProperty SystemTimeProperty = DependencyProperty.Register("SystemTime", typeof(double), typeof(NoiseEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(2)));

        #endregion

        #region NoiseResolution dependency property

        public double NoiseIntensity
        {
            get { return (double)GetValue(NoiseResolutionProperty); }
            set { SetValue(NoiseResolutionProperty, value); }
        }

        public static readonly DependencyProperty NoiseResolutionProperty = DependencyProperty.Register("NoiseResolution", typeof(double), typeof(NoiseEffect), new UIPropertyMetadata(100.0, PixelShaderConstantCallback(3)));

        #endregion
    }

    public class CRTEffect : ShaderEffect
    {
        private static PixelShader pixelShader = new PixelShader() { UriSource = MakePackUri("Shader\\CRT_Shader.fxc") };

        public CRTEffect()
        {
            PixelShader = pixelShader;
            UpdateShaderValue(SamplerProperty);
            UpdateShaderValue(CurveProperty);
            UpdateShaderValue(SizeProperty);
            UpdateShaderValue(VignetteSizeProperty);
            UpdateShaderValue(VignetteSmoothnessProperty);
            UpdateShaderValue(VignetteEdgeRoundingProperty);
            UpdateShaderValue(TimeProperty);
            UpdateShaderValue(ScanLine1DensityProperty);
            UpdateShaderValue(ScanLine2DensityProperty);
            UpdateShaderValue(ScanLine1SpeedProperty);
            UpdateShaderValue(ScanLine2SpeedProperty);
        }

        public static Uri MakePackUri(string relativeFile)
        {
            Assembly a = typeof(CRTEffect).Assembly;
            string assemblyShortName = a.ToString().Split(',')[0];
            string uriString = "pack://application:,,,/" + assemblyShortName + ";component/" + relativeFile;
            return new Uri(uriString);
        }


        #region Input dependency property

        public Brush Sampler
        {
            get { return (Brush)GetValue(SamplerProperty); }
            set { SetValue(SamplerProperty, value); }
        }

        public static readonly DependencyProperty SamplerProperty =  RegisterPixelShaderSamplerProperty("Sampler", typeof(CRTEffect), 1);

        #endregion

        #region Size dependency property

        public double Size
        {
            get { return (double)GetValue(SizeProperty); }
            set { SetValue(SizeProperty, value); }
        }

        public static readonly DependencyProperty SizeProperty = DependencyProperty.Register("Size", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(1.0, PixelShaderConstantCallback(11)));

        #endregion

        #region Curve dependency property

        public double Curve
        {
            get { return (double)GetValue(CurveProperty); }
            set { SetValue(CurveProperty, value); }
        }

        public static readonly DependencyProperty CurveProperty = DependencyProperty.Register("Curve", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(0.03, PixelShaderConstantCallback(1)));

        #endregion

        #region VignetteSize dependency property

        public double VignetteSize
        {
            get { return (double)GetValue(VignetteSizeProperty); }
            set { SetValue(VignetteSizeProperty, value); }
        }

        public static readonly DependencyProperty VignetteSizeProperty = DependencyProperty.Register("VignetteSize", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(1.9, PixelShaderConstantCallback(8)));

        #endregion

        #region VignetteSmoothness dependency property

        public double VignetteSmoothness
        {
            get { return (double)GetValue(VignetteSmoothnessProperty); }
            set { SetValue(VignetteSmoothnessProperty, value); }
        }

        public static readonly DependencyProperty VignetteSmoothnessProperty = DependencyProperty.Register("VignetteSmoothness", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(0.6, PixelShaderConstantCallback(9)));

        #endregion

        #region VignetteEdgeRounding dependency property

        public double VignetteEdgeRounding
        {
            get { return (double)GetValue(VignetteEdgeRoundingProperty); }
            set { SetValue(VignetteEdgeRoundingProperty, value); }
        }

        public static readonly DependencyProperty VignetteEdgeRoundingProperty = DependencyProperty.Register("VignetteEdgeRounding", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(8.0, PixelShaderConstantCallback(10)));

        #endregion

        #region SystemTime dependency property

        public double Time
        {
            get { return (double)GetValue(TimeProperty); }
            set { SetValue(TimeProperty, value); }
        }

        public static readonly DependencyProperty TimeProperty = DependencyProperty.Register("Time", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(12)));

        #endregion

        #region ScanLine1Density dependency property

        public double ScanLine1Density
        {
            get { return (double)GetValue(ScanLine1DensityProperty); }
            set { SetValue(ScanLine1DensityProperty, value); }
        }

        public static readonly DependencyProperty ScanLine1DensityProperty = DependencyProperty.Register("ScanLine1Density", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(4)));

        #endregion

        #region ScanLine2Density dependency property

        public double ScanLine2Density
        {
            get { return (double)GetValue(ScanLine2DensityProperty); }
            set { SetValue(ScanLine2DensityProperty, value); }
        }

        public static readonly DependencyProperty ScanLine2DensityProperty = DependencyProperty.Register("ScanLine2Density", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(5)));

        #endregion

        #region ScanLine1Speed dependency property

        public double ScanLine1Speed
        {
            get { return (double)GetValue(ScanLine1SpeedProperty); }
            set { SetValue(ScanLine1SpeedProperty, value); }
        }

        public static readonly DependencyProperty ScanLine1SpeedProperty = DependencyProperty.Register("ScanLine1Speed", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(6)));

        #endregion

        #region ScanLine2Speed dependency property

        public double ScanLine2Speed
        {
            get { return (double)GetValue(ScanLine2SpeedProperty); }
            set { SetValue(ScanLine2SpeedProperty, value); }
        }

        public static readonly DependencyProperty ScanLine2SpeedProperty = DependencyProperty.Register("ScanLine2Speed", typeof(double), typeof(CRTEffect), new UIPropertyMetadata(0.0, PixelShaderConstantCallback(7)));

        #endregion
    }
    #endregion
}
