using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UpLoadToSFTP.CSPager
{
    public delegate void PageChangedEventHandler(object sender, EventArgs e);
    public delegate void SearchAllEventHandler(object sender, EventArgs e);
    public partial class MyPager : UserControl
    {
        public event PageChangedEventHandler PageChanged;
        public event SearchAllEventHandler SearchAll;
        private int m_PageSize;
        private int m_PageCount;
        private int m_RecordCount;
        private int m_PageIndex;
        private bool m_PageSearchAllable = false;

        private Label labRecordCount;
        private Label labPageCount;
        private TextBox txtPageSize;
        private TableLayoutPanel tableLayoutPanel1;
        private Panel panel1;
        private Button btnNext;
        private Button btnFirst;
        private Button btnPrevious;
        private TextBox txtPageIndex;
        private Button btnLast;
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        public MyPager()
        {
            InitializeComponent();
            this.m_PageSize = 30;
            this.m_RecordCount = 0;
            this.m_PageIndex = 1; //默认为第一页
        }
        /// <summary> 
        /// 带参数的构造函数
        /// <param name="pageSize">每页记录数</param>
        /// <param name="recordCount">总记录数</param>
        /// </summary>
        public MyPager(int recordCount, int pageSize)
        {
            InitializeComponent();

            this.m_PageSize = pageSize;
            this.m_RecordCount = recordCount;
            this.m_PageIndex = 1; //默认为第一页
            this.InitPageInfo();
        }
        protected virtual void OnPageChanged(EventArgs e)
        {
            if (PageChanged != null)
            {
                InitPageInfo();
                PageChanged(this, e);
            }
        }


        protected virtual void OnSearchAll(EventArgs e)
        {
            if (SearchAll != null)
            {
                SearchAll(this, e);
            }
        }


        [Description("设置或获取一页中显示的记录数目"), DefaultValue(10), Category("分页")]
        public int PageSize
        {
            set
            {
                this.m_PageSize = value;
            }
            get
            {
                return this.m_PageSize;
            }
        }


        [Description("设置全部按钮是否可用"), DefaultValue(true), Category("分页")]
        public bool SearchAllable
        {
            set
            {
                this.m_PageSearchAllable = value;
            }
            get
            {
                return this.m_PageSearchAllable;
            }
        }



        [Description("获取记录总页数"), DefaultValue(0), Category("分页")]
        public int PageCount
        {
            get
            {
                return this.m_PageCount;
            }
        }

        [Description("设置或获取记录总数"), Category("分页")]
        public int RecordCount
        {
            set
            {
                this.m_RecordCount = value;
            }
            get
            {
                return this.m_RecordCount;
            }
        }

        [Description("当前的页面索引, 开始为1"), DefaultValue(0), Category("分页")]
        [Browsable(false)]
        public int PageIndex
        {
            set
            {
                this.m_PageIndex = value;
            }
            get
            {
                return this.m_PageIndex;
            }
        }

        /// <summary> 
        /// 初始化分页信息
        /// <param name="pageSize">每页记录数</param>
        /// <param name="recordCount">总记录数</param>
        /// </summary>
        public void InitPageInfo(int recordCount, int pageSize)
        {
            this.m_RecordCount = recordCount;
            this.m_PageSize = pageSize;
            this.InitPageInfo();
        }

        /// <summary> 
        /// 初始化分页信息
        /// <param name="recordCount">总记录数</param>
        /// </summary>
        public void InitPageInfo(int recordCount)
        {
            this.m_RecordCount = recordCount;
            this.InitPageInfo();
        }

        /// <summary> 
        /// 初始化分页信息
        /// </summary>
        public void InitPageInfo()
        {
            if (this.m_PageSize < 1)
            {
                this.m_PageSize = 10; //如果每页记录数不正确，即更改为10
            }

            if (this.m_RecordCount < 0)
            {
                this.m_RecordCount = 0; //如果记录总数不正确，即更改为0
            }

            //取得总页数
            if (this.m_RecordCount % this.m_PageSize == 0)
            {
                this.m_PageCount = this.m_RecordCount / this.m_PageSize;
            }
            else
            {
                this.m_PageCount = this.m_RecordCount / this.m_PageSize + 1;
            }

            //设置当前页
            if (this.m_PageIndex > this.m_PageCount)
            {
                this.m_PageIndex = this.m_PageCount;
            }
            if (this.m_PageIndex < 1)
            {
                this.m_PageIndex = 1;
            }

            //设置全部按钮状态
            //btnAll.Visible = m_PageSearchAllable;


            //设置上一页按钮的可用性
            bool enable = (this.PageIndex > 1);
            this.btnPrevious.Enabled = enable;

            //设置首页按钮的可用性
            enable = (this.PageIndex > 1);
            this.btnFirst.Enabled = enable;

            //设置下一页按钮的可用性
            enable = (this.PageIndex < this.PageCount);
            this.btnNext.Enabled = enable;

            //设置末页按钮的可用性
            enable = (this.PageIndex < this.PageCount);
            this.btnLast.Enabled = enable;

            this.txtPageIndex.Text = this.m_PageIndex.ToString();
            //this.labRecordCount.Text = string.Format("共 {0} 条记录，每页 {1} 条，共 {2} 页", this.m_RecordCount, this.m_PageSize, this.m_PageCount);
            //this.labRecordCount.Text = string.Format("共 {0} 条记录，每页", this.m_RecordCount);
            //this.labPageCount.Text = string.Format("条，共 {0} 页", this.m_PageCount);

            this.labRecordCount.Text = string.Format("Total Record:{0},Each Page", this.m_RecordCount);
            this.labPageCount.Text = string.Format("PageCount:{0}", this.m_PageCount);

            this.txtPageSize.Text = this.m_PageSize.ToString();
        }

        public void RefreshData(int page)
        {
            this.m_PageIndex = page;
            EventArgs e = new EventArgs();
            OnPageChanged(e);
        }

        private void btnFirst_Click(object sender, System.EventArgs e)
        {
            this.RefreshData(1);
        }

        private void btnPrevious_Click(object sender, System.EventArgs e)
        {
            if (this.m_PageIndex > 1)
            {
                this.RefreshData(this.m_PageIndex - 1);
            }
            else
            {
                this.RefreshData(1);
            }
        }
        private void btnNext_Click(object sender, System.EventArgs e)
        {
            if (this.m_PageIndex < this.m_PageCount)
            {
                this.RefreshData(this.m_PageIndex + 1);
            }
            else if (this.m_PageCount < 1)
            {
                this.RefreshData(1);
            }
            else
            {
                this.RefreshData(this.m_PageCount);
            }
        }

        private void btnLast_Click(object sender, System.EventArgs e)
        {
            if (this.m_PageCount > 0)
            {
                this.RefreshData(this.m_PageCount);
            }
            else
            {
                this.RefreshData(1);
            }
        }

        private void txtPageIndex_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int num;
                try
                {
                    num = Convert.ToInt16(this.txtPageIndex.Text);
                }
                catch// (Exception ex)
                {
                    num = 1;
                }

                if (num > this.m_PageCount)
                    num = this.m_PageCount;
                if (num < 1)
                    num = 1;

                this.RefreshData(num);
            }
        }

        void txtPageSize_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                int num;
                try
                {
                    num = Convert.ToInt32(this.txtPageSize.Text);
                }
                catch// (Exception ex)
                {
                    num = this.PageSize;
                }                
                m_PageSize = num;
                this.RefreshData(1);
            }
        }

        private void txtPageSize_TextChanged(object sender, EventArgs e)
        {
            int num;
            try
            {
                num = Convert.ToInt16(this.txtPageSize.Text);
            }
            catch// (Exception ex)
            {
                num = this.PageSize;
            }
            m_PageSize = num;
        }

        private void txtPageSize_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键  
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字  
                {

                    e.Handled = true;
                    return;
                }
                //if (Convert.ToInt32(this.txtPageSize.Text + e.KeyChar) > BaseSystemInfo.MaxPageSize)
                //{
                //    MessageBox.Show("最大分页件数不能超过" + BaseSystemInfo.MaxPageSize);
                //    e.Handled = true;
                //}
            }
        }

        private void txtPageIndex_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar != '\b')//这是允许输入退格键  
            {
                if ((e.KeyChar < '0') || (e.KeyChar > '9'))//这是允许输入0-9数字  
                {

                    e.Handled = true;
                }
            }
        }

        private void btnAll_Click(object sender, EventArgs e)
        {
            EventArgs eAege = new EventArgs();
            OnSearchAll(eAege);
        }
    }
}
