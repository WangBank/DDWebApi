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
    /// 出差申请审批接口
    /// </summary>
    public class ProInfo : IHttpHandler
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
        string CsJson = "";//获取请求json
        string billno = "";//单据编号

        DataTable dt = new DataTable();
        string FhJson = "";//返回JSON

        string appKey = "";
        string appSecret = "";
        string access_token = "";
        string agentId = "251741564";// 必填，微应用ID
        string corpId = "";//必填，企业ID
        int errcode = 1;
        string audiName = "";//审批人
        string Sql = "";
        string audiIdea = "";//审批意见
        string AuditingGuid = "";//内部数据库用户GUID
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
                ToolsClass.TxtLog("DDLog", "\r\nProInfo=>入参:" + CsJson + "\r\n");
            }

            //前端传入数据
            TravelApproval traApprClass = new TravelApproval();
            traApprClass = (TravelApproval)JsonConvert.DeserializeObject(CsJson, typeof(TravelApproval));
            try
            {
                if (traApprClass.IsAuditing == "1")
                {
                    audiIdea = "同意";
                }
                else if (traApprClass.IsAuditing == "2")
                {
                    audiIdea = "驳回";
                }
                else
                {
                    audiIdea = "抄送";
                }
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
                url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + traApprClass.DDAuditingId;
                FhJson = ToolsClass.ApiFun("GET", url, "");

                GetUserXq userXqClass = new GetUserXq();
                userXqClass = (GetUserXq)JsonConvert.DeserializeObject(FhJson, typeof(GetUserXq));
                errcode = userXqClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"获取审批人详细信息报错(DD6003)\",\"errcode\":1}");
                    return;
                }
                audiName = userXqClass.name;
                #endregion

                #region 获取用户guid
                Sql = "select top 1 a.guid,b.StartDate,b.EndDate from  FlowEmployee a left join (select convert(varchar(20), min(b.StartDate), 23) StartDate,convert(varchar(20), max(EndDate), 23) EndDate from  TravelReq a left join TravelReqDetail b on a.billno = b.billno where a.billno = '" + traApprClass.BillNo + "' group by a.billno) b on 1 = 1 where a.employeecode = '" + userXqClass.jobnumber + "'";
                obj = da.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD9002)\",\"errcode\":1}");
                    return;
                }
                dt = obj as DataTable;
                if (dt.Rows.Count == 0)
                {
                    context.Response.Write("{\"errmsg\":\"申请信息不存在(DD9003)\",\"errcode\":1}");
                    return;
                }
                AuditingGuid = dt.Rows[0]["guid"].ToString();
                #endregion

                #region 发送工作通知消息
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + traApprClass.DDAuditingId + "," + traApprClass.CopyPerson + "," + traApprClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + ddUrl + "/shenpi/index.html?billno=" + traApprClass.BillNo + "\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"出发日期: " + dt.Rows[0]["StartDate"].ToString() + "\r\n申请人: " + traApprClass.OperatorName + "【出差】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";

                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nProInfo=>CsJson:" + CsJson + "FhJson\r\n:" + FhJson);
                }

                XXTZ xxtzClass2 = new XXTZ();
                xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass2.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 发送普通会话消息
                //if (traApprClass.Cid != "" && traApprClass.Cid != null)
                //{
                //    CsJson = "{\"cid\":\"" + traApprClass.Cid + "\",\"sender\":\"" + traApprClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"http://oa.romens.cn:8090/shenpi/index.html?billno=" + traApprClass.BillNo + "\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "\",\"text\":\"" + audiName + audiIdea + "了审批\"}}}";

                //    url = "https://oapi.dingtalk.com/message/send_to_conversation?access_token=" + access_token;
                //    FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                //    if (isWrite == "1")
                //    {
                //        ToolsClass.TxtLog("DDLog", "\r\nProInfo=>CsJson:" + CsJson + "FhJson\r\n:" + FhJson);
                //    }

                //    XXTZ xxtzClass = new XXTZ();
                //    xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                //    errcode = xxtzClass.errcode;
                //    if (errcode != 0)
                //    {
                //        context.Response.Write("{\"errmsg\":\"您的出差申请消息通知失败(DD6004)\",\"errcode\":1}");
                //        return;
                //    }
                //}
                #endregion

                #region 更新审批信息
                //更新单据消息id与返回内容
                Sql = "update TravelReq set isauditing='" + traApprClass.IsAuditing + "',auditingdate=getdate(),auditingidea='" + traApprClass.AuditingIdea + "',DDAuditingId='" + traApprClass.DDAuditingId + "',auditingguid='" + AuditingGuid + "',auditingname='" + audiName + "' where billno='" + traApprClass.BillNo + "'";

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nProInfo=>update:" + Sql.ToString() + "\r\n");
                }

                obj = da.ExecSql(Sql);

                if (obj == null)
                {
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("DDLog", "\r\nProInfo=>update:" + Sql.ToString() + "\r\n");
                    }
                    context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                    return;
                }
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