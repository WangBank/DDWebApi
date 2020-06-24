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
    /// SendMsg 的摘要说明
    /// </summary>
    public class SendMsg : IHttpHandler
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
        string corpId = "dingea4887a230e5a3ae35c2f4657eb6378f";//必填，企业ID
        int errcode = 1;
        string audiName = "";//审批人
        string Sql = "";
        string audiIdea = "";//审批意见
        string AuditingGuid = "";//内部数据库用户GUID

        string operatorGuid = "";//申请人guid
        string billTypeNo = "";//编码规则编号
        string typeName = "";//类别名称
        string typeUrl = "";//类别地址
        string ProResult = "";//存储过程报错
        string ProName = "";//存储过程名
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
                ToolsClass.TxtLog("DDLog", "\r\nSendMsg=>入参:" + CsJson + "\r\n");
            }

            //前端传入数据
            TravelApproval traApprClass = new TravelApproval();
            traApprClass = (TravelApproval)JsonConvert.DeserializeObject(CsJson, typeof(TravelApproval));
            try
            {
                #region 设置消息跳转链接地址
                if (traApprClass.FeeType == "01")
                {
                    //市内交通费;
                    typeName = "交通费";
                    typeUrl = ddUrl + "/jtfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "02")
                {
                    //通讯费
                    typeName = "通讯费";
                    typeUrl = ddUrl + "/txfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "03")
                {
                    //车辆费
                    typeName = "车辆费";
                    typeUrl = ddUrl + "/clui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "04")
                {
                    //房租费
                    typeName = "房租费";
                    typeUrl = ddUrl + "/fzfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "05")
                {
                    //水费
                    typeName = "水费";
                    typeUrl = ddUrl + "/sfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "06")
                {
                    //电费
                    typeName = "电费";
                    typeUrl = ddUrl + "/dfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "11")
                {
                    //出差
                    typeName = "出差";
                    typeUrl = ddUrl + "/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "12")
                {
                    //差旅费
                    typeName = "差旅费";
                    typeUrl = ddUrl + "/clfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "00")
                {
                    //招待费
                    typeName = "招待费";
                    typeUrl = ddUrl + "/zdfui/shenpi/index.html?billno=";
                }
                else
                {
                    context.Response.Write("{\"errmsg\":\"提交的申请类型不存在(DD9001)\",\"errcode\":1}");
                    return;
                }
                #endregion

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

                #region 获取通知信息字段
                //出差
                if (traApprClass.FeeType == "11")
                {
                    Sql = "select a.TravelReason,convert(varchar(20),min(b.StartDate),23) StartDate,convert(varchar(20),max(EndDate),23) EndDate from  TravelReq a left join TravelReqDetail b on a.billno=b.billno where a.billno='" + traApprClass.BillNo + "' group by a.billno,a.TravelReason";
                }
                //差旅费报销申请
                else if (traApprClass.FeeType == "12")
                {
                    Sql = "select top 1 A.DepaCity1 DepaCity,A.DestCity1 DestCity,b.TranAmount,convert(varchar(20),c.BillDate,23) BillDate from ExpeTravDetail A left join (select billno,sum(TotalAmount) TranAmount from  ExpeTravDetail group by billno) b on a.billno=b.billno left join ExpeTrav c on a.BillNo=c.BillNo where a.BillNo = '" + traApprClass.BillNo + "' order by a.depadate";
                }
                //招待费
                else if (traApprClass.FeeType == "00")
                {
                    Sql = "select FeeAmount,BillCount,convert(varchar(20),ReferDate,23) BillDate,Notes from  EXPEENTEMENT where billno='" + traApprClass.BillNo + "'";
                }
                //其他费用
                else
                {
                    Sql = "select FeeAmount,BillCount,convert(varchar(20),ReferDate,23) BillDate,Notes from  EXPEOTHER where billno='" + traApprClass.BillNo + "'";
                }

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
                #endregion

                #region 发送工作通知消息
                //出差
                if (traApprClass.FeeType == "11")
                {
                    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + traApprClass.CopyPerson + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + typeUrl + traApprClass.BillNo + "\",\"picUrl\":\"@\",\"title\":\"" + traApprClass.OperatorName + "的【" + typeName + "】申请\",\"text\":\"出发日期: " + dt.Rows[0]["StartDate"].ToString() + "\r\n返程日期: " + dt.Rows[0]["EndDate"].ToString() + "\r\n事由: " + dt.Rows[0]["TravelReason"].ToString() + "\"}}}";
                }
                //差旅费报销申请
                else if (traApprClass.FeeType == "12")
                {
                    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + traApprClass.CopyPerson + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + typeUrl + traApprClass.BillNo + "\",\"picUrl\":\"@\",\"title\":\"" + traApprClass.OperatorName + "的【" + typeName + "】报销申请\",\"text\":\"金额: " + dt.Rows[0]["TranAmount"].ToString() + " ￥\r\n行程: " + dt.Rows[0]["DepaCity"].ToString() + " - " + dt.Rows[0]["DestCity"].ToString() + "\r\n申请日期: " + dt.Rows[0]["BillDate"].ToString() + "\"}}}";
                }
                //其他费用
                else
                {
                    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + traApprClass.CopyPerson + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + typeUrl + traApprClass.BillNo + "\",\"picUrl\":\"@\",\"title\":\"" + traApprClass.OperatorName + "的【" + typeName + "】报销申请\",\"text\":\"金额: " + dt.Rows[0]["FeeAmount"].ToString() + "￥  发票: " + dt.Rows[0]["BillCount"].ToString() + " 张\r\n申请日期: " + dt.Rows[0]["BillDate"].ToString() + "\r\n备注: " + dt.Rows[0]["Notes"].ToString() + "\"}}}";
                }

                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nSendMsg=>CsJson:" + CsJson + "FhJson\r\n:" + FhJson);
                }

                XXTZ xxtzClass2 = new XXTZ();
                xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass2.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的报销申请消息通知失败(DD9004)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 更新 将转发人拼接在抄送人字段后面
                //出差
                if (traApprClass.FeeType == "11")
                {
                    Sql = "update TravelReq set CopyPerson=(case when isnull(CopyPerson,' ')=' '  then '" + traApprClass.CopyPerson + "' else CopyPerson+','+'" + traApprClass.CopyPerson + "' end) where billno='" + traApprClass.BillNo + "'";
                }
                //差旅费报销申请
                else if (traApprClass.FeeType == "12")
                {
                    Sql = "update ExpeTrav set CopyPerson=(case when isnull(CopyPerson,' ')=' '  then '" + traApprClass.CopyPerson + "' else CopyPerson+','+'" + traApprClass.CopyPerson + "' end) where billno='" + traApprClass.BillNo + "'";
                }
                //招待费
                else if (traApprClass.FeeType == "00")
                {
                    Sql = "update EXPEENTEMENT set CopyPersonID=(case when isnull(CopyPersonID,' ')=' '  then '" + traApprClass.CopyPerson + "' else CopyPersonID+','+'" + traApprClass.CopyPerson + "' end) where billno='" + traApprClass.BillNo + "'";
                }
                //其他费用
                else
                {
                    Sql = "update EXPEOTHER set CopyPersonID=(case when isnull(CopyPersonID,' ')=' '  then '" + traApprClass.CopyPerson + "' else CopyPersonID+','+'" + traApprClass.CopyPerson + "' end) where billno='" + traApprClass.BillNo + "'";
                }

                obj = da.ExecSql(Sql);

                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"更新抄送人信息出错(DD9005)\",\"errcode\":1}");
                    return;
                }
                #endregion
                context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"提交的信息有误(DD9006)\",\"errcode\":1}");
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
            lstPara.Add(da.AddProcParameter("BillGuid", RomensDataType.Varchar, ParameterDirection.Input, billno));
            lstPara.Add(da.AddProcParameter("BillTypeGuid", RomensDataType.Varchar, ParameterDirection.Input, billTypeNo));
            lstPara.Add(da.AddProcParameter("OperatorGuid", RomensDataType.Varchar, ParameterDirection.Input, operatorGuid));
            lstPara.Add(da.AddProcParameter("ReturnMsg", RomensDataType.Varchar, ParameterDirection.Output, 50));
            lstPara.Add(da.AddProcParameter("ReturnValue", RomensDataType.Int, ParameterDirection.Output, 0));
            NameValueCollection returnValue = new NameValueCollection();
            returnValue = da.ExecProc(ProName, lstPara);
            ProResult = ReturnExecProMsg(returnValue);
            if (!string.IsNullOrEmpty(ProResult))
            {
                ToolsClass.TxtLog("DDLog", "执行存储过程报错：" + ProResult);
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
            returnMsg = table["ReturnMsg"].ToString();
            returnValue = int.Parse(table["ReturnValue"].ToString());

            if (returnValue != 0)
            {
                return returnMsg;
            }
            return string.Empty;
        }
    }
}