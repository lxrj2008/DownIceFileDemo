using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;

namespace BizFirewall
{
    public class HttpHelper
    {
        /// <summary>
        /// 从网站上下载文件并保存到指定目录
        /// </summary>
        /// <param name="Url">文件下载地址</param>
        /// <param name="DirectoryPath">文件下载目录</param>
        /// <param name="FileName">文件名（含扩展名）</param>
        /// <param name="FullName">下载后的文件名（含本地路径）</param>
        /// <param name="DownProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>  
        /// <returns></returns>
        public static bool DownloadFile(string Url, string DirectoryPath, string FileName, ref string FullName, Action<long, long> DownProgress = null)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
            using (HttpClient httpClient = new HttpClient())
            {
                if(!Directory.Exists(DirectoryPath))
                {
                    Directory.CreateDirectory(DirectoryPath);
                }
                var response = httpClient.GetAsync(Url, HttpCompletionOption.ResponseHeadersRead).Result;
                var totalLength = response.Content.Headers.ContentLength;
                if (response.IsSuccessStatusCode)
                {
                    using (Stream stream = response.Content.ReadAsStreamAsync().Result)
                    {
                        using (FileStream fileStream = new FileStream($"{DirectoryPath}\\{FileName}", FileMode.Create))
                        {
                            var buffer = new byte[5 * 1024];
                            int readLength = 0;
                            int length;
                            while ((length = stream.ReadAsync(buffer, 0, buffer.Length).Result) != 0)
                            {
                                readLength += length;
                                fileStream.Write(buffer, 0, length);
                                if (DownProgress != null)
                                {
                                    DownProgress(totalLength.Value, (long)readLength);//更新进度条     
                                }
                            }
                            FullName= $"{DirectoryPath}{FileName}";
                            return true;
                        }
                    }
                }
                else
                {
                    return false;
                }
            }
        }
        /// <summary>
        /// 解析解压缩后的span文件得出结算价
        /// </summary>
        /// <param name="FilePath">文件路径</param>
        /// <param name="BizDate">结算价日期</param>
        /// <returns></returns>
        public static DataTable AnalysisSettlePirceFile(string FilePath,string BizDate)
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
        public static void SaveCSV(DataTable dt, string fullPath, string columname = "")
        {
            FileInfo fi = new FileInfo(fullPath);
            if (!fi.Directory.Exists)
            {
                fi.Directory.Create();
            }
            using(FileStream fs= new FileStream(fullPath, System.IO.FileMode.Create, System.IO.FileAccess.Write))
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
    }
}
