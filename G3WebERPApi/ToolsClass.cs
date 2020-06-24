using Newtonsoft.Json;

using System;
using System.Collections;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Xml;

namespace G3WebERPApi
{
    public class ToolsClass
    {
        [DllImport("kernel32")] // 读取配置文件的接口
        private static extern int GetPrivateProfileString(string section, string key, string def, StringBuilder retVal, int size, string filePath);

        // 声明INI文件的写操作函数
        [DllImport("kernel32")]
        private static extern long WritePrivateProfileString(string section, string key, string val, string filePath);

        //反序列化 返回string
        public static Object DeserializeObject<T>(string str)
        {
            try
            {
                return (Object)JsonConvert.DeserializeObject<T>(str);
            }
            catch (Exception)
            {
                //FileOperation.write_txt("ErrLog.txt", "反序列化：" + ex.Message);
                return null;
            }
        }

        //反序列化 返回hashtable
        public static Object DeserializeObject(string str)
        {
            try
            {
                return (Object)JsonConvert.DeserializeObject(str, typeof(Hashtable));
            }
            catch (Exception)
            {
                //FileOperation.write_txt("ErrLog.txt", "反序列化：" + ex.Message);
                return null;
            }
        }

        // XML写操作
        public static void xmlSetvalue(string AppKey, string Appvalue)
        {
            XmlDocument xDoc = new XmlDocument();
            string strExecutablePath = AppDomain.CurrentDomain.BaseDirectory + "profileInfo";//保存在\bin\Debug中的profileInfo.xml中

            // 获取可执行文件的路径和名称
            xDoc.Load(strExecutablePath + ".xml");
            XmlNode xNode;
            XmlElement xElem1;
            xNode = xDoc.SelectSingleNode("//appSettings");
            xElem1 = (XmlElement)xNode.SelectSingleNode("//add[@key='" + AppKey + "']");
            if (xElem1 != null) xElem1.SetAttribute("value", Appvalue);
            xDoc.Save(strExecutablePath + ".xml");
        }

        // ini读操作
        public static string GetConfig(string key)
        {
            try
            {
                return System.Configuration.ConfigurationManager.AppSettings[key].ToString();
            }
            catch (Exception)
            {
                return "";
            }
        }

        // ini写操作
        public static void ProfileWritevalue(string section, string key, string value)
        {
            try
            {
                string file = "";

                file = "SetFile.ini";

                //配置文件位置
                string path = AppDomain.CurrentDomain.BaseDirectory + file;
                StringBuilder sb = new StringBuilder(255);
                WritePrivateProfileString(section, key, value, path);
                return;
            }
            catch (Exception)
            {
                return;
            }
        }

        // 日志保存
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
                if (!finfo.Exists)//判断日志文件不存在时
                {
                    FileStream fs;
                    fs = File.Create(fname);
                    fs.Close();
                    finfo = new FileInfo(fname);
                }

                //写入日志内容
                using (FileStream fs = finfo.OpenWrite())
                {
                    StreamWriter w = new StreamWriter(fs);
                    w.BaseStream.Seek(0, SeekOrigin.End);
                    //w.Write("\nLog Entry : ");
                    w.Write("{0}  {1}\r\n", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), str);
                    //w.Write(str + "\r\n ");
                    //w.Write("--------------------------------\r\n");
                    w.Flush();
                    w.Close();
                }
                return;
            }
            catch (Exception)
            {
                //MessageBox.Show("日志保存失败！" + ex.ToString());
                return;
            }
        }

        //md5加密
        public static string GetMD5(string str)
        {
            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();
            byte[] bytvalue, bytHash;
            bytvalue = System.Text.Encoding.UTF8.GetBytes(str);
            bytHash = md5.ComputeHash(bytvalue);
            md5.Clear();
            string sTemp = "";
            for (int i = 0; i < bytHash.Length; i++)
            {
                sTemp += bytHash[i].ToString("x").PadLeft(2, '0');
            }
            return sTemp;
        }

        public static string httpPostFun(string strURL, string path, string paramsStr)
        {
            WebClientUtils w = new WebClientUtils(30 * 1000);
            byte[] postData;
            byte[] byRemoteInfo;
            string rtn = "";
            try
            {
                w.Headers.Add("Content-Type", "application/json");
                postData = Encoding.UTF8.GetBytes(paramsStr);
                byRemoteInfo = w.UploadData(strURL, "POST", postData);
                rtn = System.Text.Encoding.UTF8.GetString(byRemoteInfo);
                return rtn;
            }
            catch (Exception ex)
            {
                TxtLog("Log", "接口请求失败:" + ex.ToString());
                return "";
            }
        }

        #region "MD5加密"

        public static string md5(string str, int code)
        {
            string strEncrypt = string.Empty;
            if (code == 16)
            {
                strEncrypt = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5").Substring(8, 16);
            }

            if (code == 32)
            {
                strEncrypt = System.Web.Security.FormsAuthentication.HashPasswordForStoringInConfigFile(str, "MD5");
            }

            return strEncrypt.ToUpper();
        }

        #endregion "MD5加密"

        //RSA解密
        public static string MD5Decrypt(string Token, string json)
        {
            try
            {
                CspParameters param = new CspParameters();
                param.KeyContainerName = "Romens" + Token;
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
                {
                    byte[] encryptdata = Convert.FromBase64String(json);
                    byte[] decryptdata = rsa.Decrypt(encryptdata, false);
                    return Encoding.Default.GetString(decryptdata);
                }
            }
            catch (Exception)
            {
                return "";
            }
        }

        //钉钉Api请求
        public static string ApiFun(string Type, string strURL, string paramsStr)
        {
            try
            {
                if (Type == "GET")
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(strURL);

                    //GET请求
                    request.Method = Type;
                    request.ReadWriteTimeout = 10000;
                    request.ContentType = "application/json";
                    //request.Headers.Add("ID", "10227");
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    Stream myResponseStream = response.GetResponseStream();
                    StreamReader myStreamReader = new StreamReader(myResponseStream, Encoding.GetEncoding("utf-8"));

                    //返回内容
                    string retString = myStreamReader.ReadToEnd();
                    return retString;
                }
                else
                {
                    WebClientUtils w = new WebClientUtils(30 * 1000);
                    byte[] postData;
                    byte[] byRemoteInfo;
                    string rtn = "";
                    w.Headers.Add("Content-Type", "application/json");
                    //w.Headers.Add("ID", "10227");
                    postData = Encoding.UTF8.GetBytes(paramsStr);
                    byRemoteInfo = w.UploadData(strURL, "POST", postData);
                    rtn = System.Text.Encoding.UTF8.GetString(byRemoteInfo);
                    return rtn;
                }
            }
            catch (Exception ex)
            {
                TxtLog("Log", "接口请求失败:" + ex.ToString());
                return "";
            }
        }

        //生成随机字符串
        public static string GetRandomString(int length)
        {
            byte[] b = new byte[4];
            new System.Security.Cryptography.RNGCryptoServiceProvider().GetBytes(b);
            Random r = new Random(BitConverter.ToInt32(b, 0));
            string s = null, str = "0123456789abcdefghijklmnopqrstuvwxyz";
            for (int i = 0; i < length; i++)
            {
                s += str.Substring(r.Next(0, str.Length - 1), 1);
            }
            return s;
        }
    }

    internal class WebClientUtils : WebClient
    {
        public int Timeout { get; set; }

        public WebClientUtils(int timeout)
        {
            Timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.Timeout = Timeout;
            request.ReadWriteTimeout = Timeout;
            return request;
        }
    }
}