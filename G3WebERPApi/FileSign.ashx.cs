using COSXML;
using COSXML.Auth;
using COSXML.Model.Tag;
using System;
using System.Collections;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi
{
    /// <summary>
    /// 文件上传下载Url(腾讯接口)
    /// </summary>
    public class FileSign : IHttpHandler
    {
        private string url = "";
        private string nonceStr = ToolsClass.GetConfig("RomensOA"); //必填，生成签名的随机串
        private string agentId = "";// 必填，微应用ID
        private string timeStamp = "";
        private string corpId = "";//必填，企业ID
        private string sign = "";
        private string FhJson = "";
        private string CsJson = "";
        private string ticket = "";
        private int errcode = 1;
        private string isWrite = "0";
        private string AppTxy = "";//腾讯云参数集
        private string[] ScList;//参数集
        private string AppID = "";//AppID
        private string AppTong = "";//存储桶地域
        private string SecretId = "";//
        private string SecretKey = "";//
        private string Token = "";//临时Token
        private string Cct = "";//存储桶
        private string Dxj = "";//对象键

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }

            try
            {
                //获取请求json
                using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                {
                    CsJson = reader.ReadToEnd();
                }

                if (CsJson == "")
                {
                    context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                    return;
                }
                CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
                //json转Hashtable
                Object jgobj = ToolsClass.DeserializeObject(CsJson);
                Hashtable returnhash = jgobj as Hashtable;
                if (returnhash == null)
                {
                    context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                    return;
                }
                string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
                string path = context.Request.Path.Replace("FileSign.ashx", "filesign");
                //验证请求sign
                string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
                ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + returnhash["Sign"].ToString() + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
                if (sign != returnhash["Sign"].ToString())
                {
                    context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                    return;
                }

                //#AppID #存储桶地域 #SecretId #SecretKey #Token #存储桶 #对象键
                AppTxy = ToolsClass.GetConfig("AppTxy");
                ScList = AppTxy.Split('$');
                AppID = ScList[0].ToString();//AppID
                AppTong = ScList[1].ToString();//存储桶地域
                SecretId = ScList[2].ToString();//SecretId
                SecretKey = ScList[3].ToString();//SecretKey
                //Token = ScList[4].ToString();//临时Token
                Cct = ScList[5].ToString();//存储桶
                Dxj = returnhash["fileFullName"].ToString();//对象键
                isWrite = ToolsClass.GetConfig("isWrite");
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("获取腾讯云上传及下载地址日志", "\r\n入参:" + CsJson + "\r\n");
                }

                if (returnhash["isUp"].ToString() == "0")
                {
                    url = DownGetUrl();
                }
                else if (returnhash["isUp"].ToString() == "1")
                {
                    url = UpGetUrl();
                }
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("获取腾讯云上传及下载地址日志", "\r\n返回Url:" + url + "\r\n");
                }
                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0,\"url\":\"" + url + "\"}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                return;
            }
        }

        public bool IsReusable
        {
            get
            {
                return false;
            }
        }

        //获取时间拽
        public static int ConvertDateTimeInt(System.DateTime time)
        {
            System.DateTime startTime = TimeZone.CurrentTimeZone.ToLocalTime(new System.DateTime(1970, 1, 1));
            return (int)(time - startTime).TotalSeconds + 600;
        }

        //临时密钥预签名请求示例-上传
        public string UpGetUrl()
        {
            try
            {
                //使用永久密钥初始化 CosXml
                CosXmlConfig config = new CosXmlConfig.Builder()
                .SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位 毫秒 ，默认 45000ms
                .SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位 毫秒 ，默认 45000ms
                .IsHttps(true)  //设置默认 https 请求
                .SetAppid(AppID)  //设置腾讯云账户的账户标识 APPID
                .SetRegion(AppTong)  //设置一个默认的存储桶地域
                .SetDebugLog(true)  //显示日志
                .Build();  //创建 CosXmlConfig 对象
                string secretId = SecretId; //"云 API 密钥 SecretId";
                string secretKey = SecretKey; //"云 API 密钥 SecretKey";
                long durationSecond = 600;  //secretKey 有效时长,单位为 秒
                QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
                CosXmlServer cosXml = new CosXmlServer(config, cosCredentialProvider);

                PreSignatureStruct preSignatureStruct = new PreSignatureStruct();
                preSignatureStruct.appid = AppID;//腾讯云账号 appid
                preSignatureStruct.region = AppTong; //存储桶地域
                preSignatureStruct.bucket = Cct; //存储桶
                preSignatureStruct.key = Dxj; //对象键
                preSignatureStruct.httpMethod = "PUT"; //http 请求方法
                preSignatureStruct.isHttps = true; //生成 https 请求URL
                preSignatureStruct.signDurationSecond = 600; //请求签名时间为 600s
                preSignatureStruct.headers = null;//签名中需要校验的header
                preSignatureStruct.queryParameters = null; //签名中需要校验的URL中请求参数
                return cosXml.GenerateSignURL(preSignatureStruct); //上传预签名 URL (使用永久密钥方式计算的签名 URL )

                #region 临时密钥

                ////使用临时密钥初始化 CosXml
                //CosXmlConfig config = new CosXmlConfig.Builder()
                //.SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位毫秒 ，默认45000ms
                //.SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位毫秒 ，默认45000ms
                //.IsHttps(true)  //设置默认 https 请求
                //.SetAppid(AppID)  //设置腾讯云账户的账户标识 APPID
                //.SetRegion(AppTong)  //设置一个默认的存储桶地域
                //.SetDebugLog(true)  //显示日志
                //.Build();  //创建 CosXmlConfig 对象
                //string tmpSecretId = "romensid"; //"临时密钥 SecretId";
                //string tmpSecretKey = "romenskey"; //"临时密钥 SecretKey";
                //string tmpToken = "romemstoken"; //"临时密钥 token";
                //long tmpExpireTime = ConvertDateTimeInt(DateTime.Now.AddDays(1));//临时密钥有效截止时间
                //QCloudCredentialProvider cosCredentialProvider = new DefaultSessionQCloudCredentialProvider(tmpSecretId, tmpSecretKey, tmpExpireTime, tmpToken);
                //CosXmlServer cosXml = new CosXmlServer(config, cosCredentialProvider);

                //PreSignatureStruct preSignatureStruct = new PreSignatureStruct();
                //preSignatureStruct.appid = AppID;//腾讯云账号 appid
                //preSignatureStruct.region = AppTong; //存储桶地域
                //preSignatureStruct.bucket = Cct; //存储桶
                //preSignatureStruct.key = Dxj; //对象键
                //preSignatureStruct.httpMethod = "PUT"; //http 请求方法
                //preSignatureStruct.isHttps = true; //生成 https 请求URL
                //preSignatureStruct.signDurationSecond = 600; //请求签名时间为 600s
                //preSignatureStruct.headers = null;//签名中需要校验的header
                //preSignatureStruct.queryParameters = null; //签名中需要校验的URL中请求参数

                ////上传预签名 URL (使用临时密钥方式计算的签名 URL )

                //return cosXml.GenerateSignURL(preSignatureStruct);

                #endregion 临时密钥
            }
            catch (COSXML.CosException.CosClientException clientEx)
            {
                //请求失败
                return ("CosClientException: " + clientEx.Message);
            }
            catch (COSXML.CosException.CosServerException serverEx)
            {
                //请求失败
                return ("CosServerException: " + serverEx.GetInfo());
            }
        }

        //临时密钥预签名请求示例-下载
        public string DownGetUrl()
        {
            try
            {
                CosXmlConfig config = new CosXmlConfig.Builder()
    .SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位 毫秒 ，默认 45000ms
    .SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位 毫秒 ，默认 45000ms
    .IsHttps(true)  //设置默认 https 请求
    .SetAppid(AppID)  //设置腾讯云账户的账户标识 APPID
    .SetRegion(AppTong)  //设置一个默认的存储桶地域
    .SetDebugLog(true)  //显示日志
    .Build();  //创建 CosXmlConfig 对象
                string secretId = SecretId; //"云 API 密钥 SecretId";
                string secretKey = SecretKey; //"云 API 密钥 SecretKey";
                long durationSecond = 600;  //secretKey 有效时长,单位为 秒
                QCloudCredentialProvider cosCredentialProvider = new DefaultQCloudCredentialProvider(secretId, secretKey, durationSecond);
                CosXmlServer cosXml = new CosXmlServer(config, cosCredentialProvider);

                PreSignatureStruct preSignatureStruct = new PreSignatureStruct();
                preSignatureStruct.appid = AppID;//腾讯云账号 appid
                preSignatureStruct.region = AppTong; //存储桶地域
                preSignatureStruct.bucket = Cct; //存储桶
                preSignatureStruct.key = Dxj; //对象键
                preSignatureStruct.httpMethod = "GET"; //http 请求方法
                preSignatureStruct.isHttps = true; //生成 https 请求URL
                preSignatureStruct.signDurationSecond = 600; //请求签名时间为 600s
                preSignatureStruct.headers = null;//签名中需要校验的header
                preSignatureStruct.queryParameters = null; //签名中需要校验的URL中请求参数

                return cosXml.GenerateSignURL(preSignatureStruct); //载请求预签名 URL (使用永久密钥方式计算的签名 URL )

                #region 临时密钥下载

                ////使用临时密钥初始化 CosXml
                //CosXmlConfig config = new CosXmlConfig.Builder()
                //.SetConnectionTimeoutMs(60000)  //设置连接超时时间，单位 毫秒 ，默认 45000ms
                //.SetReadWriteTimeoutMs(40000)  //设置读写超时时间，单位 毫秒 ，默认 45000ms
                //.IsHttps(true)  //设置默认 https 请求
                //.SetAppid(AppID)  //设置腾讯云账户的账户标识 APPID
                //.SetRegion(AppTong)  //设置一个默认的存储桶地域
                //.SetDebugLog(true)  //显示日志
                //.Build();  //创建 CosXmlConfig 对象
                //string tmpSecretId = SecretId; //"临时密钥 SecretId";
                //string tmpSecretKey = SecretKey; //"临时密钥 SecretKey";
                //string tmpToken = Token; //"临时密钥 token";
                //long tmpExpireTime = ConvertDateTimeInt(DateTime.Now);//临时密钥有效截止时间
                //QCloudCredentialProvider cosCredentialProvider = new DefaultSessionQCloudCredentialProvider(tmpSecretId, tmpSecretKey, tmpExpireTime, tmpToken);
                //CosXmlServer cosXml = new CosXmlServer(config, cosCredentialProvider);

                //PreSignatureStruct preSignatureStruct = new PreSignatureStruct();
                //preSignatureStruct.appid = AppID;//腾讯云账号 appid
                //preSignatureStruct.region = AppTong; //存储桶地域
                //preSignatureStruct.bucket = Cct; //存储桶
                //preSignatureStruct.key = Dxj; //对象键
                //preSignatureStruct.httpMethod = "GET"; //http 请求方法
                //preSignatureStruct.isHttps = true; //生成 https 请求URL
                //preSignatureStruct.signDurationSecond = 600; //请求签名时间为 600s
                //preSignatureStruct.headers = null;//签名中需要校验的header
                //preSignatureStruct.queryParameters = null; //签名中需要校验的URL中请求参数
                //return cosXml.GenerateSignURL(preSignatureStruct); //载请求预签名 URL (使用临时密钥方式计算的签名 URL )

                #endregion 临时密钥下载
            }
            catch (Exception)
            {
                return "";
            }
        }
    }
}