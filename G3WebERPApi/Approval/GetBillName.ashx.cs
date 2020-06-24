using Newtonsoft.Json;

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi.Approval
{
    /// <summary>
    /// 获取流程类型
    /// </summary>
    public class GetBillName : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;

        private string url = "";
        private string nonceStr = ToolsClass.GetConfig("RomensOA"); //必填，生成签名的随机串
        private string agentId = "251741564";// 必填，微应用ID
        private string timeStamp = "";
        private string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";//必填，企业ID
        private string sign = "";
        private string FhJson = "";
        private string CsJson = "";
        private string ticket = "";
        private string appKey = "";
        private string appSecret = "";
        private string access_token = "";
        private int errcode = 1;
        private string isWrite = "0";
        private StringBuilder rJson = new StringBuilder();

        private string sql = string.Empty;
        private object obj;
        private DataTable dt = new DataTable();

        private string SearchAall = "";
        private string AppWyy = "";//钉钉微应用参数集
        private string[] ScList;//参数集
        private string ddUrl = "";//钉钉前端地址
        private string ClassType = "";

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
                string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
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

                string path = context.Request.Path.Replace("Approval/GetBillName.ashx", "billname");
                string path1 = context.Request.Path.Replace("Approval/GetBillName.ashx", "getbillclassnode");
                //验证请求sign
                string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
                string sign1 = ToolsClass.md5(signUrl + path1 + "Romens1/DingDing2" + path1, 32);
                ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + returnhash["Sign"].ToString() + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
                if (sign != returnhash["Sign"].ToString() && sign1 != returnhash["Sign"].ToString())
                {
                    context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                    return;
                }

                if (returnhash.Contains("SearchAall"))
                {
                    SearchAall = returnhash["SearchAall"].ToString();
                    if (SearchAall == "")
                    {
                        context.Response.Write("{\"errmsg\":\"SearchAall不允许为空(DD2001)\",\"errcode\":1}");
                        return;
                    }
                }

                //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
                AppWyy = ToolsClass.GetConfig("AppWyy");
                ScList = AppWyy.Split('$');
                agentId = ScList[0].ToString();
                corpId = ScList[1].ToString();
                appKey = ScList[2].ToString();
                appSecret = ScList[3].ToString();
                isWrite = ToolsClass.GetConfig("isWrite");
                ddUrl = ToolsClass.GetConfig("ddUrl");

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("获取审批类型日志", "\r\n获取审批类型信息入参:" + CsJson.ToString() + "\r\n");
                }

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
                //获取流程类型
                if (SearchAall == "1")
                {
                    sql = "select BillClassid,BillName,BillLogoUrl,BillUrl,VisibleRange from BillClass";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    rJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"count\":").Append(dt.Rows.Count).Append(",\"data\":[");
                    if (dt.Rows.Count > 0)
                    {
                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                rJson.Append(",");
                            }
                            rJson.Append("{");
                            for (int y = 0; y < dt.Columns.Count; y++)
                            {
                                if (y > 0)
                                {
                                    rJson.Append(",");
                                }
                                rJson.Append("\"").Append(dt.Columns[y].ColumnName).Append("\":\"").Append(dt.Rows[x][dt.Columns[y].ColumnName].ToString()).Append("\"");
                            }
                            rJson.Append("}");
                        }
                    }
                    rJson.Append("]}");
                }

                //获取现在已保存的流程信息节点
                if (SearchAall == "2")
                {
                    ClassType = returnhash["ClassType"].ToString();
                    if (ClassType == "")
                    {
                        context.Response.Write("{\"errmsg\":\"ClassType不允许为空(DD2001)\",\"errcode\":1}");
                        return;
                    }
                    sql = "";
                    sql = $"select ClassRuCan from BillClass where BillClassid ='{ClassType}' ";
                    obj = da.GetValue(sql).ToString();
                    rJson.Append(obj);
                }
                context.Response.Write(rJson.Replace(" ", ""));
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("获取审批类型日志", "\r\n获取审批类型信息出参:" + rJson.ToString() + "\r\n");
                }
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
    }
}