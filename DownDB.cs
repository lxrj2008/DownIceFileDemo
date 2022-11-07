using BizFirewall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace DemoTest
{
    public partial class DownDB : Form
    {
        Action<int, int> WebDownProgressDelegate;
        public DownDB()
        {
            InitializeComponent();
            WebDownProgressDelegate = (TotalBytes, DownBytes) =>
            {
                if (InvokeRequired)
                {
                    label1.Invoke(WebDownProgressDelegate, TotalBytes, DownBytes);
                }
                else
                {
                    progressBar1.Maximum = (int)TotalBytes;
                    label1.Text = "进度：" + (DownBytes / 1024).ToString() + "kb" + "/" + (TotalBytes / 1024).ToString() + "kb";
                    progressBar1.Value = (int)DownBytes;
                }
            };
        }

        private void DownDB_Load(object sender, EventArgs e)
        {
            Task.Run(() =>
            {
                var ftp = new FTPUtil("152.179.169.94", "usqsftp", "gF0aF0aM0r");
                while (true)
                {
                    var FileList = ftp.GetFileList(true);
                    if (FileList.Count > 0)
                    {
                        var match = $"CMEClearDB_backup_{DateTime.Now.ToString("yyyy_MM_dd")}";
                        var realfile = FileList.Find(x => x.Contains(match));
                        if (realfile != null)
                        {
                            var ret = ftp.FtpDownload(realfile, $@"F:\SystemRestory\SystemUserData\admin\Documents\{realfile}", true,true,WebDownProgressDelegate);
                            if (ret)
                            {
                                break;
                            }
                            else
                            {
                                Thread.Sleep(300000);
                                continue;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                    else
                    {
                        Thread.Sleep(300000);
                        continue;
                    }
                }
            }).ContinueWith(x=>
            {
                MessageBox.Show("下载完成！");
            });
        }
    }
}
