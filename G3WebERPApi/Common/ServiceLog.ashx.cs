using Newtonsoft.Json;

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi.Common
{
    /// <summary>
    /// 钉钉服务日志功能
    /// </summary>
    public class ServiceLog : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();

        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();

        private string url = string.Empty;
        private object obj;
        private string isWrite = "0";
        private string token = "";//秘钥
        private string CsJson = "";//获取请求json
        private string billno = "";//单据编号
        private StringBuilder csjsjsjs = new StringBuilder();
        private DataTable dt = new DataTable();
        private string FhJson = "";//返回JSON

        private string ticket = "";
        private string appKey = "";
        private string appSecret = "";
        private string agentId = "";// 必填，微应用ID
        private string corpId = "";//必填，企业ID
        private int errcode = 1;
        private string operatorName = "";

        private string Sql = "";
        private string[] ScList;//参数集
        private string ddUrl = "";//钉钉前端地址

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }

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
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
            //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
            string AppWyy = ToolsClass.GetConfig("AppWyy");
            ScList = AppWyy.Split('$');
            agentId = ScList[0].ToString();
            corpId = ScList[1].ToString();
            appKey = ScList[2].ToString();
            appSecret = ScList[3].ToString();

            isWrite = ToolsClass.GetConfig("isWrite");
            ddUrl = ToolsClass.GetConfig("ddUrl");

            if (isWrite == "1")
            {
                ToolsClass.TxtLog("服务日志记录", "\r\n服务日志记录:" + CsJson + "\r\n");
            }

            SL sl = new SL();
            sl = (SL)JsonConvert.DeserializeObject(CsJson, typeof(SL));
            string path = context.Request.Path.Replace("Common/ServiceLog.ashx", "servicelog");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + sl.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != sl.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }
            try
            {
                //根据用户工号获取guid
                string userGuid = da.GetValue($"select guid from employee where employeecode ='{sl.JobNumber}'").ToString();

                //保存日志信息
                if (sl.Type == "Save")
                {
                    //获取单号
                    Sql = "select dbo.GetBillNo('100525010010','" + sl.JobNumber + "',getdate())";
                    obj = da.GetValue(Sql);
                    billno = obj.ToString();

                    if (billno == "1")
                    {
                        billno = "FW" + sl.JobNumber + DateTime.Now.ToString("yyyyMMdd") + "0001";

                        Sql = "update BillNumber set MaxNum=1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='100525010010' and BillDate<>convert(varchar(20),GETDATE(),120)";
                    }
                    else
                    {
                        Sql = "update BillNumber set MaxNum=MaxNum+1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='100525010010'";
                    }
                    obj = da.ExecSql(Sql);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"更新单号信息出错(DD6006)\",\"errcode\":1}");
                        return;
                    }
                    //获取当前操作人的部门信息相对应的guid
                    string sqlss = ",";

                    #region 保存日志信息

                    sqlList.Clear();
                    sqlTou.Clear();
                    sqlTou.Append("insert into CUSTSERVLOG(GUID,BillDate,CustCode,BillNo,ProbType,HourMuch,ProbDesc,UnSolvCaus,OperatorGUID,ProcState,SolutionDesc,IMPLPERS,SERVDUEDATE,PicUrls) values('")
                        .Append(Guid.NewGuid().ToString()).Append("','")
                        .Append(sl.BillDate).Append("','")
                        .Append(sl.CustCode).Append("','")
                        .Append(billno).Append("','")
                        .Append(sl.ProbType).Append("','")
                        .Append(sl.HourMuch).Append("','")
                        .Append(sl.ProbDesc).Append("','")
                        .Append(sl.UnSolvCaus).Append("','")
                        .Append(userGuid).Append("','")
                        .Append(sl.ProState).Append("','")
                        .Append(sl.SolutionDesc).Append("','")
                        .Append(sl.SJobNumber).Append("','")
                        .Append(sl.ServDueDate).Append("','")
                        .Append(JsonConvert.SerializeObject(sl.Urls))
                        .Append("')");
                    sqlss = sqlTou.ToString() + sqlss;
                    sqlList.Add(sqlTou.ToString());
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("服务日志记录", "\r\n保存日志信息中执行的sql:" + sqlss + "\r\n");
                    }
                    //执行SQL语句Insert
                    obj = da.ExecSql(sqlList);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"保存服务日志记录出错(DD6002)\",\"errcode\":1}");
                        return;
                    }

                    #endregion 保存日志信息

                    context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                    return;
                }

                //确定客户的时候，查询相应的信息
                if (sl.Type == "SelectSSFW")
                {
                    DataTable ssfw = da.GetDataTable($"SELECT distinct A.CUSTCODE 客户编号,A.CUSTNAME 客户名称,L.NAME 省,M.NAME  市,N.NAME  县,'' 最后合同时间,convert(varchar(10),A.SERVDUEDATE,23) 服务到期日,DateDiff(dd,getdate(),A.SERVDUEDATE) 剩余服务天数,A.SERVPERS+'-'+H.EMPLOYEENAME 服务负责人,F.EMPLOYEENAME 实施负责人,A.IMPLPERS,A.SALEPERS+'-'+G.EMPLOYEENAME 销售负责人 FROM CUSTOMER A LEFT JOIN EMPLOYEE F ON A.IMPLPERS=F.EMPLOYEECODE LEFT JOIN EMPLOYEE G ON A.SALEPERS=G.EMPLOYEECODE LEFT JOIN EMPLOYEE H ON A.SERVPERS=H.EMPLOYEECODE LEFT JOIN PROVINCES L ON A.PROVINCE=L.CODE LEFT JOIN CITIES M ON A.CITY=M.CODE LEFT JOIN AREAS N ON A.AREA=N.CODE where A.CUSTCODE = '{sl.CustCode}'");
                    FhJson = "{\"errmsg\":\"ok\",\"errcode\":\"0\",\"SSRY\":\"" + ssfw.Rows[0]["实施负责人"].ToString() + "\",\"SJobNumber\":\"" + ssfw.Rows[0]["IMPLPERS"].ToString() + "\",\"ServdueDate\":\"" + ssfw.Rows[0]["服务到期日"].ToString() + "\"}";
                    context.Response.Write(FhJson);
                    ToolsClass.TxtLog("服务日志记录", "\r\n根据客户编码查询客户信息返回:" + FhJson + "\r\n");
                    return;
                }
                //查询当前人操作的实施日志
                if (sl.Type == "SelectDT")
                {
                    DataTable ssfw = da.GetDataTable($"SELECT a.BillNo,b.custname,a.ProbType,a.ProcState,a.IMPLPERS,convert(varchar(10),a.BillDate,20) BillDate from CUSTSERVLOG a left join Customer b on a.custcode = b.custcode  where a.OperatorGUID = '{userGuid}' order by a.BillDate desc");
                    if (ssfw.Rows.Count == 0)
                    {
                        context.Response.Write("{\"errmsg\":\"\",\"errcode\":\"0\"}");
                        return;
                    }
                    csjsjsjs.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"CustSers\":[");
                    for (int i = 0; i < ssfw.Rows.Count; i++)
                    {
                        if (i > 0)
                        {
                            csjsjsjs.Append(",");
                        }
                        csjsjsjs.Append("{\"BillNo\":\"" + ssfw.Rows[i]["BillNo"] + "\",\"CustName\":\"" + ssfw.Rows[i]["custname"] + "\",\"BillDate\":\"" + ssfw.Rows[i]["BillDate"] + "\",\"ProbType\":\"" + ssfw.Rows[i]["ProbType"] + "\",\"ProcState\":\"" + ssfw.Rows[i]["ProcState"] + "\",\"EmployeeName\":\"" + da.GetValue($"select employeename from employee where employeeCode = '{ssfw.Rows[i]["IMPLPERS"].ToString()}'").ToString() + "\",\"EmployeeCode\":\"" + ssfw.Rows[i]["IMPLPERS"] + "\"}");
                    }
                    csjsjsjs.Append("]}");
                    context.Response.Write(csjsjsjs.ToString());
                    ToolsClass.TxtLog("服务日志记录", "\r\n查询服务日志列表:" + csjsjsjs.ToString() + "\r\n");
                    return;
                }

                //查询服务日志详细信息
                if (sl.Type == "SelectDTDetail")
                {
                    DataTable ssfw = da.GetDataTable($"SELECT  convert(varchar(20),a.BillDate,20) BillDate, b.CustName, a.BillNo, a.ProbType, a.HourMuch, a.ProbDesc, a.UnSolvCaus, a.ProcState , a.SolutionDesc ,a.IMPLPERS, convert(varchar(10),a.SERVDUEDATE,20) SERVDUEDATE, a.PicUrls FROM CustServLog a left join Customer b on a.custcode = b.custcode   where a.OperatorGUID  = '{userGuid}' and a.billno = '{sl.BillNo}'");
                    if (ssfw.Rows.Count == 0)
                    {
                        context.Response.Write("{\"errmsg\":\"" + "没有相应的明细" + "\",\"errcode\":1}");
                        ToolsClass.TxtLog("服务日志记录", "\r\n查询服务日志明细:没有相应的明细\r\n");
                        return;
                    }
                    else
                    {
                        csjsjsjs.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"BillDate\":\"" + ssfw.Rows[0]["BillDate"] + "\",\"CustName\":\"" + ssfw.Rows[0]["CustName"] + "\",\"BillNo\":\"" + ssfw.Rows[0]["BillNo"] + "\",\"ProbType\":\"" + ssfw.Rows[0]["ProbType"] + "\",\"HourMuch\":\"" + ssfw.Rows[0]["HourMuch"] + "\",\"ProbDesc\":\"" + ssfw.Rows[0]["ProbDesc"] + "\",\"UnSolvCaus\":\"" + ssfw.Rows[0]["UnSolvCaus"] + "\",\"ProcState\":\"" + ssfw.Rows[0]["ProcState"] + "\",\"SolutionDesc\":\"" + ssfw.Rows[0]["SolutionDesc"] + "\",\"IMPLPERSName\":\"" + da.GetValue($"select employeename from employee where employeeCode = '{ssfw.Rows[0]["IMPLPERS"].ToString()}'").ToString() + "\",\"IMPLPERS\":\"" + ssfw.Rows[0]["IMPLPERS"] + "\",\"SERVDUEDATE\":\"" + ssfw.Rows[0]["SERVDUEDATE"] + "\",\"PicUrls\":" + ssfw.Rows[0]["PicUrls"] + "}");
                        context.Response.Write(csjsjsjs.ToString());
                        ToolsClass.TxtLog("服务日志记录", "\r\n查询服务日志明细:" + csjsjsjs.ToString() + "\r\n");
                        return;
                    }
                }
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                ToolsClass.TxtLog("服务日志记录", "\r\n服务日志记录error:" + ex.Message + "\r\n");
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