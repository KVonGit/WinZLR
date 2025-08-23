namespace winZLR
{
    partial class EditValueForm
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
            TxtValue = new TextBox();
            LblValue = new Label();
            BtnCancel = new Button();
            BtnOK = new Button();
            SuspendLayout();
            // 
            // TxtValue
            // 
            TxtValue.BorderStyle = BorderStyle.FixedSingle;
            TxtValue.Location = new Point(12, 54);
            TxtValue.Name = "TxtValue";
            TxtValue.Size = new Size(489, 31);
            TxtValue.TabIndex = 4;
            TxtValue.KeyPress += TxtValue_KeyPress;
            // 
            // LblValue
            // 
            LblValue.AutoSize = true;
            LblValue.Location = new Point(9, 26);
            LblValue.Name = "LblValue";
            LblValue.Size = new Size(88, 25);
            LblValue.TabIndex = 7;
            LblValue.Text = "Edit value";
            // 
            // BtnCancel
            // 
            BtnCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnCancel.Location = new Point(389, 117);
            BtnCancel.Name = "BtnCancel";
            BtnCancel.Size = new Size(112, 34);
            BtnCancel.TabIndex = 6;
            BtnCancel.Text = "&Cancel";
            BtnCancel.UseVisualStyleBackColor = true;
            BtnCancel.Click += BtnCancel_Click;
            // 
            // BtnOK
            // 
            BtnOK.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            BtnOK.Location = new Point(271, 117);
            BtnOK.Name = "BtnOK";
            BtnOK.Size = new Size(112, 34);
            BtnOK.TabIndex = 5;
            BtnOK.Text = "&OK";
            BtnOK.UseVisualStyleBackColor = true;
            BtnOK.Click += BtnOK_Click;
            // 
            // EditValueForm
            // 
            AcceptButton = BtnOK;
            AutoScaleDimensions = new SizeF(10F, 25F);
            AutoScaleMode = AutoScaleMode.Font;
            CancelButton = BtnCancel;
            ClientSize = new Size(513, 163);
            Controls.Add(TxtValue);
            Controls.Add(LblValue);
            Controls.Add(BtnCancel);
            Controls.Add(BtnOK);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;
            MinimizeBox = false;
            Name = "EditValueForm";
            ShowIcon = false;
            ShowInTaskbar = false;
            SizeGripStyle = SizeGripStyle.Hide;
            Text = "Edit Value";
            ResumeLayout(false);
            PerformLayout();
        }

        #endregion

        private TextBox TxtValue;
        private Label LblValue;
        private Button BtnCancel;
        private Button BtnOK;
    }
}