using G3WebERPApi.Common;
using G3WebERPApi.Model;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi
{
    /// <summary>
    /// Select 的摘要说明
    /// </summary>
    public class Select : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();
        private string sql = string.Empty;
        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private string url = string.Empty;
        private object obj;

        private string selType = "";//查询类型
        private string selValue = "";//查询值
        private string CsJson = "";
        private string isWrite = "0";//是否保存日志
        private string IsLeader = "";
        private DataTable dt = new DataTable();
        private StringBuilder FhJson = new StringBuilder();//返回JSON

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

            isWrite = ToolsClass.GetConfig("isWrite");

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
                ToolsClass.TxtLog("查询组织信息日志", "\r\n入参" + CsJson + "\r\n");
                context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                return;
            }

            string path = context.Request.Path.Replace("Select.ashx", "selinfo");
            //验证请求sign
            string sign = ToolsClass.md5(signUrl + path + "Romens1/DingDing2" + path, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + sign + "传入的sign" + returnhash["Sign"].ToString() + "\r\n 后台字符串:" + signUrl + path + "Romens1/DingDing2" + path);
            if (sign != returnhash["Sign"].ToString())
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }

            selType = returnhash["TypeId"].ToString();
            selValue = returnhash["Value"].ToString();
            if (selType == "SelOrgAndAllEmployee01")
            {
                IsLeader = returnhash["IsLeader"].ToString();
            }
            if (isWrite == "1")
            {
                ToolsClass.TxtLog("查询组织信息日志", "\r\n入参TypeId:" + CsJson + "\r\n");
            }

            if (selType == "GetAuditingState")
            {
                sql = $"select count(*) from  (select a.BillNo BillNo,a.IsAccount, b.BillCount,b.FeeAmount,isnull(a.IsSp, 0) IsSp,C.employeename + '提交的雨诺差旅费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,12 FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPerson,'' CustName,A.AuditingIdea ,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState from  ExpeTrav a left join(SELECT BILLNO, SUM(TOTALAMOUNT) FeeAmount, sum(TranCount)+sum(AccoCount) + sum(CityTrafCount) BillCount FROM EXPETRAVDETAIL GROUP BY BILLNO) b ON A.BILLNO = b.BILLNO left join FlowEmployee c on a.InsteadOperatorGuid = c.ddid union select a.BillNo BillNo, a.IsAccount,a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺' + (case when a.FeeType = '01' then '交通费' when a.FeeType = '02' then '通讯费' when a.FeeType = '03' then '车辆费' when a.FeeType = '04' then '房租费' when a.FeeType = '05' then '水费' when a.FeeType = '06' then '电费'  when a.FeeType = '07' then '其他费用' end)+'审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,A.FeeType FType,  BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson,'' CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState from EXPEOTHER a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid union select a.BillNo BillNo,a.IsAccount, a.BillCount,a.FeeAmount,isnull(a.IsSp, 0) IsSp,B.employeename + '提交的雨诺招待费审批' + (case when isnull(a.IsSp,0)= 1 then '[已同意]' when isnull(a.IsSp,0)= 2 then '[已驳回]' else '' end ) Title,'00' FType, BillDate,a.Notes,a.DDOperatorId,a.SelAuditingGuid,a.CopyPersonID CopyPerson, c.CustName,A.AuditingIdea,a.IsInsteadApply,isnull(a.InsteadOperatorGuid,'') InsteadOperatorGuid,a.HangState from EXPEENTEMENT a left join FlowEmployee b on a.InsteadOperatorGuid = b.ddid left join Customer c on a.CustCode = c.CustCode) t where BillNo in (select BillNo from ApprovalComments where ApprovalID = (select distinct EmployeeCode from FlowEmployee where ddid = '{selValue}') and persontype = 2 and ApprovalStatus = '0' and BillNo<> '')   and issp = '0'";
                string result = JsonConvert.SerializeObject(new PublicResult
                {
                    errcode = "0",
                    errmsg = "ok",
                    Auditingstate = da.GetDataTable(sql).Rows.Count <= 0 ? "0" : "1",
                    BillNos = da.GetDataTable(sql).Rows.Count <= 0 ? "0" : da.GetValue(sql).ToString()
                });
                context.Response.Write(result);
                return;
            }

            #region 模糊查询客户信息

            if (selType == "SelCustom01")
            {
                try
                {
                    if (selValue.Length < 3)
                    {
                        context.Response.Write("{\"errmsg\":\"请至少输入三个字符(DD1001)\",\"errcode\":1}");
                        return;
                    }
                    sql = "select CustCode,CustName from customer where custname like '%" + selValue + "%'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"count\":").Append(dt.Rows.Count).Append(",\"data\":[");
                    if (dt.Rows.Count > 0)
                    {
                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                FhJson.Append(",");
                            }
                            var dtcountday = da.GetDataTable($"select sum(cast(isnull(replace(replace(YXQType,'天',''),'永久有效',''),0) as int)) countday from MEDCONFIG where isnull(iswrite,0)=1 and  CusCode='{dt.Rows[x]["CustCode"].ToString()}' group by CusCode  ");
                            string scountday = dtcountday.Rows.Count == 0 ? null : dtcountday.Rows[0]["countday"].ToString();
                            int countday = string.IsNullOrEmpty(scountday) ? 0 : int.Parse(scountday);

                            if (da.GetDataTable($"select billno from medconfig where CusCode='{dt.Rows[x]["CustCode"].ToString()}'").Rows.Count == 0)
                            {
                                FhJson.Append("{\"CustCode\":\"").Append(dt.Rows[x]["CustCode"].ToString())
                                    .Append("\",\"CustName\":\"").Append(dt.Rows[x]["CustName"].ToString())
                                     .Append("\",\"MedConfig180\":\"").Append(0)
                                    .Append("\"}");
                            }
                            else
                            {
                                int countdayss = countday >= 180 ? 1 : 0;

                                FhJson.Append("{\"CustCode\":\"").Append(dt.Rows[x]["CustCode"].ToString())
                                    .Append("\",\"CustName\":\"").Append(dt.Rows[x]["CustName"].ToString())
                                     .Append("\",\"MedConfig180\":\"").Append(countdayss)
                                    .Append("\"}");
                            }
                        }
                    }
                    FhJson.Append("]}");

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询客户信息报错" + ex.Message+"\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 模糊查询客户信息

            #region CODE查询客户信息

            else if (selType == "SelCustom02")
            {
                try
                {
                    sql = "select CustCode,CustName from customer where custcode='" + selValue + "'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"count\":").Append(dt.Rows.Count).Append(",\"data\":[");
                    if (dt.Rows.Count > 0)
                    {
                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                FhJson.Append(",");
                            }

                            int countday = int.Parse(da.GetValue($"select sum(cast(isnull(replace(replace(YXQType,'天',''),'永久有效',''),0) as int)) countday from MEDCONFIG where isnull(iswrite,0)=1 and  CusCode='{dt.Rows[x]["CustCode"].ToString()}' group by CusCode  ").ToString()) >= 180 ? 1 : 0;

                            FhJson.Append("{\"CustCode\":\"").Append(dt.Rows[x]["CustCode"].ToString())
                                .Append("\",\"CustName\":\"").Append(dt.Rows[x]["CustName"].ToString())
                                 .Append("\",\"MedConfig180\":\"").Append(countday)
                                .Append("\"}");
                        }
                    }
                    FhJson.Append("]}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("查询客户信息", "\r\n返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询客户信息报错" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion CODE查询客户信息

            #region 查询医保类型

            else if (selType == "GetTypeOfHealthInsurance")
            {
                try
                {
                    sql = string.Empty;
                    sql = "select selectsql from dataselectdefine where  DataSelectType = '20190930001'";
                    string selectsql = da.GetValue(sql).ToString().Replace("[选择条件]", $" and(code like'%{selValue}%' or name like '%{selValue}%') ");
                    dt = da.GetDataTable(selectsql);
                    List<HealthInsurance> data = new List<HealthInsurance>();
                    if (dt.Rows.Count == 0)
                    {
                        context.Response.Write(JsonConvert.SerializeObject(new HealthInsuranceResponse
                        {
                            errcode = "0",
                            errmsg = "暂无可用医保类型，请检查相应的数据源"
                        }));
                        return;
                    }
                    else
                    {
                        for (int i = 0; i < dt.Rows.Count; i++)
                        {
                            data.Add(new HealthInsurance
                            {
                                Status = false,
                                MedName = dt.Rows[i]["名称"].ToString(),
                                MedType = dt.Rows[i]["代码"].ToString()
                            });
                        }
                    }

                    string HealthInsuranceResponseJson = JsonConvert.SerializeObject(new HealthInsuranceResponse
                    {
                        errcode = "0",
                        errmsg = "查询成功",
                        data = data
                    });

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("医保类型查询", $"\r\n返回前端json:\r\n{HealthInsuranceResponseJson}");
                    }
                    context.Response.Write(HealthInsuranceResponseJson);
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"医保类型查询报错"  + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询医保类型

            #region 查询用户组织机构信息

            else if (selType == "SelOrg01")
            {
                try
                {
                    sql = "SELECT CODE OrgCode,NAME OrgName FROM ORGANIZATION WHERE ISNULL(ISFORBIDDEN,0)=0  AND  NAME like '%" + selValue + "%'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Detail\":[");

                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"OrgCode\":\"").Append(dt.Rows[x]["OrgCode"].ToString())
                                .Append("\",\"OrgName\":\"").Append(dt.Rows[x]["OrgName"].ToString())
                                .Append("\"}");
                        }
                        FhJson.Append("]}");
                    }
                    else
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"\",\"OrgName\":\"\"]}");
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch
                {
                    context.Response.Write("{\"errmsg\":\"查询机构信息报错(DD1006)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询用户组织机构信息

            #region 根据TypeId、isLeade、Value来决定查询方式

            else if (selType == "SelOrgAndAllEmployee01")
            {
                try
                {
                    //按照角色查询
                    if (selValue == "1")
                    {
                        sql = "select RoleId,RoleName from role ";
                        obj = da.GetDataTable(sql);
                        dt = obj as DataTable;
                        string sqlEmployees = "";
                        if (IsLeader == "0")
                        {
                            sqlEmployees = "select distinct a.employeecode EmployeeCode,a.employeename EmployeeName,a.IsLeader IsLeader,b.Roleid  RoleId from FlowEmployee a join EmpsRoleId b on a.employeecode = b.employeecode  where   a.disable = '0' and ISNULL(a.DDId,0) !='0' and b.status = '1'";
                        }
                        else if (IsLeader == "1")
                        {
                            sqlEmployees = "select distinct a.employeecode EmployeeCode,a.employeename EmployeeName,a.IsLeader IsLeader,b.Roleid  RoleId from FlowEmployee a join EmpsRoleId b on a.employeecode = b.employeecode  where   a.disable = '0' and ISNULL(a.DDId,0) !='0' and b.status = '1' and a.isleader != '0'";
                        }
                        DataTable dtEmployees = da.GetDataTable(sqlEmployees);
                        if (dt.Rows.Count > 0)
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"Detail\":[");

                            for (int x = 0; x < dt.Rows.Count; x++)
                            {
                                if (x > 0)
                                {
                                    FhJson.Append(",");
                                }
                                FhJson.Append("{\"RoleCode\":\"").Append(dt.Rows[x]["RoleId"].ToString())
                                    .Append("\",\"RoleName\":\"").Append(dt.Rows[x]["RoleName"].ToString())
                                    .Append("\",\"RoleEmployees\":[");
                                DataRow[] tmptable = dtEmployees.Select($"RoleId = '{dt.Rows[x]["RoleId"]}'");
                                //将各个角色的信息包装到里面
                                for (int i = 0; i < tmptable.Length; i++)
                                {
                                    if (i > 0)
                                    {
                                        FhJson.Append(",");
                                    }
                                    FhJson.Append("{\"EmployeeCode\":\"").Append(tmptable[i]["EmployeeCode"].ToString())
                                        .Append("\",\"EmployeeName\":\"").Append(tmptable[i]["EmployeeName"].ToString())
                                        .Append("\",\"IsLeader\":\"").Append(tmptable[i]["IsLeader"].ToString())
                                        .Append("\"}");
                                }
                                FhJson.Append("]}");
                            }
                            FhJson.Append("]}");
                        }
                        else
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"\",\"OrgName\":\"\"]}");
                        }

                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("查询组织信息日志", "\r\n返回:" + FhJson.ToString() + "\r\n");
                        }
                        context.Response.Write(FhJson.ToString());
                        return;
                    }

                    //树状结构按部门查询
                    else if (selValue == "3")
                    {
                        sql = "SELECT CODE OrgCode,NAME OrgName,ParentGuid FROM ORGANIZATION WHERE ISNULL(Disable,0)=0";
                        obj = da.GetDataTable(sql);
                        dt = obj as DataTable;
                        string sqlEmployees = "";
                        if (IsLeader == "0")
                        {
                            sqlEmployees = "select a.employeecode EmployeeCode,a.employeename EmployeeName,a.orgcode OrgCode,b.name OrgName,a.IsLeader IsLeader from FlowEmployee a left join organization b on a.orgcode = b.Code  where a.disable ='0' and ISNULL(a.DDId,0) !='0'";
                        }
                        else if (IsLeader == "1")
                        {
                            sqlEmployees = $"select a.employeecode EmployeeCode,a.employeename EmployeeName,a.orgcode OrgCode,b.name OrgName,a.IsLeader IsLeader from FlowEmployee a left join organization b on a.orgcode = b.Code  where  a.IsLeader!='0'  and  a.disable ='0' and ISNULL(a.DDId,0) !='0'";
                        }
                        DataTable dtEmployees = da.GetDataTable(sqlEmployees);
                        if (dt.Rows.Count > 0)
                        {
                            sql = "";
                            sql = "SELECT max(DATALENGTH(guid)) FROM[RomensManage].[dbo].[Organization] ";
                            int MaxCount = (int.Parse(da.GetValue(sql).ToString()) + 1) / 3 - 1;
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"MaxCount\":\"");
                            FhJson.Append(MaxCount.ToString());
                            FhJson.Append("\",\"Detail\":[");
                            for (int x = 0; x < dt.Rows.Count; x++)
                            {
                                if (x > 0)
                                {
                                    FhJson.Append(",");
                                }
                                FhJson.Append("{\"OrgCode\":\"").Append(dt.Rows[x]["OrgCode"].ToString())
                                    .Append("\",\"OrgName\":\"").Append(dt.Rows[x]["OrgName"].ToString())
                                    .Append("\",\"OrgEmployees\":[");
                                DataRow[] tmptable = dtEmployees.Select($"OrgCode = '{dt.Rows[x]["OrgCode"]}'");
                                //DataRow[] tmptable = dtEmployees.Select();
                                //将各个部门的信息包装到里面
                                for (int i = 0; i < tmptable.Length; i++)
                                {
                                    if (i > 0)
                                    {
                                        FhJson.Append(",");
                                    }
                                    FhJson.Append("{\"EmployeeCode\":\"").Append(tmptable[i]["EmployeeCode"].ToString())
                                        .Append("\",\"EmployeeName\":\"").Append(tmptable[i]["EmployeeName"].ToString())
                                        .Append("\",\"IsLeader\":\"").Append(tmptable[i]["IsLeader"].ToString())
                                        .Append("\"}");
                                }
                                FhJson.Append("]}");
                            }
                            FhJson.Append("]}");
                        }
                        else
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"\",\"OrgName\":\"\"]}");
                        }

                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                        }
                        context.Response.Write(FhJson.ToString());
                        return;
                    }

                    //按照部门查询
                    else
                    {
                        sql = "SELECT CODE OrgCode,NAME OrgName,ParentGuid FROM ORGANIZATION WHERE ISNULL(Disable,0)=0";
                        obj = da.GetDataTable(sql);
                        dt = obj as DataTable;
                        string sqlEmployees = "";
                        if (IsLeader == "0")
                        {
                            sqlEmployees = "select a.employeecode EmployeeCode,a.employeename EmployeeName,a.orgcode OrgCode,b.name OrgName,a.IsLeader IsLeader from FlowEmployee a left join organization b on a.orgcode = b.Code  where a.disable ='0' and ISNULL(a.DDId,0) !='0'";
                        }
                        else if (IsLeader == "1")
                        {
                            sqlEmployees = $"select a.employeecode EmployeeCode,a.employeename EmployeeName,a.orgcode OrgCode,b.name OrgName,a.IsLeader IsLeader from FlowEmployee a left join organization b on a.orgcode = b.Code  where  a.IsLeader!='0'  and  a.disable ='0' and ISNULL(a.DDId,0) !='0'";
                        }
                        DataTable dtEmployees = da.GetDataTable(sqlEmployees);
                        if (dt.Rows.Count > 0)
                        {
                            sql = "SELECT max(DATALENGTH(guid)) FROM[RomensManage].[dbo].[Organization] ";
                            int MaxCount = (int.Parse(da.GetValue(sql).ToString()) + 1) / 3 - 1;
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"MaxCount\":\"");
                            FhJson.Append(MaxCount.ToString());
                            FhJson.Append("\",\"Detail\":[");
                            for (int x = 0; x < dt.Rows.Count; x++)
                            {
                                if (x > 0)
                                {
                                    FhJson.Append(",");
                                }
                                FhJson.Append("{\"OrgCode\":\"").Append(dt.Rows[x]["OrgCode"].ToString())
                                    .Append("\",\"OrgName\":\"").Append(dt.Rows[x]["OrgName"].ToString())
                                    .Append("\",\"OrgEmployees\":[");
                                DataRow[] tmptable = dtEmployees.Select($"OrgCode = '{dt.Rows[x]["OrgCode"]}'");
                                //DataRow[] tmptable = dtEmployees.Select();
                                //将各个部门的信息包装到里面
                                for (int i = 0; i < tmptable.Length; i++)
                                {
                                    if (i > 0)
                                    {
                                        FhJson.Append(",");
                                    }
                                    FhJson.Append("{\"EmployeeCode\":\"").Append(tmptable[i]["EmployeeCode"].ToString())
                                        .Append("\",\"EmployeeName\":\"").Append(tmptable[i]["EmployeeName"].ToString())
                                        .Append("\",\"IsLeader\":\"").Append(tmptable[i]["IsLeader"].ToString())
                                        .Append("\"}");
                                }
                                FhJson.Append("]}");
                            }
                            FhJson.Append("]}");
                        }
                        else
                        {
                            FhJson.Clear();
                            FhJson.Append("{\"errmsg\":\"查询数据为空\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"\",\"OrgName\":\"\"]}");
                        }

                        if (isWrite == "1")
                        {
                            ToolsClass.TxtLog("DDLog", "\r\nSelect=>返回:" + FhJson.ToString() + "\r\n");
                        }
                        context.Response.Write(FhJson.ToString());
                        return;
                    }
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 根据TypeId、isLeade、Value来决定查询方式

            else
            {
                context.Response.Write("{\"errmsg\":\"查询其他信息请关机(DD1000)\",\"errcode\":1}");
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