using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace G3WebERPApi
{
    /// <summary>
    /// MD5Encrypt 的摘要说明
    /// RSA加密
    /// </summary>
    public class MD5Encrypt : IHttpHandler
    {
        //RSA加密
        private CspParameters param;

        private string token = "";//秘钥
        private string CsJson = "";//获取请求json
        private string CsJsonJm = "";
        private string FhJson = "";//返回json

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
                //获取的口令
                token = context.Request.Headers.Get("ID").ToString();
            }
            catch (Exception e)
            {
                context.Response.Write("{\"errmsg\":\"未认证，未携带认证信息(DD0002)\",\"errcode\":1}");
                return;
            }

            try
            {
                #region 报文RAS加密

                using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
                {
                    CsJson = reader.ReadToEnd();
                }

                if (CsJson == "")
                {
                    context.Response.Write("{\"errmsg\":\"报文不允许为空(DD0004)\",\"errcode\":1}");
                    return;
                }

                param = new CspParameters();
                param.KeyContainerName = "Romens" + token;//密匙容器的名称，保持加密解密一致才能解密成功
                using (RSACryptoServiceProvider rsa = new RSACryptoServiceProvider(param))
                {
                    byte[] plaindata = Encoding.Default.GetBytes(CsJson);//将要加密的字符串转换为字节数组
                    byte[] encryptdata = rsa.Encrypt(plaindata, false);//将加密后的字节数据转换为新的加密字节数组
                    CsJsonJm = Convert.ToBase64String(encryptdata);//将加密后的字节数组转换为字符串
                    FhJson = "{\"errmsg\":\"ok\",\"errcode\":0,\"json\":\"" + CsJsonJm + "\"}";
                }

                context.Response.Write(FhJson);
                return;

                #endregion 报文RAS加密
            }
            catch (Exception e)
            {
                context.Response.Write("{\"errmsg\":\"报文格式错误(DD0004)\",\"errcode\":1}");
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
    }
}