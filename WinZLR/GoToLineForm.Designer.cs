namespace winZLR
{
    partial class GoToLineForm
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
            BtnOK = new Button();
            BtnCancel = new Button();
            LblLine = new Label();
            TxtLine = new TextBox();
            SuspendLayout();
            // 
            // BtnOK
            // 
            BtnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnOK.Location = new Point(140, 117);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(112, 34);
            BtnOK.TabIndex = 1;
            BtnOK.Text = "&OK";
            BtnOK.UseVisualStyleBackColor = true;
            BtnOK.Click += BtnOK_Click;
            // 
            // BtnCancel
            // 
            BtnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnCancel.Location = new Point(258, 117);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(112, 34);
            BtnCancel.TabIndex = 2;
            BtnCancel.Text = "&Cancel";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // LblLine
            // 
            LblLine.AutoSize = true;
            LblLine.Location = new Point(9, 22);
            LblLine.Name = "LblLine";
            LblLine.Size = new Size(186, 25);
            LblLine.TabIndex = 3;
            LblLine.Text = "Line number (1-1000):";
            // 
            // TxtLine
            // 
            TxtLine.BorderStyle = BorderStyle.FixedSingle;
            TxtLine.Location = new Point(12, 50);
            TxtLine.Name = "TxtLine";
            TxtLine.Size = new Size(358, 31);
            TxtLine.TabIndex = 0;
            TxtLine.KeyPress += TxtLine_KeyPress;
            // 
            // GoToLineForm
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(382, 163);
            Controls.Add(TxtLine);
            Controls.Add(LblLine);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "GoToLineForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Go To Line";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private Button BtnOK;
        private Button BtnCancel;
        private Label LblLine;
        private TextBox TxtLine;
    }
}