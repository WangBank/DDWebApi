using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.Common
{
    /// <summary>
    /// 通用静态类
    /// </summary>
    public static class CommonHelper
    {
        /// <summary>
        /// 写日志
        /// </summary>
        /// <param name="strname"> 文件名</param>
        /// <param name="str">日志内容</param>
        public static void TxtLog(string strname, string str)
        {
            try
            {
                string fname;
                FileInfo finfo;
                string filePath = AppDomain.CurrentDomain.BaseDirectory + "\\LogFile";
                if (Directory.Exists(filePath) == false)
                {
                    Directory.CreateDirectory(filePath);
                }
                fname = filePath + "\\" + strname + "_" + DateTime.Now.ToString("yyyyMMdd") + ".txt";
                finfo = new FileInfo(fname);
                if (!finfo.Exists)
                {
                    FileStream fs;
                    fs = File.Create(fname);
                    fs.Close();
                    finfo = new FileInfo(fname);
                }
                using (FileStream fs = finfo.OpenWrite())
                {
                    StreamWriter w = new StreamWriter(fs);
                    w.BaseStream.Seek(0, SeekOrigin.End);
                    w.Write("{0}  {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), str);
                    w.Flush();
                    w.Close();
                }
                return;
            }
            catch (Exception)
            {
                
                return;
            }
        }

        internal static string GetUsedTime(DateTime date1,DateTime date2)
        {
            TimeSpan ts = new TimeSpan();
            ts = date1 - date2;
            return $"{ts.Days}天{ts.Hours}小时{ts.Minutes}分{ts.Seconds}秒";
        }

        internal static string GetUsedTime(TimeSpan ts)
        {
            return $"{ts.Days}天{ts.Hours}小时{ts.Minutes}分{ts.Seconds}秒";
        }
    }
}
