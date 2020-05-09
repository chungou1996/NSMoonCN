using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using DevComponents.DotNetBar;
using DevComponents.DotNetBar.Metro;

namespace NSMoonCN
{
    public partial class MainForm : MetroForm
    {
        public MainForm()
        {
            InitializeComponent();
        }

        private void btnUnpak_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("in");
            NSMoonPak.Pak.Unpack("SCR.PAK", "in");
        }

        private void btnPack_Click(object sender, EventArgs e)
        {
            Directory.CreateDirectory("out");
            NSMoonPak.Pak.Pack("in", "out/SCR.PAK");
        }
    }
}
