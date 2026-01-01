namespace TTNT_GUI
{
    partial class Form1
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
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.picGraph = new System.Windows.Forms.PictureBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.dgvMaTran = new System.Windows.Forms.DataGridView();
            this.btnLoad = new System.Windows.Forms.Button();
            this.btnGiai = new System.Windows.Forms.Button();
            this.btnXemLog = new System.Windows.Forms.Button();
            this.chkStep = new System.Windows.Forms.CheckBox();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picGraph)).BeginInit();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaTran)).BeginInit();
            this.SuspendLayout();
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Location = new System.Drawing.Point(1, 1);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(926, 501);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.picGraph);
            this.tabPage1.Location = new System.Drawing.Point(4, 25);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(918, 472);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Hình vẽ Đồ thị";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // picGraph
            // 
            this.picGraph.Location = new System.Drawing.Point(4, 6);
            this.picGraph.Name = "picGraph";
            this.picGraph.Size = new System.Drawing.Size(913, 465);
            this.picGraph.TabIndex = 0;
            this.picGraph.TabStop = false;
            this.picGraph.Paint += new System.Windows.Forms.PaintEventHandler(this.PicGraph_Paint);
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.dgvMaTran);
            this.tabPage2.Location = new System.Drawing.Point(4, 25);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(918, 472);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Ma trận Kề";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // dgvMaTran
            // 
            this.dgvMaTran.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvMaTran.Location = new System.Drawing.Point(4, 6);
            this.dgvMaTran.Name = "dgvMaTran";
            this.dgvMaTran.RowHeadersWidth = 51;
            this.dgvMaTran.RowTemplate.Height = 24;
            this.dgvMaTran.Size = new System.Drawing.Size(905, 460);
            this.dgvMaTran.TabIndex = 0;
            // 
            // btnLoad
            // 
            this.btnLoad.Location = new System.Drawing.Point(33, 509);
            this.btnLoad.Name = "btnLoad";
            this.btnLoad.Size = new System.Drawing.Size(102, 23);
            this.btnLoad.TabIndex = 2;
            this.btnLoad.Text = "Nhập File";
            this.btnLoad.UseVisualStyleBackColor = true;
            this.btnLoad.Click += new System.EventHandler(this.BtnLoad_Click);
            // 
            // btnGiai
            // 
            this.btnGiai.Location = new System.Drawing.Point(296, 508);
            this.btnGiai.Name = "btnGiai";
            this.btnGiai.Size = new System.Drawing.Size(97, 23);
            this.btnGiai.TabIndex = 3;
            this.btnGiai.Text = "Giải / Tô màu";
            this.btnGiai.UseVisualStyleBackColor = true;
            this.btnGiai.Click += new System.EventHandler(this.BtnGiai_Click);
            // 
            // btnXemLog
            // 
            this.btnXemLog.Location = new System.Drawing.Point(161, 509);
            this.btnXemLog.Name = "btnXemLog";
            this.btnXemLog.Size = new System.Drawing.Size(100, 23);
            this.btnXemLog.TabIndex = 4;
            this.btnXemLog.Text = "Xem Lời Giải";
            this.btnXemLog.UseVisualStyleBackColor = true;
            this.btnXemLog.Click += new System.EventHandler(this.BtnXemLog_Click);
            // 
            // chkStep
            // 
            this.chkStep.AutoSize = true;
            this.chkStep.Location = new System.Drawing.Point(437, 509);
            this.chkStep.Name = "chkStep";
            this.chkStep.Size = new System.Drawing.Size(121, 20);
            this.chkStep.TabIndex = 5;
            this.chkStep.Text = "Chạy từng bước";
            this.chkStep.UseVisualStyleBackColor = true;
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(926, 551);
            this.Controls.Add(this.chkStep);
            this.Controls.Add(this.btnXemLog);
            this.Controls.Add(this.btnGiai);
            this.Controls.Add(this.btnLoad);
            this.Controls.Add(this.tabControl1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.Name = "Form1";
            this.Text = "Form1";
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picGraph)).EndInit();
            this.tabPage2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvMaTran)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.Button btnLoad;
        private System.Windows.Forms.Button btnGiai;
        private System.Windows.Forms.Button btnXemLog;
        private System.Windows.Forms.CheckBox chkStep;
        private System.Windows.Forms.PictureBox picGraph;
        private System.Windows.Forms.DataGridView dgvMaTran;
    }
}

