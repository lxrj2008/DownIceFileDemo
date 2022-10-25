using BizFirewall;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoTest
{
    public partial class DownIceFile : Form
    {
        Action<long, long> DownProgressDelegate;
        DataTable AllData;
        public DownIceFile()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            DownProgressDelegate = (TotalBytes, DownBytes) =>
            {
                if (InvokeRequired)
                {
                    label1.Invoke(DownProgressDelegate, TotalBytes, DownBytes);
                }
                else
                {
                    progressBar1.Maximum = (int)TotalBytes;
                    label1.Text = "进度："+(DownBytes / 1024).ToString() + "kb" + "/" + (TotalBytes / 1024).ToString() + "kb";
                    progressBar1.Value = (int)DownBytes;
                }
            };
            myPager1.InitPageInfo();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.Text=="请选择")
            {
                MessageBox.Show("请选择下载文件源");
                return;
            }
            var ComboBoxTxt = comboBox1.Text;
            Task.Run(() =>
            {
                try
                {
                    var FileWebSite = "";
                    var filename = "";
                    string BizDate = dateTimePicker1.Value.ToString("yyyy-MM-dd");
                    string Year = DateTime.Parse(BizDate).ToString("yyyy");
                    string Month = DateTime.Parse(BizDate).ToString("MM");
                    string Day = DateTime.Parse(BizDate).ToString("dd");
                    if (ComboBoxTxt == "ICE美国")
                    {
                        filename = $"NYB{Month + Day}F.csv.zip";
                        FileWebSite = $"https://www.theice.com/publicdocs/irm_files/icus/{Year}/{Month}/NYB{Month + Day}F.csv.zip";
                    }
                    else if (ComboBoxTxt == "ICE新加坡")
                    {
                        filename = $"ISG{Month + Day}F.csv.zip";
                        FileWebSite = $"https://www.theice.com/publicdocs/irm_files/icsg/{Year}/{Month}/ISG{Month + Day}F.csv.zip";
                    }
                    else if (ComboBoxTxt == "ICE欧洲")
                    {
                        filename = $"IPE{Month + Day}F.csv.zip";
                        FileWebSite = $"https://www.theice.com/publicdocs/irm_files/iceu/{Year}/{Month}/IPE{Month + Day}F.CSV.zip";
                    }
                    else if (ComboBoxTxt == "ICE荷兰")
                    {
                        filename = $"NL{Month + Day}F.csv.zip";
                        FileWebSite = $"https://www.theice.com/publicdocs/irm_files/icnl/{Year}/{Month}/NL{Month + Day}F.csv.zip";
                    }
                    var FileDirectory = @"e:\CMEReport\";
                    var FileFullPath = "";
                    //下载.zip
                    var IsSuccess = HttpHelper.DownloadFile(FileWebSite, FileDirectory, filename, ref FileFullPath, DownProgressDelegate);
                    if (IsSuccess)
                    {
                        //解压zip文件
                        using (var archive = ArchiveFactory.Open(FileFullPath))
                        {
                            string LastFile = "";
                            foreach (var entry in archive.Entries)
                            {
                                if (!entry.IsDirectory)
                                {
                                    entry.WriteToDirectory(FileDirectory, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                                    LastFile = entry.FilePath;
                                }
                            }
                            //解析文件获取结算价
                            var data = HttpHelper.AnalysisSettlePirceFile(FileDirectory + $"\\{LastFile}", BizDate);
                            AllData = data;
                            myPager1.RecordCount = data.Rows.Count;
                            myPager1.PageIndex = 1;
                            if (InvokeRequired)
                            {
                                dataGridView1.Invoke(new EventHandler(delegate
                                {
                                    dataGridView1.DataSource = GetPagedTable(data, 1, myPager1.PageSize);
                                }));
                                myPager1.Invoke(new EventHandler(delegate
                                {
                                    myPager1.InitPageInfo();
                                }));
                            }
                            else
                            {
                                dataGridView1.DataSource = GetPagedTable(data, 1, myPager1.PageSize);
                                myPager1.InitPageInfo();
                            }
                            //保存到磁盘文件
                            //HttpHelper.SaveCSV(AllData, FileDirectory + "\\SettlePrice.csv", "MQMExchangeCode,ClearProductCode,MMY,MQMSecType,BizDt,PutCall,StrikePx,SettlementPx");
                        }


                    }
                    else
                    {
                        MessageBox.Show("下载失败，请确认文件是否存在！");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
        public static DataTable GetPagedTable(DataTable dt, int PageIndex, int PageSize)
        {
            DataTable result;
            if (PageIndex == 0)
            {
                result = dt;
            }
            else
            {
                DataTable newdt = dt.Clone();
                //newdt.Clear();
                int rowbegin = (PageIndex - 1) * PageSize;
                int rowend = PageIndex * PageSize;
                if (rowbegin >= dt.Rows.Count)
                {
                    result = newdt;
                }
                else
                {
                    if (rowend > dt.Rows.Count)
                    {
                        rowend = dt.Rows.Count;
                    }
                    for (int i = rowbegin; i <= rowend - 1; i++)
                    {
                        DataRow newdr = newdt.NewRow();
                        DataRow dr = dt.Rows[i];
                        foreach (DataColumn column in dt.Columns)
                        {
                            newdr[column.ColumnName] = dr[column.ColumnName];
                        }
                        newdt.Rows.Add(newdr);
                    }
                    result = newdt;
                }
            }
            return result;
        }

        private void myPager1_PageChanged(object sender, EventArgs e)
        {
            if(AllData!=null && AllData.Rows.Count>0)
            {
                var data = GetPagedTable(AllData, myPager1.PageIndex, myPager1.PageSize);
                dataGridView1.DataSource = data;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void DownIceFile_Load(object sender, EventArgs e)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("MQMExchangeCode");
            dt.Columns.Add("ClearProductCode");
            dt.Columns.Add("MMY");
            dt.Columns.Add("MQMSecType");
            dt.Columns.Add("BizDt");
            dt.Columns.Add("PutCall");
            dt.Columns.Add("StrikePx");
            dt.Columns.Add("SettlementPx");
            dataGridView1.DataSource = dt;
            dataGridView1.Columns[0].Width = 120;
            dataGridView1.Columns[1].Width = 120;
            dataGridView1.Columns[6].Width = 130;
            dataGridView1.Columns[7].Width = 150;
            
        }
    }
}
