using G3WebERPApi.user;
using Newtonsoft.Json;

using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace G3WebERPApi
{
    /// <summary>
    /// GetUser 的摘要说明
    /// </summary>
    public class GetUser : IHttpHandler
    {
        private static string connectionString = "";//数据库链接
        private DbHelper.SqlHelper da;

        private string url = "";
        private string nonceStr = ToolsClass.GetConfig("RomensOA"); //必填，生成签名的随机串
        private string agentId = "";// 必填，微应用ID
        private string timeStamp = "";
        private string corpId = "";//必填，企业ID
        private string sign = "";
        private string FhJson = "";
        private string CsJson = "";
        private string ticket = "";
        private string appKey = "";
        private string appSecret = "";
        private string access_token = "";
        private int errcode = 1;
        private string isWrite = "0";

        private string sql = string.Empty;
        private object obj;
        private DataTable dt = new DataTable();

        private string selType = "";//查询类型
        private string selId = "";//授权码
        private string AppWyy = "";//钉钉微应用参数集
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

            try
            {
                string signUrl = ToolsClass.GetConfig("signUrl"); context.Response.ContentType = "text/plain";
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
                CsJson = Regex.Replace(CsJson, @"[\n\r]", "").Replace(@"\n", ",").Replace("'", "‘").Replace("\t", ":").Replace("\r", ",").Replace("\n", ",");
                //json转Hashtable
                ToolsClass.TxtLog("GetUser入参", "\r\nGetUser入参:" + CsJson);
                Object jgobj = ToolsClass.DeserializeObject(CsJson);
                Hashtable returnhash = jgobj as Hashtable;
                if (returnhash == null)
                {
                    context.Response.Write("{\"errmsg\":\"报文格式错误(DD0003)\",\"errcode\":1}");
                    return;
                }

                string path1 = context.Request.Path.Replace("GetUser.ashx", "getuser");
                string path2 = context.Request.Path.Replace("GetUser.ashx", "getdepart");
                //验证请求sign
                string sign1 = ToolsClass.md5(signUrl + path1 + "Romens1/DingDing2" + path1, 32);
                string sign2 = ToolsClass.md5(signUrl + path2 + "Romens1/DingDing2" + path2, 32);
                ToolsClass.TxtLog("生成的sign", "生成的" + "sign1:" + sign1 + "sign2:" + sign2 + "传入的sign" + returnhash["Sign"].ToString() + "\r\n 后台字符串:" + signUrl + path2 + "Romens1/DingDing2" + path2);
                if (sign1 != returnhash["Sign"].ToString() && sign2 != returnhash["Sign"].ToString())
                {
                    context.Response.Write("{\"errmsg\":\"认证信息Sign不存在或者不正确！\",\"errcode\":1}");
                    return;
                }

                selType = returnhash["TypeId"].ToString();
                if (returnhash.Contains("id"))
                {
                    selId = returnhash["id"].ToString();
                    if (selId == "")
                    {
                        context.Response.Write("{\"errmsg\":\"ID不允许为空(DD2001)\",\"errcode\":1}");
                        return;
                    }
                }

                //#微应用ID:agentId #企业ID:corpId #应用的唯一标识:appKey #应用的密钥:appSecret
                AppWyy = ToolsClass.GetConfig("AppWyy");
                ScList = AppWyy.Split('$');
                agentId = ScList[0].ToString();
                corpId = ScList[1].ToString();
                appKey = ScList[2].ToString();
                appSecret = ScList[3].ToString();
                isWrite = ToolsClass.GetConfig("isWrite");
                ddUrl = ToolsClass.GetConfig("ddUrl");

                //获取access_token
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
                //取用户信息
                if (selType == "SelUser01")
                {
                    url = "https://oapi.dingtalk.com/user/getuserinfo?access_token=" + access_token + "&code=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");

                    //免登授权码 获取用户userid
                    GetUserId userClass = new GetUserId();
                    userClass = (GetUserId)JsonConvert.DeserializeObject(FhJson, typeof(GetUserId));
                    errcode = userClass.errcode;
                    if (errcode != 0)
                    {
                        context.Response.Write("{\"errmsg\":\"" + userClass.errmsg + "\",\"errcode\":1}");
                        return;
                    }

                    //获取用户详情
                    url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + userClass.userid;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                    GetUserXq userXqClass = new GetUserXq();
                    userXqClass = (GetUserXq)JsonConvert.DeserializeObject(FhJson, typeof(GetUserXq));
                    errcode = userXqClass.errcode;
                    if (errcode != 0)
                    {
                        context.Response.Write("{\"errmsg\":\"" + userClass.errmsg + "\",\"errcode\":1}");
                        return;
                    }

                    //查询用户信息
                    sql = "select Top 1 A.GUID OperatorGuid,A.EmployeeCode JobNumber,A.EmployeeName OperatorName,A.OrgCode,b.Name OrgName from FlowEmployee a left join ORGANIZATION b on a.OrgCode=b.Code where a.EmployeeCode='" + userXqClass.jobnumber + "'";
                    obj = da.GetDataTable(sql);
                    dt = obj as DataTable;
                    if (dt.Rows.Count > 0)
                    {
                        FhJson = "{\"errmsg\":\"ok\",\"errcode\":0,\"userid\":\"" + userClass.userid + "\",\"OperatorGuid\":\"" + dt.Rows[0]["OperatorGuid"].ToString() +
                          "\",\"OperatorName\":\"" + dt.Rows[0]["OperatorName"].ToString() +
                           "\",\"Avatar\":\"" + userXqClass.avatar +
                          "\",\"JobNumber\":\"" + dt.Rows[0]["JobNumber"].ToString() +
                          "\",\"OrgCode\":\"" + dt.Rows[0]["OrgCode"].ToString() +
                          "\",\"OrgName\":\"" + dt.Rows[0]["OrgName"].ToString() + "\"}";
                    }
                    else
                    {
                        FhJson = "{\"errmsg\":\"当前用户不存在\",\"errcode\":1}";
                    }
                }
                //获取用户详情
                else if (selType == "SelUser02")
                {
                    url = "https://oapi.dingtalk.com/user/get?access_token=" + access_token + "&userid=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取部门用户userid列表
                else if (selType == "SelUser03")
                {
                    url = "https://oapi.dingtalk.com/user/getDeptMember?access_token=" + access_token + "&deptId=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取部门用户列表
                else if (selType == "SelUser04")
                {
                    url = "https://oapi.dingtalk.com/user/simplelist?access_token=" + access_token + "&department_id=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取部门用户详情
                else if (selType == "SelUser05")
                {
                    url = "https://oapi.dingtalk.com/user/listbypage?access_token=" + access_token + "&department_id=" + selId + "&offset=0&size=10";
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取管理员列表
                else if (selType == "SelUser06")
                {
                    url = "https://oapi.dingtalk.com/user/get_admin?access_token=" + access_token;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取管理员通讯录权限范围
                else if (selType == "SelUser07")
                {
                    url = "https://oapi.dingtalk.com/topapi/user/get_admin_scope?access_token=" + access_token + "&userid=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取子部门ID列表
                else if (selType == "SelDepart01")
                {
                    url = "https://oapi.dingtalk.com/department/list_ids?access_token=" + access_token + "&id=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取部门列表
                else if (selType == "SelDepart02")
                {
                    url = "https://oapi.dingtalk.com/department/list?access_token=" + access_token + "&id=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取部门详情
                else if (selType == "SelDepart03")
                {
                    url = "https://oapi.dingtalk.com/department/get?access_token=" + access_token + "&id=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //查询部门的所有上级父部门
                else if (selType == "SelDepart04")
                {
                    url = "https://oapi.dingtalk.com/department/list_parent_depts_by_dept?access_token=" + access_token + "&id=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                //获取企业员工人数
                else if (selType == "SelDepart05")
                {
                    //onlyActive 0：包含未激活钉钉的人员数量 1：不包含未激活钉钉的人员数量
                    url = "https://oapi.dingtalk.com/user/get_org_user_count?access_token=" + access_token + "&onlyActive=" + selId;
                    FhJson = ToolsClass.ApiFun("GET", url, "");
                }
                else
                {
                    context.Response.Write("{\"errmsg\":\"查询其他信息请关机(DD1003)\",\"errcode\":1}");
                    return;
                }
                ToolsClass.TxtLog("GetUser", "\r\n返给前端json:" + FhJson);
                context.Response.Write(FhJson);
                return;
            }
            catch (Exception ex)
            {
                context.Response.Write("{\"errmsg\":\"" + ex.Message + "\",\"errcode\":1}");
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

        /// <summary>
        /// 获取时间戳timestamp（当前时间戳，具体值为当前时间到1970年1月1号的秒数）
        /// </summary>
        /// <returns></returns>
        public static string GetTimeStamp()
        {
            TimeSpan ts = DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0, 0);
            return Convert.ToInt64(ts.TotalSeconds).ToString();
        }
    }
}