using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using G3WebApiCore.Commom;
using G3WebApiCore.Model.ApiDtos;
using G3WebApiCore.Model.FreeSqlHelper;
using G3WebApiCore.Model.Request;
using G3WebApiCore.Model.Response;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace G3WebApiCore.Controllers
{
    /// <summary>
    /// 统计单据信息api
    /// </summary>
    [EnableCors("AllowCors")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApplyBillInfoController : ControllerBase
    {

        /// <summary>
        /// 主db操作
        /// </summary>
        public IFreeSql _sqlserverSql;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sqlserverSql"></param>
        public ApplyBillInfoController(IFreeSql<SqlServerFlag> sqlserverSql)
        {
            _sqlserverSql = sqlserverSql;
        }

        /// <summary>
        /// 获得所有审批单据，根据单据号划分的
        /// </summary>
        /// <param name="getInfoRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApplyBillResponse> GetInfo(GetInfoRequest getInfoRequest)
        {
            var result = new ApplyBillResponse();
            totals = 0;
            amounttotals = 0;
            if (getInfoRequest == null)
            {
                result.code = -1;
                result.message = "请检查入参";
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计费用信息出参", JsonConvert.SerializeObject(result));
                });
                return result;

            }
            try
            {
                CommonHelper.TxtLog("统计费用信息入参", JsonConvert.SerializeObject(getInfoRequest));
                var mainData = new List<ApplyBillInfo>();

                //判断选择的单据类型
                switch (getInfoRequest.BillType)
                {
                    case 0:
                        //全部单据
                        mainData = await GetAllDataAsync(getInfoRequest);
                        break;
                    case 1:
                        //差旅费
                        mainData = await GetClfDataAsync(getInfoRequest);
                        break;
                    case 2:
                        //交通费
                        mainData = await GetJtfDataAsync(getInfoRequest);
                        break;
                    case 3:
                        //通讯费
                        mainData = await GetTxfDataAsync(getInfoRequest);
                        break;
                    case 4:
                        //招待费
                        mainData = await GetZdfDataAsync(getInfoRequest);
                        break;
                    case 5:
                        //其他费用
                        mainData = await GetQtfDataAsync(getInfoRequest);
                        break;
                    default:
                        result.code = -1;
                        result.message = "BillType值不对,请检查!";
                        _ = Task.Run(() =>
                        {
                            CommonHelper.TxtLog("统计费用信息出参", JsonConvert.SerializeObject(result));
                        });
                        return result;
                }

                result.code = 0;
                result.message = "获取数据成功";
                result.data = new ApplyBillInfoData
                {
                    Total = getInfoRequest.BillType == 0 ? amounttotals : totals,
                    Items = mainData
                };
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计费用信息出参", JsonConvert.SerializeObject(result));
                });
                return result;

            }
            catch (Exception ex)
            {

                result.code = -1;
                result.message = "获取数据失败" + ex.Message;
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计费用信息出参", JsonConvert.SerializeObject(result));
                    CommonHelper.TxtLog("异常错误信息", JsonConvert.SerializeObject(ex));
                });
                return result;
            }


        }

        private long totals { get; set; }
        private long amounttotals { get; set; }

        /// <summary>
        /// 获得全部单据数据
        /// </summary>
        /// <param name="getInfoRequest"></param>
        /// <returns></returns>
        private async Task<List<ApplyBillInfo>> GetAllDataAsync(GetInfoRequest getInfoRequest)
        {
            var amountpage = getInfoRequest.Page;
            var amountlimit = getInfoRequest.Limit;
            getInfoRequest.Page = 0;
            getInfoRequest.Limit = 0;
            List<ApplyBillInfo> applyBillInfos = new List<ApplyBillInfo>();
            applyBillInfos.AddRange(await GetClfDataAsync(getInfoRequest));
            applyBillInfos.AddRange(await GetJtfDataAsync(getInfoRequest));
            applyBillInfos.AddRange(await GetTxfDataAsync(getInfoRequest));
            applyBillInfos.AddRange(await GetZdfDataAsync(getInfoRequest));
            applyBillInfos.AddRange(await GetQtfDataAsync(getInfoRequest));
            amounttotals = applyBillInfos.Count;
            var tempBill = applyBillInfos;
            if (amountpage != 0 && amountlimit != 0)
            {
                tempBill = tempBill.Skip((amountpage - 1) * amountlimit).Take(amountlimit).ToList();

            }
            
            return tempBill;
        }

        /// <summary>
        /// 获得差旅费总表明细
        /// </summary>
        /// <param name="getInfoRequest"></param>
        /// <returns></returns>
        private async Task<List<ApplyBillInfo>> GetClfDataAsync(GetInfoRequest getInfoRequest)
        {

            var flowEmployee = await _sqlserverSql.Select<FlowEmployee>().Where(f => f.Disable == false).ToListAsync();
            List<ApplyBillInfo> applyBillInfos = new List<ApplyBillInfo>();
            // 提交时间在条件中间  或者 结束时间在条件中  或者 他俩都在条件外
            var MainWfy = _sqlserverSql.Select<ExpeTrav>().Where(
               o => _sqlserverSql.Select<ApprovalComments>().As("b").Where(b => b.BillNo == o.BillNo).Any()
                &&
                (
                    (o.BillDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.BillDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.AuditingDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.BeginDate && getInfoRequest.EndDate <= o.AuditingDate)
                )
                &&
                o.IsAuditing == (getInfoRequest.ApprovalState == 1 ? true : false)
            );
          
            var Main = MainWfy.Skip((getInfoRequest.Page - 1) * getInfoRequest.Limit).Take(getInfoRequest.Limit);
            string Mainsql = Main.ToSql();
            CommonHelper.TxtLog("查询差旅费", Mainsql);
            var MainData = await Main.ToListAsync();

            if (MainData.Count==0)
            {
                return applyBillInfos;
            }
            foreach (var item in MainData)
            {
                string usedTime = getInfoRequest.ApprovalState == 1 ? ((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value.Subtract(item.BillDate).TotalSeconds).ToString() : "正在进行";
                string usedTimeFormat = getInfoRequest.ApprovalState == 1 ? CommonHelper.GetUsedTime((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value, item.BillDate) : "正在进行";

                applyBillInfos.Add(new ApplyBillInfo { 
                    ApplyName = flowEmployee.Where(f => f.ddid == item.InsteadOperatorGuid).FirstOrDefault().employeename,
                    BillDate = item.BillDate,
                    BillState = getInfoRequest.ApprovalState,
                    BillNo = item.BillNo,
                    BillType = 1,
                    Details =await getApprovalDataAsync(item.BillNo,item.BillDate,usedTime,getInfoRequest),
                    UsedTime = usedTimeFormat
                });
            }

            var nowList = applyBillInfos.Where(o => o.Details.Count != 0).ToList();
            applyBillInfos.Clear();
            applyBillInfos.AddRange(nowList);
            totals = applyBillInfos.Count();
            return applyBillInfos;
        }


        /// <summary>
        /// 获得交通费总表明细
        /// </summary>
        /// <param name="getInfoRequest"></param>
        /// <returns></returns>
        private async Task<List<ApplyBillInfo>> GetJtfDataAsync(GetInfoRequest getInfoRequest)
        {

            var flowEmployee = await _sqlserverSql.Select<FlowEmployee>().Where(f => f.Disable == false).ToListAsync();
            List<ApplyBillInfo> applyBillInfos = new List<ApplyBillInfo>();
            // 提交时间在条件中间  或者 结束时间在条件中  或者 他俩都在条件外
            var MainWfy = _sqlserverSql.Select<ExpeOther>().Where(
               o => _sqlserverSql.Select<ApprovalComments>().As("b").Where(b => b.BillNo == o.BillNo).Any()
                &&
                (
                    (o.BillDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.BillDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.AuditingDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.BeginDate && getInfoRequest.EndDate <= o.AuditingDate)
                )
                &&
                o.IsAuditing == (getInfoRequest.ApprovalState == 1 ? true : false)
                 &&
                o.FeeType == "01"
            );
          
            var Main = MainWfy.Skip((getInfoRequest.Page - 1) * getInfoRequest.Limit).Take(getInfoRequest.Limit);
            string Mainsql = Main.ToSql();
            CommonHelper.TxtLog("查询交通费", Mainsql);
            var clfMainData = await Main.ToListAsync();

            if (clfMainData.Count == 0)
            {
                return applyBillInfos;
            }
            foreach (var item in clfMainData)
            {

                string usedTime = getInfoRequest.ApprovalState == 1 ? ((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value.Subtract(item.BillDate).TotalSeconds).ToString() : "正在进行";
                string usedTimeFormat = getInfoRequest.ApprovalState == 1 ? CommonHelper.GetUsedTime((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value, item.BillDate) : "正在进行";
                applyBillInfos.Add(new ApplyBillInfo
                {
                    ApplyName = flowEmployee.Where(f => f.ddid == item.InsteadOperatorGuid).FirstOrDefault().employeename,
                    BillDate = item.BillDate,
                    BillState = getInfoRequest.ApprovalState,
                    BillNo = item.BillNo,
                    BillType = 2,
                    Details = await getApprovalDataAsync(item.BillNo, item.BillDate, usedTime, getInfoRequest),
                    UsedTime = usedTimeFormat
                });
            }

            var nowList = applyBillInfos.Where(o => o.Details.Count != 0).ToList();
            applyBillInfos.Clear();
            applyBillInfos.AddRange(nowList);
            return applyBillInfos;
        }

        
        /// <summary>
        /// 获得通讯费总表明细
        /// </summary>
        /// <param name="getInfoRequest"></param>
        /// <returns></returns>
        private async Task<List<ApplyBillInfo>> GetTxfDataAsync(GetInfoRequest getInfoRequest)
        {

            var flowEmployee = await _sqlserverSql.Select<FlowEmployee>().Where(f => f.Disable == false).ToListAsync();
            List<ApplyBillInfo> applyBillInfos = new List<ApplyBillInfo>();
            // 提交时间在条件中间  或者 结束时间在条件中  或者 他俩都在条件外
            var MainWfy = _sqlserverSql.Select<ExpeOther>().Where(
                o => _sqlserverSql.Select<ApprovalComments>().As("b").Where(b => b.BillNo == o.BillNo).Any()
                &&
                (
                    (o.BillDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.BillDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.AuditingDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.BeginDate && getInfoRequest.EndDate <= o.AuditingDate)
                )
                &&
                o.IsAuditing == (getInfoRequest.ApprovalState == 1 ? true : false)
                 &&
                o.FeeType == "02"
            );
           
            var Main = MainWfy.Skip((getInfoRequest.Page - 1) * getInfoRequest.Limit).Take(getInfoRequest.Limit);
            string Mainsql = Main.ToSql();
            CommonHelper.TxtLog("查询通讯费", Mainsql);
            var clfMainData = await Main.ToListAsync();

            if (clfMainData.Count == 0)
            {
                return applyBillInfos;
            }
            foreach (var item in clfMainData)
            {

                string usedTime = getInfoRequest.ApprovalState == 1 ? ((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value.Subtract(item.BillDate).TotalSeconds).ToString() : "正在进行";
                string usedTimeFormat = getInfoRequest.ApprovalState == 1 ? CommonHelper.GetUsedTime((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value, item.BillDate) : "正在进行";
                applyBillInfos.Add(new ApplyBillInfo
                {
                    ApplyName = flowEmployee.Where(f => f.ddid == item.InsteadOperatorGuid).FirstOrDefault().employeename,
                    BillDate = item.BillDate,
                    BillState = getInfoRequest.ApprovalState,
                    BillNo = item.BillNo,
                    BillType = 3,
                    Details = await getApprovalDataAsync(item.BillNo, item.BillDate, usedTime, getInfoRequest),
                    UsedTime = usedTimeFormat
                });
            }
            var nowList = applyBillInfos.Where(o => o.Details.Count != 0).ToList();
            applyBillInfos.Clear();
            applyBillInfos.AddRange(nowList);
            return applyBillInfos;
        }


        /// <summary>
        /// 获得招待费总表明细
        /// </summary>
        /// <param name="getInfoRequest"></param>
        /// <returns></returns>
        private async Task<List<ApplyBillInfo>> GetZdfDataAsync(GetInfoRequest getInfoRequest)
        {

            var flowEmployee = await _sqlserverSql.Select<FlowEmployee>().Where(f => f.Disable == false).ToListAsync();
            List<ApplyBillInfo> applyBillInfos = new List<ApplyBillInfo>();
            // 提交时间在条件中间  或者 结束时间在条件中  或者 他俩都在条件外
            var MainWfy = _sqlserverSql.Select<ExpeEnteMent>().Where(

                o => _sqlserverSql.Select<ApprovalComments>().As("b").Where(b => b.BillNo == o.BillNo).Any()
                &&
                (
                    (o.BillDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.BillDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.AuditingDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.BeginDate && getInfoRequest.EndDate <= o.AuditingDate)
                )
                &&
                o.IsAuditing == (getInfoRequest.ApprovalState == 1 ? true : false)
            );
           
            var Main = MainWfy.Skip((getInfoRequest.Page - 1) * getInfoRequest.Limit).Take(getInfoRequest.Limit);
            string Mainsql = Main.ToSql();
            CommonHelper.TxtLog("查询招待费", Mainsql);
            var clfMainData = await Main.ToListAsync();

            if (clfMainData.Count == 0)
            {
                return applyBillInfos;
            }
            foreach (var item in clfMainData)
            {

                string usedTime = getInfoRequest.ApprovalState == 1 ? ((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value.Subtract(item.BillDate).TotalSeconds).ToString() : "正在进行";
                string usedTimeFormat = getInfoRequest.ApprovalState == 1 ? CommonHelper.GetUsedTime((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value, item.BillDate) : "正在进行";
                applyBillInfos.Add(new ApplyBillInfo
                {
                    ApplyName = flowEmployee.Where(f => f.ddid == item.InsteadOperatorGuid).FirstOrDefault().employeename,
                    BillDate = item.BillDate,
                    BillState = getInfoRequest.ApprovalState,
                    BillNo = item.BillNo,
                    BillType =4,
                    Details = await getApprovalDataAsync(item.BillNo, item.BillDate, usedTime, getInfoRequest),
                    UsedTime = usedTimeFormat
                });
            }
            var nowList = applyBillInfos.Where(o => o.Details.Count != 0).ToList();
            applyBillInfos.Clear();
            applyBillInfos.AddRange(nowList);
            return applyBillInfos;
        }

        /// <summary>
        /// 获得其他费总表明细
        /// </summary>
        /// <param name="getInfoRequest"></param>
        /// <returns></returns>
        private async Task<List<ApplyBillInfo>> GetQtfDataAsync(GetInfoRequest getInfoRequest)
        {

            var flowEmployee = await _sqlserverSql.Select<FlowEmployee>().Where(f => f.Disable == false).ToListAsync();
            List<ApplyBillInfo> applyBillInfos = new List<ApplyBillInfo>();
            // 提交时间在条件中间  或者 结束时间在条件中  或者 他俩都在条件外
            var MainWfy = _sqlserverSql.Select<ExpeOther>().Where(
                o => _sqlserverSql.Select<ApprovalComments>().As("b").Where(b => b.BillNo == o.BillNo).Any()
                &&
                (
                    (o.BillDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.BillDate)
                    ||
                    (o.AuditingDate <= getInfoRequest.EndDate && getInfoRequest.BeginDate <= o.AuditingDate)
                        ||
                    (o.AuditingDate <= getInfoRequest.BeginDate && getInfoRequest.EndDate <= o.AuditingDate)
                )
                &&
                o.IsAuditing == (getInfoRequest.ApprovalState == 1 ? true : false)
                 &&
                o.FeeType == "07"
            );
            var Main = MainWfy.Skip((getInfoRequest.Page - 1) * getInfoRequest.Limit).Take(getInfoRequest.Limit);
            string Mainsql = Main.ToSql();
            CommonHelper.TxtLog("查询其他费用", Mainsql);
            var clfMainData = await Main.ToListAsync();

            if (clfMainData.Count == 0)
            {
                return applyBillInfos;
            }
            foreach (var item in clfMainData)
            {

                string usedTime = getInfoRequest.ApprovalState == 1 ? ((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value.Subtract(item.BillDate).TotalSeconds).ToString() : "正在进行";
                string usedTimeFormat = getInfoRequest.ApprovalState == 1 ? CommonHelper.GetUsedTime((await _sqlserverSql.Select<ApprovalComments>().Where(a => a.BillNo == item.BillNo).OrderByDescending(o => o.ApprovalDate).ToListAsync())[0].ApprovalDate.Value, item.BillDate) : "正在进行";
                applyBillInfos.Add(new ApplyBillInfo
                {
                    ApplyName = flowEmployee.Where(f => f.ddid == item.InsteadOperatorGuid).FirstOrDefault().employeename,
                    BillDate = item.BillDate,
                    BillState = getInfoRequest.ApprovalState,
                    BillNo = item.BillNo,
                    BillType =5,
                    Details = await getApprovalDataAsync(item.BillNo, item.BillDate, usedTime, getInfoRequest),
                    UsedTime = usedTimeFormat
                });
            }

            var nowList = applyBillInfos.Where(o => o.Details.Count != 0).ToList();
            applyBillInfos.Clear();
            applyBillInfos.AddRange(nowList);
            return applyBillInfos;
        }


        //获取审批流程意见表
        //根据link类型  返回单个
        //根据时间类型 返回超过的
        //改为查询出来后去掉
        private async Task<List<ApplyBillInfoDetail>> getApprovalDataAsync(string billNo, DateTime billDate, string allUsedTime, GetInfoRequest getInfoRequest)
        {
            List<ApplyBillInfoDetail> applyBillInfoDetails = new List<ApplyBillInfoDetail>();

            //查询审批明细
            var commentList =  _sqlserverSql.Select<ApprovalComments>();


            //先把直接主管和二级主管加上

            var leaderfrot = commentList.Where(
                o =>
                o.BillNo == billNo
                &&
                o.isLeader == 1
            ).OrderBy(c => c.ApprovalDate);
            CommonHelper.TxtLog("查询主管相关", leaderfrot.ToSql());
            var leader =await leaderfrot.ToListAsync();
            string ApprovalInfo = "";
            string usedTimeFormat = "";
            for (int i = 0; i < leader.Count; i++)
            {
                string usedTime = "";
                string Percentage = "";
                if (i==0)
                {
                   
                    usedTime = leader[i].ApprovalStatus == 0 ? "正在进行" : (leader[i].ApprovalDate.Value.Subtract(billDate).TotalSeconds).ToString();
                     usedTimeFormat = leader[i].ApprovalStatus == 0 ? "正在进行":CommonHelper.GetUsedTime(leader[i].ApprovalDate.Value,billDate);
                    Percentage = (usedTime != "正在进行" && allUsedTime != "正在进行" ?  Math.Abs(double.Parse(usedTime)/double.Parse(allUsedTime)).ToString("P") : "正在进行");
                   
                }
                else
                {
                    usedTime = leader[i].ApprovalStatus == 0 ? "正在进行" : (leader[i].ApprovalDate.Value.Subtract(leader[i - 1].ApprovalDate.Value).TotalSeconds).ToString();
                    usedTimeFormat = leader[i].ApprovalStatus == 0 ? "正在进行":CommonHelper.GetUsedTime(leader[i].ApprovalDate.Value,leader[i - 1].ApprovalDate.Value);
                    Percentage = (usedTime != "正在进行" && allUsedTime != "正在进行" ? Math.Abs(double.Parse(usedTime) / double.Parse(allUsedTime)).ToString("P") : "正在进行");

                    
                }
                if (usedTime == "正在进行")
                {
                    ApprovalInfo = $"{leader[i].ApprovalName}未审批完成，已持续{CommonHelper.GetUsedTime(DateTime.Now, leader[i].ApprovalDate.Value)}";
                }
                else
                {
                    ApprovalInfo = $"{leader[i].ApprovalName}审批此单据消耗时间:{CommonHelper.GetUsedTime(leader[i].ApprovalDate.Value, billDate)}";
                }
                applyBillInfoDetails.Add(new ApplyBillInfoDetail
                {
                    BeginDate = billDate,
                    EndDate = leader[i].ApprovalDate.Value,
                    LinkType = leader[i].AType,
                    ApprovalInfo = ApprovalInfo,
                    UsedTime = usedTimeFormat,
                    Percentage = Percentage

                });
            }

            //查询角色组
            var roles = await _sqlserverSql.Select<RoleGroup>().ToListAsync();

            for (int i = 0; i < roles.Count; i++)
            {
                //查当前角色的角色
                var nowrole =  await _sqlserverSql.Select<Role>().Where(r => r.RoleGroupId == roles[i].RoleGroupId).ToListAsync();
                if (nowrole.Count !=0)
                {
                    string[] rolename = new string[nowrole.Count];
                    for (int j = 0; j < nowrole.Count; j++)
                    {
                        rolename[j] = nowrole[j].RoleName;
                    }
                    var roleApprovalfront = _sqlserverSql.Select<ApprovalComments>().Where(
                       o =>
                       o.BillNo == billNo
                       &&
                       o.isLeader == 0
                       &&
                       rolename.Contains(o.AType)
                     ).OrderBy(c => c.ApprovalDate);
                    CommonHelper.TxtLog("查询角色相关", roleApprovalfront.ToSql());

                    //查询单据中的在此角色组中的流程
                    var roleApproval = await roleApprovalfront.ToListAsync();
                    //有这个角色组的角色的审批流程
                    if (roleApproval.Count != 0)
                    {
                        for (int j = 0; j < roleApproval.Count; j++)
                        {
                           
                            string usedTime = "";
                            string Percentage = "";
                            DateTime BeginDate;
                            DateTime endDate;
                            //之前有流程 求开始日期
                            if (applyBillInfoDetails.Count != 0)
                            {
                                usedTime = roleApproval[j].ApprovalStatus == 0 ? "正在进行" : (roleApproval[j].ApprovalDate.Value.Subtract(applyBillInfoDetails[applyBillInfoDetails.Count - 1].EndDate).TotalSeconds).ToString();
                                usedTimeFormat = roleApproval[j].ApprovalStatus == 0 ? "正在进行":CommonHelper.GetUsedTime(roleApproval[j].ApprovalDate.Value,applyBillInfoDetails[applyBillInfoDetails.Count - 1].EndDate);
                                
                                Percentage = (usedTime != "正在进行" && allUsedTime != "正在进行" ? Math.Abs(double.Parse(usedTime) / double.Parse(allUsedTime)).ToString("P") : "正在进行");
                                BeginDate = applyBillInfoDetails[applyBillInfoDetails.Count - 1].EndDate;
                            }
                            else
                            {
                                usedTime = roleApproval[j].ApprovalStatus == 0 ? "正在进行" : (roleApproval[j].ApprovalDate.Value.Subtract(billDate).TotalSeconds).ToString();
                                usedTimeFormat = roleApproval[j].ApprovalStatus == 0 ? "正在进行" :CommonHelper.GetUsedTime(roleApproval[j].ApprovalDate.Value,billDate);
                                Percentage = (usedTime != "正在进行" && allUsedTime != "正在进行" ? Math.Abs(double.Parse(usedTime) / double.Parse(allUsedTime)).ToString("P") : "正在进行");
                                BeginDate = billDate;
                            }

                            endDate = roleApproval[j].ApprovalDate.Value;
                            var isExistRole = applyBillInfoDetails.Where(r => r.LinkType == roles[i].RoleGroupName);

                            //判断需不需要新增
                            if (isExistRole.Any())
                            {
                                
                                var existIndex = applyBillInfoDetails.IndexOf(isExistRole.FirstOrDefault());
                                
                                if (usedTime=="正在进行" || applyBillInfoDetails[existIndex].UsedTime == "正在进行" ||allUsedTime == "正在进行")
                                {
                                    ApprovalInfo = ApprovalInfo + $",{roleApproval[j].ApprovalName}未审批完成，已持续{CommonHelper.GetUsedTime(DateTime.Now, roleApproval[j].ApprovalDate.Value)}";
                                    applyBillInfoDetails[existIndex].Percentage = "正在进行";
                                    applyBillInfoDetails[existIndex].UsedTime = "正在进行";
                                    applyBillInfoDetails[existIndex].ApprovalInfo = ApprovalInfo;
                                }
                                else //新旧都是 有具体数值的
                                {
                                   
                                    endDate = roleApproval[j].ApprovalDate.Value >= applyBillInfoDetails[existIndex].EndDate ? roleApproval[j].ApprovalDate.Value : applyBillInfoDetails[existIndex].EndDate;
                                    applyBillInfoDetails[existIndex].UsedTime = CommonHelper.GetUsedTime(endDate, BeginDate);
                                    applyBillInfoDetails[existIndex].Percentage = Math.Abs(endDate.Subtract(BeginDate).TotalSeconds / double.Parse(allUsedTime)).ToString("P");
                                    applyBillInfoDetails[existIndex].EndDate = endDate;
                                    ApprovalInfo = ApprovalInfo + $",{roleApproval[j].ApprovalName}审批此单据消耗时间:{CommonHelper.GetUsedTime(roleApproval[j].ApprovalDate.Value, BeginDate)}";
                                    applyBillInfoDetails[existIndex].ApprovalInfo = ApprovalInfo;
                                }
                                
                            }
                            else
                            {
                                if (usedTime =="正在进行")
                                {
                                    ApprovalInfo =$"{roleApproval[j].ApprovalName}未审批完成，已持续{CommonHelper.GetUsedTime(DateTime.Now, roleApproval[j].ApprovalDate.Value)}";
                                }
                                else
                                {
                                    ApprovalInfo = $"{roleApproval[j].ApprovalName}审批此单据消耗时间:{CommonHelper.GetUsedTime(roleApproval[j].ApprovalDate.Value, BeginDate)}";
                                }
                                applyBillInfoDetails.Add(new ApplyBillInfoDetail
                                {
                                    BeginDate = BeginDate,
                                    EndDate = endDate,
                                    ApprovalInfo = ApprovalInfo,
                                    LinkType = roles[i].RoleGroupName,
                                    UsedTime = usedTimeFormat,
                                    Percentage = Percentage
                                });
                            }
                        }
                    }
                }
                
                
            }

            //去除其他流程 如果有link条件
            if (!string.IsNullOrEmpty(getInfoRequest.LinkTypeName) && applyBillInfoDetails.Count !=0)
            {
                var linkList = applyBillInfoDetails.Where(o => o.LinkType == getInfoRequest.LinkTypeName).ToList();
                if (linkList.Count!=0)
                {
                    applyBillInfoDetails.Clear();
                    applyBillInfoDetails.AddRange(linkList);
                }
            }

            //去除其他流程 如果有时间条件
            if (getInfoRequest.OverDayCount!=0 && applyBillInfoDetails.Count != 0)
            {
                applyBillInfoDetails.Clear();
                foreach (var item in applyBillInfoDetails)
                {
                    if (item.UsedTime== "正在进行")
                    {
                        if (DateTime.Now.Subtract(item.BeginDate).TotalSeconds >= getInfoRequest.OverDayCount*24)
                        {
                            applyBillInfoDetails.Add(item);
                        }
                    }
                    else
                    {
                        if (double.Parse(item.UsedTime) >= getInfoRequest.OverDayCount * 24)
                        {
                            applyBillInfoDetails.Add(item);
                        }
                    }
                }
            }

            return applyBillInfoDetails;
        }
    }
}
