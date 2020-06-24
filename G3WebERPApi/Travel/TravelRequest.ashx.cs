using G3WebERPApi.user;
using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi.Travel
{
    /// <summary>
    /// 出差申请接口
    /// </summary>
    public class TravelRequest : IHttpHandler
    {
        System.Text.RegularExpressions.Regex rex = new System.Text.RegularExpressions.Regex(@"^\d+$");
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        
        ArrayList sqlList = new ArrayList();

        StringBuilder sqlTou = new StringBuilder();
        StringBuilder sqlTi = new StringBuilder();

        string url = string.Empty;
        object obj;
        string isWrite = "0";
        string token = "";//秘钥      
        string CsJson = "";//获取请求json
        string billno = "";//单据编号

        DataTable dt = new DataTable();
        string FhJson = "";//返回JSON

        string ticket = "";
        string appKey = "";
        string appSecret = "";
        string access_token = "";
        string agentId = "251741564";// 必填，微应用ID
        string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";//必填，企业ID
        int errcode = 1;
        string operatorName = "";
        string operatorGuid = "";//内部系统用户GUID

        string Sql = "";
        string AppWyy = "";//钉钉微应用参数集
        string[] ScList;//参数集
        string ddUrl = "";//钉钉前端地址

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
            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", "");
            //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
            AppWyy = ToolsClass.GetConfig( "AppWyy");
            ScList = AppWyy.Split('$');
            agentId = ScList[0].ToString();
            corpId = ScList[1].ToString();
            appKey = ScList[2].ToString();
            appSecret = ScList[3].ToString();

            isWrite = ToolsClass.GetConfig( "isWrite");
            ddUrl = ToolsClass.GetConfig( "ddUrl");

            if (isWrite == "1")
            {
                ToolsClass.TxtLog("DDLog", "\r\nTravelReq=>入参:" + CsJson + "\r\n");
            }

            CCSQ ccsqClass = new CCSQ();
            ccsqClass = (CCSQ)JsonConvert.DeserializeObject(CsJson, typeof(CCSQ));
            //ccsqClass.Notes = Regex.Replace(ccsqClass.Notes, @"[\n\r]", "").Replace("\\", "");
            //ccsqClass.TravelReason = Regex.Replace(ccsqClass.TravelReason, @"[\n\r]", "").Replace("//", "");
            if (ccsqClass.Detail.Length <= 0)
            {
                context.Response.Write("{\"errmsg\":\"行程不允许为空,请添加行程(DD6001)\",\"errcode\":1}");
                return;
            }
            try
            {
                #region 获取access_token
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
                #endregion

                #region 获取用户详情
                url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + ccsqClass.DDOperatorId;
                FhJson = ToolsClass.ApiFun("GET", url, "");

                GetUserXq userXqClass = new GetUserXq();
                userXqClass = (GetUserXq)JsonConvert.DeserializeObject(FhJson, typeof(GetUserXq));
                errcode = userXqClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"获取申请人详细信息报错(DD6003)\",\"errcode\":1}");
                    return;
                }
                operatorName = userXqClass.name;
                #endregion

                #region 获取用户guid
                Sql = "select top 1 guid from  Employee where employeecode='" + userXqClass.jobnumber + "'";
                obj = da.GetValue(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }
                operatorGuid = obj.ToString();
                #endregion

                #region 获取申请流水号
                Sql = "select dbo.GetBillNo('DDTrvelReq','" + userXqClass.jobnumber + "',getdate())";
                obj = da.GetValue(Sql);
                billno = obj.ToString();

                if (billno == "1")
                {
                    billno = "CL" + userXqClass.jobnumber + DateTime.Now.ToString("yyyyMMdd") + "0001";

                    Sql = "update BillNumber set MaxNum=1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='DDTrvelReq' and BillDate<>convert(varchar(20),GETDATE(),120)";
                }
                else
                {
                    Sql = "update BillNumber set MaxNum=MaxNum+1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='DDTrvelReq'";
                }
                obj = da.ExecSql(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 保存行程信息
                sqlList.Clear();
                sqlTou.Clear();
                sqlTou.Append("insert into TravelReq(BillNo,TravelReason,Notes,DDOperatorId,OperatorGuid,BillDate,SelAuditingGuid,CopyPerson,OperatorName,SelAuditingName,CopyPersonName,AppendixUrl,PictureUrl) Values('").Append(billno).Append("','")
                    .Append(ccsqClass.TravelReason).Append("','")
                    .Append(ccsqClass.Notes).Append("','")
                    .Append(ccsqClass.DDOperatorId).Append("','")
                    .Append(operatorGuid).Append("','")//内部数据库用户GUID
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                    .Append(ccsqClass.SelAuditingGuid).Append("','")
                    .Append(ccsqClass.CopyPerson).Append("','")
                    .Append(operatorName).Append("','")
                    .Append(ccsqClass.SelAuditingName).Append("','")
                    .Append(ccsqClass.CopyPersonName).Append("','")
                    .Append(ccsqClass.AppendixUrl).Append("','")
                    .Append(ccsqClass.PictureUrl).Append("')");
                sqlList.Add(sqlTou.ToString());

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nTravelReq=>insert:" + sqlTou.ToString() + "\r\n");
                }

                sqlTou.Clear();
                sqlTou.Append("insert into TravelReqDetail(BillNo,Guid,TranMode,OtherTranMode,IsReturn,DepaCity,DepaCity1,DepaCity2,DestCity,DestCity1,DestCity2,StartDate,EndDate,Hours,Days,BearOrga,CustCode,CustName,Peers,PeersName) Values('");
                for (int i = 0; i < ccsqClass.Detail.Length; i++)
                {
                    sqlTi.Clear();
                    sqlTi.Append(sqlTou.ToString()).Append(billno).Append("','")
                        .Append(Guid.NewGuid().ToString()).Append("','")
                        .Append(ccsqClass.Detail[i].TranMode).Append("','")
                        .Append(ccsqClass.Detail[i].OtherTranMode).Append("','")
                        .Append(ccsqClass.Detail[i].IsReturn).Append("','")
                        .Append(ccsqClass.Detail[i].DepaCity).Append("','")
                        .Append(ccsqClass.Detail[i].DepaCity1).Append("','")
                        .Append(ccsqClass.Detail[i].DepaCity2).Append("','")
                        .Append(ccsqClass.Detail[i].DestCity).Append("','")
                        .Append(ccsqClass.Detail[i].DestCity1).Append("','")
                        .Append(ccsqClass.Detail[i].DestCity2).Append("','")
                        .Append(ccsqClass.Detail[i].StartDate).Append(":01','")
                        .Append(ccsqClass.Detail[i].EndDate).Append(":01','")
                        .Append(ccsqClass.Detail[i].Hours).Append("','")
                        .Append(ccsqClass.Detail[i].Days).Append("','")
                        .Append(ccsqClass.Detail[i].BearOrga).Append("','")
                        .Append(ccsqClass.Detail[i].CustCode).Append("','")
                        .Append(ccsqClass.Detail[i].CustName).Append("','")
                        .Append(ccsqClass.Detail[i].Peers).Append("','")
                        .Append(ccsqClass.Detail[i].PeersName).Append("')");
                    sqlList.Add(sqlTi.ToString());
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("DDLog", "\r\nTravelReq=>insert:" + sqlTi.ToString() + "\r\n");
                    }
                }
                //执行SQL语句Insert
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存出差申请信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 发送工作通知消息
                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + ccsqClass.SelAuditingGuid + "," + ccsqClass.CopyPerson + "," + ccsqClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + ddUrl + "/shenpi/index.html?billno=" + billno + "\",\"picUrl\":\"@\",\"title\":\"" + operatorName + "的【出差】申请\",\"text\":\"出发日期: " + ccsqClass.Detail[0].StartDate.Substring(0, 10) + "\r\n返程日期: " + ccsqClass.Detail[0].EndDate.Substring(0, 10) + "\r\n事由: " + ccsqClass.TravelReason + "\"}}}";
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                XXTZ xxtzClass = new XXTZ();
                xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 更新单据消息id与返回内容
                //Sql = "update TravelReq set task_id='" + xxtzClass.task_id + "',request_id='" + xxtzClass.request_id + "',returnMsg='" + FhJson.ToString() + "' where billno='" + billno + "'";

                //obj = da.ExecSql(Sql);

                //if (obj == null)
                //{
                //    if (isWrite == "1")
                //    {
                //        ToolsClass.TxtLog("DDLog", "\r\nTravelReq=>update:" + Sql.ToString() + "\r\n");
                //    }
                //    context.Response.Write("{\"errmsg\":\"保存消息通知信息出错(DD6005)\",\"errcode\":1}");
                //    return;
                //}
                #endregion

                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"提交的信息有误(DD0005)\",\"errcode\":1}");
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
        //执行存储过程oracle
        public bool sqlPro()
        {
            List<DbHelper.SqlHelperParameter> lstPara = new List<DbHelper.SqlHelperParameter>();
            lstPara.Add(da.AddProcParameter("v_ReturnMsg", RomensDataType.Varchar, ParameterDirection.Output, 50));
            lstPara.Add(da.AddProcParameter("v_ReturnValue", RomensDataType.Int, ParameterDirection.Output, 0));
            NameValueCollection returnValue = new NameValueCollection();
            returnValue = da.ExecProc("HISPRESCRIPTIONPRO", lstPara);
            string result = ReturnExecProMsg(returnValue);
            if (!string.IsNullOrEmpty(result))
            {
                ToolsClass.TxtLog("DDLog", "执行存储过程报错：" + result);
                return false;
            }
            return true;
        }

        /// <summary>
        /// 执行存储过程返回结果
        /// </summary>
        private string ReturnExecProMsg(NameValueCollection outParaValue)
        {
            System.Collections.Hashtable table = new System.Collections.Hashtable();
            string returnMsg = string.Empty;
            int returnValue;
            for (int i = 0; i < outParaValue.Count; i++)
            {
                table.Add(outParaValue.Keys[i].ToString(), outParaValue[i].ToString());
            }
            returnMsg = table["v_ReturnMsg"].ToString();
            returnValue = int.Parse(table["v_ReturnValue"].ToString());

            if (returnValue != 0)
            {
                return returnMsg;
            }
            return string.Empty;
        }
    }
}