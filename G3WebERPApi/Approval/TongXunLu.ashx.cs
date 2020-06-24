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

namespace G3WebERPApi.Approval
{
    /// <summary>
    /// 组织部门管理接口
    /// </summary>
    public class TongXunLu : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private BankDbHelper.SqlHelper da;
        private ArrayList sqlList = new ArrayList();
        private string sql = string.Empty;
        private StringBuilder sqlTou = new StringBuilder();
        private StringBuilder sqlTi = new StringBuilder();
        private string url = string.Empty;
        private object obj;
        private string ymadk = System.Configuration.ConfigurationManager.AppSettings["ymadk"].ToString() + "/";
        private string selType = "";//查询类型
        private string selValue = "";//查询值
        private string ddid = "";//当前操作人员的钉钉id
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

            //数据库链接
            connectionString = ToolsClass.GetConfig("DataOnLine");
            string leaderName = ToolsClass.GetConfig("leaderName");
            //sqlServer
            da = new BankDbHelper.SqlHelper("SqlServer", connectionString);

            isWrite = ToolsClass.GetConfig("isWrite");

            //获取请求json
            using (var reader = new StreamReader(context.Request.InputStream, Encoding.UTF8))
            {
                CsJson = reader.ReadToEnd();
            }
            string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
            if (CsJson == "")
            {
                context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                return;
            }
            CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
            DeptAndPeopleInfo dapi = new DeptAndPeopleInfo();
            dapi = (DeptAndPeopleInfo)JsonConvert.DeserializeObject(CsJson, typeof(DeptAndPeopleInfo));

            string path1 = context.Request.Path.Replace("Approval/TongXunLu.ashx", "dept");
            string path2 = context.Request.Path.Replace("Approval/TongXunLu.ashx", "role");
            string path3 = context.Request.Path.Replace("Approval/TongXunLu.ashx", "people");
            //验证请求sign
            string sign1 = ToolsClass.md5(signUrl + path1 + "Romens1/DingDing2" + path1, 32);
            string sign2 = ToolsClass.md5(signUrl + path2 + "Romens1/DingDing2" + path2, 32);
            string sign3 = ToolsClass.md5(signUrl + path3 + "Romens1/DingDing2" + path3, 32);
            ToolsClass.TxtLog("生成的sign", "生成的" + "sign1:" + sign1 + "sign2:" + sign2 + "sign3:" + sign3 + "传入的sign" + dapi.Sign + "\r\n 后台字符串:" + signUrl + path3 + "Romens1/DingDing2" + path3);
            if (sign1 != dapi.Sign && sign2 != dapi.Sign && sign3 != dapi.Sign)
            {
                context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                return;
            }

            if (isWrite == "1")
            {
                ToolsClass.TxtLog("修改审批架构信息日志", "\r\n入参" + CsJson + "\r\n");
            }

            #region 增加部门  Type DeptName FatherId

            if (dapi.Type == "deptadd")
            {
                try
                {
                    sql = $"select count(*) from Organization where ParentGuid ='{dapi.FatherId}'";
                    int nowcout = int.Parse(da.GetValue(sql).ToString()) + 1;
                    string nowcounts = dapi.FatherId + "-" + nowcout.ToString().PadLeft(2, '0');
                    sql = "";
                    sql = $"insert into Organization(Guid,ParentGuid,Code,Name,FinanceCode)  values('{nowcounts}','{dapi.FatherId}','{nowcounts}','{dapi.DeptName}','{nowcounts}') ";
                    da.ExecSql(sql);
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Detail\":[{\"OrgCode\":\"" + nowcounts + "\",\"OrgName\":\"" + dapi.DeptName + "\"}]");

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n增加部门信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"增加部门信息报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 增加部门  Type DeptName FatherId

            #region 增加人员  Type DeptCode  EmployeeCode EmployeeName DDId IsLeader

            if (dapi.Type == "peopleadd")
            {
                try
                {
                    sql = "";
                    sql = $"insert into flowemployee(Guid,employeecode,employeename,orgcode,ddid,isleader)  values('{Guid.NewGuid().ToString()}','{dapi.EmployeeCode}','{dapi.EmployeeName}','{dapi.DeptCode}','{dapi.DDId}','{dapi.IsLeader}') ";
                    da.ExecSql(sql);
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n增加人员信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"增加人员信息报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 增加人员  Type DeptCode  EmployeeCode EmployeeName DDId IsLeader

            #region 查询所有部门  Type

            if (dapi.Type == "deptSelect")
            {
                try
                {
                    sql = $"select Guid,Name,ParentGuid  from Organization where isNull(disable,'0') ='0'";
                    DataTable depts = da.GetDataTable(sql);
                    List<Organization> Depts = new List<Organization>();
                    OrgModel orgModel = new OrgModel();
                    for (int i = 0; i < depts.Rows.Count; i++)
                    {
                        Depts.Add(new Organization {
                        OrgCode = depts.Rows[i]["Guid"].SqlDataBankToString(),
                        OrgName = depts.Rows[i]["Name"].SqlDataBankToString(),
                        ParentGuid = depts.Rows[i]["ParentGuid"].SqlDataBankToString()
                        });
                    }
                    orgModel.errcode = "0";
                    orgModel.errmsg = "查询成功";
                    orgModel.Depts = Depts;
                    string deptResult = JsonConvert.SerializeObject(orgModel);
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("Query dept ", $"\r\n查询部门信息返回:{deptResult}\r\n");
                    }
                    context.Response.Write(deptResult);
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write(JsonConvert.SerializeObject(new CommonModel { 
                        errcode ="1" ,
                        errmsg = "查询部门信息报错"
                    }));
                    return;
                }
            }

            #endregion 查询所有部门  Type

            #region 增加角色组 Type RoleGroupName Remarks

            if (dapi.Type == "roleGroupAdd")
            {
                try
                {
                    sql = "";
                    sql = $"insert into RoleGroup(RoleGroupId,RoleGroupName,Remarks)  values('{Guid.NewGuid().ToString()}','{dapi.RoleGroupName}','{dapi.Remarks}')";
                    da.ExecSql(sql);
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n增加角色组信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"增加角色组信息报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 增加角色组 Type RoleGroupName Remarks

            #region 增加角色 Type RoleName Remarks RoleGroupCode,RoleCode Depts  增加role 增加rolewithorg

            if (dapi.Type == "roleAdd")
            {
                try
                {
                    sql = "";
                    string roleid = Guid.NewGuid().ToString();
                    sql = $"insert into Role(RoleId,RoleName,Remarks,RoleGroupId)  values('{roleid}','{dapi.RoleName}','{dapi.Remarks}','{dapi.RoleGroupCode}') ";
                    if (dapi.IsAll == "1")
                    {
                        //批量插入RoleWithOrg表
                        StringBuilder sqlInsert = new StringBuilder();
                        DataTable depts = da.GetDataTable("select distinct guid from Organization where isnull(IsForbidden,0) !='1'");
                        sqlInsert.Append("insert into RoleWithOrg(RoleId,OrgCode)  values");
                        for (int i = 0; i < depts.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                sqlInsert.Append(",");
                            }
                            sqlInsert.Append($"('{roleid}','{depts.Rows[i]["guid"]}')");
                        }
                        da.ExecSql(sqlInsert.ToString());
                        da.ExecSql(sql);
                        FhJson.Clear();
                    }
                    else
                    {
                        //批量插入RoleWithOrg表
                        StringBuilder sqlInsert = new StringBuilder();
                        sqlInsert.Append("insert into RoleWithOrg(RoleId,OrgCode)  values");
                        for (int i = 0; i < dapi.Depts.Length; i++)
                        {
                            if (i > 0)
                            {
                                sqlInsert.Append(",");
                            }
                            sqlInsert.Append($"('{roleid}','{dapi.Depts[i].DeptCode}')");
                        }
                        da.ExecSql(sqlInsert.ToString());
                        da.ExecSql(sql);
                        FhJson.Clear();
                    }
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n增加角色信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"增加角色信息报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 增加角色 Type RoleName Remarks RoleGroupCode,RoleCode Depts  增加role 增加rolewithorg

            #region 删除角色 Type RoleCode  删除role  删除rolewithorg

            if (dapi.Type == "roleRemove")
            {
                try
                {
                    sql = "";
                    sql = $"select count(Roleid) from EmpsRoleId where Roleid='{dapi.RoleCode}' and status ='1'";
                    string count = da.GetValue(sql).ToString();
                    if (count != "0")
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"此角色有成员正在使用，不能删除\",\"errcode\":\"1\"}");
                        context.Response.Write(FhJson.ToString());
                        return;
                    }
                    else
                    {
                        if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}'").ToString() == "集团财务")
                        {
                            da.ExecSql($"delete  rolewithemp  where PersonId = '{dapi.EmployeeCode}' and type= '1'");
                        }
                        if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}'").ToString() == "出纳")
                        {
                            da.ExecSql($"delete  rolewithemp  where PersonId = '{dapi.EmployeeCode}' and type= '2'");
                        }
                        sql = "";
                        sql = $"update EmpsRoleId set status ='0' where Roleid ='{dapi.RoleCode}'";
                        da.ExecSql(sql);
                        da.ExecSql($"update rolewithorg  set status ='0' where roleid ='{dapi.RoleCode}'");
                        da.ExecSql($"update role  set status ='0' where roleid ='{dapi.RoleCode}'");
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    }
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n删除角色信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"删除角色信息报错(DD1002):" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 删除角色 Type RoleCode  删除role  删除rolewithorg

            #region 设置角色 Type EmployeeCode RoleCode  EmployeeName

            if (dapi.Type == "SetRole")
            {
                try
                {
                    sql = "";
                    sql = $"insert into EmpsRoleId(RoleId,employeecode) values('{dapi.RoleCode}','{dapi.EmployeeCode}')";

                    da.ExecSql(sql);
                    if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}' and status ='1'").ToString() == "集团财务")
                    {
                        da.ExecSql($"insert into rolewithemp(type,PersonId,PersonName) values('1','{dapi.EmployeeCode}','{dapi.EmployeeName}')");
                    }
                    if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}' and status ='1'").ToString() == "出纳")
                    {
                        da.ExecSql($"insert into rolewithemp(type,PersonId,PersonName) values('2','{dapi.EmployeeCode}','{dapi.EmployeeName}')");
                    }

                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n设置角色返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"设置角色信息报错(DD1004)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 设置角色 Type EmployeeCode RoleCode  EmployeeName

            //{"Type":"RemoveEmpRole","RoleCode":"294efa53-f23f-4c42-8685-358a303c7398","EmployeeCode":"10653","Sign":"5F85B5FDED6FDD8573BD30E2408711E3"}

            #region 移除角色 Type EmployeeCode RoleCode DeptCode

            if (dapi.Type == "RemoveEmpRole")
            {
                try
                {
                    sql = "";
                    sql = $"update EmpsRoleId set status ='0' where employeecode ='{dapi.EmployeeCode}' and roleid ='{dapi.RoleCode}' and status = '1'";
                    da.ExecSql(sql);
                    if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}'").ToString() == "集团财务")
                    {
                        da.ExecSql($"delete  rolewithemp  where PersonId = '{dapi.EmployeeCode}' and type= '1'");
                    }
                    if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}'").ToString() == "出纳")
                    {
                        da.ExecSql($"delete  rolewithemp  where PersonId = '{dapi.EmployeeCode}' and type= '2'");
                    }

                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n移除角色返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"移除角色信息报错(DD1004)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 移除角色 Type EmployeeCode RoleCode DeptCode

            #region 查询角色组 Type

            if (dapi.Type == "SelectRoleGroup")
            {
                try
                {
                    sql = "SELECT  RoleGroupId,RoleGroupName FROM RoleGroup where status ='1'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"count\":").Append(dt.Rows.Count).Append(",\"data\":[");
                    if (dt.Rows.Count > 0)
                    {
                        for (int x = 0; x < dt.Rows.Count; x++)
                        {
                            if (x > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{");
                            for (int y = 0; y < dt.Columns.Count; y++)
                            {
                                if (y > 0)
                                {
                                    FhJson.Append(",");
                                }
                                FhJson.Append("\"").Append(dt.Columns[y].ColumnName).Append("\":\"").Append(dt.Rows[x][dt.Columns[y].ColumnName].ToString()).Append("\"");
                            }
                            FhJson.Append("}");
                        }
                    }
                    FhJson.Append("]}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n查询角色返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询角色信息报错(DD1004)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询角色组 Type

            #region 给角色增加人员 Type Employees RoleCode

            if (dapi.Type == "addpeopletorole")
            {
                try
                {
                    //批量插入RoleWithOrg表
                    StringBuilder sqlInsert = new StringBuilder();
                    sqlInsert.Append("insert into EmpsRoleId(RoleId,employeecode)  values");
                    for (int i = 0; i < dapi.Employees.Length; i++)
                    {
                        if (i > 0)
                        {
                            sqlInsert.Append(",");
                        }
                        if (da.GetDataTable($"select * from EmpsRoleId where RoleId='{dapi.RoleCode}' and employeecode='{dapi.Employees[i].EmployeeCode}' and status ='1'").Rows.Count > 0)
                        {
                            context.Response.Write("{\"errmsg\":\"当前角色已经存在此人员！\",\"errcode\":1}");
                            return;
                        }
                        sqlInsert.Append($"('{dapi.RoleCode}','{dapi.Employees[i].EmployeeCode}')");
                    }
                    da.ExecSql(sqlInsert.ToString());

                    if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}'").ToString() == "集团财务")
                    {
                        sqlInsert.Append("insert into rolewithemp(Type,PersonId,PersonName)  values");
                        for (int i = 0; i < dapi.Employees.Length; i++)
                        {
                            if (i > 0)
                            {
                                sqlInsert.Append(",");
                            }
                            sqlInsert.Append($"('1','{dapi.Employees[i].EmployeeCode}','{dapi.Employees[i].EmployeeName}')");
                        }
                        da.ExecSql(sqlInsert.ToString());
                    }
                    if (da.GetValue($"select rolename from role where roleid='{dapi.RoleCode}'").ToString() == "出纳")
                    {
                        sqlInsert.Append("insert into rolewithemp(Type,PersonId,PersonName)  values");
                        for (int i = 0; i < dapi.Employees.Length; i++)
                        {
                            if (i > 0)
                            {
                                sqlInsert.Append(",");
                            }
                            sqlInsert.Append($"('2','{dapi.Employees[i].EmployeeCode}','{dapi.Employees[i].EmployeeName}')");
                        }
                        da.ExecSql(sqlInsert.ToString());
                    }

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n给角色添加人员信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"给角色添加人员信息报错" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 给角色增加人员 Type Employees RoleCode

            #region 修改角色组 Type RoleGroupName  RoleGroupCode

            if (dapi.Type == "roleGroupEdit")
            {
                try
                {
                    sql = "";
                    sql = $"update RoleGroup set RoleGroupName='{dapi.RoleGroupName}' where RoleGroupId ='{dapi.RoleGroupCode}' and status ='1'";
                    da.ExecSql(sql);
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n修改角色组信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"修改角色组信息报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 修改角色组 Type RoleGroupName  RoleGroupCode

            #region 删除角色组  RoleGroupCode Type 下面没有角色的才可以删除

            if (dapi.Type == "roleGroupRemove")
            {
                try
                {
                    sql = "";
                    sql = $"select count(RoleGroupId) from role where RoleGroupId='{dapi.RoleGroupCode}' and status ='1'";
                    string count = da.GetValue(sql).ToString();
                    if (count != "0")
                    {
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"此角色组下有正在使用角色，不能删除\",\"errcode\":\"1\"}");
                        context.Response.Write(FhJson.ToString());
                        return;
                    }
                    else
                    {
                        sql = "";
                        sql = $"update RoleGroup set status = '0'  where RoleGroupId ='{dapi.RoleGroupCode}'";
                        da.ExecSql(sql);
                        FhJson.Clear();
                        FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    }
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n删除角色组信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"删除角色组信息报错(DD1002):" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 删除角色组  RoleGroupCode Type 下面没有角色的才可以删除

            #region 查询角色组及角色 Type 以及所有人员 以及所管理部门

            if (dapi.Type == "SelectRole")
            {
                try
                {
                    sql = "";
                    sql = "select RoleGroupId, RoleGroupName from RoleGroup where status ='1'";
                    DataTable roleGroup = da.GetDataTable(sql);
                    DataTable emps = null;
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"data\":[");
                    if (roleGroup.Rows.Count > 0)
                    {
                        for (int i = 0; i < roleGroup.Rows.Count; i++)
                        {
                            if (i > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"RoleGroupId\":\"" + roleGroup.Rows[i]["RoleGroupId"] + "\",\"RoleGroupName\":\"" + roleGroup.Rows[i]["RoleGroupName"] + "\",\"Roles\":[");
                            sql = "";
                            sql = $"SELECT  Roleid Roleid,RoleName RoleName FROM Role  where RoleGroupId = '{roleGroup.Rows[i]["RoleGroupId"]}' and status = '1'";
                            obj = da.GetDataTable(sql);
                            dt = obj as DataTable;
                            for (int j = 0; j < dt.Rows.Count; j++)
                            {
                                if (j > 0)
                                {
                                    FhJson.Append(",");
                                }
                                FhJson.Append("{\"Roleid\":\"" + dt.Rows[j]["Roleid"] + "\",\"RoleName\":\"" + dt.Rows[j]["RoleName"] + "\",\"Employees\":[");
                                sql = "";
                                sql = $"select distinct a.employeename employeename,a.employeecode employeecode from flowemployee a join EmpsRoleId b on a.employeecode = b.EmployeeCode where b.roleid ='{dt.Rows[j]["Roleid"]}' and status = '1'";
                                //查询当前角色内的人员
                                emps = da.GetDataTable(sql);
                                for (int ems = 0; ems < emps.Rows.Count; ems++)
                                {
                                    if (ems > 0)
                                    {
                                        FhJson.Append(",");
                                    }
                                    FhJson.Append("{\"EmployeeCode\":\"" + emps.Rows[ems]["employeecode"] + "\",\"EmployeeName\":\"" + emps.Rows[ems]["employeename"] + "\"}");
                                }
                                FhJson.Append("]}");
                            }
                            FhJson.Append("]}");
                        }
                    }
                    FhJson.Append("]}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n查询角色返回:" + FhJson.ToString() + "\r\n");
                    }

                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询角色信息报错(DD1004)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询角色组及角色 Type 以及所有人员 以及所管理部门

            #region 修改角色 Type RoleName Remarks RoleGroupCode RoleCode Depts

            if (dapi.Type == "roleEdit")
            {
                try
                {
                    sql = "";
                    sql = $"update Role set RoleName ='{dapi.RoleName}',Remarks = '{dapi.Remarks}',RoleGroupId ='{dapi.RoleGroupCode}' where RoleId ='{dapi.RoleCode}'";
                    da.ExecSql(sql);
                    da.ExecSql($"update rolewithorg set status = '0' where roleid ='{dapi.RoleCode}'");
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    //批量插入RoleWithOrg表
                    StringBuilder sqlInsert = new StringBuilder();
                    sqlInsert.Append("insert into RoleWithOrg(RoleId,OrgCode)  values");
                    for (int i = 0; i < dapi.Depts.Length; i++)
                    {
                        if (i > 0)
                        {
                            sqlInsert.Append(",");
                        }
                        sqlInsert.Append($"('{dapi.RoleCode}','{dapi.Depts[i].DeptCode}')");
                    }
                    da.ExecSql(sqlInsert.ToString());
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n修改角色信息返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"修改角色信息报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 修改角色 Type RoleName Remarks RoleGroupCode RoleCode Depts

            #region 增加角色与部门对应关系 DeptCode RoleId

            if (dapi.Type == "roleWithOrgAdd")
            {
                try
                {
                    sql = "";
                    sql = $"insert into RoleWithOrg(OrgCode,RoleId)  values('{dapi.DeptCode}','{dapi.RoleCode}') ";
                    da.ExecSql(sql);
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n增加角色与部门对应关系:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"增加角色与部门对应关系报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 增加角色与部门对应关系 DeptCode RoleId

            #region 删除角色与部门对应关系 DeptCode RoleId

            if (dapi.Type == "roleWithOrgRemove")
            {
                try
                {
                    sql = "";
                    sql = $"update RoleWithOrg set status = '0' where  OrgCode = '{dapi.DeptCode}'  and RoleId = '{dapi.RoleCode}'";
                    da.ExecSql(sql);
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n删除角色与部门对应关系:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"删除角色与部门对应关系报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 删除角色与部门对应关系 DeptCode RoleId

            #region 修改角色与部门对应关系 Depts RoleId RoleGroupCode RoleName

            if (dapi.Type == "roleWithOrgEdit")
            {
                try
                {
                    sql = "";
                    sql = $"update RoleWithOrg set status = '0' where  RoleId = '{dapi.RoleCode}' and status ='1'";
                    da.ExecSql(sql);

                    sql = "";
                    sql = $"update Role set RoleName = '{dapi.RoleName}'   where  RoleId = '{dapi.RoleCode}' and status ='1'";
                    da.ExecSql(sql);
                    StringBuilder sqlInsert = new StringBuilder();
                    sqlInsert.Append("insert into RoleWithOrg(RoleId,OrgCode)  values");
                    for (int i = 0; i < dapi.Depts.Length; i++)
                    {
                        if (i > 0)
                        {
                            sqlInsert.Append(",");
                        }
                        sqlInsert.Append($"('{dapi.RoleCode}','{dapi.Depts[i].DeptCode}')");
                    }
                    da.ExecSql(sqlInsert.ToString());
                    da.ExecSql(sql);
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\"}");
                   
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 修改角色与部门对应关系 Depts RoleId RoleGroupCode RoleName

            #region 查询角色与部门对应关系  RoleId sign

            if (dapi.Type == "roleWithOrgSelect")
            {
                try
                {
                    sql = "";
                    sql = $"select  distinct a.OrgCode,a.RoleId ,b.RoleName,C.Name from RoleWithOrg a join role b on a.roleid = b.roleid join Organization c on a.orgcode = c.guid where a.status = '1' and a.roleid ='{dapi.RoleCode}'";
                    DataTable rwo = da.GetDataTable(sql);
                    sql = "";
                    sql = $"select distinct a.RoleId,b.RoleName from RoleWithOrg a join role b on a.roleid = b.roleid where a.roleid ='{dapi.RoleCode}' and a.status ='1' and b.status = '1'";
                    DataTable rwo2 = da.GetDataTable(sql);
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"roleWithOrg\":[");
                    for (int i = 0; i < rwo2.Rows.Count; i++)
                    {
                        if (i > 0)
                        {
                            FhJson.Append(",");
                        }
                        FhJson.Append("{\"RoleId\":\"" + rwo2.Rows[i]["RoleId"] + "\",\"RoleName\":\"" + rwo2.Rows[i]["RoleName"] + "\",\"Depts\":[");
                        for (int j = 0; j < rwo.Rows.Count; j++)
                        {
                            if (j > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"DeptCode\":\"" + rwo.Rows[j]["OrgCode"] + "\",\"DeptName\":\"" + rwo.Rows[j]["Name"] + "\"}");
                        }

                        FhJson.Append("]}");
                    }
                    FhJson.Append("]}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n查询角色与部门对应关系:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询角色与部门对应关系  RoleId sign

            #region 查询当前人的角色  (RoleId sign)

            if (dapi.Type == "personRole")
            {
                try
                {
                    //查询当前人的工号
                    sql = $"select distinct employeecode from flowemployee where ddid ='{dapi.DDId}'";
                    dapi.EmployeeCode = da.GetValue(sql).ToString();
                    sql = $"select distinct b.RoleName from EmpsRoleId a join role b on a.roleid = b.roleid  where a.status = '1' and b.status = '1' and a.EmployeeCode ='{dapi.EmployeeCode}'";
                    DataTable rwo = da.GetDataTable(sql);
                    sql = "";
                    int isCashier = 0;
                    int isJTLeader = 0;
                    var dataRows = rwo.Select("RoleName ='出纳'");
                    var isJTLeaders = leaderName.Split(',');
                    if (dataRows.Length != 0)
                    {
                        isCashier = 1;
                    }
                    for (int i = 0; i < isJTLeaders.Length; i++)
                    {
                        var dataRowsl = rwo.Select($"RoleName ='{isJTLeaders[i]}'");
                        if (dataRowsl.Length != 0)
                        {
                            isJTLeader = 1;
                            i = isJTLeaders.Length;
                        }
                    }
                    FhJson.Clear();
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":\"0\",\"isCashier\":\"" + isCashier + "\",\"isJTLeader\":\"" + isJTLeader + "\",\"personRole\":[");
                    for (int i = 0; i < rwo.Rows.Count; i++)
                    {
                        if (i > 0)
                        {
                            FhJson.Append(",");
                        }
                        FhJson.Append("{\"RoleName\":\"" + rwo.Rows[i]["RoleName"] + "\"}");
                    }
                    FhJson.Append("]}");
                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n查询当前人员的角色信息:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询当前人的角色  (RoleId sign)

            #region 查询所有角色以及管理部门  Type

            if (dapi.Type == "roleOrgAllSelect")
            {
                try
                {
                    sql = $"select RoleId,RoleName  from Role where status = '1'";
                    DataTable Roles = da.GetDataTable(sql);
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Roles\":[");
                    DataTable roleorg = new DataTable();
                    for (int i = 0; i < Roles.Rows.Count; i++)
                    {
                        if (i > 0)
                        {
                            FhJson.Append(",");
                        }
                        FhJson.Append("{\"RoleId\":\"" + Roles.Rows[i]["RoleId"] + "\",\"RoleName\":\"" + Roles.Rows[i]["RoleName"] + "\",\"Depts\":[");
                        sql = $"select a.RoleId RoleId,a.RoleName RoleName,b.OrgCode  OrgCode,c.Name OrgName from Role a join RoleWithOrg b on a.roleid = b.roleid join Organization c on b.orgcode = c.guid where a.roleid ='{Roles.Rows[i]["RoleId"]}' and  status ='1'";
                        roleorg = da.GetDataTable(sql);
                        for (int ad = 0; ad < roleorg.Rows.Count; ad++)
                        {
                            if (ad > 0)
                            {
                                FhJson.Append(",");
                            }
                            FhJson.Append("{\"OrgCode\":\"" + roleorg.Rows[ad]["OrgCode"] + "\",\"OrgName\":\"" + roleorg.Rows[ad]["OrgName"] + "\"}");
                        }
                        FhJson.Append("]}");
                    }
                    FhJson.Append("]}");

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n查询角色返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询角色报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询所有角色以及管理部门  Type

            #region 查询所有角色 Type

            if (dapi.Type == "roleAllSelect")
            {
                try
                {
                    sql = $"select RoleId,RoleName  from Role where status ='1'";
                    DataTable Roles = da.GetDataTable(sql);
                    FhJson.Append("{\"errmsg\":\"ok\",\"errcode\":0,\"Roles\":[");
                    for (int i = 0; i < Roles.Rows.Count; i++)
                    {
                        if (i > 0)
                        {
                            FhJson.Append(",");
                        }
                        FhJson.Append("{\"RoleId\":\"" + Roles.Rows[i]["RoleId"] + "\",\"RoleName\":\"" + Roles.Rows[i]["RoleName"] + "\"}");
                    }
                    FhJson.Append("]}");

                    if (isWrite == "1")
                    {
                        ToolsClass.TxtLog("修改审批架构信息日志", "\r\n查询角色返回:" + FhJson.ToString() + "\r\n");
                    }
                    context.Response.Write(FhJson.ToString());
                    return;
                }
                catch (Exception ex)
                {
                    context.Response.Write("{\"errmsg\":\"查询角色报错(DD1002)\",\"errcode\":1}");
                    return;
                }
            }

            #endregion 查询所有角色 Type

            else
            {
                context.Response.Write("{\"errmsg\":\"还需要其他功能？？17854238990(DD1000)\",\"errcode\":1}");
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