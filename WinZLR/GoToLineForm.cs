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

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winZLR
{
    public partial class GoToLineForm : Form
    {
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string LabelText
        {
            get => LblLine.Text;
            set => LblLine.Text = value;
        }

        public int GoToLine = 0;
        public GoToLineForm()
        {
            InitializeComponent();
        }

        private void TxtLine_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsControl(e.KeyChar) && !char.IsDigit(e.KeyChar))
            {
                e.Handled = true;
            }

        }

        private void BtnOK_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
 
            try
            {
               GoToLine = Convert.ToInt32(TxtLine.Text);
            }
            catch 
            {
                GoToLine = 0;
            }

            Close();
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult= DialogResult.Cancel;
            Close();
        }

    }
}
