
namespace UpLoadToSFTP.CSPager
{
    partial class MyPager
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

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要修改
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            this.labRecordCount = new System.Windows.Forms.Label();
            this.labPageCount = new System.Windows.Forms.Label();
            this.txtPageSize = new System.Windows.Forms.TextBox();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.panel1 = new System.Windows.Forms.Panel();
            this.btnNext = new System.Windows.Forms.Button();
            this.btnFirst = new System.Windows.Forms.Button();
            this.btnPrevious = new System.Windows.Forms.Button();
            this.txtPageIndex = new System.Windows.Forms.TextBox();
            this.btnLast = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // labRecordCount
            // 
            this.labRecordCount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labRecordCount.AutoSize = true;
            this.labRecordCount.Cursor = System.Windows.Forms.Cursors.Default;
            this.labRecordCount.Location = new System.Drawing.Point(3, 6);
            this.labRecordCount.Name = "labRecordCount";
            this.labRecordCount.Size = new System.Drawing.Size(161, 12);
            this.labRecordCount.TabIndex = 0;
            this.labRecordCount.Text = "Total Record:{0},Each Page";
            this.labRecordCount.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // labPageCount
            // 
            this.labPageCount.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.labPageCount.AutoSize = true;
            this.labPageCount.Cursor = System.Windows.Forms.Cursors.Default;
            this.labPageCount.Location = new System.Drawing.Point(206, 6);
            this.labPageCount.Name = "labPageCount";
            this.labPageCount.Size = new System.Drawing.Size(83, 12);
            this.labPageCount.TabIndex = 10;
            this.labPageCount.Text = "PageCount:{0}";
            this.labPageCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPageSize
            // 
            this.txtPageSize.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtPageSize.Location = new System.Drawing.Point(167, 1);
            this.txtPageSize.Margin = new System.Windows.Forms.Padding(0);
            this.txtPageSize.Name = "txtPageSize";
            this.txtPageSize.Size = new System.Drawing.Size(36, 21);
            this.txtPageSize.TabIndex = 13;
            this.txtPageSize.TextChanged += new System.EventHandler(this.txtPageSize_TextChanged);
            this.txtPageSize.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPageSize_KeyDown);
            this.txtPageSize.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPageSize_KeyPress);
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.AutoSize = true;
            this.tableLayoutPanel1.ColumnCount = 4;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel1.Controls.Add(this.panel1, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.txtPageSize, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.labRecordCount, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.labPageCount, 2, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 0);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(561, 24);
            this.tableLayoutPanel1.TabIndex = 14;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.btnNext);
            this.panel1.Controls.Add(this.btnFirst);
            this.panel1.Controls.Add(this.btnPrevious);
            this.panel1.Controls.Add(this.txtPageIndex);
            this.panel1.Controls.Add(this.btnLast);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(292, 0);
            this.panel1.Margin = new System.Windows.Forms.Padding(0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(269, 24);
            this.panel1.TabIndex = 14;
            // 
            // btnNext
            // 
            this.btnNext.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnNext.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(242)))));
            this.btnNext.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnNext.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnNext.Location = new System.Drawing.Point(151, 1);
            this.btnNext.Margin = new System.Windows.Forms.Padding(0);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(30, 20);
            this.btnNext.TabIndex = 6;
            this.btnNext.Text = ">";
            this.btnNext.UseVisualStyleBackColor = false;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // btnFirst
            // 
            this.btnFirst.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnFirst.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(242)))));
            this.btnFirst.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnFirst.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnFirst.Location = new System.Drawing.Point(31, 2);
            this.btnFirst.Margin = new System.Windows.Forms.Padding(0);
            this.btnFirst.Name = "btnFirst";
            this.btnFirst.Size = new System.Drawing.Size(30, 20);
            this.btnFirst.TabIndex = 7;
            this.btnFirst.Text = "|<";
            this.btnFirst.UseVisualStyleBackColor = false;
            this.btnFirst.Click += new System.EventHandler(this.btnFirst_Click);
            // 
            // btnPrevious
            // 
            this.btnPrevious.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnPrevious.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(242)))));
            this.btnPrevious.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnPrevious.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnPrevious.Location = new System.Drawing.Point(68, 2);
            this.btnPrevious.Margin = new System.Windows.Forms.Padding(0);
            this.btnPrevious.Name = "btnPrevious";
            this.btnPrevious.Size = new System.Drawing.Size(30, 20);
            this.btnPrevious.TabIndex = 8;
            this.btnPrevious.Text = "<";
            this.btnPrevious.UseVisualStyleBackColor = false;
            this.btnPrevious.Click += new System.EventHandler(this.btnPrevious_Click);
            // 
            // txtPageIndex
            // 
            this.txtPageIndex.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.txtPageIndex.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.txtPageIndex.Location = new System.Drawing.Point(105, 1);
            this.txtPageIndex.Margin = new System.Windows.Forms.Padding(0);
            this.txtPageIndex.Name = "txtPageIndex";
            this.txtPageIndex.Size = new System.Drawing.Size(42, 21);
            this.txtPageIndex.TabIndex = 5;
            this.txtPageIndex.Text = "1";
            this.txtPageIndex.KeyDown += new System.Windows.Forms.KeyEventHandler(this.txtPageIndex_KeyDown);
            this.txtPageIndex.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtPageIndex_KeyPress);
            // 
            // btnLast
            // 
            this.btnLast.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnLast.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(214)))), ((int)(((byte)(228)))), ((int)(((byte)(242)))));
            this.btnLast.Cursor = System.Windows.Forms.Cursors.Default;
            this.btnLast.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnLast.Location = new System.Drawing.Point(188, 1);
            this.btnLast.Margin = new System.Windows.Forms.Padding(0);
            this.btnLast.Name = "btnLast";
            this.btnLast.Size = new System.Drawing.Size(30, 20);
            this.btnLast.TabIndex = 9;
            this.btnLast.Text = ">|";
            this.btnLast.UseVisualStyleBackColor = false;
            this.btnLast.Click += new System.EventHandler(this.btnLast_Click);
            // 
            // MyPager
            // 
            this.AutoSize = true;
            this.Controls.Add(this.tableLayoutPanel1);
            this.Cursor = System.Windows.Forms.Cursors.Default;
            this.Margin = new System.Windows.Forms.Padding(0);
            this.Name = "MyPager";
            this.Size = new System.Drawing.Size(561, 24);
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
    }
}
