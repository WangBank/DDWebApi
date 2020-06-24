using G3WebERPApi.Common;
using G3WebERPApi.Travel;
using Newtonsoft.Json;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;

namespace G3WebERPApi.Approval
{
    /// <summary>
    /// 其他各类报销申请
    /// </summary>
    public class TxfJtfZdfSQ : IHttpHandler
    {
        private Dictionary<string, string> procResult = new Dictionary<string, string>();
        private Dictionary<string, string> keyValuePairs = new Dictionary<string, string>();

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

        private DataTable dt = new DataTable();
        private string FhJson = "";//返回JSON

        private string appKey = "";
        private string appSecret = "";
        private string access_token = "";
        private string agentId = "251741564";// 必填，微应用ID
        private string corpId = "";//必填，企业ID
        private int errcode = 1;
        private string Sql = "";

        private string operatorGuid = "";//申请人guid
        private string billTypeNo = "";//编码规则编号
        private string billNoPro = "";//编码前缀
        private string typeName = "";//类别名称
        private string typeUrl = "";//类别地址
        private string ProResult = "";//存储过程报错
        private string ProName = "";//存储过程名
        private string AppWyy = "";//钉钉微应用参数集
        private string[] ScList;//参数集
        private string ddUrl = "";//钉钉前端地址
        private string urlcsjson = "";

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
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
            string ddMessageId = string.Empty;
            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
            string JsonData = CsJson;
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
                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n入参:" + CsJson + "\r\n");
            }

            TxfjtfMul txfClass = new TxfjtfMul();
            txfClass = (TxfjtfMul)JsonConvert.DeserializeObject(CsJson, typeof(TxfjtfMul));

            string path = context.Request.Path.Replace("Approval/TxfJtfZdfSQ.ashx", "zlfysqmul");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + txfClass.Sign + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != txfClass.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }

            string NodeInfo = JsonConvert.SerializeObject(txfClass.NodeInfo).Replace(",{\"AType\":\"\",\"PersonId\":\"select\",\"PersonName\":\"请选择\"}", "");
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

                #endregion 获取access_token

                if (txfClass.FeeType == "01")
                {
                    //市内交通费
                    billTypeNo = "100520005015";
                    billNoPro = "JTF";
                    ProName = "EXPEREFERdd";
                    typeName = "交通费";
                    typeUrl = ddUrl + "/jtfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "02")
                {
                    //通讯费
                    billTypeNo = "100520005020";
                    billNoPro = "TXF";
                    ProName = "EXPEREFERdd";
                    typeName = "通讯费";
                    typeUrl = ddUrl + "/txfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "03")
                {
                    //车辆费
                    billTypeNo = "100520005025";
                    billNoPro = "CLF";
                    ProName = "EXPEREFERdd";
                    typeName = "车辆费";
                    typeUrl = ddUrl + "/clui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "04")
                {
                    //房租费
                    billTypeNo = "100520005030";
                    billNoPro = "FZ";
                    ProName = "EXPEREFERdd";
                    typeName = "房租费";
                    typeUrl = ddUrl + "/fzfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "05")
                {
                    //水费
                    billTypeNo = "100520005035";
                    billNoPro = "SF";
                    ProName = "EXPEREFERdd";
                    typeName = "水费";
                    typeUrl = ddUrl + "/sfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "06")
                {
                    //电费
                    billTypeNo = "100520005040";
                    billNoPro = "DF";
                    ProName = "EXPEREFERdd";
                    typeName = "电费";
                    typeUrl = ddUrl + "/dfui/shenpi/index.html?billno=";
                }
                else if (txfClass.FeeType == "00")
                {
                    //招待费
                    billTypeNo = "100520005010";
                    billNoPro = "ZDF";
                    ProName = "EXPEREFERdd";
                    typeName = "招待费";
                    typeUrl = ddUrl + "/zdfui/shenpi/index.html?billno=";
                }
                else
                {
                    context.Response.Write("{\"errmsg\":\"提交的报销类型不存在(DD9001)\",\"errcode\":1}");
                    return;
                }
                string fqrall = txfClass.DDOperatorId;
                string jnumber = txfClass.ApplPers;
                string sqr = txfClass.OperatorName;
                if (txfClass.IsInsteadApply == "1")
                {
                    fqrall = fqrall + "," + txfClass.InsteadOperatorGuid;
                    sqr = "【代】" + txfClass.InsteadOperatorName;
                    jnumber = da.GetValue($"select top 1 employeecode from flowemployee where ddid = '{txfClass.InsteadOperatorGuid}'").ToString();
                }
                else
                {
                    txfClass.OperatorGuid = da.GetValue($"select top 1 guid from flowemployee where ddid = '{fqrall}' and orgcode ='{txfClass.DeptCode}'").ToString();
                }

                #region 获取申请流水号

                Sql = "select dbo.GetBillNo('" + billTypeNo + "','" + jnumber + "',getdate())";
                obj = da.GetValue(Sql);
                billno = obj.ToString();
                if (billno == "1")
                {
                    billno = billNoPro + jnumber + DateTime.Now.ToString("yyyyMMdd") + "0001";

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

                #endregion 获取申请流水号

                #region 获取用户guid

                Sql = $"select top 1 a.GUID,b.TotalAmount,b.OffDay from  operators a left join (select sum(TotalAmount) TotalAmount, sum(OffDay) OffDay from ExpetravDetail where billno = '[申请号]' group by billno) b on 1 = 1 where a.code = '[工号]'";
                Sql = Sql.Replace("[申请号]", txfClass.BillNo).Replace("[工号]", jnumber);

                obj = da.GetDataTable(Sql);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"用户不存在(DD6000)\",\"errcode\":1}");
                    return;
                }

                dt = obj as DataTable;
                operatorGuid = dt.Rows[0]["GUID"].ToString();

                #endregion 获取用户guid

                if (txfClass.NodeInfo.Length == 0)
                {
                    //自动同意
                    //更新单据消息id与返回内容

                    #region 保存信息

                    sqlList.Clear();
                    sqlTou.Clear();
                    if (txfClass.FeeType == "00")
                    {
                        sqlTou.Append("insert into EXPEENTEMENT(BillNo,BillDate,FlowEmployeeGuid,OperatorGuid,ProcessNodeInfo,ApplPers,DDOperatorId,JsonData,BillCount,FeeAmount,DeptName,DeptCode,NoCountFee,BearOrga,AppendixUrl,PictureUrl,Notes,Urls,IsInsteadApply,InsteadOperatorGuid,CustCode) Values('")
                        .Append(billno).Append("','")
                        .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                        .Append(txfClass.OperatorGuid).Append("','")
                        .Append(operatorGuid).Append("','")
                        .Append(NodeInfo).Append("','")
                        .Append(jnumber).Append("','")
                        .Append(txfClass.DDOperatorId).Append("','")
                        .Append(JsonData).Append("','")
                        .Append(txfClass.BillCount).Append("','")
                        .Append(txfClass.FeeAmount).Append("','")
                        .Append(txfClass.DeptName).Append("','")
                        .Append(txfClass.DeptCode).Append("','")
                        .Append(txfClass.NoCountFee).Append("','")
                        .Append(txfClass.BearOrga).Append("','")
                        .Append(txfClass.AppendixUrl).Append("','")
                        .Append(txfClass.PictureUrl).Append("','")
                        .Append(txfClass.Notes).Append("','")
                        .Append(JsonConvert.SerializeObject(txfClass.Urls)).Append("','")
                         .Append(txfClass.IsInsteadApply).Append("','")
                        .Append(txfClass.InsteadOperatorGuid).Append("','")
                        .Append(txfClass.CustCode).Append("')");
                    }
                    else
                    {
                        sqlTou.Append("insert into EXPEOTHER(BillNo,BillDate,FeeType,FlowEmployeeGuid,OperatorGuid,ProcessNodeInfo,ApplPers,DDOperatorId,JsonData,BillCount,FeeAmount,DeptName,DeptCode,NoCountFee,BearOrga,AppendixUrl,Urls,IsInsteadApply,InsteadOperatorGuid,PictureUrl,Notes) Values('")
                        .Append(billno).Append("','")
                        .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                        .Append(txfClass.FeeType).Append("','")
                        .Append(txfClass.OperatorGuid).Append("','")
                         .Append(operatorGuid).Append("','")
                        .Append(NodeInfo).Append("','")
                        .Append(jnumber).Append("','")
                        .Append(txfClass.DDOperatorId).Append("','")
                         .Append(JsonData).Append("','")
                        .Append(txfClass.BillCount).Append("','")
                        .Append(txfClass.FeeAmount).Append("','")
                        .Append(txfClass.DeptName).Append("','")
                        .Append(txfClass.DeptCode).Append("','")
                        .Append(txfClass.NoCountFee).Append("','")
                        .Append(txfClass.BearOrga).Append("','")
                        .Append(txfClass.AppendixUrl).Append("','")
                        .Append(JsonConvert.SerializeObject(txfClass.Urls)).Append("','")
                         .Append(txfClass.IsInsteadApply).Append("','")
                        .Append(txfClass.InsteadOperatorGuid).Append("','")
                        .Append(txfClass.PictureUrl).Append("','")
                        .Append(txfClass.Notes)
                        .Append("')");
                    }
                    sqlList.Add(sqlTou.ToString());

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n操作Expeother表:" + sqlTou.ToString() + "\r\n");
                    }
                    obj = da.ExecSql(sqlList);
                    if (obj == null)
                    {
                        context.Response.Write("{\"errmsg\":\"保存申请信息出错(DD6002)\",\"errcode\":1}");
                        return;
                    }

                    #endregion 保存信息

                    #region 调用提交存储过程

                    keyValuePairs = CommonHelper.sqlPro(billno, billTypeNo, operatorGuid, ProName);
                    ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n调用提交存储过程:" + JsonConvert.SerializeObject(keyValuePairs) + "\r\n");
                    if (keyValuePairs["ReturnValue"].ToString() != "0")
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
                        context.Response.Write("{\"errmsg\":\"" + keyValuePairs["ReturnMsg"].ToString() + "(DD9003)\",\"errcode\":1}");
                        return;
                    }

                    #endregion 调用提交存储过程

                    if (txfClass.FeeType == "00")
                    {
                        Sql = "update EXPEENTEMENT set IsSp='1',auditingdate=getdate()  where billno='" + billno + "'";
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n操作EXPEENTEMENT表:" + Sql.ToString() + "\r\n");
                    }
                    else
                    {
                        Sql = "update EXPEOTHER set IsSp='1',auditingdate=getdate()  where billno='" + billno + "'";
                        ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n操作EXPEOTHER表:" + Sql.ToString() + "\r\n");
                    }
                    obj = da.ExecSql(Sql);
                    Sql = "";

                    #region 发送工作通知消息

                    urlcsjson = typeUrl + "" + $"{billno}&BillClassId={txfClass.BillClassId}&showmenu=false";
                    urlcsjson = System.Web.HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                    url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                    CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"" + sqr + "的【" + typeName + "】报销申请\",\"text\":\"金额: " + txfClass.FeeAmount + "￥  发票: " + txfClass.BillCount + " 张\r\n申请日期: " + DateTime.Now.ToString("yyyy-MM-dd") + "\r\n备注: " + txfClass.Notes + "\"}}}";
                    FhJson = ToolsClass.ApiFun("POST", url, CsJson);
                    var xxtzClass2 = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                    ddMessageId = xxtzClass2.task_id.ToString();

                    #endregion 发送工作通知消息

                    context.Response.Write("{\"errmsg\":\"ok\",\"errcode\":0}");
                    return;
                }

                //获取第一级流程的人员信息
                NodeInfoDetailPerson[] NodeInfodetailPeople = txfClass.NodeInfo[0].NodeInfoDetails[0].Persons;
                //从入参中得到审批人及抄送人的信息
                //指定人员的id列表
                StringBuilder piddept = new StringBuilder();
                string sql = "";

                for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                {
                    if (i > 0)
                    {
                        piddept.Append(",");
                    }

                    //判断传空
                    if (NodeInfodetailPeople[i].PersonId != "select" && NodeInfodetailPeople[i].PersonId != "")
                    {
                        sql = $"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'";
                        piddept.Append(da.GetValue(sql).ToString());
                    }
                }

                #region 保存信息

                sqlList.Clear();
                sqlTou.Clear();
                if (txfClass.FeeType == "00")
                {
                    sqlTou.Append("insert into EXPEENTEMENT(BillNo,BillDate,FlowEmployeeGuid,OperatorGuid,ProcessNodeInfo,ApplPers,DDOperatorId,JsonData,BillCount,FeeAmount,DeptName,DeptCode,NoCountFee,BearOrga,AppendixUrl,PictureUrl,Notes,Urls,IsInsteadApply,InsteadOperatorGuid,CustCode) Values('")
                    .Append(billno).Append("','")
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                    .Append(txfClass.OperatorGuid).Append("','")
                     .Append(operatorGuid).Append("','")
                    .Append(NodeInfo).Append("','")
                    .Append(jnumber).Append("','")
                    .Append(txfClass.DDOperatorId).Append("','")
                    .Append(JsonData).Append("','")
                    .Append(txfClass.BillCount).Append("','")
                    .Append(txfClass.FeeAmount).Append("','")
                    .Append(txfClass.DeptName).Append("','")
                    .Append(txfClass.DeptCode).Append("','")
                    .Append(txfClass.NoCountFee).Append("','")
                    .Append(txfClass.BearOrga).Append("','")
                    .Append(txfClass.AppendixUrl).Append("','")
                    .Append(txfClass.PictureUrl).Append("','")
                    .Append(txfClass.Notes).Append("','")
                    .Append(JsonConvert.SerializeObject(txfClass.Urls)).Append("','")
                     .Append(txfClass.IsInsteadApply).Append("','")
                        .Append(txfClass.InsteadOperatorGuid).Append("','")
                    .Append(txfClass.CustCode).Append("')");
                }
                else
                {
                    sqlTou.Append("insert into EXPEOTHER(BillNo,BillDate,FeeType,FlowEmployeeGuid,OperatorGuid,JsonData,ProcessNodeInfo,ApplPers,DDOperatorId,BillCount,FeeAmount,DeptName,DeptCode,NoCountFee,BearOrga,AppendixUrl,Urls,IsInsteadApply,InsteadOperatorGuid,PictureUrl,Notes) Values('")
                    .Append(billno).Append("','")
                    .Append(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")).Append("','")
                    .Append(txfClass.FeeType).Append("','")
                    .Append(txfClass.OperatorGuid).Append("','")
                    .Append(operatorGuid).Append("','")
                     .Append(JsonData).Append("','")
                    .Append(NodeInfo).Append("','")
                    .Append(jnumber).Append("','")
                    .Append(txfClass.DDOperatorId).Append("','")
                    .Append(txfClass.BillCount).Append("','")
                    .Append(txfClass.FeeAmount).Append("','")
                    .Append(txfClass.DeptName).Append("','")
                    .Append(txfClass.DeptCode).Append("','")
                    .Append(txfClass.NoCountFee).Append("','")
                    .Append(txfClass.BearOrga).Append("','")
                    .Append(txfClass.AppendixUrl).Append("','")
                    .Append(JsonConvert.SerializeObject(txfClass.Urls)).Append("','")
                    .Append(txfClass.IsInsteadApply).Append("','")
                        .Append(txfClass.InsteadOperatorGuid).Append("','")
                    .Append(txfClass.PictureUrl).Append("','")
                    .Append(txfClass.Notes)
                    .Append("')");
                }
                sqlList.Add(sqlTou.ToString());

                if (isWrite == "1")
                {
                    ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n操作Expeother表:" + sqlTou.ToString() + "\r\n");
                }
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存申请信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                #endregion 保存信息

                #region 调用提交存储过程

                keyValuePairs = CommonHelper.sqlPro(billno, billTypeNo, operatorGuid, ProName);
                ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n调用提交存储过程:" + JsonConvert.SerializeObject(keyValuePairs) + "\r\n");
                if (keyValuePairs["ReturnValue"].ToString() != "0")
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

                #endregion 调用提交存储过程

                #region 发送工作通知消息

                urlcsjson = typeUrl + "" + $"{billno}&BillClassId={txfClass.BillClassId}&showmenu=false";
                urlcsjson = System.Web.HttpUtility.UrlEncode(urlcsjson, System.Text.Encoding.UTF8);
                url = "https://oapi.dingtalk.com/topapi/message/corpconversation/asyncsend_v2?access_token=" + access_token;
                CsJson = "{\"agent_id\":\"" + agentId + "\",\"userid_list\":\"" + piddept.ToString() + "," + fqrall + "\",\"msg\":{\"msgtype\":\"link\",\"link\":{\"messageUrl\":\"" + "dingtalk://dingtalkclient/page/link?url=" + urlcsjson + "&pc_slide=true\",\"picUrl\":\"@\",\"title\":\"" + sqr + "的【" + typeName + "】报销申请\",\"text\":\"金额: " + txfClass.FeeAmount + "￥  发票: " + txfClass.BillCount + " 张\r\n申请日期: " + DateTime.Now.ToString("yyyy-MM-dd") + "\r\n备注: " + txfClass.Notes + "\"}}}";
                FhJson = ToolsClass.ApiFun("POST", url, CsJson);

                XXTZ xxtzClass = new XXTZ();
                xxtzClass = (XXTZ)JsonConvert.DeserializeObject(FhJson, typeof(XXTZ));
                errcode = xxtzClass.errcode;
                ddMessageId = xxtzClass.task_id.ToString();
                if (errcode != 0)
                {
                    context.Response.Write("{\"errmsg\":\"您的报销申请，消息通知失败(DD9004)\",\"errcode\":1}");
                    return;
                }

                #endregion 发送工作通知消息

                //保存流程信息到comments表
                sqlList.Clear();
                for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                {
                    sqlTou.Clear();
                    if (NodeInfodetailPeople[i].PersonId != "select")
                    {
                        sqlTou.Append("insert into ApprovalComments(CommentsId,BillClassId,BillNo,ApprovalID,ApprovalName,ApprovalComments,ApprovalStatus,DDMessageId,AType,ApprovalDate,IsAndOr,IsLeader,PersonType,NodeNumber) values('").Append(Guid.NewGuid().ToString()).Append("','")
                        .Append(txfClass.BillClassId).Append("','")
                        .Append(billno).Append("','")
                        .Append(NodeInfodetailPeople[i].PersonId).Append("','")
                        .Append(NodeInfodetailPeople[i].PersonName).Append("','")//内部数据库用户GUID
                        .Append("").Append("','")
                        .Append("0").Append("','")
                         .Append(ddMessageId).Append("','")
                         .Append(NodeInfodetailPeople[i].AType).Append("','")
                        .Append(DateTime.Now).Append("','")
                        .Append(txfClass.NodeInfo[0].NodeInfoDetails[0].IsAndOr).Append("','")
                        .Append(txfClass.NodeInfo[0].NodeInfoDetails[0].IsLeader).Append("','")
                          .Append(txfClass.NodeInfo[0].NodeInfoType).Append("','")
                        .Append("2").Append("')");
                        sqlList.Add(sqlTou.ToString());
                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n操作ApprovalComments表:" + sqlTou.ToString() + "\r\n");
                        }
                    }
                }
                //执行SQL语句Insert
                obj = da.ExecSql(sqlList);
                if (obj == null)
                {
                    context.Response.Write("{\"errmsg\":\"保存申请信息节点信息出错(DD6002)\",\"errcode\":1}");
                    return;
                }

                path = context.Request.Path.Replace("Approval/TxfJtfZdfSQ.ashx", "zlfyspmul");
                //验证请求sign
                sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
                //如果下个是抄送人
                TaskFactory taskFactory = new TaskFactory();
                if (txfClass.NodeInfo[0].NodeInfoType == "3")
                {
                    //根据数据开启多个线程调用审批接口

                    taskFactory.StartNew(() =>
                    {
                        for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                        {
                            HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "zlfyspmul");
                            webrequest.Method = "post";
                            new Action(() =>
                            {
                                fasongqingqiu ad = new fasongqingqiu
                                {
                                    BillNo = billno,
                                    DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                    IsSp = "3",
                                    DDOperatorId = txfClass.InsteadOperatorGuid,
                                    OperatorName = txfClass.InsteadOperatorName,
                                    BillClassId = txfClass.BillClassId,
                                    FeeType = txfClass.FeeType,
                                    Sign = sign
                                };
                                byte[] postdatabyte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ad));
                                webrequest.ContentLength = postdatabyte.Length;
                                Stream stream;
                                stream = webrequest.GetRequestStream();
                                stream.Write(postdatabyte, 0, postdatabyte.Length);
                                stream.Close();
                                using (var httpWebResponse = webrequest.GetResponse())
                                using (StreamReader responseStream = new StreamReader(httpWebResponse.GetResponseStream()))
                                {
                                    String ret = responseStream.ReadToEnd();
                                }
                            }).Invoke();
                        }
                    });
                }

                if (txfClass.NodeInfo[0].NodeInfoType == "2")
                {
                    DataRow[] dataRows = null;

                    sql = "";
                    sql = $"select ApprovalComments,ApprovalName,ApprovalID  from ApprovalComments where BillNo ='{billno}'  and BillClassId='{txfClass.BillClassId}' and ApprovalStatus ='1'";
                    DataTable logComments = da.GetDataTable(sql);
                    //如果下个环节中的人在之前已同意，自动调用此接口同意完成审批
                    taskFactory.StartNew(() =>
                    {
                        for (int i = 0; i < NodeInfodetailPeople.Length; i++)
                        {
                            dataRows = logComments.Select("ApprovalID ='" + NodeInfodetailPeople[i].PersonId + "'");
                            //如果之前已经同意或者是发起人
                            if (dataRows.Length != 0 || da.GetValue($"select distinct DDId from FlowEmployee where EmployeeCode ='{NodeInfodetailPeople[i].PersonId}'").ToString() == txfClass.InsteadOperatorGuid)
                            {
                                HttpWebRequest webrequest = (HttpWebRequest)WebRequest.Create(ymadk + "zlfyspmul");
                                webrequest.Method = "post";
                                new Action(() =>
                                {
                                    fasongqingqiu ad = new fasongqingqiu
                                    {
                                        BillNo = billno,
                                        DDAuditingId = da.GetValue($"select distinct ddid from FlowEmployee where employeecode='{NodeInfodetailPeople[i].PersonId}'").ToString(),
                                        IsSp = "1",
                                        DDOperatorId = txfClass.InsteadOperatorGuid,
                                        OperatorName = txfClass.InsteadOperatorName,
                                        BillClassId = txfClass.BillClassId,
                                        AuditingIdea = "同意",
                                        FeeType = txfClass.FeeType,
                                        Sign = sign
                                    };
                                    byte[] postdatabyte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(ad));
                                    webrequest.ContentLength = postdatabyte.Length;
                                    Stream stream;
                                    stream = webrequest.GetRequestStream();
                                    stream.Write(postdatabyte, 0, postdatabyte.Length);
                                    stream.Close();
                                    using (var httpWebResponse = webrequest.GetResponse())
                                    using (StreamReader responseStream = new StreamReader(httpWebResponse.GetResponseStream()))
                                    {
                                        String ret = responseStream.ReadToEnd();
                                    }
                                }).Invoke();
                            }
                        }
                    });
                }

                //如果是撤回重新提交的，删除之前的单子
                if (!string.IsNullOrEmpty(txfClass.OldBillNo))
                {
                    da.ExecSql($"delete EXPEOTHER where billno = '{txfClass.OldBillNo}'");
                    da.ExecSql($"delete EXPEENTEMENT where billno = '{txfClass.OldBillNo}'");
                    da.ExecSql($"delete approvalcomments where billno = '{txfClass.OldBillNo}'");
                    da.Dispose();
                    ToolsClass.TxtLog("交通费、通讯费、招待费等费用报销申请日志", "\r\n删除旧单据:" + $"delete EXPEOTHER where billno = '{txfClass.OldBillNo}'" + $"delete EXPEENTEMENT where billno = '{txfClass.OldBillNo}'" + "\r\n");
                }
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