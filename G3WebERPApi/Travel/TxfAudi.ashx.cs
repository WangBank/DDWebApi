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
    /// 通讯费审批接口
    /// </summary>
    public class TxfAudi : IHttpHandler
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
                ToolsClass.TxtLog("DDLog", "\r\nTxfAudi=>入参:" + CsJson + "\r\n");
            }

            //前端传入数据
            TravelApproval traApprClass = new TravelApproval();
            traApprClass = (TravelApproval)JsonConvert.DeserializeObject(CsJson, typeof(TravelApproval));
            try
            {
                if (traApprClass.IsSp == "1")
                {
                    audiIdea = "同意";
                }
                else if (traApprClass.IsSp == "2")
                {
                    audiIdea = "驳回";
                }
                else
                {
                    audiIdea = "抄送";
                }

                if (traApprClass.FeeType == "01")
                {
                    //市内交通费
                    billTypeNo = "100520005015";
                    ProName = "EXPEAUDITING";
                    typeName = "交通费";
                    typeUrl = ddUrl + "/jtfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "02")
                {
                    //通讯费
                    billTypeNo = "100520005020";
                    ProName = "EXPEAUDITING";
                    typeName = "通讯费";
                    typeUrl = ddUrl + "/txfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "03")
                {
                    //车辆费
                    billTypeNo = "100520005025";
                    ProName = "EXPEAUDITING";
                    typeName = "车辆费";
                    typeUrl = ddUrl + "/clui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "04")
                {
                    //房租费
                    billTypeNo = "100520005030";
                    ProName = "EXPEAUDITING";
                    typeName = "房租费";
                    typeUrl = ddUrl + "/fzfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "05")
                {
                    //水费
                    billTypeNo = "100520005035";
                    ProName = "EXPEAUDITING";
                    typeName = "水费";
                    typeUrl = ddUrl + "/sfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "06")
                {
                    //电费
                    billTypeNo = "100520005040";
                    ProName = "EXPEAUDITING";
                    typeName = "电费";
                    typeUrl = ddUrl + "/dfui/shenpi/index.html?billno=";
                }
                else if (traApprClass.FeeType == "00")
                {
                    //招待费
                    billTypeNo = "100520005010";
                    ProName = "EXPEAUDITING";
                    typeName = "招待费";
                    typeUrl = ddUrl + "/zdfui/shenpi/index.html?billno=";
                }
                else
                {
                    context.Response.Write("{\"errmsg\":\"提交的报销类型不存在(DD9001)\",\"errcode\":1}");
                    return;
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
                if (traApprClass.FeeType == "00")
                {
                    Sql = "select top 1 a.Guid,b.BillCount,b.FeeAmount from  Employee a left join (select BillCount,FeeAmount from EXPEENTEMENT where billno = '[申请号]') b on 1 = 1 where a.employeecode = '[工号]'";
                }
                else
                {
                    Sql = "select top 1 a.Guid,b.BillCount,b.FeeAmount from  Employee a left join (select BillCount,FeeAmount from EXPEOTHER where billno = '[申请号]') b on 1 = 1 where a.employeecode = '[工号]'";
                }
                Sql = Sql.Replace("[申请号]", traApprClass.BillNo).Replace("[工号]", userXqClass.jobnumber);

                obj = da.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }

                dt = obj as DataTable;
                AuditingGuid = dt.Rows[0]["Guid"].ToString();
                #endregion

                #region 更新审批信息
                if (traApprClass.FeeType == "00")
                {
                    //更新单据消息id与返回内容
                    Sql = "update EXPEENTEMENT set IsSp='" + traApprClass.IsSp + "',auditingidea='" + traApprClass.AuditingIdea + "',DDAuditingId='" + traApprClass.DDAuditingId + "',auditingguid='" + AuditingGuid + "' where billno='" + traApprClass.BillNo + "'";
                }
                else
                {
                    //更新单据消息id与返回内容
                    Sql = "update EXPEOTHER set IsSp='" + traApprClass.IsSp + "',auditingidea='" + traApprClass.AuditingIdea + "',DDAuditingId='" + traApprClass.DDAuditingId + "',auditingguid='" + AuditingGuid + "' where billno='" + traApprClass.BillNo + "'";
                }

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nTxfAudi=>update:" + Sql.ToString() + "\r\n");
                }

                obj = da.ExecSql(Sql);

                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"更新审批信息出错(DD6006)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 调用审核存储过程
                if (traApprClass.IsSp == "1")
                {
                    billno = traApprClass.BillNo;
                    if (!sqlPro())
                    {
                        if (traApprClass.FeeType == "00")
                        {
                            Sql = "update EXPEENTEMENT set IsSp='0',auditingidea=null,DDAuditingId=null,auditingguid=null where billno='" + traApprClass.BillNo + "'";
                        }
                        else
                        {
                            Sql = "update EXPEOTHER set IsSp='0',auditingidea=null,DDAuditingId=null,auditingguid=null where billno='" + traApprClass.BillNo + "'";
                        }

                        obj = da.ExecSql(Sql);
                        if (obj == null)
                        {
                            context.Response.Write("{\"errmsg\":\"更新审批状态出错(DD6006)\",\"errcode\":1}");
                            return;
                        }

                        context.Response.Write("{\"errmsg\":\"" + ProResult + "(DD9003)\",\"errcode\":1}");
                        return;
                    }
                }
                #endregion

                #region 发送工作通知消息
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + traApprClass.DDAuditingId + "," + traApprClass.CopyPerson + "," + traApprClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + typeUrl + "" + traApprClass.BillNo + "\",\"picUrl\":\"@\",\"title\":\"已" + audiIdea + "【" + audiName + "】\",\"text\":\"金额: " + dt.Rows[0]["FeeAmount"].ToString() + "￥ 发票: " + dt.Rows[0]["BillCount"].ToString() + "张\r\n申请人: " + traApprClass.OperatorName + "【" + typeName + "】\r\n审批意见: " + traApprClass.AuditingIdea + "\"}}}";

                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nTxfAudi=>CsJson:" + CsJson + "FhJson\r\n:" + FhJson);
                }

                XXTZ xxtzClass2 = new XXTZ();
                xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass2.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的报销申请消息通知失败(DD6004)\",\"errcode\":1}");
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