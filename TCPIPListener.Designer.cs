namespace TCPIPListener {
    partial class TCPIPListener {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if (disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.Start = new System.Windows.Forms.Button();
            this.Stop = new System.Windows.Forms.Button();
            this.dbConnLabel = new System.Windows.Forms.Label();
            this.cmdConnLabel = new System.Windows.Forms.Label();
            this.gwConnLabel = new System.Windows.Forms.Label();
            this.garbageCollect = new System.Windows.Forms.Button();
            this.cmdStats = new System.Windows.Forms.ListBox();
            this.gwStats = new System.Windows.Forms.ListBox();
            this.dbStatsLabel = new System.Windows.Forms.Label();
            this.cmdStatsLabel = new System.Windows.Forms.Label();
            this.gwStatusLabel = new System.Windows.Forms.Label();
            this.dBStats = new System.Windows.Forms.ListBox();
            this.systemTimeLabel = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.dbListBox = new System.Windows.Forms.ListBox();
            this.cmdListBox = new System.Windows.Forms.ListBox();
            this.gwListBox = new System.Windows.Forms.ListBox();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // Start
            // 
            this.Start.ForeColor = System.Drawing.Color.Black;
            this.Start.Location = new System.Drawing.Point(3, 16);
            this.Start.Name = "Start";
            this.Start.Size = new System.Drawing.Size(115, 27);
            this.Start.TabIndex = 0;
            this.Start.Text = "Start";
            this.Start.UseVisualStyleBackColor = true;
            this.Start.Click += new System.EventHandler(this.start_Click);
            // 
            // Stop
            // 
            this.Stop.Location = new System.Drawing.Point(347, 16);
            this.Stop.Name = "Stop";
            this.Stop.Size = new System.Drawing.Size(122, 27);
            this.Stop.TabIndex = 1;
            this.Stop.Text = "Stop";
            this.Stop.UseVisualStyleBackColor = true;
            this.Stop.Click += new System.EventHandler(this.stop_Click);
            // 
            // dbConnLabel
            // 
            this.dbConnLabel.AutoSize = true;
            this.dbConnLabel.Location = new System.Drawing.Point(3, 46);
            this.dbConnLabel.Name = "dbConnLabel";
            this.dbConnLabel.Size = new System.Drawing.Size(202, 13);
            this.dbConnLabel.TabIndex = 6;
            this.dbConnLabel.Text = "Database Connection LISTENER PORT:";
            // 
            // cmdConnLabel
            // 
            this.cmdConnLabel.AutoSize = true;
            this.cmdConnLabel.Location = new System.Drawing.Point(347, 46);
            this.cmdConnLabel.Name = "cmdConnLabel";
            this.cmdConnLabel.Size = new System.Drawing.Size(219, 13);
            this.cmdConnLabel.TabIndex = 7;
            this.cmdConnLabel.Text = "Commandline Connection LISTENER PORT:";
            // 
            // gwConnLabel
            // 
            this.gwConnLabel.AutoSize = true;
            this.gwConnLabel.Location = new System.Drawing.Point(691, 46);
            this.gwConnLabel.Name = "gwConnLabel";
            this.gwConnLabel.Size = new System.Drawing.Size(198, 13);
            this.gwConnLabel.TabIndex = 9;
            this.gwConnLabel.Text = "Gateway Connection LISTENER PORT:";
            // 
            // garbageCollect
            // 
            this.garbageCollect.Location = new System.Drawing.Point(691, 16);
            this.garbageCollect.Name = "garbageCollect";
            this.garbageCollect.Size = new System.Drawing.Size(111, 27);
            this.garbageCollect.TabIndex = 10;
            this.garbageCollect.Text = "Garbage Collect";
            this.garbageCollect.UseVisualStyleBackColor = true;
            this.garbageCollect.Click += new System.EventHandler(this.garbageCollect_Click);
            // 
            // cmdStats
            // 
            this.cmdStats.BackColor = System.Drawing.SystemColors.ControlLight;
            this.cmdStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdStats.ForeColor = System.Drawing.Color.DarkBlue;
            this.cmdStats.FormattingEnabled = true;
            this.cmdStats.Location = new System.Drawing.Point(347, 392);
            this.cmdStats.Name = "cmdStats";
            this.cmdStats.Size = new System.Drawing.Size(338, 131);
            this.cmdStats.TabIndex = 12;
            // 
            // gwStats
            // 
            this.gwStats.BackColor = System.Drawing.SystemColors.ControlLight;
            this.gwStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwStats.ForeColor = System.Drawing.Color.DarkBlue;
            this.gwStats.FormattingEnabled = true;
            this.gwStats.Location = new System.Drawing.Point(691, 392);
            this.gwStats.Name = "gwStats";
            this.gwStats.Size = new System.Drawing.Size(340, 131);
            this.gwStats.TabIndex = 13;
            // 
            // dbStatsLabel
            // 
            this.dbStatsLabel.AutoSize = true;
            this.dbStatsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbStatsLabel.Location = new System.Drawing.Point(3, 376);
            this.dbStatsLabel.Name = "dbStatsLabel";
            this.dbStatsLabel.Size = new System.Drawing.Size(338, 13);
            this.dbStatsLabel.TabIndex = 14;
            this.dbStatsLabel.Text = "Database Listener Statistics";
            // 
            // cmdStatsLabel
            // 
            this.cmdStatsLabel.AutoSize = true;
            this.cmdStatsLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdStatsLabel.Location = new System.Drawing.Point(347, 376);
            this.cmdStatsLabel.Name = "cmdStatsLabel";
            this.cmdStatsLabel.Size = new System.Drawing.Size(338, 13);
            this.cmdStatsLabel.TabIndex = 15;
            this.cmdStatsLabel.Text = "Command Listener Statistics";
            // 
            // gwStatusLabel
            // 
            this.gwStatusLabel.AutoSize = true;
            this.gwStatusLabel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwStatusLabel.Location = new System.Drawing.Point(691, 376);
            this.gwStatusLabel.Name = "gwStatusLabel";
            this.gwStatusLabel.Size = new System.Drawing.Size(340, 13);
            this.gwStatusLabel.TabIndex = 16;
            this.gwStatusLabel.Text = "Gateway Listener Statistics";
            // 
            // dBStats
            // 
            this.dBStats.BackColor = System.Drawing.SystemColors.ControlLight;
            this.dBStats.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dBStats.ForeColor = System.Drawing.Color.DarkBlue;
            this.dBStats.FormattingEnabled = true;
            this.dBStats.Location = new System.Drawing.Point(3, 392);
            this.dBStats.Name = "dBStats";
            this.dBStats.Size = new System.Drawing.Size(338, 131);
            this.dBStats.TabIndex = 17;
            // 
            // systemTimeLabel
            // 
            this.systemTimeLabel.AutoSize = true;
            this.systemTimeLabel.Location = new System.Drawing.Point(3, 0);
            this.systemTimeLabel.Name = "systemTimeLabel";
            this.systemTimeLabel.Size = new System.Drawing.Size(67, 13);
            this.systemTimeLabel.TabIndex = 18;
            this.systemTimeLabel.Text = "System Time";
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 3;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33333F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 33.33334F));
            this.tableLayoutPanel1.Controls.Add(this.Start, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.gwConnLabel, 2, 2);
            this.tableLayoutPanel1.Controls.Add(this.gwStatusLabel, 2, 4);
            this.tableLayoutPanel1.Controls.Add(this.cmdConnLabel, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.gwStats, 2, 5);
            this.tableLayoutPanel1.Controls.Add(this.cmdStatsLabel, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.systemTimeLabel, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.Stop, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.cmdStats, 1, 5);
            this.tableLayoutPanel1.Controls.Add(this.garbageCollect, 2, 1);
            this.tableLayoutPanel1.Controls.Add(this.dbStatsLabel, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.dBStats, 0, 5);
            this.tableLayoutPanel1.Controls.Add(this.dbConnLabel, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.dbListBox, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.cmdListBox, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.gwListBox, 2, 3);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 6;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 70F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1034, 526);
            this.tableLayoutPanel1.TabIndex = 19;
            this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
            // 
            // dbListBox
            // 
            this.dbListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.dbListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dbListBox.FormattingEnabled = true;
            this.dbListBox.HorizontalScrollbar = true;
            this.dbListBox.Location = new System.Drawing.Point(3, 62);
            this.dbListBox.Name = "dbListBox";
            this.dbListBox.Size = new System.Drawing.Size(338, 311);
            this.dbListBox.TabIndex = 25;
            // 
            // cmdListBox
            // 
            this.cmdListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.cmdListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.cmdListBox.FormattingEnabled = true;
            this.cmdListBox.HorizontalScrollbar = true;
            this.cmdListBox.Location = new System.Drawing.Point(347, 62);
            this.cmdListBox.Name = "cmdListBox";
            this.cmdListBox.Size = new System.Drawing.Size(338, 311);
            this.cmdListBox.TabIndex = 26;
            // 
            // gwListBox
            // 
            this.gwListBox.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(255)))), ((int)(((byte)(255)))), ((int)(((byte)(192)))));
            this.gwListBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gwListBox.FormattingEnabled = true;
            this.gwListBox.HorizontalScrollbar = true;
            this.gwListBox.Location = new System.Drawing.Point(691, 62);
            this.gwListBox.Name = "gwListBox";
            this.gwListBox.Size = new System.Drawing.Size(340, 311);
            this.gwListBox.TabIndex = 27;
            // 
            // TCPIPListener
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1034, 526);
            this.Controls.Add(this.tableLayoutPanel1);
            this.MinimumSize = new System.Drawing.Size(1050, 560);
            this.Name = "TCPIPListener";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "TCPIP Listener";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.TCPIPListener_FormClosing);
            this.Load += new System.EventHandler(this.TCPIPListener_Load);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button Start;
        private System.Windows.Forms.Button Stop;
        private System.Windows.Forms.Label dbConnLabel;
        private System.Windows.Forms.Label cmdConnLabel;
        private System.Windows.Forms.Label gwConnLabel;
        private System.Windows.Forms.Button garbageCollect;
        private System.Windows.Forms.ListBox cmdStats;
        private System.Windows.Forms.ListBox gwStats;
        private System.Windows.Forms.Label dbStatsLabel;
        private System.Windows.Forms.Label cmdStatsLabel;
        private System.Windows.Forms.Label gwStatusLabel;
        private System.Windows.Forms.ListBox dBStats;
        private System.Windows.Forms.Label systemTimeLabel;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.ListBox dbListBox;
        private System.Windows.Forms.ListBox cmdListBox;
        private System.Windows.Forms.ListBox gwListBox;
    }
}

