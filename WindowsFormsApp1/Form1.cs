using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO.Compression;
using BrotliSharpLib;
using Newtonsoft.Json;
using CefSharp;
using CefSharp.WinForms;
using System.Threading;

namespace WindowsFormsApp1
{
    public partial class Form1 : Form
    {
        ChromiumWebBrowser webview = null;
        string url = "https://cn.investing.com/dividends-calendar/";

        public Form1()
        {
            InitializeComponent();
            InitCEF();
        }
        private void InitCEF()
        {
            var settings = new CefSettings();


            // By default CEF uses an in memory cache, to save cached data e.g. to persist cookies you need to specify a cache path
            // NOTE: The executing user must have sufficient privileges to write to this folder.
            settings.CachePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "CefSharp\\Cache");
            settings.PersistSessionCookies = true;
            //settings.UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/107.0.0.0 Safari/537.36";

            Cef.Initialize(settings);
        }
        private void Form1_Load(object sender, EventArgs e)
        {
            webview = new ChromiumWebBrowser();
            webview.Dock = DockStyle.Fill;
            this.panel1.Controls.Add(webview);
            dateTimePicker2.Value = dateTimePicker1.Value.AddDays(7);
            webview.FrameLoadEnd += WebBrower_FrameLoadEnd;
        }
        private void button1_Click(object sender, EventArgs e)
        {
            var url = "https://cn.investing.com/dividends-calendar/Service/getCalendarFilteredData";
            var paramList = new List<KeyValuePair<string, string>>();
            paramList.Add(new KeyValuePair<string, string>("currentTab", "custom"));
            paramList.Add(new KeyValuePair<string, string>("dateFrom", dateTimePicker1.Value.ToString("yyyy-MM-dd")));
            paramList.Add(new KeyValuePair<string, string>("dateTo", dateTimePicker2.Value.ToString("yyyy-MM-dd")));
            paramList.Add(new KeyValuePair<string, string>("limit_from", "0"));
            paramList.Add(new KeyValuePair<string, string>("country[]", "4"));
            paramList.Add(new KeyValuePair<string, string>("country[]", "5"));
            paramList.Add(new KeyValuePair<string, string>("country[]", "36"));
            paramList.Add(new KeyValuePair<string, string>("country[]", "37"));
            paramList.Add(new KeyValuePair<string, string>("country[]", "39"));
            var userAgent = "Mozilla/5.0 (Windows NT 6.2; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
            var referer = "https://cn.investing.com/dividends-calendar/";
            var objdata = RequestPost<GuXiApiData>(url, paramList, userAgent, referer);
            if (objdata != null)
            {
                webBrowser1.DocumentText = "<html><table>" + objdata.data + "</html></table>";
            }

        }
        private void WebBrower_FrameLoadEnd(object sender, CefSharp.FrameLoadEndEventArgs e)
        {
            //if (e.Frame.IsMain)
            //{
            //    SimulationRun();
            //}
        }
        private T RequestPost<T>(string url, List<KeyValuePair<string, string>> paramList, string UserAgent = null, string referer = null)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
            var requestUrl = url;
            var paramStr = "";
            foreach (var param in paramList)
            {
                paramStr += string.Format(@"{0}={1}&", param.Key, param.Value);
            }
            paramStr = paramStr.TrimEnd('&');
            var bytes = Encoding.UTF8.GetBytes(paramStr);

            var request = (HttpWebRequest)WebRequest.Create(requestUrl);
            request.Method = "POST";
            request.Timeout = 2 * 60 * 1000;
            request.Headers["Cookie"] = "udid=4dcba8f7c32514ad0f3e813fb12a95fd;cf_clearance=1260377ad2d8a41a129a6c529eceff790320d141-1666882298-0-150";
            request.Headers["Accept-Encoding"] = "gzip, deflate, br";
            request.Headers["Accept-Language"] = "zh-CN,zh;q=0.9";
            request.Headers["X-Requested-With"] = "XMLHttpRequest";
            request.ContentType = "application/x-www-form-urlencoded";
            if (!string.IsNullOrEmpty(UserAgent))
                request.UserAgent = UserAgent;
            if (!string.IsNullOrEmpty(referer))
                request.Referer = referer;
            request.ContentLength = bytes.Length;
            var ReadStr = "";
            try
            {
                using (var requestStream = request.GetRequestStream())
                {
                    requestStream.Write(bytes, 0, bytes.Length);
                    requestStream.Close();
                    using (var reponse = (HttpWebResponse)request.GetResponse())
                    {
                        if (reponse.ContentEncoding.ToLower().Contains("gzip"))
                        {
                            ReadStr = new StreamReader(new GZipStream(reponse.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
                        }
                        else if (reponse.ContentEncoding.ToLower().Contains("deflate"))
                        {
                            ReadStr = new StreamReader(new DeflateStream(reponse.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
                        }
                        else if (reponse.ContentEncoding.ToLower().Contains("br"))
                        {
                            //需要从NuGet引用 Brotli.Net
                            ReadStr = new StreamReader(new BrotliStream(reponse.GetResponseStream(), CompressionMode.Decompress)).ReadToEnd();
                        }
                        else
                        {
                            ReadStr = new StreamReader(reponse.GetResponseStream(), Encoding.UTF8).ReadToEnd();

                        }
                        var result = JsonConvert.DeserializeObject<T>(ReadStr);
                        return result;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return default(T);
        }
        private bool IsSameMonth()
        {
            if(DateTime.Now.Month==DateTime.Now.AddDays(7).Month)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        public  async Task<string> GetScriptResult(string javascript)
        {
            var result = await webview.EvaluateScriptAsync(javascript);
            if (result.Success)
            {
                return result.Result.ToString();
            }
            else
            {
                return await webview.GetSourceAsync();
            }
        }
        private void SimulationRun()
        {
            webview.GetMainFrame().EvaluateScriptAsync("document.getElementById('datePickerToggleBtn').click();");
            var task0 = GetScriptResult("function tempFunction0() { var a=$($(\".ui-datepicker-month\")[0]).html();var b=$($(\".ui-datepicker-month\")[1]).html();return a+b;} tempFunction0();");
            string[] hh=new string[2];
           
            hh= task0.Result.Replace("月", ",").TrimEnd(',').Split(',');
            Thread.Sleep(3000);
            var CurrentMonth = DateTime.Now.Month.ToString();
            if(CurrentMonth==hh[0])
            {
                if (IsSameMonth())
                {
                    webview.GetMainFrame().EvaluateScriptAsync("$(\".ui-datepicker-group.ui-datepicker-group-middle a[target = '_blank']\").each(function(){var val=$(this).text();if(val==\"20\"){$(this).click()}})");
                }
                else
                {
                    webview.GetMainFrame().EvaluateScriptAsync("$(\".ui-datepicker-group.ui-datepicker-group-last a[target = '_blank']\").each(function(){var val=$(this).text();if(val==\"5\"){$(this).click()}})");
                }
            }
            if(CurrentMonth == hh[1])
            {
                webview.GetMainFrame().EvaluateScriptAsync("$(\".ui-datepicker-group.ui-datepicker-group-last a[target = '_blank']\").each(function(){var val=$(this).text();if(val==\"7\"){$(this).click()}})");
            }
            Thread.Sleep(2000);
            webview.GetMainFrame().EvaluateScriptAsync("document.getElementById('applyBtn').click();");
            int i = 100;
            DateTime dtime = DateTime.Now;
            do
            {
                webview.GetBrowser().MainFrame.EvaluateScriptAsync("window.scrollTo(0," + i + ")");
                i += 100;
                Thread.Sleep(80);
            } while ((DateTime.Now - dtime).Seconds < 30);
            webview.GetBrowser().MainFrame.EvaluateScriptAsync("window.scrollTo(0,200)");
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("function tempFunction() {");
            sb.AppendLine(" return document.getElementById('dividendsCalendarData').outerHTML; ");
            sb.AppendLine("}");
            sb.AppendLine("tempFunction();");
            Thread.Sleep(5000);
            var task1 = GetScriptResult(sb.ToString());
            var tablestr = task1.Result;
            webBrowser1.DocumentText = tablestr;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            webview.LoadUrl(url);
            Thread.Sleep(5000);
            Task.Run(() =>
            {
                SimulationRun();
            });
            
        }

        private void button3_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
    public class GuXiApiData
    {
        public bool bind_scroll_handler { get; set; }
        public string last_time_scope { get; set; }
        public string data{ get;set;}
        public string dateFrom { get; set; }
        public string dateTo { get; set; }
        public int rows_num { get; set; }
        public string timeframe { get; set; }
    }
}
