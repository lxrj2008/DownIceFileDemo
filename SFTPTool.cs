using BizFirewall;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace test
{
    public partial class SFTPTool : Form
    {
        delegate void ShowMessageCallback(ListBox listbox, string text);
        private delegate void SetPos(int ipos, string vinfo);
        public SFTPTool()
        {
            InitializeComponent();
        }
        List<string> fileNamesWithDirectory = new List<string>();
        List<string> fileNamesWithoutDirectory = new List<string>();
        private void button2_Click(object sender, EventArgs e)
        {
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                fileNamesWithDirectory.Clear();
                fileNamesWithoutDirectory.Clear();
                textBox1.Text = "";
                foreach (string s in openFileDialog1.FileNames)
                {
                    fileNamesWithoutDirectory.Add(Path.GetFileName(s));
                    fileNamesWithDirectory.Add(s);
                    textBox1.Text += s + ";";
                    progressBar1.Maximum = fileNamesWithDirectory.Count;
                    progressBar1.Value = 0;
                    label1.Text = "0/" + fileNamesWithDirectory.Count;
                    listBox1.Items.Clear();
                }
                
            }
        }
        private async void button1_Click(object sender, EventArgs e)
        {
            listBox1.Items.Clear();
            label1.Text = "0/" + fileNamesWithDirectory.Count;
            this.progressBar1.Value = 0;
            await uploadFile();
        }
        private void ShowMessage(ListBox listbox, string text)
        {
            listbox.Items.Add(text);
        }
       private async Task uploadFile()
        {
            Task.Run(() =>
            {
                if (fileNamesWithDirectory.Count == 0)
                    return;
                var ip = ConfigurationManager.AppSettings["sftpURL"];
                var port = ConfigurationManager.AppSettings["sftpPort"];
                var userName = ConfigurationManager.AppSettings["sftpUser"];
                var pwd = ConfigurationManager.AppSettings["sftpPass"];
                var sftp = new SFTPHelper(ip, port, userName, pwd);
                for (var i = 0; i < fileNamesWithDirectory.Count; i++)
                {
                    var isSuccess = sftp.uploadSFTP(fileNamesWithDirectory[i], "/" + fileNamesWithoutDirectory[i]);
                    if (isSuccess)
                    {
                        if (listBox1.InvokeRequired)
                        {
                            ShowMessageCallback showmessagecallback = ShowMessage;
                            listBox1.Invoke(showmessagecallback, new object[] { listBox1, DateTime.Now.ToString() + ":" + fileNamesWithoutDirectory[i] + "上传成功" });
                        }
                        else
                        {
                            listBox1.Items.Add(DateTime.Now.ToString() + ":" + fileNamesWithoutDirectory[i] + "上传成功");
                        }
                    }
                    else
                    {
                        if (listBox1.InvokeRequired)
                        {
                            ShowMessageCallback showmessagecallback = ShowMessage;
                            listBox1.Invoke(showmessagecallback, new object[] { listBox1, DateTime.Now.ToString() + ":" + fileNamesWithoutDirectory[i] + "上传失败" });
                        }
                        else
                        {
                            listBox1.Items.Add(DateTime.Now.ToString() + ":" + fileNamesWithoutDirectory[i] + "上传失败");
                        }
                    }
                    SetTextMesssage(i+1, i.ToString() + "\r\n");
                }
                sftp.Disconnect();
            });
        }

       private void SetTextMesssage(int ipos, string vinfo)
       {
           if (this.InvokeRequired)
           {
               SetPos setpos = new SetPos(SetTextMesssage);
               this.Invoke(setpos, new object[] { ipos, vinfo });
           }
           else
           {
               this.label1.Text = ipos.ToString() + "/" + fileNamesWithDirectory.Count;
               this.progressBar1.Value = Convert.ToInt32(ipos);
           }
       }

       private void listBox1_DrawItem(object sender, DrawItemEventArgs e)
       {
           Brush FontBrush = null;
           ListBox listBox = sender as ListBox;
           if (e.Index > -1)
           {
               var itemVal = listBox.Items[e.Index].ToString();
               if (itemVal.Contains("上传失败"))
                   FontBrush = Brushes.Red;
               else
                   FontBrush = Brushes.Black;
               e.DrawBackground();
               e.Graphics.DrawString(listBox.Items[e.Index].ToString(), e.Font, FontBrush, e.Bounds);
               e.DrawFocusRectangle();
           }
       }
    }
}
