namespace winZLR
{
    partial class WinZLRForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            BtnStart = new Button();
            txtLog = new SubRichTextBox();
            TxtSource = new SubRichTextBox();
            MnuSourceCodeDisassembly = new ContextMenuStrip(components);
            MnuItmContinue = new ToolStripMenuItem();
            MnuItmBreak = new ToolStripMenuItem();
            MnuItmRestart = new ToolStripMenuItem();
            toolStripSeparator1 = new ToolStripSeparator();
            MnuItmStepOver = new ToolStripMenuItem();
            MnuItmStepInto = new ToolStripMenuItem();
            MnuItemStepOut = new ToolStripMenuItem();
            toolStripSeparator2 = new ToolStripSeparator();
            MnuItmEditValue = new ToolStripMenuItem();
            MnuItmGoToDefinition = new ToolStripMenuItem();
            toolStripSeparator3 = new ToolStripSeparator();
            MnuItmBreakpoint = new ToolStripMenuItem();
            MnuItmDeleteBreakpoints = new ToolStripMenuItem();
            toolStripSeparator4 = new ToolStripSeparator();
            MnuRunToCursor = new ToolStripMenuItem();
            MnuItmSetNextStatement = new ToolStripMenuItem();
            MnuItmDisassembly = new ToolStripMenuItem();
            toolStripSeparator5 = new ToolStripSeparator();
            MnuItmSearch = new ToolStripMenuItem();
            MnuItmSearchNext = new ToolStripMenuItem();
            MnuItmSearchPrevious = new ToolStripMenuItem();
            toolStripSeparator6 = new ToolStripSeparator();
            MnuItmGotoLine = new ToolStripMenuItem();
            TmrResponse = new System.Windows.Forms.Timer(components);
            TabWinZLR = new TabControl();
            TabPageSource = new TabPage();
            LblFile = new Label();
            CboFiles = new ComboBox();
            TabPageDisassembly = new TabPage();
            TxtDisassembly = new SubRichTextBox();
            TabPageLog = new TabPage();
            BtnSendCommand = new Button();
            TxtCommand = new TextBox();
            lblCommand = new Label();
            TabSettings = new TabPage();
            TxtUnZPath = new TextBox();
            LblUnZPath = new Label();
            TxtConsoleZLRPath = new TextBox();
            LblConsolZLRPath = new Label();
            ChkShowLineNumbers = new CheckBox();
            ChkShowSyntaxHighlighting = new CheckBox();
            TxtSearchPaths = new TextBox();
            LblSearchPaths = new Label();
            BtnClose = new Button();
            TvwVariables = new TreeView();
            MnuVariables = new ContextMenuStrip(components);
            MnuEditValue = new ToolStripMenuItem();
            LblVariables = new Label();
            BtnOpen = new Button();
            SelectFileDialog = new OpenFileDialog();
            BtnBreak = new Button();
            BtnStop = new Button();
            label1 = new Label();
            TipVariables = new ToolTip(components);
            BtnAbout = new Button();
            MnuSourceCodeDisassembly.SuspendLayout();
            TabWinZLR.SuspendLayout();
            TabPageSource.SuspendLayout();
            TabPageDisassembly.SuspendLayout();
            TabPageLog.SuspendLayout();
            TabSettings.SuspendLayout();
            MnuVariables.SuspendLayout();
            SuspendLayout();
            // 
            // BtnStart
            // 
            BtnStart.Location = new Point(130, 9);
            BtnStart.Name = "BtnStart";
            BtnStart.Size = new Size(112, 34);
            BtnStart.TabIndex = 0;
            BtnStart.Text = "Start";
            BtnStart.UseVisualStyleBackColor = true;
            BtnStart.Click += BtnStart_Click;
            // 
            // txtLog
            // 
            txtLog.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            txtLog.BackColor = SystemColors.WindowText;
            txtLog.Font = new Font("Courier New", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            txtLog.ForeColor = SystemColors.Window;
            txtLog.Location = new Point(6, 54);
            txtLog.Name = "txtLog";
            txtLog.Size = new Size(1415, 563);
            txtLog.TabIndex = 5;
            txtLog.Text = "";
            // 
            // TxtSource
            // 
            TxtSource.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TxtSource.ContextMenuStrip = MnuSourceCodeDisassembly;
            TxtSource.Font = new Font("Courier New", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TxtSource.Location = new Point(6, 66);
            TxtSource.Name = "TxtSource";
            TxtSource.Size = new Size(1415, 551);
            TxtSource.TabIndex = 6;
            TxtSource.Text = "";
            TxtSource.WordWrap = false;
            TxtSource.MouseDown += TxtSource_MouseDown;
            TxtSource.MouseHover += TxtSource_MouseHover;
            TxtSource.MouseMove += TxtSource_MouseMove;
            // 
            // MnuSourceCodeDisassembly
            // 
            MnuSourceCodeDisassembly.ImageScalingSize = new Size(24, 24);
            MnuSourceCodeDisassembly.Items.AddRange(new ToolStripItem[] { MnuItmContinue, MnuItmBreak, MnuItmRestart, toolStripSeparator1, MnuItmStepOver, MnuItmStepInto, MnuItemStepOut, toolStripSeparator2, MnuItmEditValue, MnuItmGoToDefinition, toolStripSeparator3, MnuItmBreakpoint, MnuItmDeleteBreakpoints, toolStripSeparator4, MnuRunToCursor, MnuItmSetNextStatement, MnuItmDisassembly, toolStripSeparator5, MnuItmSearch, MnuItmSearchNext, MnuItmSearchPrevious, toolStripSeparator6, MnuItmGotoLine });
            MnuSourceCodeDisassembly.Name = "MnuSourceCode";
            MnuSourceCodeDisassembly.Size = new Size(376, 584);
            MnuSourceCodeDisassembly.Opening += MnuSourceCodeDisassembly_Opening;
            // 
            // MnuItmContinue
            // 
            MnuItmContinue.Name = "MnuItmContinue";
            MnuItmContinue.ShortcutKeys = Keys.F5;
            MnuItmContinue.Size = new Size(375, 32);
            MnuItmContinue.Text = "Continue";
            MnuItmContinue.Click += MnuItmContinue_Click;
            // 
            // MnuItmBreak
            // 
            MnuItmBreak.Name = "MnuItmBreak";
            MnuItmBreak.ShortcutKeys = Keys.Control | Keys.Alt | Keys.B;
            MnuItmBreak.Size = new Size(375, 32);
            MnuItmBreak.Text = "Break";
            MnuItmBreak.Click += MnuItmBreak_Click;
            // 
            // MnuItmRestart
            // 
            MnuItmRestart.Name = "MnuItmRestart";
            MnuItmRestart.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F5;
            MnuItmRestart.Size = new Size(375, 32);
            MnuItmRestart.Text = "Restart";
            MnuItmRestart.Click += MnuItmRestart_Click;
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new Size(372, 6);
            // 
            // MnuItmStepOver
            // 
            MnuItmStepOver.Name = "MnuItmStepOver";
            MnuItmStepOver.ShortcutKeys = Keys.F10;
            MnuItmStepOver.Size = new Size(375, 32);
            MnuItmStepOver.Text = "Step Over";
            MnuItmStepOver.Click += MnuItmStepOver_Click;
            // 
            // MnuItmStepInto
            // 
            MnuItmStepInto.Name = "MnuItmStepInto";
            MnuItmStepInto.ShortcutKeys = Keys.F11;
            MnuItmStepInto.Size = new Size(375, 32);
            MnuItmStepInto.Text = "Step Into";
            MnuItmStepInto.Click += MnuItmStepInto_Click;
            // 
            // MnuItemStepOut
            // 
            MnuItemStepOut.Name = "MnuItemStepOut";
            MnuItemStepOut.ShortcutKeys = Keys.Shift | Keys.F11;
            MnuItemStepOut.Size = new Size(375, 32);
            MnuItemStepOut.Text = "Step Out";
            MnuItemStepOut.Click += MnuItemStepOut_Click;
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new Size(372, 6);
            // 
            // MnuItmEditValue
            // 
            MnuItmEditValue.Name = "MnuItmEditValue";
            MnuItmEditValue.ShortcutKeys = Keys.F2;
            MnuItmEditValue.Size = new Size(375, 32);
            MnuItmEditValue.Text = "Edit Value";
            MnuItmEditValue.Click += MnuItmEditValue_Click;
            // 
            // MnuItmGoToDefinition
            // 
            MnuItmGoToDefinition.Name = "MnuItmGoToDefinition";
            MnuItmGoToDefinition.ShortcutKeys = Keys.F12;
            MnuItmGoToDefinition.Size = new Size(375, 32);
            MnuItmGoToDefinition.Text = "Go To Definition";
            MnuItmGoToDefinition.Click += MnuItmGoToDefinition_Click;
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new Size(372, 6);
            // 
            // MnuItmBreakpoint
            // 
            MnuItmBreakpoint.Name = "MnuItmBreakpoint";
            MnuItmBreakpoint.ShortcutKeys = Keys.F9;
            MnuItmBreakpoint.Size = new Size(375, 32);
            MnuItmBreakpoint.Text = "Toggle Breakpoint";
            MnuItmBreakpoint.Click += MnuItmBreakpoint_Click;
            // 
            // MnuItmDeleteBreakpoints
            // 
            MnuItmDeleteBreakpoints.Name = "MnuItmDeleteBreakpoints";
            MnuItmDeleteBreakpoints.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F9;
            MnuItmDeleteBreakpoints.Size = new Size(375, 32);
            MnuItmDeleteBreakpoints.Text = "Delete All Breakpoints";
            MnuItmDeleteBreakpoints.Click += MnuItmDeleteBreakpoints_Click;
            // 
            // toolStripSeparator4
            // 
            toolStripSeparator4.Name = "toolStripSeparator4";
            toolStripSeparator4.Size = new Size(372, 6);
            // 
            // MnuRunToCursor
            // 
            MnuRunToCursor.Name = "MnuRunToCursor";
            MnuRunToCursor.ShortcutKeys = Keys.Control | Keys.F10;
            MnuRunToCursor.Size = new Size(375, 32);
            MnuRunToCursor.Text = "Run To Cursor";
            MnuRunToCursor.Click += MnuRunToCursor_Click;
            // 
            // MnuItmSetNextStatement
            // 
            MnuItmSetNextStatement.Name = "MnuItmSetNextStatement";
            MnuItmSetNextStatement.ShortcutKeys = Keys.Control | Keys.Shift | Keys.F10;
            MnuItmSetNextStatement.Size = new Size(375, 32);
            MnuItmSetNextStatement.Text = "Set Next Statement";
            MnuItmSetNextStatement.Click += MnuItmSetNextStatement_Click;
            // 
            // MnuItmDisassembly
            // 
            MnuItmDisassembly.Name = "MnuItmDisassembly";
            MnuItmDisassembly.ShortcutKeys = Keys.Control | Keys.K;
            MnuItmDisassembly.Size = new Size(375, 32);
            MnuItmDisassembly.Text = "Go To Disassembly";
            MnuItmDisassembly.Click += MnuItmDisassembly_Click;
            // 
            // toolStripSeparator5
            // 
            toolStripSeparator5.Name = "toolStripSeparator5";
            toolStripSeparator5.Size = new Size(372, 6);
            // 
            // MnuItmSearch
            // 
            MnuItmSearch.Name = "MnuItmSearch";
            MnuItmSearch.ShortcutKeys = Keys.Control | Keys.F;
            MnuItmSearch.Size = new Size(375, 32);
            MnuItmSearch.Text = "Search...";
            MnuItmSearch.Click += MnuItmSearch_Click;
            // 
            // MnuItmSearchNext
            // 
            MnuItmSearchNext.Name = "MnuItmSearchNext";
            MnuItmSearchNext.ShortcutKeys = Keys.F3;
            MnuItmSearchNext.Size = new Size(375, 32);
            MnuItmSearchNext.Text = "Search Next";
            // 
            // MnuItmSearchPrevious
            // 
            MnuItmSearchPrevious.Name = "MnuItmSearchPrevious";
            MnuItmSearchPrevious.ShortcutKeys = Keys.Shift | Keys.F3;
            MnuItmSearchPrevious.Size = new Size(375, 32);
            MnuItmSearchPrevious.Text = "Search Previous";
            // 
            // toolStripSeparator6
            // 
            toolStripSeparator6.Name = "toolStripSeparator6";
            toolStripSeparator6.Size = new Size(372, 6);
            // 
            // MnuItmGotoLine
            // 
            MnuItmGotoLine.Name = "MnuItmGotoLine";
            MnuItmGotoLine.ShortcutKeys = Keys.Control | Keys.G;
            MnuItmGotoLine.Size = new Size(375, 32);
            MnuItmGotoLine.Text = "Go To Line";
            MnuItmGotoLine.Click += MnuItmGotoLine_Click;
            // 
            // TmrResponse
            // 
            TmrResponse.Tick += TmrResponse_Tick;
            // 
            // TabWinZLR
            // 
            TabWinZLR.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TabWinZLR.Controls.Add(TabPageSource);
            TabWinZLR.Controls.Add(TabPageDisassembly);
            TabWinZLR.Controls.Add(TabPageLog);
            TabWinZLR.Controls.Add(TabSettings);
            TabWinZLR.Location = new Point(12, 49);
            TabWinZLR.Name = "TabWinZLR";
            TabWinZLR.SelectedIndex = 0;
            TabWinZLR.Size = new Size(1435, 661);
            TabWinZLR.TabIndex = 7;
            // 
            // TabPageSource
            // 
            TabPageSource.Controls.Add(LblFile);
            TabPageSource.Controls.Add(CboFiles);
            TabPageSource.Controls.Add(TxtSource);
            TabPageSource.Location = new Point(4, 34);
            TabPageSource.Name = "TabPageSource";
            TabPageSource.Padding = new Padding(3);
            TabPageSource.Size = new Size(1427, 623);
            TabPageSource.TabIndex = 0;
            TabPageSource.Text = "Source Code";
            TabPageSource.UseVisualStyleBackColor = true;
            // 
            // LblFile
            // 
            LblFile.AutoSize = true;
            LblFile.Location = new Point(6, 23);
            LblFile.Name = "LblFile";
            LblFile.Size = new Size(42, 25);
            LblFile.TabIndex = 8;
            LblFile.Text = "File:";
            // 
            // CboFiles
            // 
            CboFiles.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            CboFiles.DropDownStyle = ComboBoxStyle.DropDownList;
            CboFiles.FormattingEnabled = true;
            CboFiles.Location = new Point(54, 20);
            CboFiles.Name = "CboFiles";
            CboFiles.Size = new Size(1367, 33);
            CboFiles.TabIndex = 7;
            CboFiles.SelectedIndexChanged += CboFiles_SelectedIndexChanged;
            // 
            // TabPageDisassembly
            // 
            TabPageDisassembly.Controls.Add(TxtDisassembly);
            TabPageDisassembly.Location = new Point(4, 34);
            TabPageDisassembly.Name = "TabPageDisassembly";
            TabPageDisassembly.Padding = new Padding(3);
            TabPageDisassembly.Size = new Size(1427, 623);
            TabPageDisassembly.TabIndex = 2;
            TabPageDisassembly.Text = "Disassembly";
            TabPageDisassembly.UseVisualStyleBackColor = true;
            // 
            // TxtDisassembly
            // 
            TxtDisassembly.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            TxtDisassembly.ContextMenuStrip = MnuSourceCodeDisassembly;
            TxtDisassembly.Font = new Font("Courier New", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TxtDisassembly.HideSelection = false;
            TxtDisassembly.Location = new Point(6, 6);
            TxtDisassembly.Name = "TxtDisassembly";
            TxtDisassembly.Size = new Size(1415, 611);
            TxtDisassembly.TabIndex = 0;
            TxtDisassembly.Text = "";
            TxtDisassembly.WordWrap = false;
            TxtDisassembly.MouseDown += TxtDisassembly_MouseDown;
            // 
            // TabPageLog
            // 
            TabPageLog.Controls.Add(BtnSendCommand);
            TabPageLog.Controls.Add(TxtCommand);
            TabPageLog.Controls.Add(lblCommand);
            TabPageLog.Controls.Add(txtLog);
            TabPageLog.Location = new Point(4, 34);
            TabPageLog.Name = "TabPageLog";
            TabPageLog.Padding = new Padding(3);
            TabPageLog.Size = new Size(1427, 623);
            TabPageLog.TabIndex = 1;
            TabPageLog.Text = "Log";
            TabPageLog.UseVisualStyleBackColor = true;
            // 
            // BtnSendCommand
            // 
            BtnSendCommand.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            BtnSendCommand.Location = new Point(1309, 17);
            BtnSendCommand.Name = "BtnSendCommand";
            BtnSendCommand.Size = new Size(112, 31);
            BtnSendCommand.TabIndex = 8;
            BtnSendCommand.Text = "&Send";
            BtnSendCommand.UseVisualStyleBackColor = true;
            BtnSendCommand.Click += BtnSendCommand_Click;
            // 
            // TxtCommand
            // 
            TxtCommand.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtCommand.Location = new Point(114, 17);
            TxtCommand.Name = "TxtCommand";
            TxtCommand.Size = new Size(1189, 31);
            TxtCommand.TabIndex = 7;
            TxtCommand.KeyPress += TxtCommand_KeyPress;
            // 
            // lblCommand
            // 
            lblCommand.AutoSize = true;
            lblCommand.Location = new Point(6, 20);
            lblCommand.Name = "lblCommand";
            lblCommand.Size = new Size(100, 25);
            lblCommand.TabIndex = 6;
            lblCommand.Text = "Command:";
            // 
            // TabSettings
            // 
            TabSettings.Controls.Add(TxtUnZPath);
            TabSettings.Controls.Add(LblUnZPath);
            TabSettings.Controls.Add(TxtConsoleZLRPath);
            TabSettings.Controls.Add(LblConsolZLRPath);
            TabSettings.Controls.Add(ChkShowLineNumbers);
            TabSettings.Controls.Add(ChkShowSyntaxHighlighting);
            TabSettings.Controls.Add(TxtSearchPaths);
            TabSettings.Controls.Add(LblSearchPaths);
            TabSettings.Location = new Point(4, 34);
            TabSettings.Name = "TabSettings";
            TabSettings.Padding = new Padding(3);
            TabSettings.Size = new Size(1427, 623);
            TabSettings.TabIndex = 3;
            TabSettings.Text = "Settings";
            TabSettings.UseVisualStyleBackColor = true;
            // 
            // TxtUnZPath
            // 
            TxtUnZPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtUnZPath.Location = new Point(10, 195);
            TxtUnZPath.Name = "TxtUnZPath";
            TxtUnZPath.Size = new Size(827, 31);
            TxtUnZPath.TabIndex = 7;
            // 
            // LblUnZPath
            // 
            LblUnZPath.AutoSize = true;
            LblUnZPath.Location = new Point(10, 167);
            LblUnZPath.Name = "LblUnZPath";
            LblUnZPath.Size = new Size(494, 25);
            LblUnZPath.TabIndex = 6;
            LblUnZPath.Text = "Path to disassembler UnZ (empty searches in ordinary paths):";
            // 
            // TxtConsoleZLRPath
            // 
            TxtConsoleZLRPath.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtConsoleZLRPath.Location = new Point(6, 119);
            TxtConsoleZLRPath.Name = "TxtConsoleZLRPath";
            TxtConsoleZLRPath.Size = new Size(831, 31);
            TxtConsoleZLRPath.TabIndex = 5;
            // 
            // LblConsolZLRPath
            // 
            LblConsolZLRPath.AutoSize = true;
            LblConsolZLRPath.Location = new Point(10, 91);
            LblConsolZLRPath.Name = "LblConsolZLRPath";
            LblConsolZLRPath.Size = new Size(455, 25);
            LblConsolZLRPath.TabIndex = 4;
            LblConsolZLRPath.Text = "Paths to ConsoleZLR (empty searches in ordinary paths):";
            // 
            // ChkShowLineNumbers
            // 
            ChkShowLineNumbers.AutoSize = true;
            ChkShowLineNumbers.Location = new Point(10, 290);
            ChkShowLineNumbers.Name = "ChkShowLineNumbers";
            ChkShowLineNumbers.Size = new Size(196, 29);
            ChkShowLineNumbers.TabIndex = 3;
            ChkShowLineNumbers.Text = "Show Line Numbers";
            ChkShowLineNumbers.UseVisualStyleBackColor = true;
            ChkShowLineNumbers.CheckedChanged += ChkShowLineNumbers_CheckedChanged;
            // 
            // ChkShowSyntaxHighlighting
            // 
            ChkShowSyntaxHighlighting.AutoSize = true;
            ChkShowSyntaxHighlighting.Location = new Point(10, 255);
            ChkShowSyntaxHighlighting.Name = "ChkShowSyntaxHighlighting";
            ChkShowSyntaxHighlighting.Size = new Size(241, 29);
            ChkShowSyntaxHighlighting.TabIndex = 2;
            ChkShowSyntaxHighlighting.Text = "Show Syntax Highlighting";
            ChkShowSyntaxHighlighting.UseVisualStyleBackColor = true;
            ChkShowSyntaxHighlighting.CheckedChanged += ChkShowSyntaxHighlighting_CheckedChanged;
            // 
            // TxtSearchPaths
            // 
            TxtSearchPaths.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtSearchPaths.Location = new Point(10, 44);
            TxtSearchPaths.Name = "TxtSearchPaths";
            TxtSearchPaths.Size = new Size(827, 31);
            TxtSearchPaths.TabIndex = 1;
            // 
            // LblSearchPaths
            // 
            LblSearchPaths.AutoSize = true;
            LblSearchPaths.Location = new Point(6, 16);
            LblSearchPaths.Name = "LblSearchPaths";
            LblSearchPaths.Size = new Size(814, 25);
            LblSearchPaths.TabIndex = 0;
            LblSearchPaths.Text = "Search paths (in addition to story path) for source code files, seperate multiple paths by semicolon (;):";
            // 
            // BtnClose
            // 
            BtnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnClose.Location = new Point(1752, 716);
            BtnClose.Name = "BtnClose";
            BtnClose.Size = new Size(112, 34);
            BtnClose.TabIndex = 8;
            BtnClose.Text = "Close";
            BtnClose.UseVisualStyleBackColor = true;
            BtnClose.Click += BtnClose_Click;
            // 
            // TvwVariables
            // 
            TvwVariables.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Right;
            TvwVariables.ContextMenuStrip = MnuVariables;
            TvwVariables.Font = new Font("Courier New", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
            TvwVariables.Location = new Point(1453, 83);
            TvwVariables.Name = "TvwVariables";
            TvwVariables.Size = new Size(411, 623);
            TvwVariables.TabIndex = 9;
            TvwVariables.NodeMouseClick += TvwVariables_NodeMouseClick;
            TvwVariables.NodeMouseDoubleClick += TvwVariables_NodeMouseDoubleClick;
            // 
            // MnuVariables
            // 
            MnuVariables.ImageScalingSize = new Size(24, 24);
            MnuVariables.Items.AddRange(new ToolStripItem[] { MnuEditValue });
            MnuVariables.Name = "MnuVariables";
            MnuVariables.Size = new Size(174, 36);
            MnuVariables.Opening += MnuVariables_Opening;
            // 
            // MnuEditValue
            // 
            MnuEditValue.Name = "MnuEditValue";
            MnuEditValue.Size = new Size(173, 32);
            MnuEditValue.Text = "Edit Value...";
            MnuEditValue.Click += MnuEditValue_Click;
            // 
            // LblVariables
            // 
            LblVariables.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            LblVariables.AutoSize = true;
            LblVariables.Location = new Point(1453, 55);
            LblVariables.Name = "LblVariables";
            LblVariables.Size = new Size(86, 25);
            LblVariables.TabIndex = 10;
            LblVariables.Text = "Variables:";
            // 
            // BtnOpen
            // 
            BtnOpen.Location = new Point(12, 9);
            BtnOpen.Name = "BtnOpen";
            BtnOpen.Size = new Size(112, 34);
            BtnOpen.TabIndex = 13;
            BtnOpen.Text = "Open";
            BtnOpen.UseVisualStyleBackColor = true;
            BtnOpen.Click += BtnOpen_Click;
            // 
            // BtnBreak
            // 
            BtnBreak.Location = new Point(248, 9);
            BtnBreak.Name = "BtnBreak";
            BtnBreak.Size = new Size(112, 34);
            BtnBreak.TabIndex = 14;
            BtnBreak.Text = "Break";
            BtnBreak.UseVisualStyleBackColor = true;
            BtnBreak.Click += BtnBreak_Click;
            // 
            // BtnStop
            // 
            BtnStop.Location = new Point(366, 9);
            BtnStop.Name = "BtnStop";
            BtnStop.Size = new Size(112, 34);
            BtnStop.TabIndex = 15;
            BtnStop.Text = "Stop";
            BtnStop.UseVisualStyleBackColor = true;
            BtnStop.Click += BtnStop_Click;
            // 
            // label1
            // 
            label1.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            label1.Location = new Point(12, 725);
            label1.Name = "label1";
            label1.Size = new Size(1734, 25);
            label1.TabIndex = 16;
            label1.Text = "label1";
            // 
            // TipVariables
            // 
            TipVariables.ShowAlways = true;
            // 
            // BtnAbout
            // 
            BtnAbout.Location = new Point(484, 9);
            BtnAbout.Name = "BtnAbout";
            BtnAbout.Size = new Size(112, 34);
            BtnAbout.TabIndex = 17;
            BtnAbout.Text = "About";
            BtnAbout.UseVisualStyleBackColor = true;
            BtnAbout.Click += BtnAbout_Click;
            // 
            // WinZLRForm
            // 
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = SystemColors.Control;
            CancelButton = BtnClose;
            ClientSize = new Size(1880, 762);
            Controls.Add(BtnAbout);
            Controls.Add(label1);
            Controls.Add(BtnStop);
            Controls.Add(BtnBreak);
            Controls.Add(BtnOpen);
            Controls.Add(LblVariables);
            Controls.Add(TvwVariables);
            Controls.Add(BtnClose);
            Controls.Add(TabWinZLR);
            Controls.Add(BtnStart);
            KeyPreview = true;
            Name = "WinZLRForm";
            ShowIcon = false;
            Text = "WinZLR";
            FormClosing += WinZLRForm_FormClosing;
            MnuSourceCodeDisassembly.ResumeLayout(false);
            TabWinZLR.ResumeLayout(false);
            TabPageSource.ResumeLayout(false);
            TabPageSource.PerformLayout();
            TabPageDisassembly.ResumeLayout(false);
            TabPageLog.ResumeLayout(false);
            TabPageLog.PerformLayout();
            TabSettings.ResumeLayout(false);
            TabSettings.PerformLayout();
            MnuVariables.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button BtnStart;
        private SubRichTextBox txtLog;
        private SubRichTextBox TxtSource;
        private System.Windows.Forms.Timer TmrResponse;
        private ContextMenuStrip MnuSourceCodeDisassembly;
        private ToolStripMenuItem MnuItmBreakpoint;
        private ToolStripMenuItem MnuItmContinue;
        private ToolStripMenuItem MnuItmStepOver;
        private ToolStripMenuItem MnuItmStepInto;
        private ToolStripMenuItem MnuItmRestart;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem MnuItemStepOut;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem MnuItmDisassembly;
        private ToolStripMenuItem MnuItmBreak;
        private ToolStripMenuItem MnuItmDeleteBreakpoints;
        private ToolStripMenuItem MnuItmSearch;
        private ToolStripMenuItem MnuItmSearchNext;
        private ToolStripSeparator toolStripSeparator4;
        private ToolStripMenuItem MnuItmSearchPrevious;
        private ToolStripSeparator toolStripSeparator5;
        private ToolStripMenuItem MnuItmGotoLine;
        private TabControl TabWinZLR;
        private TabPage TabPageSource;
        private TabPage TabPageLog;
        private TabPage TabPageDisassembly;
        private Label LblFile;
        private ComboBox CboFiles;
        private Button BtnClose;
        private SubRichTextBox TxtDisassembly;
        private TreeView TvwVariables;
        private Label LblVariables;
        private ToolStripMenuItem MnuRunToCursor;
        private Button BtnOpen;
        private OpenFileDialog SelectFileDialog;
        private TabPage TabSettings;
        private TextBox TxtSearchPaths;
        private Label LblSearchPaths;
        private Button BtnBreak;
        private Button BtnStop;
        private CheckBox ChkShowSyntaxHighlighting;
        private CheckBox ChkShowLineNumbers;
        private TextBox TxtUnZPath;
        private Label LblUnZPath;
        private TextBox TxtConsoleZLRPath;
        private Label LblConsolZLRPath;
        private Label label1;
        private ToolTip TipVariables;
        private ContextMenuStrip MnuVariables;
        private ToolStripMenuItem MnuEditValue;
        private ToolStripMenuItem MnuItmGoToDefinition;
        private ToolStripSeparator toolStripSeparator6;
        private Button BtnSendCommand;
        private TextBox TxtCommand;
        private Label lblCommand;
        private ToolStripMenuItem MnuItmSetNextStatement;
        private ToolStripMenuItem MnuItmEditValue;
        private Button BtnAbout;
    }
}
