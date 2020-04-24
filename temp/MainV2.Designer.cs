namespace aeromagtec
{
    partial class MainV2
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainV2));
            this.CTX_mainmenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.滤波器设计 = new System.Windows.Forms.ToolStripMenuItem();
            this.fullScreenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.readonlyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionOptionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.connectionListToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.UartDatatimer = new System.Windows.Forms.Timer(this.components);
            this.timer_send = new System.Windows.Forms.Timer(this.components);
            this.updateDateTimer = new System.Windows.Forms.Timer(this.components);
            this.bindingSourcerawdata = new System.Windows.Forms.BindingSource(this.components);
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.cmb_Connection = new System.Windows.Forms.ComboBox();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.toolStripStatusLabel1 = new System.Windows.Forms.ToolStripStatusLabel();
            this.MainMenu = new System.Windows.Forms.MenuStrip();
            this.MenuFlightData = new System.Windows.Forms.ToolStripButton();
            this.MenuFlightPlanner = new System.Windows.Forms.ToolStripButton();
            this.MenuInitConfig = new System.Windows.Forms.ToolStripButton();
            this.MenuConfigTune = new System.Windows.Forms.ToolStripButton();
            this.MenuSimulation = new System.Windows.Forms.ToolStripButton();
            this.MenuTerminal = new System.Windows.Forms.ToolStripButton();
            this.MenuHelp = new System.Windows.Forms.ToolStripButton();
            this.MenuConnect = new System.Windows.Forms.ToolStripButton();
            this.CTX_mainmenu.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcerawdata)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.MainMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // CTX_mainmenu
            // 
            this.CTX_mainmenu.ImageScalingSize = new System.Drawing.Size(20, 20);
            this.CTX_mainmenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.滤波器设计,
            this.fullScreenToolStripMenuItem,
            this.readonlyToolStripMenuItem,
            this.connectionOptionsToolStripMenuItem,
            this.connectionListToolStripMenuItem});
            this.CTX_mainmenu.Name = "CTX_mainmenu";
            this.CTX_mainmenu.Size = new System.Drawing.Size(192, 114);
            // 
            // 滤波器设计
            // 
            this.滤波器设计.CheckOnClick = true;
            this.滤波器设计.Name = "滤波器设计";
            this.滤波器设计.Size = new System.Drawing.Size(191, 22);
            this.滤波器设计.Text = "AutoHide";
            // 
            // fullScreenToolStripMenuItem
            // 
            this.fullScreenToolStripMenuItem.CheckOnClick = true;
            this.fullScreenToolStripMenuItem.Name = "fullScreenToolStripMenuItem";
            this.fullScreenToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.fullScreenToolStripMenuItem.Text = "Full Screen";
            // 
            // readonlyToolStripMenuItem
            // 
            this.readonlyToolStripMenuItem.CheckOnClick = true;
            this.readonlyToolStripMenuItem.Name = "readonlyToolStripMenuItem";
            this.readonlyToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.readonlyToolStripMenuItem.Text = "Readonly";
            // 
            // connectionOptionsToolStripMenuItem
            // 
            this.connectionOptionsToolStripMenuItem.Name = "connectionOptionsToolStripMenuItem";
            this.connectionOptionsToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.connectionOptionsToolStripMenuItem.Text = "Connection Options";
            // 
            // connectionListToolStripMenuItem
            // 
            this.connectionListToolStripMenuItem.Name = "connectionListToolStripMenuItem";
            this.connectionListToolStripMenuItem.Size = new System.Drawing.Size(191, 22);
            this.connectionListToolStripMenuItem.Text = "Connection List";
            // 
            // timer_send
            // 
            this.timer_send.Enabled = true;
            // 
            // updateDateTimer
            // 
            this.updateDateTimer.Interval = 800;
            this.updateDateTimer.Tick += new System.EventHandler(this.updateDateTimer_Tick);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(0, 0);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 537);
            this.splitter1.TabIndex = 5;
            this.splitter1.TabStop = false;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.splitContainer1.Location = new System.Drawing.Point(3, 71);
            this.splitContainer1.Margin = new System.Windows.Forms.Padding(1);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.cmb_Connection);
            this.splitContainer1.Panel1.Resize += new System.EventHandler(this.splitContainer1_Panel1_Resize);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer1.Size = new System.Drawing.Size(1197, 466);
            this.splitContainer1.SplitterDistance = 437;
            this.splitContainer1.SplitterWidth = 1;
            this.splitContainer1.TabIndex = 1;
            // 
            // cmb_Connection
            // 
            this.cmb_Connection.BackColor = System.Drawing.Color.Black;
            this.cmb_Connection.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
            this.cmb_Connection.DropDownWidth = 200;
            this.cmb_Connection.ForeColor = System.Drawing.Color.White;
            this.cmb_Connection.FormattingEnabled = true;
            this.cmb_Connection.Location = new System.Drawing.Point(660, -22);
            this.cmb_Connection.Name = "cmb_Connection";
            this.cmb_Connection.Size = new System.Drawing.Size(121, 22);
            this.cmb_Connection.TabIndex = 7;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripStatusLabel1});
            this.statusStrip1.Location = new System.Drawing.Point(0, 6);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1197, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "state show";
            // 
            // toolStripStatusLabel1
            // 
            this.toolStripStatusLabel1.Name = "toolStripStatusLabel1";
            this.toolStripStatusLabel1.Size = new System.Drawing.Size(131, 17);
            this.toolStripStatusLabel1.Text = "toolStripStatusLabel1";
            // 
            // MainMenu
            // 
            this.MainMenu.BackgroundImage = ((System.Drawing.Image)(resources.GetObject("MainMenu.BackgroundImage")));
            this.MainMenu.GripMargin = new System.Windows.Forms.Padding(0);
            this.MainMenu.GripStyle = System.Windows.Forms.ToolStripGripStyle.Visible;
            this.MainMenu.ImageScalingSize = new System.Drawing.Size(0, 0);
            this.MainMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.MenuFlightData,
            this.MenuFlightPlanner,
            this.MenuInitConfig,
            this.MenuConfigTune,
            this.MenuSimulation,
            this.MenuConnect,
            this.MenuTerminal,
            this.MenuHelp});
            this.MainMenu.Location = new System.Drawing.Point(3, 0);
            this.MainMenu.Margin = new System.Windows.Forms.Padding(1);
            this.MainMenu.Name = "MainMenu";
            this.MainMenu.Padding = new System.Windows.Forms.Padding(0);
            this.MainMenu.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
            this.MainMenu.ShowItemToolTips = true;
            this.MainMenu.Size = new System.Drawing.Size(1197, 69);
            this.MainMenu.TabIndex = 2;
            this.MainMenu.Text = "menuStrip1";
            // 
            // MenuFlightData
            // 
            this.MenuFlightData.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuFlightData.ForeColor = System.Drawing.Color.White;
            this.MenuFlightData.Image = ((System.Drawing.Image)(resources.GetObject("MenuFlightData.Image")));
            this.MenuFlightData.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuFlightData.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuFlightData.Margin = new System.Windows.Forms.Padding(0);
            this.MenuFlightData.Name = "MenuFlightData";
            this.MenuFlightData.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MenuFlightData.Size = new System.Drawing.Size(53, 69);
            this.MenuFlightData.Text = "飞行数据";
            this.MenuFlightData.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuFlightData.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuFlightData.ToolTipText = "飞行数据";
            this.MenuFlightData.Click += new System.EventHandler(this.MenuFlightData_Click);
            // 
            // MenuFlightPlanner
            // 
            this.MenuFlightPlanner.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuFlightPlanner.ForeColor = System.Drawing.Color.White;
            this.MenuFlightPlanner.Image = ((System.Drawing.Image)(resources.GetObject("MenuFlightPlanner.Image")));
            this.MenuFlightPlanner.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuFlightPlanner.ImageTransparentColor = System.Drawing.Color.White;
            this.MenuFlightPlanner.Margin = new System.Windows.Forms.Padding(0);
            this.MenuFlightPlanner.Name = "MenuFlightPlanner";
            this.MenuFlightPlanner.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MenuFlightPlanner.Size = new System.Drawing.Size(53, 69);
            this.MenuFlightPlanner.Text = "飞机计划";
            this.MenuFlightPlanner.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuFlightPlanner.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuFlightPlanner.ToolTipText = "飞行计划";
            // 
            // MenuInitConfig
            // 
            this.MenuInitConfig.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuInitConfig.ForeColor = System.Drawing.Color.White;
            this.MenuInitConfig.Image = ((System.Drawing.Image)(resources.GetObject("MenuInitConfig.Image")));
            this.MenuInitConfig.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuInitConfig.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuInitConfig.Margin = new System.Windows.Forms.Padding(0);
            this.MenuInitConfig.Name = "MenuInitConfig";
            this.MenuInitConfig.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MenuInitConfig.Size = new System.Drawing.Size(56, 69);
            this.MenuInitConfig.Text = "设置";
            this.MenuInitConfig.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuInitConfig.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuInitConfig.ToolTipText = "工具";
            this.MenuInitConfig.Visible = false;
            // 
            // MenuConfigTune
            // 
            this.MenuConfigTune.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuConfigTune.ForeColor = System.Drawing.Color.White;
            this.MenuConfigTune.Image = ((System.Drawing.Image)(resources.GetObject("MenuConfigTune.Image")));
            this.MenuConfigTune.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuConfigTune.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuConfigTune.Margin = new System.Windows.Forms.Padding(0);
            this.MenuConfigTune.Name = "MenuConfigTune";
            this.MenuConfigTune.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MenuConfigTune.Size = new System.Drawing.Size(56, 69);
            this.MenuConfigTune.Text = "工具";
            this.MenuConfigTune.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuConfigTune.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuConfigTune.ToolTipText = "设置";
            // 
            // MenuSimulation
            // 
            this.MenuSimulation.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuSimulation.ForeColor = System.Drawing.Color.White;
            this.MenuSimulation.Image = ((System.Drawing.Image)(resources.GetObject("MenuSimulation.Image")));
            this.MenuSimulation.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuSimulation.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuSimulation.Margin = new System.Windows.Forms.Padding(0);
            this.MenuSimulation.Name = "MenuSimulation";
            this.MenuSimulation.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MenuSimulation.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.MenuSimulation.RightToLeftAutoMirrorImage = true;
            this.MenuSimulation.Size = new System.Drawing.Size(68, 69);
            this.MenuSimulation.Text = "模拟";
            this.MenuSimulation.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuSimulation.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuSimulation.ToolTipText = "模拟";
            this.MenuSimulation.Visible = false;
            // 
            // MenuTerminal
            // 
            this.MenuTerminal.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuTerminal.ForeColor = System.Drawing.Color.White;
            this.MenuTerminal.Image = ((System.Drawing.Image)(resources.GetObject("MenuTerminal.Image")));
            this.MenuTerminal.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuTerminal.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuTerminal.Margin = new System.Windows.Forms.Padding(0);
            this.MenuTerminal.Name = "MenuTerminal";
            this.MenuTerminal.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MenuTerminal.Size = new System.Drawing.Size(57, 69);
            this.MenuTerminal.Text = "数据分析";
            this.MenuTerminal.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuTerminal.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuTerminal.ToolTipText = "数据分析";
            // 
            // MenuHelp
            // 
            this.MenuHelp.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuHelp.ForeColor = System.Drawing.Color.White;
            this.MenuHelp.Image = ((System.Drawing.Image)(resources.GetObject("MenuHelp.Image")));
            this.MenuHelp.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuHelp.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuHelp.Margin = new System.Windows.Forms.Padding(0);
            this.MenuHelp.Name = "MenuHelp";
            this.MenuHelp.Padding = new System.Windows.Forms.Padding(0, 0, 0, 10);
            this.MenuHelp.Size = new System.Drawing.Size(58, 69);
            this.MenuHelp.Text = "帮助";
            this.MenuHelp.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuHelp.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuHelp.ToolTipText = "帮助";
            // 
            // MenuConnect
            // 
            this.MenuConnect.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.MenuConnect.BackColor = System.Drawing.Color.Transparent;
            this.MenuConnect.Font = new System.Drawing.Font("Arial", 6F);
            this.MenuConnect.ForeColor = System.Drawing.Color.White;
            this.MenuConnect.Image = ((System.Drawing.Image)(resources.GetObject("MenuConnect.Image")));
            this.MenuConnect.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.MenuConnect.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.MenuConnect.Margin = new System.Windows.Forms.Padding(1);
            this.MenuConnect.Name = "MenuConnect";
            this.MenuConnect.Padding = new System.Windows.Forms.Padding(1);
            this.MenuConnect.Size = new System.Drawing.Size(58, 67);
            this.MenuConnect.Text = "连接";
            this.MenuConnect.TextAlign = System.Drawing.ContentAlignment.BottomCenter;
            this.MenuConnect.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageAboveText;
            this.MenuConnect.ToolTipText = "connect";
            this.MenuConnect.Click += new System.EventHandler(this.MenuConnect_Click);
            // 
            // MainV2
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.ClientSize = new System.Drawing.Size(1200, 537);
            this.Controls.Add(this.MainMenu);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.splitContainer1);
            this.IsMdiContainer = true;
            this.Name = "MainV2";
            this.Text = "Aeromagtec";
            this.Resize += new System.EventHandler(this.MainV2_Resize);
            this.CTX_mainmenu.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.bindingSourcerawdata)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.MainMenu.ResumeLayout(false);
            this.MainMenu.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ContextMenuStrip CTX_mainmenu;
        private System.Windows.Forms.ToolStripMenuItem 滤波器设计;
        private System.Windows.Forms.ToolStripMenuItem fullScreenToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem readonlyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionOptionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem connectionListToolStripMenuItem;
        private System.Windows.Forms.Timer UartDatatimer;
        private System.Windows.Forms.Timer timer_send;
        private System.Windows.Forms.Timer updateDateTimer;
        private System.Windows.Forms.BindingSource bindingSourcerawdata;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ComboBox cmb_Connection;
        private System.Windows.Forms.ToolStripStatusLabel toolStripStatusLabel1;
        public System.Windows.Forms.MenuStrip MainMenu;
        public System.Windows.Forms.ToolStripButton MenuFlightData;
        public System.Windows.Forms.ToolStripButton MenuFlightPlanner;
        public System.Windows.Forms.ToolStripButton MenuInitConfig;
        public System.Windows.Forms.ToolStripButton MenuConfigTune;
        public System.Windows.Forms.ToolStripButton MenuSimulation;
        public System.Windows.Forms.ToolStripButton MenuTerminal;
        private System.Windows.Forms.ToolStripButton MenuHelp;
        public System.Windows.Forms.ToolStripButton MenuConnect;
    }
}

