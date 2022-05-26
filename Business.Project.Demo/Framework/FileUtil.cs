
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Business.Project.Demo.Framework
{
    public class FileUtil
    {
        static IdentifyEncoding enc = new IdentifyEncoding();

        /// <summary>
        ///获取文本内容
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetFileContentDemo(string path)
        {
            path = @"D:\Heng_Sheng_DianZi\etc\20220520.txt";
            string txtContent = ReadTxtOrCsv(path);
            return txtContent;
        }

        #region 读取文本内容
        /// <summary>
        /// 读取Txt或Csv格式文件
        /// </summary>
        /// <param name="fullName">文件目录及路径</param>
        /// <param name="encode">字符编码</param>
        /// <returns>返回文件内容</returns>
        public static string ReadTxtOrCsv(string fileName, string encode = "")
        {
            Encoding encoding = Encoding.GetEncoding("UTF-8");
            try
            {
                if (!string.IsNullOrEmpty(encode))
                {
                    encoding = Encoding.GetEncoding(encode);
                }
                else
                {
                    encoding = enc.GetEncoding(new Uri(fileName));
                }
            }
            catch (Exception e)
            {
                //WriteLog("GetEncoding：" + e.Message + e.StackTrace);
            }
            return ReadTxtOrCsv(fileName, encoding);
        }

        public static string ReadTxtOrCsv(string fileName, Encoding coding)
        {
            try
            {
                using (var file = new FileStream(fileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (var reader = new StreamReader(file, coding))
                    {
                        return reader.ReadToEnd();
                    }
                }
            }
            catch (Exception e)
            {
                //string info = CheckFileUsed(fileName);
                //WriteLog("ReadTxtOrCsv：" + e.Message + e.StackTrace + info);
            }
            return "";
        }
        #endregion

        #region 写日志
        static readonly object _objLog = new object();
        static string path = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.FullName;
        /// <summary>
        /// 得到当前启动系统的上一级目录
        /// </summary>
        public static string AppParentPath
        {
            get { return path; }
        }

        public static void WriteLog(string msg)
        {
            try
            {
                lock (_objLog)
                {
                    string log = AppParentPath + @"\log\" + DateTime.Now.ToString("yyyy-MM-dd") + "_server.log";
                    msg = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss:ffff")
                        + "[进程ID:" + Process.GetCurrentProcess().Id + "][线程ID:" + Thread.CurrentThread.ManagedThreadId + "] " + msg;

                    if (File.Exists(log))
                    {
                        FileInfo fi = new FileInfo(log);
                        if ((fi.Length / 1024.0) > 800)
                        {
                            File.Copy(log, GetNewPathForDupes(log));
                            DeleteFile(log);
                        }
                    }

                    WriteTxtFile(log, msg, false, true);
                }
            }
            catch { }
        }

        public static bool DeleteFile(string fileName)
        {
            try
            {
                if (File.Exists(fileName))
                {
                    File.Delete(fileName);
                }
                return false;

            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                //throw;
            }
            return true;
        }

        private static string GetNewPathForDupes(string path)
        {
            string directory = Path.GetDirectoryName(path);
            string filename = Path.GetFileNameWithoutExtension(path);
            string extension = Path.GetExtension(path);
            int counter = 1;
            string newFullPath;
            do
            {
                string newFilename = string.Format("{0}({1}).{2}", filename, counter, extension);
                //string newFilename = "{0}({1}).{2}".Format(filename, counter, extension);
                newFullPath = Path.Combine(directory, newFilename);
                counter++;
            } while (File.Exists(newFullPath));

            return newFullPath;
        }


        /// <summary>
        /// 写文件
        /// </summary>
        /// <param name="fullName">写的文件路径、文件名</param>
        /// <param name="s">写入的文件信息</param>
        /// <param name="writeSattus">如写入的文件已存在，true重写 false往文件中增加内容信息</param>
        public static bool WriteTxtFile(string fileName, string content, bool writeSattus, bool isWriteLog = true)
        {
            return WriteText(fileName, content, !writeSattus, Encoding.GetEncoding("UTF-8"), isWriteLog);
        }

        public static bool WriteText(string fileName, string content, bool writeSattus, Encoding encoding, bool isWriteLog = true)
        {
            if (!CreateDir(fileName))
                return false;

            try
            {
                if (encoding == null)
                {
                    encoding = Encoding.UTF8;
                }
                StreamWriter sr = new StreamWriter(fileName, writeSattus, encoding);
                sr.WriteLine(content);
                sr.Close();
                return true;
            }
            catch (Exception ex)
            {
                
            }
            return false;
        }

        public static bool CreateDir(string fullName)
        {
            string msg = "";
            return CreateDir(fullName, ref msg);
        }

        /// <summary>
        /// 传入的参数可以是文件名的全称
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        public static bool CreateDir(string fullName, ref string msg)
        {
            if (string.IsNullOrEmpty(fullName))
                return false;

            string path = "";
            try
            {
                if (fullName.Contains("\\"))
                {
                    path = fullName.Substring(0, fullName.LastIndexOf("\\"));
                }
                else if (fullName.Contains("/"))
                {
                    path = fullName.Substring(0, fullName.LastIndexOf("/"));
                }
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
                return true;
            }
            catch (Exception ex)
            {
                msg = ex.Message + ex.StackTrace;
                WriteLog("创建目录失败：" + msg);
            }
            return false;
        }


        #endregion


    }
}
