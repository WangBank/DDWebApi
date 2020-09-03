using System;
using System.Web;

namespace G3WebERPApi
{
    public class Global : System.Web.HttpApplication
    {
        private string CsJson = "";

        protected void Application_Start(object sender, EventArgs e)
        {
        }

        protected void Session_Start(object sender, EventArgs e)
        {
        }

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication httpa = (HttpApplication)sender;
            string path = httpa.Context.Request.Path;
            string Requestip = "";
            if (string.IsNullOrEmpty(Requestip)) { Requestip = httpa.Context.Request.ServerVariables["REMOTE_ADDR"]; }
            if (string.IsNullOrEmpty(Requestip)) { Requestip = httpa.Context.Request.UserHostAddress; }
            if (string.IsNullOrEmpty(Requestip)) { Requestip = "0.0.0.0"; }
            string signUrl = httpa.Context.Request.Url.AbsoluteUri;
            ToolsClass.TxtLog("登录后台系统信息", ";\r\nIP地址为：" + Requestip + ";\r\n 访问接口地址：" + signUrl + ";\r\n入参：" + CsJson);

            if (path.ToLower().Contains("/login"))
            {
                // 登录
                httpa.Context.RewritePath(path.ToLower().Replace("/login", "Login.ashx"));
            }

            if (path.ToLower().Contains("/md5encrypt"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/md5encrypt", "MD5Encrypt.ashx"));
            }

            //获取签名
            if (path.ToLower().Contains("/getsign"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/getsign", "Sign.ashx"));
            }

            //出差申请
            if (path.ToLower().Contains("/travelrequest"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/travelrequest", "Travel/TravelRequest.ashx"));
            }

            //查询接口
            if (path.ToLower().Contains("/selinfo"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/selinfo", "Select.ashx"));
            }

            //多级审批查询接口
            if (path.ToLower().Contains("/selinfomul"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/selinfomul", "MULSelect.ashx"));
            }

            //查询DD用户信息接口
            if (path.ToLower().Contains("/getuser"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/getuser", "GetUser.ashx"));
            }

            //查询DD部门信息接口
            if (path.ToLower().Contains("/getdepart"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/getdepart", "GetUser.ashx"));
            }

            //出差申请审批接口
            if (path.ToLower().Contains("/travelapproval"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/travelapproval", "Travel/ProInfo.ashx"));
            }

            //多级审批出差申请提交申请接口
            if (path.ToLower().Contains("/tam"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/tam", "Approval/TAMultistage.ashx"));
            }

            //多级审批出差申请审批接口
            if (path.ToLower().Contains("/tasp"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/tasp", "Approval/TSPM.ashx"));
            }

            //差旅费报销申请接口
            if (path.ToLower().Contains("/expetravreq"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/expetravreq", "Travel/ExpeTravReq.ashx"));
            }

            //通讯费报销申请接口
            if (path.ToLower().Contains("/txfapproval"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/txfapproval", "Travel/TxfApproval.ashx"));
            }

            //通讯费报销申请审批接口
            if (path.ToLower().Contains("/txfaudi"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/txfaudi", "Travel/TxfAudi.ashx"));
            }

            //差旅费报销申请审批接口
            if (path.ToLower().Contains("/clfaudi"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/clfaudi", "Travel/ClfAudi.ashx"));
            }

            //多级流程差旅费报销申请接口
            if (path.ToLower().Contains("/clfbxsqmul"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/clfbxsqmul", "Approval/CLFBXSQ.ashx"));
            }

            //多级流程差旅费报销申请审批接口
            if (path.ToLower().Contains("/clfbxspmul"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/clfbxspmul", "Approval/CLFBXSP.ashx"));
            }

            //多级流程通讯费报销申请接口
            if (path.ToLower().Contains("/zlfysqmul"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/zlfysqmul", "Approval/TxfJtfZdfSQ.ashx"));
            }
            //多级流程通讯费报销申请审批接口
            if (path.ToLower().Contains("/zlfyspmul"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/zlfyspmul", "Approval/TxfjtfXdfSP.ashx"));
            }

            //查询审批流信息接口
            if (path.ToLower().Contains("/selapproval"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/selapproval", "Travel/SelApproval.ashx"));
            }
            //转发消息通知接口
            if (path.ToLower().Contains("/sendmsg"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/sendmsg", "Travel/SendMsg.ashx"));
            }

            //文件上传下载秘钥Url接口
            if (path.ToLower().Contains("/filesign"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/filesign", "FileSign.ashx"));
            }

            //获取申请审批表单名称接口
            if (path.ToLower().Contains("/billname"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/billname", "Approval/GetBillName.ashx"));
            }

            //保存审批流程信息接口
            if (path.ToLower().Contains("/saveprocess"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/saveprocess", "Approval/ProcessSave.ashx"));
            }

            //进入审批功能的时候，返回审批流程信息接口
            if (path.ToLower().Contains("/getprocessbegin"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/getprocessbegin", "Approval/ProcessInfo.ashx"));
            }

            //获取现在已保存的流程信息节点
            if (path.ToLower().Contains("/getbillclassnode"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/getbillclassnode", "Approval/GetBillName.ashx"));
            }

            //dept管理接口
            if (path.ToLower().Contains("/dept"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/dept", "Approval/TongXunLu.ashx"));
            }

            //role管理接口
            if (path.ToLower().Contains("/role"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/role", "Approval/TongXunLu.ashx"));
            }

            //people管理接口
            if (path.ToLower().Contains("/people"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/people", "Approval/TongXunLu.ashx"));
            }

            // 钉钉服务日志功能 保存与查询
            if (path.ToLower().Contains("/servicelog"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/servicelog", "Common/ServiceLog.ashx"));
            }

            // 审批时修改申请内容
            if (path.ToLower().Contains("/changeclfbxdetails"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/changeclfbxdetails", "Approval/ChangeSQDetails.ashx"));
            }
            if (path.ToLower().Contains("/changeqitadetails"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/changeqitadetails", "Approval/ChangeSQDetails.ashx"));
            }
            if (path.ToLower().Contains("/changezddetails"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/changezddetails", "Approval/ChangeSQDetails.ashx"));
            }
            if (path.ToLower().Contains("/changeqtfydetails"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/changeqtfydetails", "Approval/ChangeSQDetails.ashx"));
            }

            // 修改25号之前错误的差旅费数据
            if (path.ToLower().Contains("/clfbxdetailchange"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/clfbxdetailchange", "Common/CLFBXDetailChange.ashx"));
            }

            // 其他费用报销申请
            if (path.ToLower().Contains("/othercostsq"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/othercostsq", "Approval/OtherCostSQ.ashx"));
            }
            // 其他费用报销申请审批
            if (path.ToLower().Contains("/othercostsp"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/othercostsp", "Approval/OtherCostSP.ashx"));
            }

            // 其他费用报销申请审批
            if (path.ToLower().Contains("/cashierpay"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/cashierpay", "Approval/CashierPay.ashx"));
            }

            // 医保授权申请
            if (path.ToLower().Contains("/medconfigreq"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/medconfigreq", "Approval/MedConfigReq.ashx"));
            }
            if (path.ToLower().Contains("/medconfigauditing"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/medconfigauditing", "Approval/MedConfigAuditing.ashx"));
            }

            if (path.ToLower().Contains("/setsignfile"))
            {
                httpa.Context.RewritePath(path.ToLower().Replace("/setsignfile", "Approval/SetSignFile.ashx"));
            }
        }

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {
        }

        protected void Application_Error(object sender, EventArgs e)
        {
        }

        protected void Session_End(object sender, EventArgs e)
        {
        }

        protected void Application_End(object sender, EventArgs e)
        {
        }
    }
}