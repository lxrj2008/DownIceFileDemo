using Renci.SshNet;
using Renci.SshNet.Sftp;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

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

        #region 删除SFTP文件
        /// <summary>
        /// 删除SFTP文件 
        /// </summary>
        /// <param name="remoteFile">远程路径</param>
        public void Delete(string remoteFile)
        {
            try
            {
                Connect();
                sftp.Delete(remoteFile);
                Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件删除失败，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 获取SFTP文件列表
        /// <summary>
        /// 获取SFTP文件列表
        /// </summary>
        /// <param name="remotePath">远程目录</param>
        /// <param name="fileSuffix">文件后缀</param>
        /// <returns></returns>
        public ArrayList GetFileList(string remotePath, string fileSuffix)
        {
            try
            {
                Connect();
                var files = sftp.ListDirectory(remotePath);
                Disconnect();
                var objList = new ArrayList();
                foreach (var file in files)
                {
                    string name = file.Name;
                    if (name.Length > (fileSuffix.Length + 1) && fileSuffix == name.Substring(name.Length - fileSuffix.Length))
                    {
                        objList.Add(name);
                    }
                }
                return objList;
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件列表获取失败，原因：{0}", ex.Message));
            }
        }
        #endregion

        #region 移动SFTP文件
        /// <summary>
        /// 移动SFTP文件
        /// </summary>
        /// <param name="oldRemotePath">旧远程路径</param>
        /// <param name="newRemotePath">新远程路径</param>
        public void Move(string oldRemotePath, string newRemotePath)
        {
            try
            {
                Connect();
                sftp.RenameFile(oldRemotePath, newRemotePath);
                Disconnect();
            }
            catch (Exception ex)
            {
                throw new Exception(string.Format("SFTP文件移动失败，原因：{0}", ex.Message));
            }
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

    public class FTPUtil
    {
        #region 变量属性
        /// <summary>  
        /// Ftp服务器ip  
        /// </summary>  
        private string FtpServerIP = string.Empty;
        /// <summary>  
        /// Ftp 指定用户名  
        /// </summary>  
        private string FtpUserID = string.Empty;
        /// <summary>  
        /// Ftp 指定用户密码  
        /// </summary>  
        private string FtpPassword = string.Empty;
        public FTPUtil(string ServerIP,string UserId,string Pwd)
        {
            FtpServerIP = ServerIP;
            FtpUserID = UserId;
            FtpPassword = Pwd;
        }

        #endregion

        #region 从FTP服务器下载文件，指定本地路径和本地文件名
        /// <summary>  
        /// 从FTP服务器下载文件，指定本地路径和本地文件名  
        /// </summary>  
        /// <param name="remoteFileName">远程文件名</param>  
        /// <param name="localFileName">保存本地的文件名（包含路径）</param>  
        /// <param name="ifCredential">是否启用身份验证（false：表示允许用户匿名下载）</param>  
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>  
        /// <returns>是否下载成功</returns>  
        public bool FtpDownload(string remoteFileName, string localFileName, bool ifCredential, Action<int, int> updateProgress = null)
        {
            FtpWebRequest ftpreq, ftpsize;
            Stream ftpStream = null;
            FtpWebResponse response = null;
            FileStream outputStream = null; 
            try
            {

                outputStream = new FileStream(localFileName, FileMode.Create);
                if (string.IsNullOrEmpty(FtpServerIP))
                {
                    throw new Exception("ftp下载目标服务器地址未设置！");
                }
                Uri uri = new Uri("ftp://" + FtpServerIP + "/" + remoteFileName);
                ftpsize = (FtpWebRequest)FtpWebRequest.Create(uri);
                ftpsize.UseBinary = true;

                ftpreq = (FtpWebRequest)FtpWebRequest.Create(uri);
                ftpreq.UseBinary = true;
                ftpreq.KeepAlive = true;
                if (ifCredential)//使用用户身份认证  
                {
                    ftpsize.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                    ftpreq.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                }
                ftpsize.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse re = (FtpWebResponse)ftpsize.GetResponse();
                long totalBytes = re.ContentLength;
                re.Close();

                ftpreq.Method = WebRequestMethods.Ftp.DownloadFile;
                response = (FtpWebResponse)ftpreq.GetResponse();
                ftpStream = response.GetResponseStream();
                long totalDownloadedByte = 0;
                if (updateProgress != null)
                {
                    updateProgress((int)totalBytes, (int)totalDownloadedByte);//更新进度条     
                }
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    totalDownloadedByte = readCount + totalDownloadedByte;
                    outputStream.Write(buffer, 0, readCount);
                    //更新进度    
                    if (updateProgress != null)
                    {
                        updateProgress((int)totalBytes, (int)totalDownloadedByte);//更新进度条     
                    }
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Dispose();
                outputStream.Dispose();
                response.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (ftpStream != null)
                {
                    ftpStream.Dispose();
                }
                if (outputStream != null)
                {
                    outputStream.Dispose();
                }
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        /// <summary>  
        /// 从FTP服务器下载文件，指定本地路径和本地文件名（支持断点下载）  
        /// </summary>  
        /// <param name="remoteFileName">远程文件名</param>  
        /// <param name="localFileName">保存本地的文件名（包含路径）</param>  
        /// <param name="ifCredential">是否启用身份验证（false：表示允许用户匿名下载）</param>  
        /// <param name="size">已下载文件流大小</param>  
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>  
        /// <returns>是否下载成功</returns>  
        private bool FtpBrokenDownload(string remoteFileName, string localFileName, bool ifCredential, long size, Action<int, int> updateProgress = null)
        {
            FtpWebRequest ftpreq, ftpsize;
            Stream ftpStream = null;
            FtpWebResponse response = null;
            FileStream outputStream = null;
            try
            {

                outputStream = new FileStream(localFileName, FileMode.Append);
                if (string.IsNullOrEmpty(FtpServerIP))
                {
                    throw new Exception("ftp下载目标服务器地址未设置！");
                }
                Uri uri = new Uri("ftp://" + FtpServerIP + "/" + remoteFileName);
                ftpsize = (FtpWebRequest)FtpWebRequest.Create(uri);
                ftpsize.UseBinary = true;
                ftpsize.ContentOffset = size;

                ftpreq = (FtpWebRequest)FtpWebRequest.Create(uri);
                ftpreq.UseBinary = true;
                ftpreq.KeepAlive = true;
                ftpreq.ContentOffset = size;
                if (ifCredential)//使用用户身份认证  
                {
                    ftpsize.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                    ftpreq.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                }
                ftpsize.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse re = (FtpWebResponse)ftpsize.GetResponse();
                long totalBytes = re.ContentLength;
                re.Close();

                ftpreq.Method = WebRequestMethods.Ftp.DownloadFile;
                response = (FtpWebResponse)ftpreq.GetResponse();
                ftpStream = response.GetResponseStream();
                ftpStream.ReadTimeout = 30000;
                long totalDownloadedByte = size;
                if (updateProgress != null)
                {
                    updateProgress((int)totalBytes, (int)totalDownloadedByte);//更新进度条     
                }
                int bufferSize = 2048;
                int readCount;
                byte[] buffer = new byte[bufferSize];
                readCount = ftpStream.Read(buffer, 0, bufferSize);
                while (readCount > 0)
                {
                    totalDownloadedByte = readCount + totalDownloadedByte;
                    outputStream.Write(buffer, 0, readCount);
                    //更新进度    
                    if (updateProgress != null)
                    {
                        updateProgress((int)totalBytes, (int)totalDownloadedByte);//更新进度条     
                    }
                    readCount = ftpStream.Read(buffer, 0, bufferSize);
                }
                ftpStream.Dispose();
                outputStream.Dispose();
                response.Dispose();
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
            finally
            {
                if (ftpStream != null)
                {
                    ftpStream.Dispose();
                }
                if (outputStream != null)
                {
                    outputStream.Dispose();
                }
                if (response != null)
                {
                    response.Dispose();
                }
            }
        }

        /// <summary>  
        /// 从FTP服务器下载文件，指定本地路径和本地文件名  
        /// </summary>  
        /// <param name="remoteFileName">远程文件名</param>  
        /// <param name="localFileName">保存本地的文件名（包含路径）</param>  
        /// <param name="ifCredential">是否启用身份验证（false：表示允许用户匿名下载）</param>  
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>  
        /// <param name="brokenOpen">是否断点下载：true 会在localFileName 找是否存在已经下载的文件，并计算文件流大小</param>  
        /// <returns>是否下载成功</returns>  
        public bool FtpDownload(string remoteFileName, string localFileName, bool ifCredential, bool brokenOpen, Action<int, int> updateProgress = null)
        {
            if (brokenOpen)
            {
                try
                {
                    long size = 0;
                    if (File.Exists(localFileName))
                    {
                        using (FileStream outputStream = new FileStream(localFileName, FileMode.Open))
                        {
                            size = outputStream.Length;
                        }
                    }
                    return FtpBrokenDownload(remoteFileName, localFileName, ifCredential, size, updateProgress);
                }
                catch(Exception ex)
                {
                    return false;
                }
            }
            else
            {
                return FtpDownload(remoteFileName, localFileName, ifCredential, updateProgress);
            }
        }
        public List<string> GetFileList(bool ifCredential)
        {
            List<string> list = new List<string>();
            try
            { 
                if (string.IsNullOrEmpty(FtpServerIP))
                {
                    throw new Exception("ftp下载目标服务器地址未设置！");
                }
                Uri uri = new Uri("ftp://" + FtpServerIP + "/");
                var ftpreq = (FtpWebRequest)FtpWebRequest.Create(uri);
                ftpreq.UseBinary = true;
                ftpreq.KeepAlive = true;
                ftpreq.Method = WebRequestMethods.Ftp.ListDirectory;
                if (ifCredential)//使用用户身份认证  
                {
                    ftpreq.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
                }
                var response = (FtpWebResponse)ftpreq.GetResponse();
                var read = new StreamReader(response.GetResponseStream(), Encoding.UTF8);
                string line = read.ReadLine();
                while (line != null)
                {
                    list.Add(line);
                    line=read.ReadLine();
                }
                response.Dispose();
                response.Dispose();
            }
            catch(Exception ex)
            {

            }
            return list;
        }

        #endregion

        #region 上传文件到FTP服务器
        /// <summary>  
        /// 上传文件到FTP服务器  
        /// </summary>  
        /// <param name="localFullPath">本地带有完整路径的文件名</param>  
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>  
        /// <returns>是否下载成功</returns>  
        public bool FtpUploadFile(string localFullPathName, Action<int, int> updateProgress = null)
        {
            FtpWebRequest reqFTP;
            Stream stream = null;
            FtpWebResponse response = null;
            FileStream fs = null;
            try
            {
                FileInfo finfo = new FileInfo(localFullPathName);
                if (FtpServerIP == null || FtpServerIP.Trim().Length == 0)
                {
                    throw new Exception("ftp上传目标服务器地址未设置！");
                }
                Uri uri = new Uri("ftp://" + FtpServerIP + "/" + finfo.Name);
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
                reqFTP.KeepAlive = true;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用户，密码  
                reqFTP.Method = WebRequestMethods.Ftp.UploadFile;//向服务器发出下载请求命令  
                reqFTP.ContentLength = finfo.Length;//为request指定上传文件的大小  
                response = reqFTP.GetResponse() as FtpWebResponse;
                reqFTP.ContentLength = finfo.Length;
                int buffLength = 1024;
                byte[] buff = new byte[buffLength];
                int contentLen;
                fs = finfo.OpenRead();
                stream = reqFTP.GetRequestStream();
                contentLen = fs.Read(buff, 0, buffLength);
                int allbye = (int)finfo.Length;
                //更新进度    
                if (updateProgress != null)
                {
                    updateProgress((int)allbye, 0);//更新进度条     
                }
                int startbye = 0;
                while (contentLen != 0)
                {
                    startbye = contentLen + startbye;
                    stream.Write(buff, 0, contentLen);
                    //更新进度    
                    if (updateProgress != null)
                    {
                        updateProgress((int)allbye, (int)startbye);//更新进度条     
                    }
                    contentLen = fs.Read(buff, 0, buffLength);
                }
                stream.Close();
                fs.Close();
                response.Close();
                return true;

            }
            catch (Exception)
            {
                return false;
                throw;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
                if (stream != null)
                {
                    stream.Close();
                }
                if (response != null)
                {
                    response.Close();
                }
            }
        }

        /// <summary>  
        /// 上传文件到FTP服务器(断点续传)  
        /// </summary>  
        /// <param name="localFullPath">本地文件全路径名称：C:\Users\JianKunKing\Desktop\IronPython脚本测试工具</param>  
        /// <param name="remoteFilepath">远程文件所在文件夹路径</param>  
        /// <param name="updateProgress">报告进度的处理(第一个参数：总大小，第二个参数：当前进度)</param>  
        /// <returns></returns>         
        public bool FtpUploadBroken(string localFullPath, string remoteFilepath, Action<int, int> updateProgress = null)
        {
            if (remoteFilepath == null)
            {
                remoteFilepath = "";
            }
            string newFileName = string.Empty;
            bool success = true;
            FileInfo fileInf = new FileInfo(localFullPath);
            long allbye = (long)fileInf.Length;
            if (fileInf.Name.IndexOf("#") == -1)
            {
                newFileName = RemoveSpaces(fileInf.Name);
            }
            else
            {
                newFileName = fileInf.Name.Replace("#", "＃");
                newFileName = RemoveSpaces(newFileName);
            }
            long startfilesize = GetFileSize(newFileName, remoteFilepath);
            if (startfilesize >= allbye)
            {
                return false;
            }
            long startbye = startfilesize;
            //更新进度    
            if (updateProgress != null)
            {
                updateProgress((int)allbye, (int)startfilesize);//更新进度条     
            }

            string uri;
            if (remoteFilepath.Length == 0)
            {
                uri = "ftp://" + FtpServerIP + "/" + newFileName;
            }
            else
            {
                uri = "ftp://" + FtpServerIP + "/" + remoteFilepath + "/" + newFileName;
            }
            FtpWebRequest reqFTP;
            // 根据uri创建FtpWebRequest对象   
            reqFTP = (FtpWebRequest)FtpWebRequest.Create(new Uri(uri));
            // ftp用户名和密码   
            reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);
            // 默认为true，连接不会被关闭   
            // 在一个命令之后被执行   
            reqFTP.KeepAlive = true;
            // 指定执行什么命令   
            reqFTP.Method = WebRequestMethods.Ftp.AppendFile;
            // 指定数据传输类型   
            reqFTP.UseBinary = true;
            // 上传文件时通知服务器文件的大小   
            reqFTP.ContentLength = fileInf.Length;
            int buffLength = 2048;// 缓冲大小设置为2kb   
            byte[] buff = new byte[buffLength];
            // 打开一个文件流 (System.IO.FileStream) 去读上传的文件   
            FileStream fs = fileInf.OpenRead();
            Stream strm = null;
            try
            {
                // 把上传的文件写入流   
                strm = reqFTP.GetRequestStream();
                // 每次读文件流的2kb     
                fs.Seek(startfilesize, 0);
                int contentLen = fs.Read(buff, 0, buffLength);
                // 流内容没有结束   
                while (contentLen != 0)
                {
                    // 把内容从file stream 写入 upload stream   
                    strm.Write(buff, 0, contentLen);
                    contentLen = fs.Read(buff, 0, buffLength);
                    startbye += contentLen;
                    //更新进度    
                    if (updateProgress != null)
                    {
                        updateProgress((int)allbye, (int)startbye);//更新进度条     
                    }
                }
                // 关闭两个流   
                strm.Close();
                fs.Close();
            }
            catch
            {
                success = false;
                throw;
            }
            finally
            {
                if (fs != null)
                {
                    fs.Close();
                }
                if (strm != null)
                {
                    strm.Close();
                }
            }
            return success;
        }

        /// <summary>  
        /// 去除空格  
        /// </summary>  
        /// <param name="str"></param>  
        /// <returns></returns>  
        private string RemoveSpaces(string str)
        {
            string a = "";
            CharEnumerator CEnumerator = str.GetEnumerator();
            while (CEnumerator.MoveNext())
            {
                byte[] array = new byte[1];
                array = System.Text.Encoding.ASCII.GetBytes(CEnumerator.Current.ToString());
                int asciicode = (short)(array[0]);
                if (asciicode != 32)
                {
                    a += CEnumerator.Current.ToString();
                }
            }
            string sdate = System.DateTime.Now.Year.ToString() + System.DateTime.Now.Month.ToString() + System.DateTime.Now.Day.ToString() + System.DateTime.Now.Hour.ToString()
                + System.DateTime.Now.Minute.ToString() + System.DateTime.Now.Second.ToString() + System.DateTime.Now.Millisecond.ToString();
            return a.Split('.')[a.Split('.').Length - 2] + "." + a.Split('.')[a.Split('.').Length - 1];
        }
        /// <summary>  
        /// 获取已上传文件大小  
        /// </summary>  
        /// <param name="filename">文件名称</param>  
        /// <param name="path">服务器文件路径</param>  
        /// <returns></returns>  
        public long GetFileSize(string filename, string remoteFilepath)
        {
            long filesize = 0;
            try
            {
                FtpWebRequest reqFTP;
                FileInfo fi = new FileInfo(filename);
                string uri;
                if (remoteFilepath.Length == 0)
                {
                    uri = "ftp://" + FtpServerIP + "/" + fi.Name;
                }
                else
                {
                    uri = "ftp://" + FtpServerIP + "/" + remoteFilepath + "/" + fi.Name;
                }
                reqFTP = (FtpWebRequest)FtpWebRequest.Create(uri);
                reqFTP.KeepAlive = true;
                reqFTP.UseBinary = true;
                reqFTP.Credentials = new NetworkCredential(FtpUserID, FtpPassword);//用户，密码  
                reqFTP.Method = WebRequestMethods.Ftp.GetFileSize;
                FtpWebResponse response = (FtpWebResponse)reqFTP.GetResponse();
                filesize = response.ContentLength;
                return filesize;
            }
            catch
            {
                return 0;
            }
        }
        #endregion
    }
}
