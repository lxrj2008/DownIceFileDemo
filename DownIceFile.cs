using BizFirewall;
using SharpCompress.Archive;
using SharpCompress.Common;
using System;
using System.Configuration;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace DemoTest
{
    public partial class DownIceFile : Form
    {
        Action<long, long> WebDownProgressDelegate;
        Action<ulong> FtpDwonProgressDelegate;
        DataTable FileData;
        long Filelen;
        public DownIceFile()
        {
            InitializeComponent();
            comboBox1.SelectedIndex = 0;
            WebDownProgressDelegate = (TotalBytes, DownBytes) =>
            {
                if (InvokeRequired)
                {
                    label1.Invoke(WebDownProgressDelegate, TotalBytes, DownBytes);
                }
                else
                {
                    progressBar1.Maximum = (int)TotalBytes;
                    label1.Text = "进度："+(DownBytes / 1024).ToString() + "kb" + "/" + (TotalBytes / 1024).ToString() + "kb";
                    progressBar1.Value = (int)DownBytes;
                }
            };
            FtpDwonProgressDelegate = (DownBytes) =>
              {
                  if(InvokeRequired)
                  {
                      label1.Invoke(FtpDwonProgressDelegate, DownBytes);
                  }
                  else
                  {
                      progressBar1.Maximum = (int)Filelen;
                      label1.Text = "进度：" + ((long)DownBytes / 1024).ToString() + "kb" + "/" + (Filelen / 1024).ToString() + "kb";
                      progressBar1.Value = (int)DownBytes;
                  }

              };
            myPager1.InitPageInfo();
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
        private void button1_Click(object sender, EventArgs e)
        {
            if(comboBox1.Text=="请选择")
            {
                MessageBox.Show("请选择下载文件源");
                return;
            }
            var ComboBoxTxt = comboBox1.Text;
            string downloadFilePath = ConfigurationManager.AppSettings["downloadFilePath"].ToString();
            if(!Directory.Exists(downloadFilePath))
            {
                Directory.CreateDirectory(downloadFilePath);
            }
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
                    else if(ComboBoxTxt=="CME")
                    {
                        filename = $"cme.settle.{dateTimePicker1.Value.ToString("yyyyMMdd")}.s.xml.zip";
                    }
                    else if(ComboBoxTxt=="CBT")
                    {
                        filename= $"cbt.settle.{dateTimePicker1.Value.ToString("yyyyMMdd")}.s.xml.zip";
                    }
                    else if(ComboBoxTxt=="NYMEX")
                    {
                        filename = $"nymex.settle.{dateTimePicker1.Value.ToString("yyyyMMdd")}.s.xml.zip";
                    }
                    else if (ComboBoxTxt == "COMEX")
                    {
                        filename = $"comex.settle.{dateTimePicker1.Value.ToString("yyyyMMdd")}.s.xml.zip";
                    }
                    var FileFullPath = "";
                    //FTP下载
                    if (ComboBoxTxt == "CME" || ComboBoxTxt == "CBT" || ComboBoxTxt == "COMEX" || ComboBoxTxt == "NYMEX")
                    {
                        string remotePath = "pub/SETTLE/";
                        SFTPUtil sftp = new SFTPUtil(ConfigurationManager.AppSettings["sftpURL"].ToString(), ConfigurationManager.AppSettings["sftpPort"].ToString(), ConfigurationManager.AppSettings["sftpUser"].ToString(), ConfigurationManager.AppSettings["sftpPass"].ToString());
                        Filelen = sftp.GetFileSiez(remotePath + filename);
                        var IsSuccess = sftp.DownFileFromFtp(remotePath + filename, downloadFilePath + "\\" + filename,FtpDwonProgressDelegate);
                        if(IsSuccess)
                        {
                            //解压zip文件
                            using (var archive = ArchiveFactory.Open(downloadFilePath + "\\" + filename))
                            {
                                string LastFile = "";
                                foreach (var entry in archive.Entries)
                                {
                                    if (!entry.IsDirectory)
                                    {
                                        entry.WriteToDirectory(downloadFilePath, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                                        LastFile = entry.FilePath;
                                    }
                                }
                                //解析文件获取结算价
                                FileData = AnalysisPirce_CME(downloadFilePath + $"\\{LastFile}");
                                myPager1.RecordCount = FileData.Rows.Count;
                                myPager1.PageIndex = 1;
                                if (InvokeRequired)
                                {
                                    dataGridView1.Invoke(new EventHandler(delegate
                                    {
                                        dataGridView1.DataSource = GetPagedTable(FileData, 1, myPager1.PageSize);
                                    }));
                                    myPager1.Invoke(new EventHandler(delegate
                                    {
                                        myPager1.InitPageInfo();
                                    }));
                                }
                                else
                                {
                                    dataGridView1.DataSource = GetPagedTable(FileData, 1, myPager1.PageSize);
                                    myPager1.InitPageInfo();
                                }
                            }
                        }
                    }
                    else //网站下载
                    {
                        var IsSuccess = HttpUtil.DownFileFromWeb(FileWebSite, downloadFilePath, filename, ref FileFullPath, WebDownProgressDelegate);
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
                                        entry.WriteToDirectory(downloadFilePath, ExtractOptions.ExtractFullPath | ExtractOptions.Overwrite);
                                        LastFile = entry.FilePath;
                                    }
                                }
                                //解析文件获取结算价
                                FileData = AnalysisPirce_ICE(downloadFilePath + $"\\{LastFile}", BizDate);
                                myPager1.RecordCount = FileData.Rows.Count;
                                myPager1.PageIndex = 1;
                                if (InvokeRequired)
                                {
                                    dataGridView1.Invoke(new EventHandler(delegate
                                    {
                                        dataGridView1.DataSource = GetPagedTable(FileData, 1, myPager1.PageSize);
                                    }));
                                    myPager1.Invoke(new EventHandler(delegate
                                    {
                                        myPager1.InitPageInfo();
                                    }));
                                }
                                else
                                {
                                    dataGridView1.DataSource = GetPagedTable(FileData, 1, myPager1.PageSize);
                                    myPager1.InitPageInfo();
                                }
                            }


                        }
                        else
                        {
                            MessageBox.Show("下载失败，请确认文件是否存在！");
                        }
                    }
                    //保存到磁盘文件
                    if (FileData != null && FileData.Rows.Count > 0)
                    {
                        //SaveCSV(FileData, downloadFilePath + $"\\SettlePrice_{DateTime.Now.ToString("HH.mm.ss")}.csv", "MQMExchangeCode,ClearProductCode,MMY,MQMSecType,BizDt,PutCall,StrikePx,SettlementPx");
                    }
                }
                catch(Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }
            });
        }
        /// <summary>
        /// datatable内存分页
        /// </summary>
        /// <param name="dt">数据源</param>
        /// <param name="PageIndex">当前页码</param>
        /// <param name="PageSize">每页显示行数</param>
        /// <returns></returns>
        private DataTable GetPagedTable(DataTable dt, int PageIndex, int PageSize)
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
        /// <summary>
        /// 从CME（cme,cbt,comex,nymex） 结算价文件里解析解压缩后的文件得出结算价
        /// </summary>
        /// <param name="FilePath"></param>
        /// <param name="ExchangeCode"></param>
        /// <param name="BizDate"></param>
        /// <returns></returns>
        private DataTable AnalysisPirce_CME(string FilePath)
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
            XDocument doc = XDocument.Load(FilePath);
            var DataList = from t in doc.Descendants("MktDataFull")
                                     select new
                                     {
                                         MQMExchangeCode = t.Element("Instrmt").Attribute("Exch").Value,
                                         ClearProductCode = t.Element("Instrmt").Attribute("ID").Value,
                                         MMY = t.Element("Instrmt").Attribute("MMY").Value.Length == 8 ? t.Element("Instrmt").Attribute("MMY").Value : t.Element("Instrmt").Attribute("MMY").Value + "00",
                                         MQMSecType = t.Element("Instrmt").Attribute("SecTyp").Value,
                                         BizDt = t.Attribute("BizDt").Value,
                                         PutCall = t.Element("Instrmt").Attribute("PutCall") == null ? null : t.Element("Instrmt").Attribute("PutCall").Value,
                                         StrikePx = t.Element("Instrmt").Attribute("StrkPx") == null ? null : t.Element("Instrmt").Attribute("StrkPx").Value,
                                         Px = t.Elements("Full").ToList().Find(p => p.Attribute("Typ").Value == "6"),
                                         FCommodityNo = t.Element("Instrmt").Attribute("Sym").Value
                                     };
            foreach (var fee in DataList)
            {
                var newrow= dt.NewRow();
                if (fee.MQMSecType == "OOF")
                {
                    newrow["PutCall"] = fee.PutCall == "0" ? "P" : "C";
                    newrow["StrikePx"] = fee.StrikePx;
                }
                newrow["MQMExchangeCode"] = fee.MQMExchangeCode;
                newrow["ClearProductCode"] = fee.ClearProductCode;
                newrow["MMY"] = fee.MMY;
                newrow["MQMSecType"] = fee.MQMSecType;
                newrow["BizDt"] = fee.BizDt;
                newrow["SettlementPx"] = fee.Px.Attribute("Px").Value.ToString() == "9999999" ? fee.Px.Attribute("LowPx") != null ? fee.Px.Attribute("LowPx").Value.ToString() : "0" : fee.Px.Attribute("Px").Value.ToString();
                dt.Rows.Add(newrow);
            }
            return dt;
        }

        /// <summary>
        /// 从ICE Span文件里解析解压缩后的span文件得出结算价
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="BizDate">结算价日期</param>
        /// <returns></returns>
        private  DataTable AnalysisPirce_ICE(string FilePath, string BizDate)
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
            string line;
            DataRow dr;
            string[] SettleInfo;
            string ProductCode = string.Empty;
            string SecType = string.Empty;
            string MMY = string.Empty;
            decimal TickSize = 1;
            var pattern = @"^[-]?\d+[.]?\d*$";
            using (StreamReader sr = new StreamReader(FilePath, Encoding.UTF8))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace("\"", "");
                    SettleInfo = line.Split(',');
                    if (SettleInfo[0] == "40" || SettleInfo[0] == "50" || SettleInfo[0] == "60")
                    {
                        if (SettleInfo[0] == "40")
                        {
                            ProductCode = SettleInfo[1].Trim();
                            if (SettleInfo[2].Trim() == "F")
                            {
                                SecType = "FUT";
                            }
                            else if (SettleInfo[2].Trim() == "O")
                            {
                                SecType = "OOF";
                            }
                            if (Regex.IsMatch(SettleInfo[7].Trim(), pattern))
                            {
                                TickSize = decimal.Parse(SettleInfo[7].Trim());
                            }
                            else
                            {
                                continue;
                            }

                        }
                    }
                    if (SettleInfo[0] == "50")
                    {
                        MMY = SettleInfo[1].Trim();
                    }
                    if (SettleInfo[0] == "60")
                    {
                        if (string.IsNullOrEmpty(SecType))
                        {
                            continue;
                        }
                        dr = dt.NewRow();
                        dr["MQMExchangeCode"] = "ICE";
                        dr["ClearProductCode"] = ProductCode;
                        dr["MQMSecType"] = SecType;
                        dr["MMY"] = MMY;
                        if (SecType == "OOF")
                        {
                            dr["PutCall"] = SettleInfo[2].Trim();
                            dr["StrikePx"] = decimal.Parse(SettleInfo[1]) * TickSize;
                        }
                        dr["BizDt"] = BizDate;
                        dr["SettlementPx"] = decimal.Parse(SettleInfo[4]) * TickSize;
                        dt.Rows.Add(dr);
                    }
                }
            }
            return dt;
        }

        /// <summary>
        /// 将DataTable中数据写入到CSV文件中
        /// </summary>
        /// <param name="dt">提供保存数据的DataTable</param>
        /// <param name="fullPath">CSV的文件路径</param>
        /// <param name="columname">字段标题,逗号分隔</param>
        private void SaveCSV(DataTable dt, string fullPath, string columname = "")
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            using (FileStream fs = new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
            {
                string data = "";
                using (StreamWriter sw = new StreamWriter(fs, System.Text.Encoding.UTF8))
                {
                    //写出列名称
                    if (!string.IsNullOrEmpty(columname))
                    {
                        var columnArr = columname.Split(',');
                        for (var i = 0; i < columnArr.Length; i++)
                        {
                            data += columnArr[i];
                            if (i < columnArr.Length - 1)
                            {
                                data += ",";
                            }
                        }
                        sw.WriteLine(data);
                    }
                    //写出各行数据
                    for (int i = 0; i < dt.Rows.Count; i++)
                    {
                        data = "";
                        for (int j = 0; j < dt.Columns.Count; j++)
                        {
                            string str = dt.Rows[i][j].ToString();
                            str = str.Replace("\"", "\"\"");//替换英文冒号 英文冒号需要换成两个冒号
                            if (str.Contains(',') || str.Contains('"')
                                || str.Contains('\r') || str.Contains('\n')) //含逗号 冒号 换行符的需要放到引号中
                            {
                                str = string.Format("\"{0}\"", str);
                            }

                            data += str;
                            if (j < dt.Columns.Count - 1)
                            {
                                data += ",";
                            }
                        }
                        sw.WriteLine(data);
                    }
                }
            }
        }

        private void myPager1_PageChanged(object sender, EventArgs e)
        {
            if (FileData != null && FileData.Rows.Count > 0)
            {
                var data = GetPagedTable(FileData, myPager1.PageIndex, myPager1.PageSize);
                dataGridView1.DataSource = data;
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.Close();
        }

    }
}
