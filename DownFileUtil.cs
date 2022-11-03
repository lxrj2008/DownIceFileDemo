using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.IO;
using System.Net;
using System.Net.Http;

namespace BizFirewall
{
    public class SFTPUtil
    {

        #region 字段或属性
        private SftpClient sftp;
        /// <summary>
        /// SFTP连接状态
        /// </summary>
        public bool Connected { get { return sftp.IsConnected; } }
        #endregion

        #region 构造
        /// <summary>
        /// 构造
        /// </summary>
        /// <param name="ip">IP</param>
        /// <param name="port">端口</param>
        /// <param name="user">用户名</param>
        /// <param name="pwd">密码</param>
        public SFTPUtil(string ip, string port, string user, string pwd)
        {
            sftp = new SftpClient(ip, Int32.Parse(port), user, pwd);
        }
        #endregion

        #region 连接SFTP
        /// <summary>
        /// 连接SFTP
        /// </summary>
        /// <returns>true成功</returns>
        public bool Connect()
        {
            try
            {
                if (!Connected)
                {
                    sftp.Connect();
                }
                return true;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("连接SFTP失败，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 断开SFTP
        /// <summary>
        /// 断开SFTP
        /// </summary> 
        public void Disconnect()
        {
            try
            {
                if (sftp != null && Connected)
                {
                    sftp.Disconnect();
                }
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("断开SFTP失败，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 下载SFTP文件
       
        /// <summary>
        /// 从sftp下载文件
        /// </summary>
        /// <param name="RemoteFilePath">远程路径</param>
        /// <param name="LocalFilePath">本地路径</param>
        /// <param name="DownProgress">下载进度条委托时间，可以不该值</param>
        /// <returns></returns>
        
        public bool DownFileFromFtp(string RemoteFilePath,string LocalFilePath, Action<ulong> DownProgress = null)
        {
            try
            {
                Connect();
                using (FileStream fileStream = new FileStream(LocalFilePath, FileMode.Create))
                {
                    sftp.DownloadFile(RemoteFilePath, fileStream, DownProgress);
                }
                Disconnect();
                return true;   
                
            }
            catch(Exception ex)
            {
                return false;
            }
        }
        #endregion

        #region 获取文件大小
        /// <summary>
        /// 获取文件大小
        /// </summary>
        /// <param name="RemoteFilePath">远程路径</param>
        /// <returns></returns>
        public long GetFileSiez(string RemoteFilePath)
        {
            long size;
            try
            {
                Connect();
                SftpFile sftpfile = sftp.Get(RemoteFilePath);
                size = sftpfile.Length;
                Disconnect();

            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
            return size;
        }
        #endregion
    }

    public class HttpUtil
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
        public static bool DownFileFromWeb(string Url, string DirectoryPath, string FileName, ref string FullName, Action<long, long> DownProgress = null)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
                | SecurityProtocolType.Tls11
                | SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback += (sender, certificate, chain, errors) => { return true; };
            using (HttpClient httpClient = new HttpClient())
            {
                if (!Directory.Exists(DirectoryPath))
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
                            FullName = $"{DirectoryPath}{FileName}";
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
    }
}
