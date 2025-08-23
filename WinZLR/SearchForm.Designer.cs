namespace winZLR
{
    partial class SearchForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
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
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            BtnPrevious = new Button();
            BtnNext = new Button();
            BtnClose = new Button();
            TxtSearch = new TextBox();
            ChkMatchCase = new CheckBox();
            ChkSearchAllFiles = new CheckBox();
            LblSearch = new Label();
            SuspendLayout();
            // 
            // BtnPrevious
            // 
            BtnPrevious.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnPrevious.Location = new Point(84, 176);
            BtnPrevious.Name = "BtnPrevious";
            BtnPrevious.Size = new Size(138, 34);
            BtnPrevious.TabIndex = 3;
            BtnPrevious.Text = "Find &Previous";
            BtnPrevious.UseVisualStyleBackColor = true;
            BtnPrevious.Click += BtnPrevious_Click;
            // 
            // BtnNext
            // 
            BtnNext.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnNext.Location = new Point(228, 176);
            BtnNext.Name = "BtnNext";
            BtnNext.Size = new Size(138, 34);
            BtnNext.TabIndex = 4;
            BtnNext.Text = "&Find Next";
            BtnNext.UseVisualStyleBackColor = true;
            BtnNext.Click += BtnNext_Click;
            // 
            // BtnClose
            // 
            BtnClose.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnClose.Location = new Point(372, 176);
            BtnClose.Name = "BtnClose";
            BtnClose.Size = new Size(138, 34);
            BtnClose.TabIndex = 5;
            BtnClose.Text = "&Close";
            BtnClose.UseVisualStyleBackColor = true;
            BtnClose.Click += BtnClose_Click;
            // 
            // TxtSearch
            // 
            TxtSearch.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            TxtSearch.BorderStyle = BorderStyle.FixedSingle;
            TxtSearch.Location = new Point(12, 50);
            TxtSearch.Name = "TxtSearch";
            TxtSearch.Size = new Size(498, 31);
            TxtSearch.TabIndex = 0;
            // 
            // ChkMatchCase
            // 
            ChkMatchCase.AutoSize = true;
            ChkMatchCase.Location = new Point(12, 87);
            ChkMatchCase.Name = "ChkMatchCase";
            ChkMatchCase.Size = new Size(126, 29);
            ChkMatchCase.TabIndex = 1;
            ChkMatchCase.Text = "Match case";
            ChkMatchCase.UseVisualStyleBackColor = true;
            // 
            // ChkSearchAllFiles
            // 
            ChkSearchAllFiles.AutoSize = true;
            ChkSearchAllFiles.Checked = true;
            ChkSearchAllFiles.CheckState = CheckState.Checked;
            ChkSearchAllFiles.Location = new Point(12, 122);
            ChkSearchAllFiles.Name = "ChkSearchAllFiles";
            ChkSearchAllFiles.Size = new Size(167, 29);
            ChkSearchAllFiles.TabIndex = 2;
            ChkSearchAllFiles.Text = "Search in &all files";
            ChkSearchAllFiles.UseVisualStyleBackColor = true;
            // 
            // LblSearch
            // 
            LblSearch.AutoSize = true;
            LblSearch.Location = new Point(9, 22);
            LblSearch.Name = "LblSearch";
            LblSearch.Size = new Size(105, 25);
            LblSearch.TabIndex = 6;
            LblSearch.Text = "Find in files:";
            // 
            // SearchForm
            // 
            AcceptButton = BtnNext;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnClose;
            ClientSize = new Size(522, 222);
            Controls.Add(LblSearch);
            Controls.Add(ChkSearchAllFiles);
            Controls.Add(ChkMatchCase);
            Controls.Add(TxtSearch);
            Controls.Add(BtnClose);
            Controls.Add(BtnNext);
            Controls.Add(BtnPrevious);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "SearchForm";
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Find";
            Load += SearchForm_Load;
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button BtnPrevious;
        private Button BtnNext;
        private Button BtnClose;
        private TextBox TxtSearch;
        private CheckBox ChkMatchCase;
        private CheckBox ChkSearchAllFiles;
        private Label LblSearch;
    }
}