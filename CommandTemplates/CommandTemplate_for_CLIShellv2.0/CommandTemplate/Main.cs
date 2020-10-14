using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using CLIShell;
using ICSharpCode.AvalonEdit;

namespace CommandTemplate
{
    public class Main
    {
        //Environment variables
        //
        //  Constant:  
        //      DIRECTORY
        //      FORECOLOR
        //      BACKCOLOR
        //      FONT_FAMILY
        //      FONT_SIZE
        //      FONT_STYLE
        //      FULLSCREEN
        //      SHMOTD
        //      MOTD
        //      SUBKEY
        //      DEF_HIST_SIZE
        //      REG_HIST_SIZE
        //      SYS_CARET_BLINK
        //      SYS_CARET_BLINK_INTERVAL
        //      SYS_EFFECTS
        //      SYS_CARET
        //      SYS_CARET_HEIGHT
        //      SYS_CARET_WIDTH
        //      SYS_CARET_BORDER
        //      SYS_CARET_FOOTPRINT
        //      SYS_CARET_FADEIN_SPEED
        //      SYS_CARET_FADEOUT_SPEED
        //      SYS_CARET_FOOTPRINT_FADEOUT_SPEED
        //      SYS_CARET_BLINK_IDLE_TIME
        //      SYS_CARET_FILL_COLOR
        //      SYS_CARET_SHADOW_BLUR_RADIUS
        //      SYS_CARET_SHADOW_DEPTH
        //      SYS_CARET_SHADOW_COLOR
        //      SYS_CARET_BORDER_COLOR
        //      BLUR_RADIUS
        //      SHADOW_BLUR_RADIUS
        //      SHADOW_DEPTH
        //      SHADOW_COLOR
        //      BLUR_TYPE
        //      SHADOW_BLUR_RADIUS
        //      SHADOW_DEPTH
        //      SHADOW_COLOR
        //      BLUR_TYPE
        //      CRT_CURVE
        //      CRT_NOISE_ITENSITY
        //      CRT_SCANLINE_DENSITY
        //      CRT_SCANLINE_SPEED
        //      CRT_VIGNETTE_SIZE
        //      CRT_VIGNETTE_SMOOTHNESS
        //      CRT_VIGNETTE_EDGE_ROUNDING
        //      CLI_MODE
        //  
        //  Runtime:
        //      IOFIELD (TextEditor object, AvalonEdit required!)
        //      DEF_CMD_POOL
        //      REG_CMD_POOL
        //      CMD_MGMT (CommandManagenemt object)
        //
        //  RuntimeConstant:
        //      EXEC_LEVEL
        //      DEF_HISTORY
        //      REG_HISTORY


        public Command CMD = null;

        public ArgumentTable TABLE = null;

        public Command GetCommand()
        {
            CMD = new Command("CMD_NAME", TABLE, false, "DESCRIPTION", ExecutionLevel.User, CLIMode.Default);

            CMD.SetFunction(() =>
            {
                //Code here...
                
                return "";
            });
            //OR
            CMD.SetAsyncFunction(async () =>
            {
                //Code here...

                return "";
            });
            //If both set, async function has higher priority!

            return CMD;
        }
    }
}
