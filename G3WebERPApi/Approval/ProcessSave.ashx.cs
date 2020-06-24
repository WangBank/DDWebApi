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
    /// 多级流程信息保存接口，
    /// </summary>
    public class ProcessSave : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();

        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();

        private string url = string.Empty;
        private object obj;
        private string isWrite = "0";
        private string CsJson = "";//获取请求json

        private DataTable dt = new DataTable();
        private string FhJson ,ticket,appKey, appSecret,access_token, agentId, corpId, operatorName,operatorGuid= "";
        private string AppWyy = "";//钉钉微应用参数集
        private string[] ScList;//参数集
        private string ddUrl = "";//钉钉前端地址
        private int errcode = 1;
        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }
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
                ToolsClass.TxtLog("保存流程信息日志", "\r\n保存流程入参:" + CsJson + "\r\n");
            }

            PSP psp = new PSP();
            psp = (PSP)JsonConvert.DeserializeObject(CsJson, typeof(PSP));
            if (psp.Data.Length <= 0)
            {
                context.Response.Write("{\"errmsg\":\"流程不允许为空,请添加流程(DD6001)\",\"errcode\":1}");
                return;
            }

            string path = context.Request.Path.Replace("Approval/ProcessSave.ashx", "saveprocess");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + psp.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != psp.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }

            string sqlss = ",";
            try
            {
                #region 保存流程信息

                sqlList.Clear();
                sqlTou.Clear();
                for (int i = 1; i <= psp.Data.Length; i++)
                {
                    if (i > 1)
                    {
                        sqlTou.Clear();
                    }
                    sqlTou.Append("insert into ApprovalNode(nodeid,BilliClassid,BillClassName,NodeNumber,CharacterTypes,ApprovalType,persons,IsAndOr,IsEnd) values('")
                    .Append(Guid.NewGuid().ToString()).Append("','")
                    .Append(psp.BillClassId).Append("','")
                    .Append(psp.BillName).Append("','")
                    .Append(i.ToString()).Append("','")
                    .Append(psp.Data[i - 1].CharacterTypes).Append("','")
                    .Append(JsonConvert.SerializeObject(psp.Data[i - 1].ApprovalType[0])).Append("','")
                    //.Append(JsonConvert.SerializeObject(psp.Data[i - 1].Persons).Replace("[","").Replace("]", "")).Append("','")
                    .Append(JsonConvert.SerializeObject(psp.Data[i - 1].Persons)).Append("','")
                    .Append(psp.Data[i - 1].IsAndOr).Append("','")
                    .Append(psp.Data[i - 1].IsEnd).Append("')");
                    sqlss = sqlTou.ToString() + sqlss;
                    sqlList.Add(sqlTou.ToString());
                }
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("保存流程信息日志", "\r\n保存流程入参:" + sqlss + "\r\n");
                }
                //执行SQL语句Insert
                da.ExecSql($"delete ApprovalNode where BilliClassid ='{psp.BillClassId}'");
                da.ExecSql($"update BillClass set ClassRuCan = '{CsJson}' where BillClassid ='{psp.BillClassId}'");
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存流程信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                #endregion 保存流程信息

                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                context.Response.End();
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