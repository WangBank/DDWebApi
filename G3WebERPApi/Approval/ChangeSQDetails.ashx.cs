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
    /// ChangeSQDetails 的摘要说明
    /// </summary>
    public class ChangeSQDetails : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();
        private string CsJson = "";//获取请求json
        private DataTable dt = new DataTable();
        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private int isclf;
        private ChangeCLFBXSQRC changeCLFBXSQRC;
        private ChangeQTSQRC changeQTSQRC;
        private ChangeZDSQRC changeZDSQRC;
        private ChangeQTFYSQRC changeQTFYSQRC;

        public void ProcessRequest(HttpContext context)
        {
            //判断客户端请求是否为post方法
            if (context.Request.HttpMethod.ToUpper() != "POST")
            {
                context.Response.Write("{\"errmsg\":\"请求方式不允许,请使用POST方式(DD0001)\",\"errcode\":1}");
                return;
            }
            string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");

            da = new DbHelper.SqlHelper("SqlServer", connectionString);
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }

            Object jgobj = ToolsClass.DeserializeObject(CsJson);
            Hashtable returnhash = jgobj as Hashtable;
            if (CsJson == "")
            {
                context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                return;
            }

            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");

            ToolsClass.TxtLog("审批信息修改日志", context.Request.Path.ToLower() + "\r\n修改后的数据:" + CsJson + "\r\n");

            string path1 = context.Request.Path.Replace("Approval/ChangeSQDetails.ashx", "changeclfbxdetails");
            string path2 = context.Request.Path.Replace("Approval/ChangeSQDetails.ashx", "changeqitadetails");
            string path3 = context.Request.Path.Replace("Approval/ChangeSQDetails.ashx", "changezddetails");
            string path4 = context.Request.Path.Replace("Approval/ChangeSQDetails.ashx", "changeqtfydetails");
            //验证请求sign
            string sign1 = ToolsClass.md5(signUrl + path1 + "Romens1/DingDing2" + path1, 32);
            string sign2 = ToolsClass.md5(signUrl + path2 + "Romens1/DingDing2" + path2, 32);
            string sign3 = ToolsClass.md5(signUrl + path3 + "Romens1/DingDing2" + path3, 32);
            string sign4 = ToolsClass.md5(signUrl + path4 + "Romens1/DingDing2" + path4, 32);

            ToolsClass.TxtLog("生成的sign", "生成的" + "sign:" + sign1 + ";" + sign2 + ";" + sign3 + ";" + sign4 + ";" + "传入的sign" + returnhash["Sign"].ToString() + "\r\n");

            if (sign1 != returnhash["Sign"].ToString() && sign2 != returnhash["Sign"].ToString() && sign3 != returnhash["Sign"].ToString() && sign4 != returnhash["Sign"].ToString())
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }
            try
            {
                if (returnhash["BillNo"].ToString().Contains("CL"))
                {
                    //前端传入数据
                    changeCLFBXSQRC = new ChangeCLFBXSQRC();
                    changeCLFBXSQRC = (ChangeCLFBXSQRC)JsonConvert.DeserializeObject(CsJson, typeof(ChangeCLFBXSQRC));

                    isclf = 1;
                }
                else if (returnhash["BillNo"].ToString().Contains("TXF") || returnhash["BillNo"].ToString().Contains("JTF"))
                {
                    //前端传入数据
                    changeQTSQRC = new ChangeQTSQRC();
                    changeQTSQRC = (ChangeQTSQRC)JsonConvert.DeserializeObject(CsJson, typeof(ChangeQTSQRC));

                    isclf = 2;
                }
                else if (returnhash["BillNo"].ToString().Contains("ZDF"))
                {
                    //前端传入数据
                    changeZDSQRC = new ChangeZDSQRC();
                    changeZDSQRC = (ChangeZDSQRC)JsonConvert.DeserializeObject(CsJson, typeof(ChangeZDSQRC));

                    isclf = 3;
                }
                else if (returnhash["BillNo"].ToString().Contains("QTFY"))
                {
                    //前端传入数据
                    changeQTFYSQRC = new ChangeQTFYSQRC();
                    changeQTFYSQRC = (ChangeQTFYSQRC)JsonConvert.DeserializeObject(CsJson, typeof(ChangeQTFYSQRC));

                    isclf = 4;
                }
                string updatedetail = "";
                string updateCCnum = "";
                string insertLogMain = "";
                string insertLogDetail = "";

                if (isclf == 1)
                {
                    updateCCnum = $"update ExpeTrav set CCNum = CCNum -1 where billno = '{changeCLFBXSQRC.BillNo}'";
                    string guid = Guid.NewGuid().ToString();
                    //修改可修改次数减一
                    //保存修改的日志内容
                    insertLogMain = $"insert into BILLCHANGELOG(GUID,BILLTYPE,BILLNAME,DDID,SPPName,BILLNO,ChangeReason)  values('{guid}','{changeCLFBXSQRC.BillType}','差旅费报销','{changeCLFBXSQRC.SPPDDid}','{changeCLFBXSQRC.SPPName}','{changeCLFBXSQRC.BillNo}','{changeCLFBXSQRC.ChangeReason}')";
                    insertLogDetail = $"INSERT INTO ExpeTravDetailLOG(BillNo, Detailno, CustCode, DepaDate, RetuDate , DepaCity, DestCity, TranMode, TranCount, TranAmount, AlloDay,AlloPric,AlloAmount, AccoCount, AccoAmount , CityTrafCount , CityTraAmont , TotalAmount, GUID , OtherFee , GasAmount , HSRAmount , OffDay , TaxAmount , TripNo, DepaCity1, DestCity1, MAINGUID) SELECT BillNo, Detailno, CustCode, DepaDate, RetuDate , DepaCity, DestCity, TranMode, TranCount, TranAmount, AlloDay,AlloPric,AlloAmount, AccoCount, AccoAmount , CityTrafCount , CityTraAmont , TotalAmount, GUID , OtherFee , GasAmount , HSRAmount , OffDay , TaxAmount , TripNo, DepaCity1, DestCity1, '{guid}' FROM ExpeTravDetail  where  BillNo='{changeCLFBXSQRC.BillNo}'";

                    sqlList.Clear();
                    sqlList.Add(updateCCnum);
                    sqlList.Add(insertLogMain);
                    sqlList.Add(insertLogDetail);
                    da.ExecSql(sqlList);
                    sqlList.Clear();

                    #region 修改差旅费

                    //,isnull(B.TripNo,0) TripNo,convert(varchar(20),B.DepaDate,120) DepaDate,convert(varchar(20),B.RetuDate,120) RetuDate,B.DepaCity,B.DestCity,B.CustCode,e.CustName,B.DetailNo,B.AlloDay,B.OffDay,AlloPric, AlloAmount,OtherFee,B.TranMode,B.TranCount,TranAmount,GasAmount,HsrAmount,B.AccoCount,AccoAmount,B.CityTrafCount,CityTraAmont,TotalAmount
                    for (int i = 0; i < changeCLFBXSQRC.ExpeTravDetail.Length; i++)
                    {
                        double ta = 0.00;
                        ta = double.Parse(changeCLFBXSQRC.ExpeTravDetail[i].TranAmount) + double.Parse(changeCLFBXSQRC.ExpeTravDetail[i].AlloAmount) + double.Parse(changeCLFBXSQRC.ExpeTravDetail[i].OtherFee);
                        updatedetail = $"update ExpeTravDetail set TranCount ='{changeCLFBXSQRC.ExpeTravDetail[i].TranCount}',TranAmount='{changeCLFBXSQRC.ExpeTravDetail[i].TranAmount}',AlloDay='{changeCLFBXSQRC.ExpeTravDetail[i].AlloDay}',AlloPric='{changeCLFBXSQRC.ExpeTravDetail[i].AlloPric}',AlloAmount='{changeCLFBXSQRC.ExpeTravDetail[i].AlloAmount}',AccoCount='{changeCLFBXSQRC.ExpeTravDetail[i].AccoCount}',AccoAmount='{changeCLFBXSQRC.ExpeTravDetail[i].AccoAmount}',CityTrafCount='{changeCLFBXSQRC.ExpeTravDetail[i].CityTrafCount}',CityTraAmont='{changeCLFBXSQRC.ExpeTravDetail[i].CityTraAmont}',TotalAmount='{ta}',OtherFee='{changeCLFBXSQRC.ExpeTravDetail[i].OtherFee}',DepaDate='{changeCLFBXSQRC.ExpeTravDetail[i].DepaDate}',RetuDate='{changeCLFBXSQRC.ExpeTravDetail[i].RetuDate}',DepaCity='{changeCLFBXSQRC.ExpeTravDetail[i].DepaCity}',DestCity='{changeCLFBXSQRC.ExpeTravDetail[i].DestCity}',GasAmount='{changeCLFBXSQRC.ExpeTravDetail[i].GasAmount}',HSRAmount='{changeCLFBXSQRC.ExpeTravDetail[i].HsrAmount}',TranMode='{changeCLFBXSQRC.ExpeTravDetail[i].TranMode}',OffDay='{changeCLFBXSQRC.ExpeTravDetail[i].OffDay}'  where BillNo = '{changeCLFBXSQRC.BillNo}' and Detailno = '{changeCLFBXSQRC.ExpeTravDetail[i].DetailNo}' and TripNo='{changeCLFBXSQRC.ExpeTravDetail[i].TripNo}'";
                        sqlTou.Clear();
                        sqlTou.Append(updatedetail);
                        sqlList.Add(sqlTou.ToString());
                        ToolsClass.TxtLog("审批信息修改日志", "\r\n执行的sql语句:" + updatedetail + "\r\n");
                    }

                    string ss = da.ExecSql(sqlList);
                    da.ExecSql($"update ExpeTravDetail");
                    ToolsClass.TxtLog("审批信息修改日志", "\r\n执行的sql语句返回:" + ss + "\r\n");
                    if (ss == null)
                    {
                        context.Response.Write("{\"errmsg\":\"更新流程失败\",\"errcode\":1}");
                        return;
                    }

                    ToolsClass.TxtLog("审批信息修改日志", $"\r\n 修改单据明细:{updatedetail} \r\n  修改可修改次数：{updateCCnum}  \r\n    保存日志主表:{insertLogMain} \r\n   保存日志明细：{insertLogDetail}");

                    #endregion 修改差旅费
                }
                else if (isclf == 2)
                {
                    //修改可修改次数减一
                    updateCCnum = $"update ExpeOther set CCNum = CCNum -1 where billno = '{changeQTSQRC.BillNo}'";
                    string guid = Guid.NewGuid().ToString();

                    //保存修改的日志内容
                    insertLogMain = $"insert into BILLCHANGELOG(GUID,BILLTYPE,BILLNAME,DDID,SPPName,BILLNO,ChangeReason)  values('{guid}','{changeQTSQRC.BillType}','交通费、通讯费报销','{changeQTSQRC.SPPDDid}','{changeQTSQRC.SPPName}','{changeQTSQRC.BillNo}','{changeQTSQRC.ChangeReason}')";
                    insertLogDetail = $"INSERT INTO ExpeOtherLOG(BillNo,BillDate,FeeType ,ApplPers,BearOrga,BillCount,FeeAmount,Notes ,OperatorGuid,IsAuditing ,AuditingGUID ,AuditingDate,IsAccount ,AccountGUID ,AccountDate,REALDATE,ISREFER,REFERGUID,REFERDATE ,NoCountFee ,DDOperatorId,DDAuditingId ,AppendixUrl ,PictureUrl ,SelAuditingGuid ,SelAuditingName,CopypersonID ,CopyPersonName ,IsSp ,AuditingIdea,ProcessNodeInfo,Urls,DeptCode ,DeptName ,CCNum,MAINGUID) SELECT BillNo,BillDate,FeeType ,ApplPers,BearOrga,BillCount,FeeAmount,Notes ,OperatorGuid,IsAuditing ,AuditingGUID ,AuditingDate,IsAccount ,AccountGUID ,AccountDate,REALDATE,ISREFER,REFERGUID,REFERDATE ,NoCountFee ,DDOperatorId,DDAuditingId ,AppendixUrl ,PictureUrl ,SelAuditingGuid ,SelAuditingName,CopypersonID ,CopyPersonName ,IsSp ,AuditingIdea,ProcessNodeInfo,Urls,DeptCode ,DeptName ,CCNum, '{guid}' FROM ExpeOther  where  BillNo='{changeQTSQRC.BillNo}'";

                    sqlList.Clear();
                    sqlList.Add(updateCCnum);
                    sqlList.Add(insertLogMain);
                    sqlList.Add(insertLogDetail);
                    da.ExecSql(sqlList);

                    updatedetail = $"update ExpeOther set BillCount='{changeQTSQRC.BillCount}',FeeAmount='{changeQTSQRC.FeeAmount}'  where BillNo = '{changeQTSQRC.BillNo}'";

                    if (da.ExecSql(updatedetail) == null)
                    {
                        context.Response.Write("{\"errmsg\":\"更新流程失败\",\"errcode\":1}");
                        return;
                    }

                    ToolsClass.TxtLog("审批信息修改日志", $"\r\n 修改单据明细:{updatedetail} \r\n  修改可修改次数：{updateCCnum}  \r\n    保存日志主表:{insertLogMain} \r\n   保存日志明细：{insertLogDetail}");
                }
                else if (isclf == 3)
                {
                    //修改可修改次数减一
                    updateCCnum = $"update ExpeEnteMent set CCNum = CCNum -1 where billno = '{changeZDSQRC.BillNo}'";
                    string guid = Guid.NewGuid().ToString();

                    //保存修改的日志内容
                    insertLogMain = $"insert into BILLCHANGELOG(GUID,BILLTYPE,BILLNAME,DDID,SPPName,BILLNO,ChangeReason)  values('{guid}','{changeZDSQRC.BillType}','招待费报销','{changeZDSQRC.SPPDDid}','{changeZDSQRC.SPPName}','{changeZDSQRC.BillNo}','{changeZDSQRC.ChangeReason}')";
                    insertLogDetail = $"INSERT INTO ExpeEnteMentLOG( BillNo,BillDate,ApplPers,BearOrga ,CustCode ,BillCount,FeeAmount ,Notes ,OperatorGuid,IsAuditing ,AuditingGUID,AuditingDate ,IsAccount ,AccountGUID,AccountDate,REALDATE ,ISREFER ,REFERGUID ,REFERDATE ,NoCountFee,DDOperatorId,DDAuditingId,AppendixUrl,PictureUrl,SelAuditingGuid ,SelAuditingName ,CopypersonID ,CopyPersonName,IsSp ,AuditingIdea ,ProcessNodeInfo,Urls,DeptCode,DeptName ,CCNum,MAINGUID) SELECT BillNo,BillDate,ApplPers,BearOrga ,CustCode ,BillCount,FeeAmount ,Notes ,OperatorGuid,IsAuditing ,AuditingGUID,AuditingDate ,IsAccount ,AccountGUID,AccountDate,REALDATE ,ISREFER ,REFERGUID ,REFERDATE ,NoCountFee,DDOperatorId,DDAuditingId,AppendixUrl,PictureUrl,SelAuditingGuid ,SelAuditingName ,CopypersonID ,CopyPersonName,IsSp ,AuditingIdea ,ProcessNodeInfo,Urls,DeptCode,DeptName ,CCNum, '{guid}' FROM ExpeEnteMent  where  BillNo='{changeZDSQRC.BillNo}'";

                    sqlList.Clear();
                    sqlList.Add(updateCCnum);
                    sqlList.Add(insertLogMain);
                    sqlList.Add(insertLogDetail);
                    da.ExecSql(sqlList);

                    updatedetail = $"update ExpeEnteMent set BillCount='{changeZDSQRC.BillCount}',FeeAmount='{changeZDSQRC.FeeAmount}'  where BillNo = '{changeZDSQRC.BillNo}'";

                    if (da.ExecSql(updatedetail) == null)
                    {
                        context.Response.Write("{\"errmsg\":\"更新流程失败\",\"errcode\":1}");
                        return;
                    }

                    ToolsClass.TxtLog("审批信息修改日志", $"\r\n 修改单据明细:{updatedetail} \r\n  修改可修改次数：{updateCCnum}  \r\n    保存日志主表:{insertLogMain} \r\n   保存日志明细：{insertLogDetail}");
                }
                else if (isclf == 4)
                {
                    //修改可修改次数减一
                    updateCCnum = $"update ExpeOther set CCNum = CCNum -1 where billno = '{changeQTFYSQRC.BillNo}'";
                    string guid = Guid.NewGuid().ToString();

                    //保存修改的日志内容
                    insertLogMain = $"insert into BILLCHANGELOG(GUID,BILLTYPE,BILLNAME,DDID,SPPName,BILLNO,ChangeReason)  values('{guid}','{changeQTFYSQRC.BillType}','其他费用报销','{changeQTFYSQRC.SPPDDid}','{changeQTFYSQRC.SPPName}','{changeQTFYSQRC.BillNo}','{changeQTFYSQRC.ChangeReason}')";
                    insertLogDetail = $"INSERT INTO ExpeOtherLOG(BillNo,BillDate,FeeType ,ApplPers,BearOrga,BillCount,FeeAmount,Notes ,OperatorGuid,IsAuditing ,AuditingGUID ,AuditingDate,IsAccount ,AccountGUID ,AccountDate,REALDATE,ISREFER,REFERGUID,REFERDATE ,NoCountFee ,DDOperatorId,DDAuditingId ,AppendixUrl ,PictureUrl ,SelAuditingGuid ,SelAuditingName,CopypersonID ,CopyPersonName ,IsSp ,AuditingIdea,ProcessNodeInfo,Urls,DeptCode ,DeptName ,CCNum,MAINGUID) SELECT BillNo,BillDate,FeeType ,ApplPers,BearOrga,BillCount,FeeAmount,Notes ,OperatorGuid,IsAuditing ,AuditingGUID ,AuditingDate,IsAccount ,AccountGUID ,AccountDate,REALDATE,ISREFER,REFERGUID,REFERDATE ,NoCountFee ,DDOperatorId,DDAuditingId ,AppendixUrl ,PictureUrl ,SelAuditingGuid ,SelAuditingName,CopypersonID ,CopyPersonName ,IsSp ,AuditingIdea,ProcessNodeInfo,Urls,DeptCode ,DeptName ,CCNum, '{guid}' FROM ExpeOther  where  BillNo='{changeQTFYSQRC.BillNo}'";

                    sqlList.Clear();
                    sqlList.Add(updateCCnum);
                    sqlList.Add(insertLogMain);
                    sqlList.Add(insertLogDetail);
                    ToolsClass.TxtLog("审批信息修改日志", $"\r\n 修改可修改次数：{updateCCnum}  \r\n    保存日志主表:{insertLogMain} \r\n   保存日志明细：{insertLogDetail}");
                    da.ExecSql(sqlList);

                    sqlList.Clear();
                    for (int i = 0; i < changeQTFYSQRC.OtherCostSQModels.Count; i++)
                    {
                        updatedetail = $"insert into ExpeOtherDetailLOG(BillNo,MAINGUID,BillCount,BillAmount,FeeTypeDetail) values('{changeQTFYSQRC.BillNo}','{guid}','{changeQTFYSQRC.OtherCostSQModels[i].Count}','{changeQTFYSQRC.OtherCostSQModels[i].Amount}','{changeQTFYSQRC.OtherCostSQModels[i].FType}')";
                        sqlList.Add(updatedetail);
                        ToolsClass.TxtLog("审批信息修改日志", "\r\n执行的sql语句:" + updatedetail + "\r\n");
                    }
                    da.ExecSql(sqlList);
                    sqlList.Clear();
                    updatedetail = $"update ExpeOther set BillCount='{changeQTFYSQRC.BillCount}',FeeAmount='{changeQTFYSQRC.FeeAmount}'  where BillNo = '{changeQTFYSQRC.BillNo}'";
                    da.ExecSql(updatedetail);
                    ToolsClass.TxtLog("审批信息修改日志", "\r\n执行的sql语句:" + updatedetail + "\r\n");
                    for (int i = 0; i < changeQTFYSQRC.OtherCostSQModels.Count; i++)
                    {
                        updatedetail = string.Empty;
                        updatedetail = $"update ExpeOtherDetail set BillCount ='{changeQTFYSQRC.OtherCostSQModels[i].Count}',BillAmount='{changeQTFYSQRC.OtherCostSQModels[i].Amount}'  where BillNo = '{changeQTFYSQRC.BillNo}' and FeeTypeDetail = '{changeQTFYSQRC.OtherCostSQModels[i].FType}'";
                        sqlTou.Clear();
                        sqlTou.Append(updatedetail);
                        sqlList.Add(sqlTou.ToString());
                        ToolsClass.TxtLog("审批信息修改日志", "\r\n执行的sql语句:" + updatedetail + "\r\n");
                    }
                    da.ExecSql(sqlList);
                    if (da.ExecSql(updatedetail) == null)
                    {
                        context.Response.Write("{\"errmsg\":\"更新流程失败\",\"errcode\":1}");
                        return;
                    }
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