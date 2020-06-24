using Newtonsoft.Json;
using System;
using System.Collections;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi
{
    /// <summary>
    /// Sign 的摘要说明
    /// </summary>
    ///
    public class Sign : IHttpHandler
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
        private string appKey = "";
        private string appSecret = "";
        private string access_token = "";
        private int errcode = 1;
        private string isWrite = "0";
        private string AppWyy = "";//钉钉微应用参数集
        private string[] ScList;//参数集

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
                string path = context.Request.Path.Replace("Sign.ashx", "getsign");
                //验证请求sign
                string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
                ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + returnhash["Sign"].ToString() + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
                if (sign != returnhash["Sign"].ToString())
                {
                    context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                    return;
                }

                //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
                AppWyy = ToolsClass.GetConfig("AppWyy");
                ScList = AppWyy.Split('$');
                agentId = ScList[0].ToString();
                corpId = ScList[1].ToString();
                appKey = ScList[2].ToString();
                appSecret = ScList[3].ToString();

                isWrite = ToolsClass.GetConfig("isWrite");

                //获取access_token
                url = "https://oapi.dingtalk.com/gettoken?appkey=" + appKey + "&appsecret=" + appSecret;
                FhJson = ToolsClass.ApiFun("GET", url, "");

                TokenClass tokenClass = new TokenClass();
                tokenClass = (TokenClass)JsonConvert.DeserializeObject(FhJson, typeof(TokenClass));
                access_token = tokenClass.access_token;
                errcode = tokenClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"获取ACCESS_TOKEN报错(DD0004)\",\"errcode\":1}");
                    return;
                }

                //JSAPI鉴权
                url = returnhash["url"].ToString();
                FhJson = ToolsClass.ApiFun("GET", "https://oapi.dingtalk.com/get_jsapi_ticket?access_token=" + access_token, "");

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nSign=>入参:" + CsJson + "\r\nappKey:" + appKey + "\r\nappsecret:" + appSecret + "\r\naccess_token:" + access_token + "\r\nJSAPI鉴权Fh:" + FhJson);
                }

                SignClass signClass = new SignClass();
                signClass = (SignClass)JsonConvert.DeserializeObject(FhJson, typeof(SignClass));
                ticket = signClass.ticket;
                errcode = signClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"JSAPI鉴权报错," + signClass.errmsg + "(DD0005)\",\"errcode\":1}");
                    return;
                }

                //获取的口令
                //token = context.Request.Headers.Get("ID").ToString();
                timeStamp = GetTimeStamp();
                nonceStr = ToolsClass.GetRandomString(8);
                string assemble = string.Format("jsapi_ticket={0}&noncestr={1}&timestamp={2}&url={3}", ticket, nonceStr, timeStamp, url);
                SHA1 sha;
                ASCIIEncoding enc;
                sha = new SHA1CryptoServiceProvider();
                enc = new ASCIIEncoding();
                byte[] dataToHash = enc.GetBytes(assemble);
                byte[] dataHashed = sha.ComputeHash(dataToHash);
                sign = BitConverter.ToString(dataHashed).Replace("-", "");
                sign = sign.ToLower();

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\n签名串:" + assemble + "\r\nSign:" + sign + "\r\n");
                }
                string fh = "{\"errmsg\":\"ok\",\"errcode\":0,\"url\":\"" + url + "\",\"nonceStr\":\"" + nonceStr + "\",\"agentId\":\"" + agentId + "\",\"timeStamp\":\"" + timeStamp + "\",\"corpId\":\"" + corpId + "\",\"signature\":\"" + sign + "\"}";
                ToolsClass.TxtLog("DDLog", "\r\n返回给前端的信息:" + fh + "\r\n");
                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0,\"url\":\"" + url + "\",\"nonceStr\":\"" + nonceStr + "\",\"agentId\":\"" + agentId + "\",\"timeStamp\":\"" + timeStamp + "\",\"corpId\":\"" + corpId + "\",\"signature\":\"" + sign + "\"}");
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

        /// <summary>
        /// 获取时间戳timestamp（当前时间戳，具体值为当前时间到1970年1月1号的秒数）
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}