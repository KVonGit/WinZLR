/*  MIT License

    Copyright(c) 2021-2025 Henrik Åsman

    Permission Is hereby granted, free Of charge, to any person obtaining a copy
    of this software And associated documentation files (the "Software"), to deal
    in the Software without restriction, including without limitation the rights
    to use, copy, modify, merge, publish, distribute, sublicense, And/Or sell
    copies of the Software, And to permit persons to whom the Software Is
    furnished to do so, subject to the following conditions:

    The above copyright notice And this permission notice shall be included In all
    copies Or substantial portions of the Software.

    THE SOFTWARE Is PROVIDED "AS IS", WITHOUT WARRANTY Of ANY KIND, EXPRESS Or
    IMPLIED, INCLUDING BUT Not LIMITED To THE WARRANTIES Of MERCHANTABILITY,
    FITNESS FOR A PARTICULAR PURPOSE And NONINFRINGEMENT. IN NO EVENT SHALL THE
    AUTHORS Or COPYRIGHT HOLDERS BE LIABLE For ANY CLAIM, DAMAGES Or OTHER
    LIABILITY, WHETHER In AN ACTION Of CONTRACT, TORT Or OTHERWISE, ARISING FROM,
    OUT OF Or IN CONNECTION WITH THE SOFTWARE Or THE USE Or OTHER DEALINGS IN THE
    SOFTWARE.                                                                         */

using Microsoft.VisualBasic;
using System;
using System.Collections;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Numerics;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Xml;
using System.Security.Policy;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using winZLR.WinFormsSyntaxHighlighter;

// WinFormsSyntaxHighlighter by Sina Iravanian, https://github.com/sinairv/WinFormsSyntaxHighlighter
// ConsoleZLR by Tara McGrew, https://foss.heptapod.net/zilf/zlr/-/tree/branch/default
//                            
// * Setting for UnZ syntax format
// * Object tree and object details
// * Allow use of hex when changing value
// * Toolbar & menu
// * Move DLLImport to seperate class
// * Credit for WinFormsSyntaxHighlighter and ConsoleZLR
// * Setting: matching brackets (or not)
// * Regex for Hamburg.inf, line 285
// * Regex for advent_puny, line 1472 'e\\'
// * Respect position info when highlighting code
// * Read arrays, actions & globals from debugInfo and make regex
// * Highlight debugInfo.Actions
// * Highlight debugInfo.Arrays
// * Highlight debugInfo.Attributes
// * Highlight debugInfo.Objects
// * Highlight debugInfo.Properties
// * Highlight debugInfo.Routines
// * FindNext, FindPrevious make sure that hit is visible
// * Work directly againts ZLR.VM
// * << (back) and >> (forward) for stepping through codepoints
// * Clean up all temp-files

namespace winZLR
{
    public partial class WinZLRForm : Form
    {
        #region "member constants and variables"
        private const int TVW_NODE_GLOBALS = 0;
        private const int TVW_NODE_LOCALS = 1;
        private const int TVW_NODE_STACK = 2;
        private const int TVW_NODE_CALL_STACK = 3;

        private readonly int port;
        private Socket? client;
        private Task? logTask;
        private bool RunLogTask = true;          // Setting this to false will end thread
        private readonly Queue<string> messagesZLR = [];
        private bool youGotMail = false;
        private readonly List<Breakpoint> breakpoints = [];
        private int currentLine = -1;
        private string currentAddress = "";
        private string currentFile = "";
        private Point currentClick = new();
        private System.Diagnostics.Process? advProcess;
        private readonly Dictionary<string, int> DisassemblyAddressLineHash = [];
        private readonly Dictionary<int, string> DisassemblyLineAddressHash = [];
        private List<SourceCode> SourceFiles = [];
        private string gameFile = "";
        private string debugFile = "";
        private readonly List<string> searchPaths = [];
        private bool storyRunning = false;
        private bool storyStarted = false;
        private bool storyLoaded = false;
        private DebugInfo? debugInfo;
        private bool zilSource = false;
        private bool delayHighlighting = false;
        int mouseX, mouseY;
        #endregion

        #region "DLLImports"
        // MoveWindow moves a window or changes its size based on a window handle.
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool MoveWindow(IntPtr hWnd, int X, int Y, int nWidth, int nHeight, bool bRepaint);

        // ShowWindow specifies how the window is to be shown
        private const int SW_HIDE = 0;
        private const int SW_SHOWNORMAL = 1;
        private const int SW_NORMAL = 1;
        private const int SW_SHOWMINIMIZED = 2;
        private const int SW_SHOWMAXIMIZED = 3;
        private const int SW_MAXIMIZE = 3;
        private const int SW_SHOWNOACTIVATE = 4;
        private const int SW_SHOW = 5;
        private const int SW_MINIMIZE = 6;
        private const int SW_SHOWMINNOACTIVE = 7;
        private const int SW_SHOWNA = 8;
        private const int SW_RESTORE = 9;
        private const int SW_SHOWDEFAULT = 10;
        private const int SW_FORCEMINIMIZE = 11;
        private const int SW_MAX = 11;

        [DllImport("user32.dll")]
        static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        #endregion

        private class Breakpoint
        {
            public int Line = 0;                // 0    = No line breakpoint
            public string? Address = null;      // null = No address breakpoint
            public string File = "";            // Filename with full path
            public bool Hidden = false;

            public bool Equals(Breakpoint other)
            {
                if (other == null) return false;
                if ((this.File == other.File && this.Line == other.Line) || this.Address == other.Address) return true;
                return false;
            }
        }

        private class SourceCode
        {
            public string Name = "";                    // name used in this app
            public string FullPath = "";                // actual name used in ConsoleZLR
            public string SourceTxt = "";
            public string? SourceRtf = "";
            public bool FilePathResolved = false;
        }

        public WinZLRForm()
        {
            InitializeComponent();

            // Find a free port
            var udp = new UdpClient(0, AddressFamily.InterNetwork);
            if (udp.Client.LocalEndPoint != null)
                port = ((IPEndPoint)udp.Client.LocalEndPoint).Port;

            // Set up timer
            TmrResponse.Interval = 100;
            TmrResponse.Start();

            // Init controls
            txtLog.ReadOnly = true;
            txtLog.WordWrap = false;
            TxtSource.ReadOnly = true;
            TxtSource.BackColor = Color.White;
            TxtSource.HideSelection = false;
            TxtSource.WordWrap = false;
            TxtDisassembly.ReadOnly = true;
            TxtDisassembly.BackColor = Color.White;
            TxtDisassembly.HideSelection = false;
            TxtDisassembly.WordWrap = false;
            BtnStart.Enabled = false;
            BtnBreak.Enabled = false;
            BtnStop.Enabled = false;
            txtLog.Font = new Font("Courier New", 10, FontStyle.Regular);
            TxtSource.Font = new Font("Courier New", 10, FontStyle.Regular);
            TxtSource.ShowLineNumbers = Properties.Settings.Default.ShowLineNumbers;
            TxtDisassembly.Font = new Font("Courier New", 10, FontStyle.Regular);
            TvwVariables.HideSelection = false;

            // restore settings
            ChkShowSyntaxHighlighting.Checked = Properties.Settings.Default.ShowSyntaxHighlighting;
            ChkShowLineNumbers.Checked = Properties.Settings.Default.ShowLineNumbers;
            TxtSearchPaths.Text = Properties.Settings.Default.SearchPaths;
            TxtConsoleZLRPath.Text = Properties.Settings.Default.ConsoleZLRPath;
            TxtUnZPath.Text = Properties.Settings.Default.UnZPath;

            // init form and start it upper-left and half-width
            StartPosition = FormStartPosition.Manual;
            int screenWidth = Screen.AllScreens[0].WorkingArea.Width;
            int screenHeight = Screen.AllScreens[0].WorkingArea.Height;
            Location = Screen.AllScreens[0].WorkingArea.Location;
            Size = new Size(screenWidth / 2, screenHeight);
            ShowIcon = false;
            KeyPreview = true;
            Text = "WinZLR - Z-machine debugger (no story loaded)";
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            if (storyStarted)
            {
                SendContinue();
                return;
            }

            // Start ZLR in seperate process, that listens to <port>
            advProcess = new();
            advProcess.StartInfo.FileName = string.Concat(TxtConsoleZLRPath.Text, "ConsoleZLR.exe");
            advProcess.StartInfo.WorkingDirectory = Application.StartupPath;
            advProcess.StartInfo.Arguments = string.Concat("-debug -listen ", port.ToString(), " \"", gameFile, "\" \"", debugFile, "\"");
            advProcess.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;    // start minimized to get the MainWindowHandle below
            advProcess.Start();

            // Wait for it to be responsive
            SpinWait.SpinUntil(() => advProcess.Responding);

            // Wait for port
            SpinWait.SpinUntil(() => IsPortInUse(port));

            // Move story window to right panel and show it
            int screenWidth = Screen.AllScreens[0].WorkingArea.Width;
            int screenHeight = Screen.AllScreens[0].WorkingArea.Height;
            advProcess.Refresh();
            ShowWindow(advProcess.MainWindowHandle, SW_NORMAL);
            MoveWindow(advProcess.MainWindowHandle, screenWidth / 2, 0, screenWidth / 2, screenHeight, true);

            // Set up client socket on <port>
            client = new(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            client.Connect("127.0.0.1", port);

            // Start seperate thread that listens to responses
            RunLogTask = true;
            logTask = Task.Run(LoggWrite);

            // update treeview
            SendVariables();

            Text = string.Concat("WinZLR - Z - machine debugger, ", Path.GetFileName(gameFile), " (story running)");

            storyStarted = true;
            BtnStart.Text = "Continue";
            SetButtons();
        }

        private void SetButtons()
        {
            if (!storyLoaded)
            {
                BtnOpen.Enabled = true;
                BtnStart.Enabled = false;
                BtnBreak.Enabled = false;
                BtnStop.Enabled = false;
                return;
            }

            if (!storyStarted)
            {
                BtnOpen.Enabled = true;
                BtnStart.Enabled = true;
                BtnBreak.Enabled = false;
                BtnStop.Enabled = false;
            }
            if (storyStarted && storyRunning)
            {
                BtnOpen.Enabled = false;
                BtnStart.Enabled = false;
                BtnBreak.Enabled = true;
                BtnStop.Enabled = true;
            }
            if (storyStarted && !storyRunning)
            {
                BtnOpen.Enabled = false;
                BtnStart.Enabled = true;
                BtnBreak.Enabled = false;
                BtnStop.Enabled = true;
            }
        }

        private void SetRoutineNamesInDisassembly()
        {
            if (debugInfo == null) return;

            string disassembly = TxtDisassembly.Text;
            disassembly = disassembly.Replace("Main routine:", "Routine:");
            foreach (RoutineInfo ri in debugInfo.Routines)
            {
                if (ri.CodeStart > 0)
                {
                    string tmp = string.Concat("Routine: 0x", ri.CodeStart.ToString("X5"));
                    disassembly = disassembly.Replace(tmp, string.Concat("Name:    ", ri.Name, "\n", tmp));
                }
            }
            TxtDisassembly.Text = disassembly;
        }

        private void LoggWrite()
        {
            while (RunLogTask)
            {
                if (youGotMail || messagesZLR.Count > 0) continue;
                var buffer = new byte[2048];
                if (client != null)
                {
                    if (client.Available > 0)
                    {
                        string postBox = "";
                        while (client.Available > 0)
                        {
                            var received = client.Receive(buffer, SocketFlags.None);
                            postBox = string.Concat(postBox, Encoding.UTF8.GetString(buffer, 0, received));
                            Thread.Sleep(100);    // Wait a bit, if client still is sending
                        }

                        string[] messages = postBox.Split("\nD> ");     // The debugger prompt signals split between responses
                        for (int i = 0; i < messages.Length; i++)
                            if (messages[i].Length > 0) messagesZLR.Enqueue(messages[i]);
                        youGotMail = true;
                    }
                }
            }
        }

        public static bool IsPortInUse(int port)
        {
            bool inUse = false;

            IPGlobalProperties ipProperties = IPGlobalProperties.GetIPGlobalProperties();
            IPEndPoint[] ipEndPoints = ipProperties.GetActiveTcpListeners();

            foreach (IPEndPoint endPoint in ipEndPoints)
            {
                if (endPoint.Port == port)
                {
                    inUse = true;
                    break;
                }
            }

            return inUse;
        }

        private void TmrResponse_Tick(object sender, EventArgs e)
        {
            int startHex;
            bool scroll = true;

            TmrResponse.Enabled = false;

            if (youGotMail)
            {
                this.Activate();    // move focus to this form when breaking in ConsoleZLR

                while (messagesZLR.Count > 0)
                {
                    string msg = messagesZLR.Dequeue();

                    // write to log window
                    txtLog.AppendText(msg + "\n");
                    txtLog.ScrollToCaret();

                    if (msg.Contains("ZLR Debugger")) SendVariables();

                    // CallStack
                    if (msg.Contains("call depth: ", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateCallStack(msg);
                        continue;
                    }

                    // locals and stack
                    if (msg.Contains("no call frame.", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("no data on stack.", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("local variables:", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("word on stack:", StringComparison.OrdinalIgnoreCase) ||
                        msg.Contains("words on stack:", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateLocals(msg);
                        continue;
                    }

                    // globals
                    if (msg.Contains("\n  ", StringComparison.OrdinalIgnoreCase) && msg.Contains(" = "))
                    {
                        UpdateGlobals(msg, false);
                        continue;
                    }

                    // set breakpoint
                    if (msg.Contains("set breakpoint at", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateHighlights(false);
                        scroll = false; // No scrolling when setting breakpoint
                        continue;
                    }

                    // clear breakpoint
                    if (msg.Contains("cleared breakpoint at", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateHighlights(false);
                        scroll = false; // No scrolling when clearing breakpoint
                        continue;
                    }

                    // show object
                    if (msg.Contains("(table at $", StringComparison.OrdinalIgnoreCase))
                    {
                        UpdateHighlights(false);
                        scroll = false; // No scrolling
                        continue;
                    }

                    // extract source code file and current position in source
                    currentAddress = "";
                    currentFile = "";
                    currentLine = -1;
                    LineInfo? li = null;
                    startHex = msg.IndexOf('$');
                    if (startHex > -1)
                    {
                        currentAddress = msg.Substring(startHex + 1, 5);
                        li = debugInfo?.FindLine(HexToInt(currentAddress));
                    }
                    if (li == null)
                        TabWinZLR.SelectedIndex = 1;
                    else
                    {
                        currentFile = li.Value.File;
                        currentLine = li.Value.Line;
                    }
                }

                if (scroll)
                {
                    // Scroll in source
                    GoToLineInSource(Path.GetFileName(currentFile).ToString(), currentLine, true, false);

                    // Scroll in disassembly
                    if (currentAddress != "") GoToAddressInDisassembly(HexToInt(currentAddress), false);

                    UpdateHighlights(false);
                }
                storyRunning = false;

                // Remove hidden breakpoint story paused and we stand on it (RunToCursor)
                foreach (Breakpoint bp in breakpoints)
                {
                    if (bp.Hidden && bp.Line == currentLine && bp.File == currentFile)
                    {
                        ToggleBreakpoint(bp);
                        SendVariables();
                        break;
                    }
                }
            }

            youGotMail = false;
            TmrResponse.Enabled = true;

            if (storyRunning && Text.Contains("(story paused)")) Text = string.Concat("WinZLR - Z - machine debugger, ", Path.GetFileName(gameFile), " (story running)");
            if (!storyRunning && Text.Contains("(story running)")) Text = string.Concat("WinZLR - Z - machine debugger, ", Path.GetFileName(gameFile), " (story paused)");
            SetButtons();
        }

        private void UpdateGlobals(string msg, bool init)
        {
            // init fills the node with all globals from debugInfo
            if (init && debugInfo != null)
            {
                TvwVariables.Nodes[TVW_NODE_GLOBALS].Nodes.Clear();

                // add globals to treeview in numeric order, using name as key (it should be unique)
                for (byte i = 0; i < 240; i++)
                    if (debugInfo.Globals.backward.TryGetValue(i, out string? key))
                    {
                        string id = "G" + i.ToString("X2");
                        string tmp = id + ": " + key + " (uninitialized)";
                        TvwVariables.Nodes[TVW_NODE_GLOBALS].Nodes.Add(key, tmp);
                    }
            }
            else
            {
                string[] s = msg.Split("\n");

                for (int i = 0; i < s.Length; i++)
                {

                    if (s[i][0..2] == "  ")
                    {
                        // Globals
                        string key = s[i][..(s[i].IndexOf('=') - 1)].Trim();
                        string id = "G" + debugInfo?.Globals.forward[key].ToString("X2");
                        string tmp = id + ": " + s[i].Trim();
                        TreeNode[] nodes = TvwVariables.Nodes[TVW_NODE_GLOBALS].Nodes.Find(key, false);
                        if (nodes.Length > 0 && nodes[0].Text != null)
                        {
                            if (nodes[0].Text != tmp)
                            {
                                // changed
                                nodes[0].Text = tmp;
                                nodes[0].ForeColor = Color.Red;
                            }
                            else nodes[0].ForeColor = Color.Black;
                        }
                        else
                        {
                            // new, should never happen
                            TvwVariables.Nodes[TVW_NODE_GLOBALS].Nodes.Add(key, tmp);
                        }
                    }
                }
            }
            TvwVariables.Nodes[TVW_NODE_GLOBALS].Text = "GLOBALS (" + TvwVariables.Nodes[TVW_NODE_GLOBALS].Nodes.Count.ToString() + " globals)";
        }

        private void UpdateLocals(string msg)
        {
            string localsStr = "", stackStr = "";
            int divider = msg.IndexOf(" on stack");
            if (divider != -1)
            {
                localsStr = msg[..divider];
                stackStr = msg[(divider + 1)..];
            }

            // locals
            if (msg.Contains("no call frame.", StringComparison.OrdinalIgnoreCase))
            {
                TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes.Clear();
                TvwVariables.Nodes[TVW_NODE_LOCALS].Text = "LOCALS (no locals in frame)";
            }
            else
            {
                String[] str = localsStr.Split("\n");

                int l = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i][0..2] == "  ")
                    {
                        // Locals
                        string key = "L" + l.ToString("X2");
                        string tmp = key + ": " + str[i].Trim();
                        TreeNode[] nodes = TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes.Find(key, false);
                        if (nodes.Length > 0 && nodes[0].Text != null)
                        {
                            if (nodes[0].Text != tmp)
                            {
                                // changed
                                nodes[0].Text = tmp;
                                nodes[0].ForeColor = Color.Red;
                            }
                            else nodes[0].ForeColor = Color.Black;
                        }
                        else
                        {
                            // new
                            TreeNode treeNode = TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes.Add(key, tmp);
                            treeNode.Tag = l;
                        }
                        l++;
                    }
                }
                if (l > 1)
                    TvwVariables.Nodes[TVW_NODE_LOCALS].Text = "LOCALS (" + l.ToString() + " locals in frame)";
                else
                    TvwVariables.Nodes[TVW_NODE_LOCALS].Text = "LOCALS (1 local in frame)";
                for (int i = TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes.Count - 1; i > 0; i--)
                    if (Convert.ToInt32(TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes[i].Tag) >= l)
                        TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes[i].Remove();
            }

            // stack
            if (msg.Contains("no data on stack", StringComparison.OrdinalIgnoreCase))
            {
                TvwVariables.Nodes[TVW_NODE_STACK].Nodes.Clear();
                TvwVariables.Nodes[TVW_NODE_STACK].Text = "STACK (no data on stack)";
            }
            else
            {
                String[] str = stackStr.Split("\n");

                int s = 0;
                for (int i = 0; i < str.Length; i++)
                {
                    if (str[i][0..2] == "  ")
                    {
                        // words on stack
                        string key = "S" + s.ToString("X2");
                        string tmp = str[i].Trim();
                        TreeNode[] nodes = TvwVariables.Nodes[TVW_NODE_STACK].Nodes.Find(key, false);
                        if (nodes.Length > 0 && nodes[0].Text != null)
                        {
                            if (nodes[0].Text != tmp)
                                nodes[0].Text = tmp;
                        }
                        else
                        {
                            TreeNode treeNode = TvwVariables.Nodes[TVW_NODE_STACK].Nodes.Add(key, tmp);
                            treeNode.Tag = s;
                        }
                        s++;
                    }
                }
                if (s > 1)
                    TvwVariables.Nodes[TVW_NODE_STACK].Text = "STACK (" + s.ToString() + " words on stack)";
                else
                    TvwVariables.Nodes[TVW_NODE_STACK].Text = "STACK (1 word on stack)";
                for (int i = TvwVariables.Nodes[TVW_NODE_STACK].Nodes.Count - 1; i > 0; i--)
                    if (Convert.ToInt32(TvwVariables.Nodes[TVW_NODE_STACK].Nodes[i].Tag) >= s)
                        TvwVariables.Nodes[TVW_NODE_STACK].Nodes[i].Remove();
            }
        }

        private void UpdateCallStack(string msg)
        {
            if (debugInfo == null) return;

            String[] s = msg.Split("==========");

            int callDepth = s.Length - 1;

            TvwVariables.BeginUpdate();

            TvwVariables.Nodes[TVW_NODE_CALL_STACK].Nodes.Clear();
            TvwVariables.Nodes[TVW_NODE_CALL_STACK].Text = string.Concat("CALL STACK (depth = ", callDepth.ToString(), ")");

            for (int i = 0; i < callDepth; i++)
            {
                int addressStart = s[i].IndexOf('$');
                int address = HexToInt(s[i].Substring(addressStart + 1, 5));
                LineInfo? li = debugInfo.FindLine(address);
                if (i > 0)
                {
                    // return PC points at next line. Find calling line and address
                    li = debugInfo.FindLine(address - 1);
                    if (li == null)
                        address = HexToInt(FindRelativeAddress(TxtDisassembly, address.ToString("x5"), -1));
                    else
                        address = debugInfo.FindCodeAddress(li.Value.File, li.Value.Line);
                }
                RoutineInfo? ri = debugInfo.FindRoutine(address);
                string fileName = "[external code]";
                if (li != null) fileName = string.Concat("[", Path.GetFileName(li.Value.File), "]");
                string routineName = "";
                if (ri != null) routineName = ri.Name;
                int line = 0;
                if (li != null) line = li.Value.Line;
                string callStackText = string.Concat(fileName, ".", routineName, " : ");
                if (li == null)
                    callStackText += string.Concat("address $", address.ToString("x5"));
                else
                    callStackText += string.Concat("line ", line.ToString());
                TreeNode treeNode = TvwVariables.Nodes[TVW_NODE_CALL_STACK].Nodes.Add(callStackText);
                if (li == null) treeNode.Tag = address; else treeNode.Tag = li;
            }

            TvwVariables.EndUpdate();
        }

        private void UpdateHighlights(bool drawOnlyBreakpoints)
        {
            // Stop update of source
            TxtSource.BeginUpdate();

            int selectionStart = TxtSource.SelectionStart;

            int caretPos = TxtSource.GetCharIndexFromPosition(new Point(2, 2));

            // Clear all highlights
            HighlightLine(TxtSource, -1, TxtSource.BackColor, 0);

            // Highlight breakpoints red
            for (int i = 0; i < breakpoints.Count; i++)
                if (!breakpoints[i].Hidden && breakpoints[i].File == SourceFilesFindFullPath(CboFiles.Text)) HighlightLine(TxtSource, breakpoints[i].Line, Color.Red, 0);

            // Highlight current line yellow
            if (!drawOnlyBreakpoints && currentLine != -1)
            {
                string tmp = TxtSource.Lines[currentLine - 1];
                int indent = 0;
                while (tmp[indent] == '\t' || tmp[indent] == ' ') indent++;
                HighlightLine(TxtSource, currentLine, Color.Yellow, indent);
            }
            TxtSource.Select(caretPos, 0);
            TxtSource.ScrollToCaret();

            // Start update of source
            TxtSource.EndUpdate();
            TxtSource.Refresh();

            // Disassembly
            TxtDisassembly.BeginUpdate();

            caretPos = TxtDisassembly.GetCharIndexFromPosition(new Point(2, 2));

            // Clear all highlights
            HighlightAddress(TxtDisassembly, "", TxtDisassembly.BackColor, 0);

            // Highlight breakpoints red
            for (int i = 0; i < breakpoints.Count; i++)
                if (!breakpoints[i].Hidden) HighlightAddress(TxtDisassembly, breakpoints[i].Address, Color.Red, 0);

            // Highlight current line yellow
            if (!drawOnlyBreakpoints)
                HighlightAddress(TxtDisassembly, currentAddress, Color.Yellow, 6);

            TxtDisassembly.Select(caretPos, 0);
            TxtDisassembly.ScrollToCaret();

            TxtSource.SelectionStart = selectionStart;

            TxtDisassembly.EndUpdate();
            TxtDisassembly.Refresh();
        }

        // Find address relative to address, offset < 0 search backwards
        private string FindRelativeAddress(RichTextBox rb, string address, int offset)
        {
            if (offset == 0) return address;
            if (!DisassemblyAddressLineHash.ContainsKey(address.ToUpper())) return address;

            int line = DisassemblyAddressLineHash[address.ToUpper()];

            string[] lines = rb.Lines;
            if (offset < 0)
            {
                while (true)
                {
                    line--;
                    if (line < 0) return address;
                    if (lines[line].Length < 5) continue;
                    if (!IsHex(lines[line][..5])) continue;
                    offset++;
                    if (offset == 0) return lines[line][..5].ToLower();
                }
            }

            if (offset > 0)
            {
                while (true)
                {
                    line++;
                    if (line >= lines.Length) return address;
                    if (lines[line].Length < 5) continue;
                    if (!IsHex(lines[line][..5])) continue;
                    offset--;
                    if (offset == 0) return lines[line][..5].ToLower();
                }
            }

            return address;
        }

        public static bool IsHex(String str)
        {
            string s = str.ToUpper();
            if (s == null) return false;
            int n = s.Length;

            if (n == 0) return false;

            for (int i = 0; i < n; i++)
            {
                char ch = s[i];

                if ((ch < '0' || ch > '9') &&
                    (ch < 'A' || ch > 'F'))
                {
                    return false;
                }
            }
            return true;
        }

        private static int GetFirstCharIndexFromAddress(RichTextBox rb, string address)
        {
            return rb.Find(string.Concat(address.ToUpper(), " "));
        }

        // highlight line with color, line = -1, set all lines to color
        //                            line =  0, do nothing
        private static void HighlightLine(RichTextBox rb, int line, Color color, int indent)
        {
            if (line == 0) return;
            if (line == -1)
            {
                rb.SelectAll();
                rb.SelectionBackColor = color;
                return;
            }

            // Find position
            int idxStart = rb.GetFirstCharIndexFromLine(line - 1);
            int idxEnd = rb.GetFirstCharIndexFromLine(line);
            if (idxEnd < idxStart) idxEnd = rb.TextLength;

            // Higlight
            rb.Select(idxStart + indent, idxEnd - idxStart - indent);
            rb.SelectionBackColor = color;
        }

        // highlight address with color, address =   "", set all lines to color
        //                               address = null, do nothing
        private void HighlightAddress(RichTextBox rb, string? address, Color color, int indent)
        {
            if (address == null) return;
            if (HexToInt(address) == -1) return;

            if (address == "")
            {
                rb.SelectAll();
                rb.SelectionBackColor = color;
                return;
            }

            // Find position
            int idxStart = GetFirstCharIndexFromAddress(rb, address);
            string nextAddress = FindRelativeAddress(rb, address, 1);
            int idxEnd = GetFirstCharIndexFromAddress(rb, nextAddress);
            if (idxEnd < idxStart) idxEnd = rb.TextLength;
            if (rb.Text[idxStart..idxEnd].Contains("\n\n", StringComparison.OrdinalIgnoreCase))
                idxEnd = idxStart + rb.Text[idxStart..idxEnd].IndexOf("\n\n");

            // Higlight
            rb.Select(idxStart + indent, idxEnd - idxStart - indent);
            rb.SelectionBackColor = color;
        }

        private void ToggleBreakpoint(Breakpoint breakpoint)
        {
            if (storyRunning)
            {
                MessageBox.Show("Can't set or clear breakpoints while story is running.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            if (!storyStarted)
            {
                MessageBox.Show("Can't set or clear breakpoints until story is started.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            Breakpoint? findBP = breakpoints.Find(x => x.Equals(breakpoint));
            if (findBP == null)
            {
                // add breakpoint
                breakpoints.Add(breakpoint);
                if (breakpoint.Line > 0)
                    client?.SendAsync(Encoding.ASCII.GetBytes(string.Concat("break ", breakpoint.File, ":", breakpoint.Line.ToString(), "\n")));
                else
                    client?.SendAsync(Encoding.ASCII.GetBytes(string.Concat("break $", breakpoint.Address, "\n")));
            }
            else
            {
                // Remove breakpoint
                breakpoints.Remove(findBP);
                if (breakpoint.Line > 0)
                    client?.SendAsync(Encoding.ASCII.GetBytes(string.Concat("clear ", breakpoint.File, ":", breakpoint.Line.ToString(), "\n")));
                else
                    client?.SendAsync(Encoding.ASCII.GetBytes(string.Concat("clear $", breakpoint.Address, "\n")));
            }
            UpdateHighlights(false);

        }

        private void TxtSource_MouseDown(object sender, MouseEventArgs e)
        {
            currentClick = e.Location;
        }

        private void MnuItmBreakpoint_Click(object sender, EventArgs e)
        {
            SendToggleBreakpoint(true);
        }

        private void MnuItmContinue_Click(object sender, EventArgs e)
        {
            SendContinue();
        }

        private void MnuItmStepOver_Click(object sender, EventArgs e)
        {
            SendStepOver();
        }

        private void MnuItmStepInto_Click(object sender, EventArgs e)
        {
            SendStepInto();
        }

        private void MnuItmBreak_Click(object sender, EventArgs e)
        {
            SendBreak();
        }

        private void MnuItemStepOut_Click(object sender, EventArgs e)
        {
            SendStepOut();
        }

        private void MnuItmDeleteBreakpoints_Click(object sender, EventArgs e)
        {
            SendDeleteBreakpoints();
        }

        private void MnuItmDisassembly_Click(object sender, EventArgs e)
        {
            MenuGoToDisassembly();
        }

        private void MnuItmRestart_Click(object sender, EventArgs e)
        {
            SendRestart();
        }

        private void MenuGoToDisassembly()
        {
            if (TabWinZLR.SelectedIndex == 0)
            {
                if (TxtSource.Text == "")
                {
                    TabWinZLR.SelectedIndex = 1;
                    return;
                }
                int line = TxtSource.GetLineFromCharIndex(TxtSource.SelectionStart) + 1;
                string file = SourceFilesFindFullPath(CboFiles.Text);
                int address = debugInfo!.FindCodeAddress(file, line);
                if (address == -1)
                {
                    MessageBox.Show("There is no disassembly line for this source line.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                if (address == 0)
                {
                    MessageBox.Show("Debug file is missing address data for this line.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                GoToAddressInDisassembly(address, true);
                return;
            }
            if (TabWinZLR.SelectedIndex == 1)
            {
                if (TxtDisassembly.Text == "")
                {
                    TabWinZLR.SelectedIndex = 0;
                    return;
                }
                int line = TxtDisassembly.GetLineFromCharIndex(TxtDisassembly.SelectionStart);
                while (line > -1 && (TxtDisassembly.Lines[line].Length < 6 || !IsHex(TxtDisassembly.Lines[line][0..5]))) line--;

                LineInfo? li = null;
                if (line >= 0) li = debugInfo?.FindLine(HexToInt(TxtDisassembly.Lines[line][0..5]));

                if (li == null)
                {
                    MessageBox.Show("There is no source code line for this disassembly line.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                GoToLineInSource(li.Value.File, li.Value.Line, true, true);
                return;
            }
        }

        private void SendRestart()
        {
            if (!storyStarted) return;
            if (storyRunning)
            {
                MessageBox.Show("Can't do that while story is running.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            client?.SendAsync(Encoding.ASCII.GetBytes("reset\n"));
            SendVariables();
        }

        private void SendBreak()
        {
            if (!storyStarted) return;
            client?.SendAsync(Encoding.ASCII.GetBytes("!pause\n"));
            SendVariables();
        }

        private void SendDeleteBreakpoints()
        {
            for (int i = 0; i < breakpoints.Count; i++)
            {
                ToggleBreakpoint(breakpoints[i]);
            }
            breakpoints.Clear();
        }

        private void SendStepOut()
        {
            if (!storyStarted) return;
            client?.SendAsync(Encoding.ASCII.GetBytes("up\n"));
            SendVariables();
        }

        private void SendToggleBreakpoint(bool fromClick)
        {
            if (TabWinZLR.SelectedIndex == 0)
            {
                if (TxtSource.Text == "") return;
                int line;
                if (fromClick)
                    line = TxtSource.GetLineFromCharIndex(TxtSource.GetCharIndexFromPosition(currentClick));
                else
                    line = TxtSource.GetLineFromCharIndex(TxtSource.SelectionStart);
                line++;
                string file = SourceFilesFindFullPath(CboFiles.Text);
                int address = debugInfo!.FindCodeAddress(file, line);
                if (address == -1)
                    MessageBox.Show("A breakpoint could not be inserted at this location.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    ToggleBreakpoint(new Breakpoint() { Line = line, File = file, Address = address.ToString("x5") });
            }
            if (TabWinZLR.SelectedIndex == 1)
            {
                if (TxtDisassembly.Text == "") return;
                int line;
                if (fromClick)
                    line = TxtDisassembly.GetLineFromCharIndex(TxtDisassembly.GetCharIndexFromPosition(currentClick));
                else
                    line = TxtDisassembly.GetLineFromCharIndex(TxtDisassembly.SelectionStart);
                while (TxtDisassembly.Lines[line].Length < 6 || !IsHex(TxtDisassembly.Lines[line][0..5])) line--;
                int address = HexToInt(TxtDisassembly.Lines[line][0..5]);
                LineInfo? li;
                li = debugInfo?.FindLine(address);
                string file = "";
                line = 0;
                if (li != null && debugInfo!.FindCodeAddress(li.Value.File, li.Value.Line) == address)
                {
                    file = li.Value.File;
                    line = li.Value.Line;
                }
                if (address == -1)
                    MessageBox.Show("A breakpoint could not be inserted at this location.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                    ToggleBreakpoint(new Breakpoint() { Line = line, File = file, Address = address.ToString("x5") });
            }
        }

        private void SendContinue()
        {
            if (!storyStarted) 
                if (storyLoaded) BtnStart.PerformClick(); else return;
            storyRunning = true;
            UpdateHighlights(true);
            client?.SendAsync(Encoding.ASCII.GetBytes("run\n"));
            SendVariables();
        }

        private void SendStepInto()
        {
            if (!storyStarted) return;
            storyRunning = true;
            if (TabWinZLR.SelectedIndex == 1)
                client?.SendAsync(Encoding.ASCII.GetBytes("step\n"));
            else
                client?.SendAsync(Encoding.ASCII.GetBytes("stepline\n"));
            SendVariables();
        }

        private void SendStepOver()
        {
            if (!storyStarted) return;
            storyRunning = true;
            if (TabWinZLR.SelectedIndex == 1)
                client?.SendAsync(Encoding.ASCII.GetBytes("over\n"));
            else
                client?.SendAsync(Encoding.ASCII.GetBytes("overline\n"));
            SendVariables();
        }

        private void SendVariables()
        {
            if (!storyStarted) return;
            client?.SendAsync(Encoding.ASCII.GetBytes("globals\n"));
            client?.SendAsync(Encoding.ASCII.GetBytes("locals\n"));
            client?.SendAsync(Encoding.ASCII.GetBytes("backtrace\n"));
        }
        private void SendRunToCursor(bool fromClick)
        {
            if (!storyStarted) return;

            if (TabWinZLR.SelectedIndex == 0)
            {
                if (TxtSource.Text == "") return;
                int line;
                if (fromClick)
                    line = TxtSource.GetLineFromCharIndex(TxtSource.GetCharIndexFromPosition(currentClick));
                else
                    line = TxtSource.GetLineFromCharIndex(TxtSource.SelectionStart);
                line++;
                string file = SourceFilesFindFullPath(CboFiles.Text);
                int address = debugInfo!.FindCodeAddress(file, line);
                if (address == -1)
                    MessageBox.Show("A breakpoint could not be inserted at this location.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    ToggleBreakpoint(new Breakpoint() { Line = line, File = file, Address = address.ToString("x5"), Hidden = true });
                    SendContinue();
                }
            }
            if (TabWinZLR.SelectedIndex == 1)
            {
                if (TxtDisassembly.Text == "") return;
                int line;
                if (fromClick)
                    line = TxtDisassembly.GetLineFromCharIndex(TxtDisassembly.GetCharIndexFromPosition(currentClick));
                else
                    line = TxtDisassembly.GetLineFromCharIndex(TxtDisassembly.SelectionStart);
                while (TxtDisassembly.Lines[line].Length < 6 || !IsHex(TxtDisassembly.Lines[line][0..5])) line--;
                int address = HexToInt(TxtDisassembly.Lines[line][0..5]);
                LineInfo? li;
                li = debugInfo?.FindLine(address);
                string file = "";
                line = 0;
                if (li != null && debugInfo!.FindCodeAddress(li.Value.File, li.Value.Line) == address)
                {
                    file = li.Value.File;
                    line = li.Value.Line;
                }
                if (address == -1)
                    MessageBox.Show("A breakpoint could not be inserted at this location.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                else
                {
                    ToggleBreakpoint(new Breakpoint() { Line = line, File = file, Address = address.ToString("x5"), Hidden = true });
                    SendContinue();
                }
            }

        }

        private void SendNewValue(string var, int val, bool global)
        {
            if (!storyStarted) return;
            if (global)
                client?.SendAsync(Encoding.ASCII.GetBytes(string.Concat("p <SETG ", var, " ", val.ToString(), ">\n")));
            else
                client?.SendAsync(Encoding.ASCII.GetBytes(string.Concat("p <SET ", var, " ", val.ToString(), ">\n")));
            SendVariables();
        }
        private void BtnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void MnuRunToCursor_Click(object sender, EventArgs e)
        {
            SendRunToCursor(true);
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            switch (keyData)
            {
                case (Keys.F | Keys.Control):
                    SearchFor();
                    return true;
                case Keys.F2:
                    EditValue();
                    return true;
                case Keys.F3:
                    FindNext(searchString, searchMatcCase, searchAllFiles);
                    return true;
                case (Keys.F3 | Keys.Shift):
                    FindPrevious(searchString, searchMatcCase, searchAllFiles);
                    return true;
                case (Keys.G | Keys.Control):
                    GoToLine();
                    return true;
                case (Keys.K | Keys.Control):
                    MenuGoToDisassembly();
                    return true;
                case Keys.F5:
                    SendContinue();
                    return true;
                case (Keys.F5 | Keys.Control | Keys.Shift):
                    SendRestart();
                    return true;
                case Keys.F9:
                    SendToggleBreakpoint(false);
                    return true;
                case (Keys.F9 | Keys.Control | Keys.Shift):
                    SendDeleteBreakpoints();
                    return true;
                case Keys.F10:
                    SendStepOver();
                    return true;
                case (Keys.F10 | Keys.Control):
                    SendRunToCursor(false);
                    return true;
                case (Keys.F10 | Keys.Control | Keys.Shift):
                    SetNextStatement();
                    return true;
                case Keys.F11:
                    SendStepInto();
                    return true;
                case (Keys.F11 | Keys.Shift):
                    SendStepOut();
                    return true;
                case (Keys.B | Keys.Control):
                    SendBreak();
                    return true;
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        private void GoToLine()
        {
            if (TabWinZLR.SelectedIndex != 0) return;
            if (TxtSource.Text == "") return;

            using GoToLineForm form = new();
            int last = TxtSource.Lines.Length - 1;
            while (TxtSource.Lines[last] == "") last--;

            form.LabelText = String.Concat("Line number (1-", last.ToString(), "):");
            if (form.ShowDialog() == DialogResult.OK)
            {
                int searchLine = form.GoToLine;
                if (searchLine > 0 && searchLine <= last)
                {
                    TxtSource.SelectionStart = TxtSource.GetFirstCharIndexFromLine(searchLine - 1);
                    TxtSource.ScrollToCaret();
                }
            }
        }

        private int OldCboFilesSelectedIndex = -1;
        private void CboFiles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (OldCboFilesSelectedIndex == CboFiles.SelectedIndex) return;

            OldCboFilesSelectedIndex = CboFiles.SelectedIndex;

            if (!delayHighlighting) TxtSource.BeginUpdate();

            foreach (SourceCode source in SourceFiles)
            {
                if (source.Name == CboFiles.Text)
                    if (ChkShowSyntaxHighlighting.Checked)
                        TxtSource.Rtf = source.SourceRtf;
                    else
                        TxtSource.Text = source.SourceTxt;
            }

            if (!delayHighlighting)
            {
                TxtSource.UpdateIndent();
                UpdateHighlights(!(Path.GetFileName(currentFile) == CboFiles.Text));
                TxtSource.EndUpdate();
            }
        }

        private string SourceFilesFindFullPath(string filename)
        {
            foreach (SourceCode source in SourceFiles)
            {
                if (source.Name == filename) return source.FullPath;
            }
            return "";
        }

        private void BtnOpen_Click(object sender, EventArgs e)
        {
            if (storyLoaded) ResetGlobalVariables();

            if (SelectFileDialog.ShowDialog() == DialogResult.OK)
            {
                // check that file is a z-machine story file
                Stream gameStream = SelectFileDialog.OpenFile();
                byte b = (byte)gameStream.ReadByte();
                gameStream.Close();
                if (b < 1 || b > 8)
                {
                    MessageBox.Show("Unknown z-code version!", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    gameFile = "";
                    debugFile = "";
                    return;
                }
                gameFile = SelectFileDialog.FileName;

                // search for dbg
                string path = Path.GetDirectoryName(gameFile)!;
                string filename = Path.GetFileNameWithoutExtension(gameFile);
                if (File.Exists(Path.Combine(path, String.Concat(filename, ".dbg"))))
                    debugFile = Path.Combine(path, String.Concat(filename, ".dbg"));
                else if (File.Exists(Path.Combine(path, "gameinfo.dbg")))
                    debugFile = Path.Combine(path, "gameinfo.dbg");
                else
                {
                    MessageBox.Show(String.Concat("Can't find the debug file (", filename, ".dbg or gameinfo.dbg) in current directory!"), "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    gameFile = "";
                    debugFile = "";
                    return;
                }

                // is dbg in xml-format, then convert it
                Stream debugStream = new FileStream(debugFile, FileMode.Open, FileAccess.Read);
                ushort w = (ushort)(debugStream.ReadByte() * 256 + debugStream.ReadByte());
                debugStream.Close();
                if (w != 0xDEBF)            // magic number!
                {
                    try
                    {
                        System.Xml.XmlDocument xmlDoc = new();
                        xmlDoc.Load(debugFile);

                        // convert xml to dbg
                        string tmpFile = Path.GetTempFileName();
                        DebugWriter.XmlToDbg(debugFile, tmpFile);
                        debugFile = tmpFile;
                    }
                    catch
                    {
                        MessageBox.Show("Unknown format of the debug file!", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        gameFile = "";
                        debugFile = "";
                        return;
                    }
                }
                // open instance of DebugInfo
                debugStream = new FileStream(debugFile, FileMode.Open, FileAccess.Read);
                debugInfo = new(debugStream);
                // is dbg matching story-file?
                gameStream = new FileStream(gameFile, FileMode.Open, FileAccess.Read);
                if (!debugInfo.MatchesGameFile(gameStream))
                {
                    MessageBox.Show("The debug file do not match the story file!", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    gameFile = "";
                    debugFile = "";
                    debugStream.Close();
                    gameStream.Close();
                    return;
                }
                debugStream.Close();
                gameStream.Close();

                // prepare search paths for resolving filenames
                searchPaths.Clear();
                searchPaths.Add(path);          // directory where story file is
                string[] paths = TxtSearchPaths.Text.Split(';');
                foreach (string searchPath in paths) searchPaths.Add(searchPath);

                // is all files in dbg resolved?
                if (debugInfo != null)
                {
                    var filenameList = debugInfo.Filenames.Values.ToList();
                    List<string> fullFilenames = [];
                    bool debugFileNeedFix = false;
                    for (int i = 0; i < filenameList.Count; i++)
                    {
                        if (File.Exists(filenameList[i]))
                        {
                            fullFilenames.Add(filenameList[i]);
                        }
                        else
                        {
                            if (SearchForResolvedPath(filenameList[i]) != "")
                            {
                                fullFilenames.Add(SearchForResolvedPath(filenameList[i]));
                                debugFileNeedFix = true;
                            }
                            else
                            {
                                MessageBox.Show(String.Concat("Can't find source file '", filenameList[i], "'!"), "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                gameFile = "";
                                debugFile = "";
                                return;
                            }
                        }
                    }

                    if (debugFileNeedFix)
                    {
                        string tmpFile = Path.GetTempFileName();
                        DebugWriter.FixActualFile(debugFile, tmpFile, fullFilenames);
                        debugFile = tmpFile;

                        debugStream = new FileStream(debugFile, FileMode.Open, FileAccess.Read);
                        debugInfo = new(debugStream);       // reload debugInfo with correct paths
                    }
                }

                // load source code
                if (debugInfo != null)
                {
                    if (debugInfo.Version > 2000) zilSource = true; else zilSource = false;     // zilf write 2001 here, Inform6 write 16xx
                    var filenameList = debugInfo.Filenames.Values.ToList();
                    bool ffWarning = false;
                    for (int i = 0; i < filenameList.Count; i++)
                    {
                        string file = filenameList[i];
                        string name = Path.GetFileName(file);
                        string source = "";

                        // Load source code
                        bool pathResolved = File.Exists(file);
                        if (pathResolved)
                        {
                            StringBuilder sb = new();
                            int lineNumber = 1;
                            StreamReader reader = new(file);
                            do
                            {
                                string? line = reader.ReadLine();
                                if (line is not null)
                                {
                                    if (line.Contains('\f'))
                                    {
                                        if (!ffWarning)
                                        {
                                            MessageBox.Show("The source files contains the Form Feed character 0xC (ASCII 12, often showed as \\ff). " +
                                                            "Form Feed messes up the line count in the debug file so it is recommended that they are " +
                                                            "removed and the project recompiled.", "Form Feed Character", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                                            ffWarning = true;
                                        }
                                        continue;      // skip lines with 0xC (Form Feed), they mess up the line count
                                    }
                                    sb.AppendLine(line);
                                    lineNumber++;
                                }
                            } while (!reader.EndOfStream);
                            source = sb.ToString();
                        }

                        SourceFiles.Add(new SourceCode { FullPath = file, Name = name, SourceTxt = source, FilePathResolved = pathResolved });
                    }
                }

                // Fill ComboBox with all source filenames
                SourceFiles = [.. SourceFiles.OrderBy(x => x.Name)];
                for (int i = 0; i < SourceFiles.Count; i++) CboFiles.Items.Add(SourceFiles[i].Name);

                // Apply syntax highlighter
                ApplySyntaxHighlighters();

                // Create disassembly file
                filename = Path.GetTempFileName();
                Process p = new();
                p.StartInfo.FileName = "cmd.exe";
                p.StartInfo.WorkingDirectory = Application.StartupPath;
                p.StartInfo.Arguments = string.Concat("/C \"\"", TxtUnZPath.Text, "unz.exe\" \"", gameFile, "\" -z --hide >", filename, " & exit\"");
                p.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
                p.Start();
                SpinWait.SpinUntil(() => p.HasExited);
                TxtDisassembly.LoadFile(filename, RichTextBoxStreamType.PlainText);
                File.Delete(filename);
                SetRoutineNamesInDisassembly();

                // Create lookup-tables for Address <--> Line
                string[] lines = TxtDisassembly.Lines;
                for (int i = 0; i < lines.Length; i++)
                {
                    string tmp = "";
                    if (lines[i].Length > 5) tmp = lines[i][..5];
                    if (IsHex(tmp))
                    {
                        DisassemblyAddressLineHash.Add(tmp, i);
                        DisassemblyLineAddressHash.Add(i, tmp);
                    }
                }

                // set up matching brackets ()[]{}<> for Zil and ()[]{} for Inform6
                if (zilSource) TxtSource.Brackets = "()[]{}<>"; else TxtSource.Brackets = "()[]{}";

                // Prepare variables treeview
                TvwVariables.ShowRootLines = true;
                TvwVariables.ShowPlusMinus = true;
                TvwVariables.Nodes.Clear();
                TvwVariables.Nodes.Add(new TreeNode("GLOBALS"));
                TvwVariables.Nodes.Add(new TreeNode("LOCALS"));
                TvwVariables.Nodes.Add(new TreeNode("STACK"));
                TvwVariables.Nodes.Add(new TreeNode("CALL STACK"));
                UpdateGlobals("", true);

                Text = String.Concat("WinZLR - Z - machine debugger, ", Path.GetFileName(gameFile), " (story loaded)");
                storyLoaded = true;
                SetButtons();
            }
        }

        private string SearchForResolvedPath(string filename)
        {
            foreach (string path in searchPaths)
                if (File.Exists(Path.Combine(path, filename))) return Path.Combine(path, filename);

            return "";
        }

        private void WinZLRForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            RunLogTask = false;
            advProcess?.Kill(true);

            // save settings
            Properties.Settings.Default.ShowSyntaxHighlighting = ChkShowSyntaxHighlighting.Checked;
            Properties.Settings.Default.ShowLineNumbers = ChkShowLineNumbers.Checked;
            Properties.Settings.Default.ConsoleZLRPath = TxtConsoleZLRPath.Text;
            Properties.Settings.Default.UnZPath = TxtConsoleZLRPath.Text;
            Properties.Settings.Default.SearchPaths = TxtSearchPaths.Text;
            Properties.Settings.Default.Save();
        }

        private void MnuItmGotoLine_Click(object sender, EventArgs e)
        {
            GoToLine();
        }

        private void MnuItmSearch_Click(object sender, EventArgs e)
        {
            SearchFor();
        }

        #region "Find"
        private string searchString = "";
        private bool searchMatcCase = false;
        private bool searchAllFiles = false;

        private void FindNext(string searchFor, bool matchCase, bool searchInAllFiles)
        {
            if (TabWinZLR.SelectedIndex > 1) return;
            if (TabWinZLR.SelectedIndex == 0 && TxtSource.Text == "") return;
            if (TabWinZLR.SelectedIndex == 1 && TxtDisassembly.Text == "") return;
            if (searchFor == "") return;

            searchString = searchFor;
            searchMatcCase = matchCase;
            searchAllFiles = searchInAllFiles;

            RichTextBox rb = TxtSource;
            if (TabWinZLR.SelectedIndex == 1) rb = TxtDisassembly;

            int searchResult;

            RichTextBoxFinds options = new();
            if (matchCase) options |= RichTextBoxFinds.MatchCase;

            searchResult = rb.Find(searchFor, rb.SelectionStart + 1, options);
            if (searchResult < 0)
            {
                if (searchInAllFiles && TabWinZLR.SelectedIndex == 0)
                {
                    // find next file that contains searchtext and change to that file
                    delayHighlighting = true;
                    int fileNumber = CboFiles.SelectedIndex + 1;
                    if (fileNumber == CboFiles.Items.Count) fileNumber = 0;
                    StringComparison comparison = StringComparison.OrdinalIgnoreCase;
                    if (matchCase) comparison = StringComparison.Ordinal;

                    while (fileNumber != CboFiles.SelectedIndex)
                    {
                        if (SourceFiles[fileNumber].SourceTxt.Contains(searchFor, comparison))
                        {
                            CboFiles.SelectedIndex = fileNumber;
                            break;
                        }
                        fileNumber++;
                        if (fileNumber == CboFiles.Items.Count) fileNumber = 0;
                    }
                }
                searchResult = rb.Find(searchFor, 0, options);
                TxtSource.UpdateIndent();
                delayHighlighting = false;
            }
            if (searchResult < 0) MessageBox.Show("The text was not found.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void FindPrevious(string searchFor, bool matchCase, bool searchInAllFiles)
        {
            if (TabWinZLR.SelectedIndex > 1) return;
            if (TabWinZLR.SelectedIndex == 0 && TxtSource.Text == "") return;
            if (TabWinZLR.SelectedIndex == 1 && TxtDisassembly.Text == "") return;
            if (searchFor == "") return;

            searchString = searchFor;
            searchMatcCase = matchCase;
            searchAllFiles = searchInAllFiles;

            RichTextBox rb = TxtSource;
            if (TabWinZLR.SelectedIndex == 1) rb = TxtDisassembly;

            int searchResult;

            RichTextBoxFinds options = new();
            options |= RichTextBoxFinds.Reverse;
            if (matchCase) options |= RichTextBoxFinds.MatchCase;

            searchResult = rb.Find(searchFor, 0, rb.SelectionStart, options);
            if (searchResult < 0)
            {
                if (searchInAllFiles && TabWinZLR.SelectedIndex == 0)
                {
                    // find next file that contains searchtext and change to that file
                    delayHighlighting = true;
                    int fileNumber = CboFiles.SelectedIndex - 1;
                    if (fileNumber < 0) fileNumber = CboFiles.Items.Count - 1;
                    StringComparison comparison = StringComparison.OrdinalIgnoreCase;
                    if (matchCase) comparison = StringComparison.Ordinal;

                    while (fileNumber != CboFiles.SelectedIndex)
                    {
                        if (SourceFiles[fileNumber].SourceTxt.Contains(searchFor, comparison))
                        {
                            CboFiles.SelectedIndex = fileNumber;
                            break;
                        }
                        fileNumber--;
                        if (fileNumber < 0) fileNumber = CboFiles.Items.Count - 1;
                    }
                }
                searchResult = rb.Find(searchFor, 0, rb.Text.Length, options);
                TxtSource.UpdateIndent();
                delayHighlighting = false;
            }
            if (searchResult < 0) MessageBox.Show("The text was not found.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void SearchFor()
        {
            if (TabWinZLR.SelectedIndex > 1) return;
            if (!storyLoaded) return;

            using SearchForm form = new();

            form.findNext = new SearchForm.SearchNext(FindNext);
            form.findPrevious = new SearchForm.SearchPrevious(FindPrevious);
            form.searchFiles = (TabWinZLR.SelectedIndex == 0);
            if (TabWinZLR.SelectedIndex == 0 && TxtSource.SelectionLength > 0)
                form.SearchText = TxtSource.Text.Substring(TxtSource.SelectionStart, TxtSource.SelectionLength);
            if (TabWinZLR.SelectedIndex == 0 && TxtDisassembly.SelectionLength > 0)
                form.SearchText = TxtDisassembly.Text.Substring(TxtDisassembly.SelectionStart, TxtDisassembly.SelectionLength);
            form.ShowDialog();
        }
        #endregion

        private static int HexToInt(string hex)
        {
            try { return Convert.ToInt32("0x" + hex, 16); }
            catch { return -1; }
        }

        private void TvwVariables_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node != null) EditValue(e.Node);
        }

        private void GoToAddressInDisassembly(int address, bool changeToDisassemblyTab)
        {
            if (address == -1) return;

            TxtDisassembly.BeginUpdate();

            string addressStr = address.ToString("X5");

            if (changeToDisassemblyTab) TabWinZLR.SelectedIndex = 1;

            // Scroll in disassembly
            if (addressStr != "")
            {
                TxtDisassembly.SetHScrollPos(0);

                int idxStart = TxtDisassembly.Find(string.Concat(addressStr.ToUpper(), " "));
                int idxTop = TxtDisassembly.GetCharIndexFromPosition(new Point(2, 2));
                int idxBottom = TxtDisassembly.GetCharIndexFromPosition(new Point(2, TxtDisassembly.Height - 4));

                if (idxStart < idxTop || idxStart > idxBottom)
                {
                    string previousAddress = FindRelativeAddress(TxtDisassembly, addressStr, -5);
                    idxStart = TxtDisassembly.Find(string.Concat(previousAddress.ToUpper(), " "));

                    TxtDisassembly.Select(idxStart, 0);
                    TxtDisassembly.ScrollToCaret();
                }
            }
            TxtDisassembly.EndUpdate();
        }

        private void GoToLineInSource(string file, int line, bool changeSourceFile, bool changeToSourceTab)
        {
            if (changeToSourceTab) TabWinZLR.SelectedIndex = 0;

            bool alreadyAtRightFile = (CboFiles.Text == Path.GetFileName(file));

            if (!changeSourceFile && !alreadyAtRightFile) return;

            TxtSource.BeginUpdate();

            if (changeSourceFile && !alreadyAtRightFile)
            {
                // Set right source file
                delayHighlighting = true;
                if (CboFiles != null && CboFiles.Items != null)
                    for (int i = 0; i < CboFiles?.Items.Count; i++)
                    {
                        if (CboFiles?.Items[i]?.ToString() == Path.GetFileName(file) && CboFiles.SelectedIndex != i) CboFiles.SelectedIndex = i;
                    }
            }

            // Scroll in source
            if (line > -1)
            {
                TxtSource.SetHScrollPos(0);
                int idxTop = TxtSource.GetCharIndexFromPosition(new Point(2, 2));
                int idxBottom = TxtSource.GetCharIndexFromPosition(new Point(2, TxtSource.Height - 4));
                int lineTop = TxtSource.GetLineFromCharIndex(idxTop) + 1;
                int lineBottom = TxtSource.GetLineFromCharIndex(idxBottom) + 1;

                // Scroll
                if (line < lineTop || line > lineBottom)
                {
                    // Find position to scroll to
                    int offset = (lineBottom - lineTop) / 2;
                    if (line - offset < 1) offset = line - 1;
                    int idxStart = TxtSource.GetFirstCharIndexFromLine(line - offset - 1);
                    if (idxStart < 0) idxStart = 0;

                    TxtSource.Select(idxStart, 0);
                    TxtSource.ScrollToCaret();
                }
            }

            if (!alreadyAtRightFile)
            {
                TxtSource.UpdateIndent();
                UpdateHighlights(false);
                delayHighlighting = false;
            }
            if (line > 0) TxtSource.Select(TxtSource.GetFirstCharIndexFromLine(line - 1), TxtSource.Lines[line - 1].Length);

            TxtSource.EndUpdate();
        }

        private void BtnBreak_Click(object sender, EventArgs e)
        {
            SendBreak();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            // End loggtask thread and kill the ConsoleZLR window
            RunLogTask = false;
            advProcess?.Kill(true);

            ResetGlobalVariables();
        }

        private void ResetGlobalVariables()
        {
            // Reset all globals
            TxtSource.Text = "";
            TxtSource.Rtf = "";
            TxtDisassembly.Text = "";
            txtLog.Text = "";
            CboFiles.Items.Clear();
            OldCboFilesSelectedIndex = -1;
            storyRunning = false;
            storyStarted = false;
            storyLoaded = false;
            DisassemblyAddressLineHash.Clear();
            DisassemblyLineAddressHash.Clear();
            SourceFiles.Clear();
            breakpoints.Clear();
            searchPaths.Clear();
            TvwVariables.Nodes.Clear();
            Text = "WinZLR - Z-machine debugger (no story loaded)";
            BtnStart.Text = "Start";
            SetButtons();
        }

        private void MnuSourceCodeDisassembly_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TabWinZLR.SelectedIndex == 0)
            {
                MnuItmDisassembly.Text = "Go To Disassembly";
                MnuItmGotoLine.Visible = true;
                toolStripSeparator6.Visible = true;
            }
            else
            {
                MnuItmDisassembly.Text = "Go To Source Code";
                MnuItmGotoLine.Visible = false;
                toolStripSeparator6.Visible = false;
            }
            if (storyStarted)
                MnuItmContinue.Text = "Continue";
            else
                MnuItmContinue.Text = "Start";
        }

        private void TxtDisassembly_MouseDown(object sender, MouseEventArgs e)
        {
            currentClick = e.Location;
        }

        private void ApplySyntaxHighlighters()
        {
            if (debugInfo == null) return;

            RichTextBox tmpRTB = new();
            tmpRTB.Font = TxtSource.Font;

            SyntaxHighlighter highlighterSource = new(tmpRTB);

            if (zilSource)
            {
                //ZIL, the definition order matters
                // capture ' \" ' to prevent <BUZZWORD to be treated as begining of a string
                highlighterSource.AddPattern(new PatternDefinition(@"( \\\"" )"), new SyntaxStyle(Color.Black, false, false));
                // form comment
                highlighterSource.AddPattern(new PatternDefinition(@"(;<)(?><(?<c>)|[^<>]+|>(?<-c>))*(?(c)(?!))\>"), new SyntaxStyle(Color.Green, false, false));
                // multi-line comments
                highlighterSource.AddPattern(new PatternDefinition(@";\""(.|[\r\n])*?\"""), new SyntaxStyle(Color.Green, false, false));
                // double quote strings
                highlighterSource.AddPattern(new PatternDefinition(@"\""(\\.|[^\""])*\"""), new SyntaxStyle(Color.Maroon, false, false));
                // single-line comments
                highlighterSource.AddPattern(new PatternDefinition(@";.*?$"), new SyntaxStyle(Color.Green, false, false));
                // numbers
                highlighterSource.AddPattern(new PatternDefinition(@"(?<=\s)\d+"), new SyntaxStyle(Color.DarkOrange, true, false));
                // routines
                highlighterSource.AddPattern(new PatternDefinition(@"(?<=<ROUTINE\s)(\S*)"), new SyntaxStyle(Color.Black, true, false));
                // constants
                highlighterSource.AddPattern(new PatternDefinition(@"(?<=<CONSTANT\s)(\S*)"), new SyntaxStyle(Color.FromArgb(128, 0, 255), false, false));
                // functions and properties
                highlighterSource.AddPattern(new PatternDefinition(@"<\S+"), new SyntaxStyle(Color.FromArgb(0, 128, 255), true, false));
                // keywords1
                highlighterSource.AddPattern(new PatternDefinition(">", "[", "]", "(", ")", "'", "%"), new SyntaxStyle(Color.FromArgb(0, 128, 255), true, false));
                // keywords2
                highlighterSource.AddPattern(new CaseInsensitivePatternDefinition("T", "CR", "ELSE", "TO", "IF"), new SyntaxStyle(Color.FromArgb(0, 128, 255), true, false));
                // locals
                highlighterSource.AddPattern(new PatternDefinition(@"\.\S+"), new SyntaxStyle(Color.FromArgb(163, 21, 21), false, false));
                // globals
                highlighterSource.AddPattern(new PatternDefinition(@"\,\S+"), new SyntaxStyle(Color.FromArgb(128, 0, 255), false, false));
                highlighterSource.AddPattern(new PatternDefinition([.. debugInfo.Globals.Keys]), new SyntaxStyle(Color.FromArgb(128, 0, 255), false, false));
                // objects
                List<string> o = [];
                for (int i = 0; i < debugInfo.Objects.Count; i++) o.Add(debugInfo.Objects[i].Name!);
                o.Sort((x, y) => y.Length.CompareTo(x.Length));     // sort by length so that shorter names don't spoil the longer ones
                highlighterSource.AddPattern(new PatternDefinition([.. o]), new SyntaxStyle(Color.Black, false, false));
                // arrays
                highlighterSource.AddPattern(new PatternDefinition([.. debugInfo.Arrays.Keys]), new SyntaxStyle(Color.FromArgb(128, 0, 255), false, false));
                // properties
                string[] p1 = [.. debugInfo.Properties.Keys];
                string[] p2 = new string[p1.Length + 2];
                p2[0] = "DESC";
                p2[1] = "FLAGS";
                for (int i = 0; i < p1.Length; i++) p2[i + 2] = p1[i].Replace("P?", "");
                highlighterSource.AddPattern(new PatternDefinition(p1), new SyntaxStyle(Color.FromArgb(128, 0, 255), false, false));
                highlighterSource.AddPattern(new PatternDefinition(p2), new SyntaxStyle(Color.FromArgb(0, 128, 255), false, false));
                // attributes
                highlighterSource.AddPattern(new PatternDefinition([.. debugInfo.Attributes.Keys]), new SyntaxStyle(Color.FromArgb(128, 0, 255), false, false));

            }
            else
            {
                //Inform6, the definition order matters
                // comment that contains string should all be comment 
                highlighterSource.AddPattern(new PatternDefinition(@"!.*"".*"".*", RegexOptions.Multiline), new SyntaxStyle(Color.Green, false, false));
                // ! at start of line is single-line comments 
                highlighterSource.AddPattern(new PatternDefinition(@"(^![\s|\w]).*?$", RegexOptions.Multiline), new SyntaxStyle(Color.Green, false, false));
                // double quote strings
                highlighterSource.AddPattern(new PatternDefinition(@"\""(\\.|[^\""])*\"""), new SyntaxStyle(Color.Maroon, false, false));
                // compiler-directives
                highlighterSource.AddPattern(new PatternDefinition(@"(!%).*?$", RegexOptions.Multiline), new SyntaxStyle(Color.Maroon, false, false));
                // other ! is a comment
                highlighterSource.AddPattern(new PatternDefinition(@"!.*?$", RegexOptions.Multiline), new SyntaxStyle(Color.Green, false, false));
                // keywords2
                highlighterSource.AddPattern(new PatternDefinition("(", ")", ";", ":", "+", "-", "*", "/", "%", "&", "|", "~", "=", ">", "<", ",", ".", ".#", ".&", "[", "]", "{", "}"), new SyntaxStyle(Color.Blue, true, false));
                // ## & @
                highlighterSource.AddPattern(new PatternDefinition(@"(##\S*)", RegexOptions.Multiline), new SyntaxStyle(Color.Red, false, false));
                highlighterSource.AddPattern(new PatternDefinition(@"(@\S*)", RegexOptions.Multiline), new SyntaxStyle(Color.Red, false, false));
                // compiler-directives
                highlighterSource.AddPattern(new PatternDefinition(@"#.*?$", RegexOptions.Multiline), new SyntaxStyle(Color.Maroon, false, false));
                // << >>
                highlighterSource.AddPattern(new PatternDefinition(@"(<<.*>>)"), new SyntaxStyle(Color.Red, false, false));
                // ' '
                highlighterSource.AddPattern(new PatternDefinition(@"('.*')"), new SyntaxStyle(Color.Maroon, false, false));
                // numbers
                highlighterSource.AddPattern(new PatternDefinition(@"(?<=\s)(-?\d+)"), new SyntaxStyle(Color.DarkOrange, false, false));
                highlighterSource.AddPattern(new PatternDefinition(@"(?<=\s)(\${1,2}[\dA-Fa-f]+)"), new SyntaxStyle(Color.DarkOrange, false, false));
                // keywords1
                highlighterSource.AddPattern(new CaseInsensitivePatternDefinition("a", "abbreviate", "additive", "address", "alias", "an", "array", "assembly", "attribute", "bold", "box", "break",
                                                                                  "buffer", "char", "child", "children", "class", "constant", "continue", "creature", "data", "default", "dictionary",
                                                                                  "do", "elder", "eldest", "else", "error", "expressions", "extend", "fake_action", "fatalerror", "first", "fixed",
                                                                                  "font", "for", "from", "give", "glk", "global", "held", "if", "import", "include", "indirect", "individual",
                                                                                  "initstr", "inversion", "jump", "last", "lines", "link", "linker", "long", "lowstring", "message", "meta",
                                                                                  "metaclass", "move", "multi", "multiheld", "multiexcept", "multiinside", "near", "nearby", "new_line", "noun",
                                                                                  "object", "objectloop", "objects", "off", "on", "only", "origsource", "parent", "print", "print_ret", "private",
                                                                                  "property", "quit", "random", "read", "release", "remove", "replace", "restore", "return", "reverse", "rfalse",
                                                                                  "roman", "rtrue", "save", "scope", "serial", "sibling", "spaces", "special", "string", "style", "switch",
                                                                                  "switches", "static", "statusline", "stub", "symbols", "system_file", "table", "terminating", "the", "to",
                                                                                  "tokens", "topic", "trace", "underline", "until", "verb", "verbs", "version", "warning", "while", "with",
                                                                                  "younger", "youngest", "zcharacter"), new SyntaxStyle(Color.Blue, true, false));
                // keywords3
                highlighterSource.AddPattern(new PatternDefinition(" has ", " hasnt ", " in ", " notin ", " ofclass ", " or ", " provides "), new SyntaxStyle(Color.Blue, true, false));
                // globals
                highlighterSource.AddPattern(new PatternDefinition([.. debugInfo.Globals.Keys]), new SyntaxStyle(Color.FromArgb(128, 0, 255), false, false));
            }

            CboFiles.SelectedIndex = -1;
            for (int i = 0; i < SourceFiles.Count; i++)
            {
                tmpRTB.Text = SourceFiles[i].SourceTxt;
                highlighterSource?.ReHighlight();
                SourceFiles[i].SourceRtf = tmpRTB.Rtf;
                if (i == 0) TxtSource.Clear();
            }
            TxtSource.UpdateIndent();
            Debug.Print("One {0}", DateTime.Now);
            if (CboFiles.Items.Count > 0) CboFiles.SelectedIndex = 0;
            Debug.Print("Two {0}", DateTime.Now);

            TxtSource.ShowMatchingBrackets = true;
        }

        private void ChkShowLineNumbers_CheckedChanged(object sender, EventArgs e)
        {
            TxtSource.ShowLineNumbers = ChkShowLineNumbers.Checked;
            UpdateHighlights(false);
        }

        private void ChkShowSyntaxHighlighting_CheckedChanged(object sender, EventArgs e)
        {
            if (storyLoaded)
            {
                int vScroll = TxtSource.GetVScrollPos();
                int selStart = TxtSource.SelectionStart;
                int selLen = TxtSource.SelectionLength;
                if (ChkShowSyntaxHighlighting.Checked)
                    TxtSource.Rtf = SourceFiles[CboFiles.SelectedIndex].SourceRtf;
                else
                    TxtSource.Text = SourceFiles[CboFiles.SelectedIndex].SourceTxt;
                TxtSource.Select(selStart, selLen);
                TxtSource.SetVScrollPos(vScroll);
                TxtSource.UpdateIndent();
                UpdateHighlights(false);
            }
        }

        private void TxtSource_MouseMove(object sender, MouseEventArgs e)
        {
            if (Math.Abs(e.Location.X - mouseX) > 2 || Math.Abs(e.Location.Y - mouseY) > 2)
            {
                TipVariables.Hide(TxtSource);
                mouseX = e.X;
                mouseY = e.Y;
            }
        }

        private void TxtSource_MouseHover(object sender, EventArgs e)
        {
            string tipText = TokenUnderPosition(mouseX, mouseY);

            if (tipText == "")
                TipVariables.Hide(TxtSource);
            else
                TipVariables.Show(tipText, TxtSource, mouseX + 20, mouseY);
        }

        private string TokenUnderCaret(int caret)
        {
            int textLength = TxtSource.Text.Length;

            if (caret < textLength)
            {
                int start = caret;
                int end = caret;

                string stopChars = " ()<>[]{}-+*/=.,:;\n\t";
                if (zilSource) stopChars = " ()<>[]{}+*/=.,:;\n\t";
                while (start > 0 && !stopChars.Contains(TxtSource.Text[start])) start--;
                while (end < textLength && !stopChars.Contains(TxtSource.Text[end])) end++;
                start++;
                if (end > start) return TxtSource.Text[start..end];
            }
            return "";
        }
        private string TokenUnderPosition(int x, int y)
        {
            int textLength = TxtSource.Text.Length;

            string token = TokenUnderCaret(TxtSource.GetCharIndexFromPosition(new Point(x, y)));

            if (token != "")
            {
                TreeNode[] nodesGlobal = TvwVariables.Nodes[TVW_NODE_GLOBALS].Nodes.Find(token, false);
                if (nodesGlobal.Length > 0)
                {
                    return "(global variable) " + nodesGlobal[0].Text;
                }
                else
                {
                    TreeNodeCollection nodesLocal = TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes;
                    for (int i = 0; i < nodesLocal.Count; i++)
                    {
                        if (nodesLocal[i].Text.Contains(" " + token + " = $"))
                        {
                            return "(local variable) " + nodesLocal[i].Text;
                        }
                    }
                }
            }
            return "";
        }

        private void MnuVariables_Opening(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (TvwVariables.SelectedNode == null || !storyStarted || storyRunning)
            {
                e.Cancel = true;
                return;
            }
            if (TvwVariables.SelectedNode.Parent != TvwVariables.Nodes[TVW_NODE_GLOBALS] && TvwVariables.SelectedNode.Parent != TvwVariables.Nodes[TVW_NODE_LOCALS])
            {
                e.Cancel = true;
                return;
            }
        }

        private void TvwVariables_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Button == MouseButtons.Right) TvwVariables.SelectedNode = e.Node;
        }

        private void MnuEditValue_Click(object sender, EventArgs e)
        {
            if (TvwVariables.SelectedNode != null) EditValue(TvwVariables.SelectedNode);
        }

        private void EditValue()
        {
            if (TabWinZLR.SelectedIndex != 0) return;

            string token = TokenUnderCaret(TxtSource.SelectionStart);

            if (token != "")
            {
                TreeNode[] nodesGlobal = TvwVariables.Nodes[TVW_NODE_GLOBALS].Nodes.Find(token, false);
                if (nodesGlobal.Length > 0)
                    EditValue(nodesGlobal[0]);
                else
                {
                    TreeNodeCollection nodesLocal = TvwVariables.Nodes[TVW_NODE_LOCALS].Nodes;
                    for (int i = 0; i < nodesLocal.Count; i++)
                        if (nodesLocal[i].Text.Contains(" " + token + " = $"))
                            EditValue(nodesLocal[i]);
                }
            }
        }

        private void EditValue(TreeNode node)
        {
            if (node != null && node.Level == 1 && node.Parent != null && node.Parent.Text.Contains("CALL STACK"))
            {
                if (node.Text.Contains("address"))
                {
                    int address = Convert.ToInt32(node.Tag);
                    GoToAddressInDisassembly(address, true);
                }
                else
                {
                    LineInfo li = (LineInfo)node.Tag!;
                    GoToLineInSource(li.File, li.Line, true, true);
                }
            }
            if (node != null && node.Level == 1 && node.Parent != null && (node.Parent.Text.Contains("GLOBALS") || node.Parent.Text.Contains("LOCALS")))
            {
                if (!storyStarted) return;

                if (storyRunning)
                {
                    MessageBox.Show("Can't edit values on variables while story is running.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                bool global = true;
                if (node.Parent.Text.Contains("LOCALS")) global = false;
                int colon = node.Text.IndexOf(':');
                int equal = node.Text.IndexOf('=');
                int parantStart = node.Text.IndexOf('(');
                int parantEnd = node.Text.IndexOf(')');
                string var = node.Text.Substring(colon + 1, equal - colon - 1).Trim();
                int val = Convert.ToInt32(node.Text.Substring(parantStart + 1, parantEnd - parantStart - 1));

                using EditValueForm form = new();

                form.NewValue = val;
                form.ValueText = val.ToString();
                form.LabelText = String.Concat("Edit value of local: (", var, " = ", val.ToString(), "):");
                if (global) form.LabelText = String.Concat("Change value of global: (", var, " = ", val.ToString(), "):");
                if (form.ShowDialog() == DialogResult.OK)
                {
                    val = form.NewValue;
                    SendNewValue(var, val, global);
                }
            }
        }

        private void MnuItmGoToDefinition_Click(object sender, EventArgs e)
        {
            string token = TokenUnderCaret(TxtSource.SelectionStart);

            if (token != null)
            {
                RoutineInfo? r = debugInfo?.FindRoutine(token);
                if (r != null)
                {
                    string? file = r.DefinedAt.File;
                    int line = r.DefinedAt.Line;
                    if (file == null && r.LineInfos != null && r.LineInfos.Length > 0)
                    {
                        file = r.LineInfos[0].File;
                        line = r.LineInfos[0].Line;
                    }
                    if (r.DefinedAt.File == null && file != null)
                    {
                        GoToLineInSource(file, line, true, true);
                        string tmp = TxtSource.Lines[line - 1];
                        if (tmp.Contains(token)) TxtSource.SelectionStart = TxtSource.GetFirstCharIndexFromLine(line);
                        tmp = searchString;
                        FindPrevious(token, true, true);
                        searchString = tmp;
                    }
                    else if (file != null) GoToLineInSource(file, line, true, true);
                }
            }
        }

        private void BtnSendCommand_Click(object sender, EventArgs e)
        {
            if (!storyStarted) return;

            if (storyRunning)
                MessageBox.Show("Can't send command while story is running.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
            else
            {
                client?.SendAsync(Encoding.ASCII.GetBytes(TxtCommand.Text + "\n"));
                TxtCommand.Text = "";
            }
        }

        private void MnuItmSetNextStatement_Click(object sender, EventArgs e)
        {
            SetNextStatement();
        }

        private void SetNextStatement()
        {
            if (debugInfo == null) return;
            if (!storyStarted) return;
            if (TabWinZLR.SelectedIndex > 1) return;

            if (storyRunning)
            {
                MessageBox.Show("Can't do that while story is running.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int address = -1;
            if (TabWinZLR.SelectedIndex == 0)
            {
                int line = TxtSource.GetLineFromCharIndex(TxtSource.SelectionStart) + 1;
                string file = SourceFilesFindFullPath(CboFiles.Text);
                address = debugInfo.FindCodeAddress(file, line);
            }
            else
            {
                int line = TxtDisassembly.GetLineFromCharIndex(TxtDisassembly.SelectionStart);
                while (TxtDisassembly.Lines[line].Length < 6 || !IsHex(TxtDisassembly.Lines[line][0..5])) line--;
                address = HexToInt(TxtDisassembly.Lines[line][0..5]);
            }

            LineInfo? li;
            li = debugInfo.FindLine(address);

            if (li == null)
            {
                MessageBox.Show("Unable to set the next statement. There is no executable code at this location in the source code.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            RoutineInfo? r = debugInfo.FindRoutine(HexToInt(currentAddress));
            if (r == null) return;

            foreach (LineInfo l in r.LineInfos)
            {
                if (l.File == li?.File && l.Line == li?.Line)
                {
                    client?.SendAsync(Encoding.ASCII.GetBytes("jump $" + address.ToString("x5") + "\n"));
                    return;
                }
            }

            MessageBox.Show("Unable to set the next statement. The next statement cannot be set to another function.", "WinZLR", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void TxtCommand_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)Keys.Enter)
            {
                e.Handled = true;
                BtnSendCommand.PerformClick();
            }
        }

        private void MnuItmEditValue_Click(object sender, EventArgs e)
        {
            EditValue();
        }

        private void BtnAbout_Click(object sender, EventArgs e)
        {
            AboutBoxForm a = new AboutBoxForm();
            a.ShowDialog();
        }
    }
}