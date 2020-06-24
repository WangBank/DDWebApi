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
    /// 通讯费报销申请
    /// </summary>
    public class TxfApproval : IHttpHandler
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
        string corpId = "";//必填，企业ID
        int errcode = 1;
        string Sql = "";

        string operatorGuid = "";//申请人guid
        string billTypeNo = "";//编码规则编号
        string billNoPro = "";//编码前缀
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
                ToolsClass.TxtLog("DDLog", "\r\nTxfApproval=>入参:" + CsJson + "\r\n");
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
                ToolsClass.TxtLog("DDLog", "\r\nTxfApproval=>入参:" + CsJson + "\r\n");
            }

            TxfClass txfClass = new TxfClass();
            txfClass = (TxfClass)JsonConvert.DeserializeObject(CsJson, typeof(TxfClass));
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

                if (txfClass.FeeType == "01")
                {
                    //市内交通费
                    billTypeNo = "100520005015";
                    billNoPro = "JTF";
                    ProName = "EXPEREFER";
                    typeName = "交通费";
                    typeUrl = ddUrl + "/jtfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "02")
                {
                    //通讯费
                    billTypeNo = "100520005020";
                    billNoPro = "TXF";
                    ProName = "EXPEREFER";
                    typeName = "通讯费";
                    typeUrl = ddUrl + "/txfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "03")
                {
                    //车辆费
                    billTypeNo = "100520005025";
                    billNoPro = "CLF";
                    ProName = "EXPEREFER";
                    typeName = "车辆费";
                    typeUrl = ddUrl + "/clui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "04")
                {
                    //房租费
                    billTypeNo = "100520005030";
                    billNoPro = "FZ";
                    ProName = "EXPEREFER";
                    typeName = "房租费";
                    typeUrl = ddUrl + "/fzfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "05")
                {
                    //水费
                    billTypeNo = "100520005035";
                    billNoPro = "SF";
                    ProName = "EXPEREFER";
                    typeName = "水费";
                    typeUrl = ddUrl + "/sfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "06")
                {
                    //电费
                    billTypeNo = "100520005040";
                    billNoPro = "DF";
                    ProName = "EXPEREFER";
                    typeName = "电费";
                    typeUrl = ddUrl + "/dfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "00")
                {
                    //招待费
                    billTypeNo = "100520005010";
                    billNoPro = "ZDF";
                    ProName = "EXPEREFER";
                    typeName = "招待费";
                    typeUrl = ddUrl + "/zdfui/shenpi/index.html?billno=";
                }
                else
                {
                    context.Response.Write("{\"errmsg\":\"提交的报销类型不存在(DD9001)\",\"errcode\":1}");
                    return;
                }

                #region 获取申请流水号
                Sql = "select dbo.GetBillNo('" + billTypeNo + "','" + txfClass.ApplPers + "',getdate())";
                obj = da.GetValue(Sql);
                billno = obj.ToString();
                operatorGuid = txfClass.OperatorGuid;
                if (billno == "1")
                {
                    billno = billNoPro + txfClass.ApplPers + DateTime.Now.ToString("yyyyMMdd") + "0001";

                    Sql = "update BillNumber set MaxNum=1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='" + billTypeNo + "' and BillDate<>convert(varchar(20),GETDATE(),120)";
                }
                else
                {
                    Sql = "update BillNumber set MaxNum=MaxNum+1,BillDate=convert(varchar(20),GETDATE(),120) where BillGuid='" + billTypeNo + "'";
                }

                obj = da.ExecSql(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"更新通信费单号出错(DD9002)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 保存信息
                sqlList.Clear();
                sqlTou.Clear();
                if (txfClass.FeeType == "00")
                {
                    sqlTou.Append("insert into EXPEENTEMENT(BillNo,BillDate,OperatorGuid,ApplPers,DDOperatorId,BillCount,FeeAmount,NoCountFee,BearOrga,AppendixUrl,PictureUrl,Notes,SelAuditingGuid,SelAuditingName,CopypersonID,CopyPersonName,CustCode) Values('")
                    .Append(billno).Append("','")
                    .Append(DateTime.Now.ToString("yyyy-MM-dd")).Append("','")
                    .Append(txfClass.OperatorGuid).Append("','")
                    .Append(txfClass.ApplPers).Append("','")
                    .Append(txfClass.DDOperatorId).Append("','")
                    .Append(txfClass.BillCount).Append("','")
                    .Append(txfClass.FeeAmount).Append("','")
                    .Append(txfClass.NoCountFee).Append("','")
                    .Append(txfClass.BearOrga).Append("','")
                    .Append(txfClass.AppendixUrl).Append("','")
                    .Append(txfClass.PictureUrl).Append("','")
                    .Append(txfClass.Notes).Append("','")
                    .Append(txfClass.SelAuditingGuid).Append("','")
                    .Append(txfClass.SelAuditingName).Append("','")
                    .Append(txfClass.CopypersonID).Append("','")
                    .Append(txfClass.CopyPersonName).Append("','")
                    .Append(txfClass.CustCode).Append("')");
                }
                else
                {
                    sqlTou.Append("insert into EXPEOTHER(BillNo,BillDate,FeeType,OperatorGuid,ApplPers,DDOperatorId,BillCount,FeeAmount,NoCountFee,BearOrga,AppendixUrl,PictureUrl,Notes,SelAuditingGuid,SelAuditingName,CopypersonID,CopyPersonName) Values('")
                    .Append(billno).Append("','")
                    .Append(DateTime.Now.ToString("yyyy-MM-dd")).Append("','")
                    .Append(txfClass.FeeType).Append("','")
                    .Append(txfClass.OperatorGuid).Append("','")
                    .Append(txfClass.ApplPers).Append("','")
                    .Append(txfClass.DDOperatorId).Append("','")
                    .Append(txfClass.BillCount).Append("','")
                    .Append(txfClass.FeeAmount).Append("','")
                    .Append(txfClass.NoCountFee).Append("','")
                    .Append(txfClass.BearOrga).Append("','")
                    .Append(txfClass.AppendixUrl).Append("','")
                    .Append(txfClass.PictureUrl).Append("','")
                    .Append(txfClass.Notes).Append("','")
                    .Append(txfClass.SelAuditingGuid).Append("','")
                    .Append(txfClass.SelAuditingName).Append("','")
                    .Append(txfClass.CopypersonID).Append("','")
                    .Append(txfClass.CopyPersonName).Append("')");
                }
                sqlList.Add(sqlTou.ToString());

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("DDLog", "\r\nTxfApproval=>insert:" + sqlTou.ToString() + "\r\n");
                }
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存申请信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 调用提交存储过程
                if (!sqlPro())
                {
                    //招待费
                    if (txfClass.FeeType == "00")
                    {
                        sqlList.Clear();
                        Sql = "delete from EXPEENTEMENT where BillNo='" + billno + "'";
                        sqlList.Add(Sql);
                    }
                    else
                    {
                        sqlList.Clear();
                        Sql = "delete from EXPEOTHER where BillNo='" + billno + "'";
                        sqlList.Add(Sql);
                    }
                    obj = da.ExecSql(sqlList);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"删除提交信息出错(DD6006)\",\"errcode\":1}");
                        return;
                    }

                    context.Response.Write("{\"errmsg\":\"" + ProResult + "(DD9003)\",\"errcode\":1}");
                    return;
                }
                #endregion

                #region 发送工作通知消息
                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + txfClass.SelAuditingGuid + "," + txfClass.CopypersonID + "," + txfClass.DDOperatorId + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + typeUrl + "" + billno + "\",\"picUrl\":\"@\",\"title\":\"" + txfClass.OperatorName + "的【" + typeName + "】报销申请\",\"text\":\"金额: " + txfClass.FeeAmount + "￥  发票: " + txfClass.BillCount + " 张\r\n申请日期: " + DateTime.Now.ToString("yyyy-MM-dd") + "\r\n备注: " + txfClass.Notes + "\"}}}";
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                XXTZ xxtzClass = new XXTZ();
                xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass.errcode;
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的报销申请，消息通知失败(DD9004)\",\"errcode\":1}");
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