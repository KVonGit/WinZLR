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
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace winZLR
{
    public partial class SearchForm : Form
    {
        public delegate void SearchNext(string searchFor, bool matchCase, bool searchInAllFiles);
        public delegate void SearchPrevious(string searchFor, bool matchCase, bool searchInAllFiles);

        public SearchNext? findNext;
        public SearchPrevious? findPrevious;

        public bool searchFiles = true;
        public string SearchText = "";
        public SearchForm()
        {
            InitializeComponent();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnNext_Click(object sender, EventArgs e)
        {
            findNext?.Invoke(TxtSearch.Text, ChkMatchCase.Checked, ChkSearchAllFiles.Checked);
        }

        private void BtnPrevious_Click(object sender, EventArgs e)
        {
            findPrevious?.Invoke(TxtSearch.Text, ChkMatchCase.Checked, ChkSearchAllFiles.Checked);
        }

        private void SearchForm_Load(object sender, EventArgs e)
        {
            if (searchFiles)
            {
                ChkSearchAllFiles.Visible = true;
                LblSearch.Text = "Find in files:";
            }
            else
            {
                ChkSearchAllFiles.Visible = false;
                LblSearch.Text = "Find in disassembly:";
            }
            TxtSearch.Text = SearchText;

        }
    }
}
