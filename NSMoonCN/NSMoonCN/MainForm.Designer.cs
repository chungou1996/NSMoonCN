namespace NSMoonCN
{
    partial class MainForm
    {
        /// <summary>
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows 窗体设计器生成的代码

        /// <summary>
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
            this.lbDec = new DevComponents.DotNetBar.LabelX();
            this.btnUnpak = new DevComponents.DotNetBar.ButtonX();
            this.btnPack = new DevComponents.DotNetBar.ButtonX();
            this.SuspendLayout();
            // 
            // lbDec
            // 
            // 
            // 
            // 
            this.lbDec.BackgroundStyle.CornerType = DevComponents.DotNetBar.eCornerType.Square;
            this.lbDec.Location = new System.Drawing.Point(12, 2);
            this.lbDec.Name = "lbDec";
            this.lbDec.Size = new System.Drawing.Size(420, 105);
            this.lbDec.TabIndex = 0;
            this.lbDec.Text = "NSMoon CN\r\n基于【痴汉工贼 Crass】基础开发的NSMoon Pak，解包和封包工具，此软件仅供交流学习。\r\n\r\n大魔法师Kuku";
            this.lbDec.WordWrap = true;
            // 
            // btnUnpak
            // 
            this.btnUnpak.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnUnpak.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnUnpak.Location = new System.Drawing.Point(12, 139);
            this.btnUnpak.Name = "btnUnpak";
            this.btnUnpak.Size = new System.Drawing.Size(75, 23);
            this.btnUnpak.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnUnpak.TabIndex = 1;
            this.btnUnpak.Text = "解包";
            this.btnUnpak.Click += new System.EventHandler(this.btnUnpak_Click);
            // 
            // btnPack
            // 
            this.btnPack.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
            this.btnPack.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
            this.btnPack.Location = new System.Drawing.Point(357, 139);
            this.btnPack.Name = "btnPack";
            this.btnPack.Size = new System.Drawing.Size(75, 23);
            this.btnPack.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
            this.btnPack.TabIndex = 2;
            this.btnPack.Text = "封包";
            this.btnPack.Click += new System.EventHandler(this.btnPack_Click);
            // 
            // MainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 174);
            this.Controls.Add(this.btnPack);
            this.Controls.Add(this.btnUnpak);
            this.Controls.Add(this.lbDec);
            this.DoubleBuffered = true;
            this.ForeColor = System.Drawing.Color.Black;
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "MainForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "NSMoon CN";
            this.ResumeLayout(false);

        }

        #endregion

        private DevComponents.DotNetBar.LabelX lbDec;
        private DevComponents.DotNetBar.ButtonX btnUnpak;
        private DevComponents.DotNetBar.ButtonX btnPack;
    }
}

