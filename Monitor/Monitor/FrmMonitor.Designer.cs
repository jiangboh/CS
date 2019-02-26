namespace Monitor
{
    partial class FrmMonitor
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FrmMonitor));
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dataGridViewStatus = new System.Windows.Forms.DataGridView();
            this.name = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.status = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.panel2 = new System.Windows.Forms.Panel();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.labelErrRebootCnt = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1LastUpTime = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.label1RunTime = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.timerRunTime = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.panel1.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatus)).BeginInit();
            this.panel2.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "server监控器";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel1.Controls.Add(this.groupBox1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 94);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(444, 428);
            this.panel1.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.dataGridViewStatus);
            this.groupBox1.Location = new System.Drawing.Point(11, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(424, 409);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "线程状态";
            // 
            // dataGridViewStatus
            // 
            this.dataGridViewStatus.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            this.dataGridViewStatus.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            this.dataGridViewStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            this.dataGridViewStatus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.name,
            this.status});
            this.dataGridViewStatus.Location = new System.Drawing.Point(19, 30);
            this.dataGridViewStatus.Name = "dataGridViewStatus";
            this.dataGridViewStatus.ReadOnly = true;
            this.dataGridViewStatus.RowTemplate.Height = 23;
            this.dataGridViewStatus.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dataGridViewStatus.Size = new System.Drawing.Size(388, 363);
            this.dataGridViewStatus.TabIndex = 0;
            this.dataGridViewStatus.CellPainting += new System.Windows.Forms.DataGridViewCellPaintingEventHandler(this.dataGridViewStatus_CellPainting);
            this.dataGridViewStatus.RowPostPaint += new System.Windows.Forms.DataGridViewRowPostPaintEventHandler(this.dataGridViewStatus_RowPostPaint);
            // 
            // name
            // 
            dataGridViewCellStyle1.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.name.DefaultCellStyle = dataGridViewCellStyle1;
            this.name.FillWeight = 121.8274F;
            this.name.HeaderText = "名称";
            this.name.Name = "name";
            this.name.ReadOnly = true;
            this.name.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // status
            // 
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;
            dataGridViewCellStyle2.Font = new System.Drawing.Font("宋体", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.status.DefaultCellStyle = dataGridViewCellStyle2;
            this.status.FillWeight = 73.26402F;
            this.status.HeaderText = "状态";
            this.status.Name = "status";
            this.status.ReadOnly = true;
            this.status.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel2.Controls.Add(this.groupBox4);
            this.panel2.Controls.Add(this.groupBox3);
            this.panel2.Controls.Add(this.groupBox2);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 0);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(444, 94);
            this.panel2.TabIndex = 2;
            // 
            // groupBox4
            // 
            this.groupBox4.Controls.Add(this.labelErrRebootCnt);
            this.groupBox4.Location = new System.Drawing.Point(333, 15);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(100, 54);
            this.groupBox4.TabIndex = 0;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "错误数/重启数";
            // 
            // labelErrRebootCnt
            // 
            this.labelErrRebootCnt.AutoSize = true;
            this.labelErrRebootCnt.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.labelErrRebootCnt.Location = new System.Drawing.Point(32, 25);
            this.labelErrRebootCnt.Name = "labelErrRebootCnt";
            this.labelErrRebootCnt.Size = new System.Drawing.Size(38, 18);
            this.labelErrRebootCnt.TabIndex = 0;
            this.labelErrRebootCnt.Text = "3/9";
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label1LastUpTime);
            this.groupBox3.Location = new System.Drawing.Point(169, 15);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(148, 54);
            this.groupBox3.TabIndex = 0;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "最近更新/总次数";
            // 
            // label1LastUpTime
            // 
            this.label1LastUpTime.AutoSize = true;
            this.label1LastUpTime.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1LastUpTime.Location = new System.Drawing.Point(16, 25);
            this.label1LastUpTime.Name = "label1LastUpTime";
            this.label1LastUpTime.Size = new System.Drawing.Size(118, 18);
            this.label1LastUpTime.TabIndex = 0;
            this.label1LastUpTime.Text = "12:34/12345";
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.label1RunTime);
            this.groupBox2.Location = new System.Drawing.Point(13, 15);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(140, 54);
            this.groupBox2.TabIndex = 0;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "运行总时间";
            // 
            // label1RunTime
            // 
            this.label1RunTime.AutoSize = true;
            this.label1RunTime.Font = new System.Drawing.Font("Courier New", 12F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1RunTime.Location = new System.Drawing.Point(10, 25);
            this.label1RunTime.Name = "label1RunTime";
            this.label1RunTime.Size = new System.Drawing.Size(118, 18);
            this.label1RunTime.TabIndex = 0;
            this.label1RunTime.Text = "30 12:34:56";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ActiveCaption;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter1.Enabled = false;
            this.splitter1.Location = new System.Drawing.Point(0, 89);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(444, 5);
            this.splitter1.TabIndex = 3;
            this.splitter1.TabStop = false;
            // 
            // timerRunTime
            // 
            this.timerRunTime.Enabled = true;
            this.timerRunTime.Interval = 1000;
            this.timerRunTime.Tick += new System.EventHandler(this.timerRunTime_Tick);
            // 
            // FrmMonitor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(444, 522);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "FrmMonitor";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Scanner监控器";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.FrmMonitor_FormClosing);
            this.Load += new System.EventHandler(this.FrmMonitor_Load);
            this.SizeChanged += new System.EventHandler(this.FrmMonitor_SizeChanged);
            this.panel1.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridViewStatus)).EndInit();
            this.panel2.ResumeLayout(false);
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Label labelErrRebootCnt;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label1LastUpTime;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label1RunTime;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Timer timerRunTime;
        private System.Windows.Forms.DataGridView dataGridViewStatus;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.DataGridViewTextBoxColumn name;
        private System.Windows.Forms.DataGridViewTextBoxColumn status;
    }
}

