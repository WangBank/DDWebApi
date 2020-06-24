using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Web;

namespace G3WebERPApi
{
    /// <summary>
    /// Logion 的摘要说明
    /// </summary>
    public class Login : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();
        private string sql = string.Empty;
        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private string url = string.Empty;
        private object obj;

        private string token = "";//秘钥
        private string CsJson = "";//获取请求json
        private string CsJsonJm = "";//请求json加密

        private DataTable dt = new DataTable();
        private StringBuilder FhJson = new StringBuilder();//返回JSON

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            //string Requestip = context.Request.ServerVariables["HTTP_X_FORWARDED_FOR"];
            //if (string.IsNullOrEmpty(Requestip)) { Requestip = context.Request.ServerVariables["REMOTE_ADDR"]; }
            //if (string.IsNullOrEmpty(Requestip)) { Requestip = context.Request.UserHostAddress; }
            //if (string.IsNullOrEmpty(Requestip)) { Requestip = "0.0.0.0"; }

            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            //sqlServer
            da = new DbHelper.SqlHelper("SqlServer", connectionString);

            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }
            if (CsJson == "")
            {
                context.Response.Write("{\"errmsg\":\"调用接口入参不能为空(DD0003)\",\"errcode\":1}");
                return;
            }
            ToolsClass.TxtLog("登录后台入参", "入参:" + CsJson.ToString());
            //json转Hashtable
            Object jgobj = ToolsClass.DeserializeObject(CsJson);

            Hashtable returnhash = jgobj as Hashtable;

            byte[] outputb = Convert.FromBase64String(returnhash["UserPwd"].ToString());
            string pwd1 = Encoding.Default.GetString(outputb);
            byte[] outputb2 = Convert.FromBase64String(returnhash["UserName"].ToString());
            string username = Encoding.Default.GetString(outputb2);
            string pwd = ToolsClass.md5(pwd1 + "fanfanfan", 32);
            try
            {
                sql = $"select employeecode,employeename,pwd from flowEmployee where EmployeeCode='{username}' and pwd = '{pwd}'";
                ToolsClass.TxtLog("登录后台入参", "查询语句" + sql);
                obj = da.GetDataTable(sql);
                dt = obj as DataTable;
                FhJson.Clear();
                string sign = ToolsClass.md5(username + "Romens1/DingDing2/Login3", 32);
                ToolsClass.TxtLog("登录后台入参", "测试" + sign);
                if (sign != returnhash["Sign"].ToString())
                {
                    context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                    return;
                }
                if (dt.Rows.Count > 0)
                {
                    context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                }
                else
                {
                    context.Response.Write("{\"errmsg\":\"用户名不正确或密码错误！\",\"errcode\":1}");
                    return;
                }
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
    }
}