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
    /// 获取某段时间内的审批人信息api
    /// </summary>
    [EnableCors("AllowCors")]
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class ApproverInfoController : ControllerBase
    {
        /// <summary>
        /// 主db操作
        /// </summary>
        public IFreeSql _sqlserverSql;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sqlserverSql"></param>
        public ApproverInfoController(IFreeSql<SqlServerFlag> sqlserverSql)
        {
            _sqlserverSql = sqlserverSql;
        }


        /// <summary>
        /// 获得这一段时间所有参与审批的审批人
        /// </summary>
        /// <param name="approverRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ApproverResponse> GetApproverInfo(ApproverRequest approverRequest)
        {
            var result = new ApproverResponse();
            if (approverRequest == null)
            {
                result.code = -1;
                result.message = "请检查入参";
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计审批人出参", JsonConvert.SerializeObject(result));
                });
                return result;

            }
            try
            {
                CommonHelper.TxtLog("统计审批人入参", JsonConvert.SerializeObject(approverRequest));
                var mainData = new List<Approver>();
                if (approverRequest.Page == 0 || approverRequest.Limit == 0)
                {
                    approverRequest.Page = 1;
                    approverRequest.Limit = 10;
                }
                var amountpage = approverRequest.Page;
                var amountlimit = approverRequest.Limit;
                //判断选择的单据类型
                switch (approverRequest.LinkDetailName)
                {
                    case "全部流程":
                        //全部单据
                        mainData = await GetAllApproverAsync(approverRequest);
                        break;

                    default:
                        //根据流程名获取人
                        mainData = await GetApproverByNameAsync(approverRequest);
                        break;
                }
                //超出时间限制
                if (approverRequest.OverDayCount!=0)
                {
                    mainData = mainData.Where(o => o.AvgTimeSpanTime.TotalSeconds >= approverRequest.OverDayCount * 24 * 60 * 60).ToList();
                }

                var totals = mainData.Count();
                mainData = mainData.Skip((amountpage - 1) * amountlimit).Take(amountlimit).ToList();
                result.code = 0;
                result.message = "获取数据成功";
                result.data = new ApproverData
                {
                    Total = totals,
                    Items = mainData
                };
                return result;

            }
            catch (Exception ex)
            {

                result.code = -1;
                result.message = "获取数据失败" + ex.Message;
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计审批人出参", JsonConvert.SerializeObject(result));
                    CommonHelper.TxtLog("异常错误信息", JsonConvert.SerializeObject(ex));
                });
                return result;
            }


        }

        /// <summary>
        /// 获取某审批人的审批单据
        /// </summary>
        /// <param name="approver_BillInfoRequest"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<Approver_BillInfoResponse> GetBillInfo(Approver_BillInfoRequest approver_BillInfoRequest)
        {
            if (approver_BillInfoRequest.Limit == 0 || approver_BillInfoRequest.Page == 0)
            {
                approver_BillInfoRequest.Limit = 10; 
                approver_BillInfoRequest.Page = 1;
            }
            var result = new Approver_BillInfoResponse();
            if (approver_BillInfoRequest == null)
            {
                result.code = -1;
                result.message = "请检查入参";
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计审批人单据出参", JsonConvert.SerializeObject(result));
                });
                return result;

            }
            approver_BillInfoRequest.BeginDate = approver_BillInfoRequest.BeginDate >= DateTime.Parse("2019-11-15") ? approver_BillInfoRequest.BeginDate : DateTime.Parse("2019-11-16");
            try
            {
               CommonHelper.TxtLog("统计审批人单据入参", JsonConvert.SerializeObject(approver_BillInfoRequest));
                TimeSpan allTimeSpanUsed = new TimeSpan();
                string allUsedTime = string.Empty;
                var mainData = new List<Approver_BillInfo>();
                //var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments, ExpeTrav, ExpeOther, ExpeEnteMent>()
                //  .InnerJoin((a, b, c, d) => a.BillNo == b.BillNo)
                //  .InnerJoin((a, b, c, d) => a.BillNo == c.BillNo)
                //  .InnerJoin((a, b, c, d) => a.BillNo == d.BillNo)
                //  .Where(
                //    (a, b, c, d) =>
                //      a.ApprovalID == approver_BillInfoRequest.JobNumber
                //      &&
                //     (
                //     ((b.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.BillDate)
                //     ||
                //     (b.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.AuditingDate)
                //     ||
                //     (b.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= b.AuditingDate))
                //     ||

                //     ((c.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.BillDate)
                //     ||
                //     (c.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.AuditingDate)
                //     ||
                //     (c.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= c.AuditingDate))
                //     ||

                //     ((d.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.BillDate)
                //     ||
                //     (d.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.AuditingDate)
                //     ||
                //     (d.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= d.AuditingDate))
                //     )
                //  );





                var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments>()
                    .Where(a => a.ApprovalID == approver_BillInfoRequest.JobNumber)
                  .Where(a =>
                    _sqlserverSql.Select<ExpeOther>().As("b").Where(
                        b => b.BillNo == a.BillNo
                        &&
                        ((b.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.BillDate)
                        ||
                        (b.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.AuditingDate)
                        ||
                        (b.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= b.AuditingDate))
                        &&
                        b.BillDate> DateTime.Parse("2019-11-15")
                    ).Any()
                    ||
                     _sqlserverSql.Select<ExpeTrav>().As("c").Where(
                       c => a.BillNo == c.BillNo
                        &&
                       ((c.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.BillDate)
                         ||
                         (c.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.AuditingDate)
                         ||
                         (c.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= c.AuditingDate))
                         &&
                        c.BillDate > DateTime.Parse("2019-11-15")
                    ).Any()
                     ||
                     _sqlserverSql.Select<ExpeEnteMent>().As("d").Where(
                       d => a.BillNo == d.BillNo
                        &&
                       ((d.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.BillDate)
                     ||
                     (d.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.AuditingDate)
                     ||
                     (d.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= d.AuditingDate))
                     &&
                       d.BillDate > DateTime.Parse("2019-11-15")
                    ).Any());

                //var sqlss = approvalCommentsmain.ToSql();

               
                var approvalCommentsdata = await approvalCommentsmain.ToListAsync();
                if (approvalCommentsdata.Count==0)
                {
                    result.code = -1;
                    result.message = "请检查入参";
                    return result;
                }
                foreach (var item in approvalCommentsdata)
                {
                    string UsedTime = string.Empty;
                    TimeSpan timespanUsedTime=new TimeSpan();
                    //查询当前单号的花费时间 
                    //如果是正在进行的，用当前时间减去
                    if (item.ApprovalStatus ==0)
                    {
                       timespanUsedTime = DateTime.Now - item.ApprovalDate.Value;
                        UsedTime = CommonHelper.GetUsedTime(timespanUsedTime);
                        mainData.Add(new Approver_BillInfo
                        {
                            ApproverType = item.AType,
                            BillType = (await _sqlserverSql.Select<BillClass>().Where(o => o.BillClassid == item.BillClassid).FirstAsync()).BillName.Trim(),
                            BillNo = item.BillNo,
                            UsedTime = UsedTime,
                            TimeSpanUsed = timespanUsedTime,
                            ApprovalState = 0
                        });
                        
                    }
                    else //已审批
                    {
                        //之前是否计算了这个单据的时间
                        var isExistBillNo = mainData.Where(o => o.BillNo == item.BillNo);
                        if (!isExistBillNo.Any())
                        {
                            //求出这个单据流程的上一级审批时间
                            var nowBillNoInfo =await _sqlserverSql.Select<ApprovalComments>().Where(c => c.BillNo == item.BillNo).OrderBy(c => c.ApprovalDate).ToListAsync();

                            //zhege wanyi shifou zai diyige shenpiguo
                            if (nowBillNoInfo[0].ApprovalID== item.ApprovalID)
                            {
                                DateTime beginTime = new DateTime();
                                //求开始时间
                                if (item.BillNo.ToLower().StartsWith("cl"))
                                {
                                    beginTime = await _sqlserverSql.Select<ExpeTrav>().Where(o => o.BillNo == item.BillNo).FirstAsync(o => o.BillDate);
                                }
                                else if (item.BillNo.ToLower().StartsWith("zdf"))
                                {
                                    beginTime = await _sqlserverSql.Select<ExpeEnteMent>().Where(o => o.BillNo == item.BillNo).FirstAsync(o => o.BillDate);
                                }
                                else
                                {
                                    beginTime = await _sqlserverSql.Select<ExpeOther>().Where(o => o.BillNo == item.BillNo).FirstAsync(o => o.BillDate);
                                }
                                timespanUsedTime = item.ApprovalDate.Value - beginTime;
                                UsedTime = CommonHelper.GetUsedTime(timespanUsedTime);

                            }
                            else //不是第一个流程
                            {
                                //求出上一个流程的审批时间
                                var tempComment = nowBillNoInfo.Where(o => o.CommentsId == item.CommentsId).First();
                                int sygLc = nowBillNoInfo.IndexOf(tempComment);
                                timespanUsedTime = tempComment.ApprovalDate.Value- nowBillNoInfo[sygLc-1].ApprovalDate.Value;
                                UsedTime = CommonHelper.GetUsedTime(timespanUsedTime);
                            }

                            // I am Very irritable, do not want to get a result that this bill 是否是这个人发起又审批的了
                            // if you see this,good luck to you
                            if (timespanUsedTime.TotalSeconds >2)
                            {
                                mainData.Add(new Approver_BillInfo
                                {
                                    ApproverType = item.AType,
                                    BillType = (await _sqlserverSql.Select<BillClass>().Where(o => o.BillClassid == item.BillClassid).FirstAsync()).BillName.Trim(),
                                    BillNo = item.BillNo,
                                    UsedTime = UsedTime,
                                    TimeSpanUsed = timespanUsedTime,
                                    ApprovalState = 1
                                });
                            }
                           
                        }
                        else//那就是一个单据多个审批角色，时间忽略不计，角色上当前的
                        {
                            var existIndex = mainData.IndexOf(isExistBillNo.FirstOrDefault());
                            mainData[existIndex].ApproverType = $"{mainData[existIndex].ApproverType},{item.AType}";
                        }

                    }

                    if (timespanUsedTime.TotalSeconds > 2)
                    {
                        allTimeSpanUsed = allTimeSpanUsed + timespanUsedTime;
                        allUsedTime = CommonHelper.GetUsedTime(allTimeSpanUsed);
                    }
                    
                }
                if (mainData.Count==0)
                {
                    result.code = -1;
                    result.message = "获取数据失败";
                    return result;
                }
                var maindatafy = mainData.Skip((approver_BillInfoRequest.Page - 1) * approver_BillInfoRequest.Limit).Take(approver_BillInfoRequest.Limit).ToList();
                result.code = 0;
                result.message = "获取数据成功";
                result.data = new Approver_BillInfoData
                {
                    Total = mainData.Count,
                    Items = maindatafy,
                    AllTimeSpanUsed = allTimeSpanUsed,
                    AllTimeUsed = allUsedTime
                };
               
                return result;
            }
            catch (Exception ex)
            {
                result.code = -1;
                result.message = "获取数据失败" + ex.Message;
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计审批人单据出参", JsonConvert.SerializeObject(result));
                    CommonHelper.TxtLog("异常错误信息", JsonConvert.SerializeObject(ex));
                });
                return result;
            }
        }


        private async Task<Approver_BillInfoResponse> GetBillInfoMain(Approver_BillInfoRequest approver_BillInfoRequest)
        {
            var result = new Approver_BillInfoResponse();
            approver_BillInfoRequest.BeginDate = approver_BillInfoRequest.BeginDate >= DateTime.Parse("2019-11-15") ? approver_BillInfoRequest.BeginDate : DateTime.Parse("2019-11-16");
            try
            {
                TimeSpan allTimeSpanUsed = new TimeSpan();
                string allUsedTime = string.Empty;
                var mainData = new List<Approver_BillInfo>();

                var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments>()
                    .Where(a => a.ApprovalID == approver_BillInfoRequest.JobNumber)
                  .Where(a =>
                    _sqlserverSql.Select<ExpeOther>().As("b").Where(
                        b => b.BillNo == a.BillNo
                        &&
                        ((b.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.BillDate)
                        ||
                        (b.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.AuditingDate)
                        ||
                        (b.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= b.AuditingDate))
                        &&
                        b.BillDate > DateTime.Parse("2019-11-15")
                    ).Any()
                    ||
                     _sqlserverSql.Select<ExpeTrav>().As("c").Where(
                       c => a.BillNo == c.BillNo
                        &&
                       ((c.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.BillDate)
                         ||
                         (c.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.AuditingDate)
                         ||
                         (c.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= c.AuditingDate))
                         &&
                        c.BillDate > DateTime.Parse("2019-11-15")
                    ).Any()
                     ||
                     _sqlserverSql.Select<ExpeEnteMent>().As("d").Where(
                       d => a.BillNo == d.BillNo
                        &&
                       ((d.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.BillDate)
                     ||
                     (d.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.AuditingDate)
                     ||
                     (d.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= d.AuditingDate))
                     &&
                       d.BillDate > DateTime.Parse("2019-11-15")
                    ).Any());
                var approvalCommentsdata = await approvalCommentsmain.ToListAsync();
                if (approvalCommentsdata.Count == 0)
                {
                    result.code = -1;
                    result.message = "请检查入参";
                    return result;
                }
                foreach (var item in approvalCommentsdata)
                {
                    string UsedTime = string.Empty;
                    TimeSpan timespanUsedTime = new TimeSpan();
                    //查询当前单号的花费时间 
                    //如果是正在进行的，用当前时间减去
                    if (item.ApprovalStatus == 0)
                    {
                        timespanUsedTime = DateTime.Now - item.ApprovalDate.Value;
                        UsedTime = CommonHelper.GetUsedTime(timespanUsedTime);
                        mainData.Add(new Approver_BillInfo
                        {
                            ApproverType = item.AType,
                            BillType = (await _sqlserverSql.Select<BillClass>().Where(o => o.BillClassid == item.BillClassid).FirstAsync()).BillName.Trim(),
                            BillNo = item.BillNo,
                            UsedTime = UsedTime,
                            TimeSpanUsed = timespanUsedTime,
                            ApprovalState = 0
                        });

                    }
                    else //已审批
                    {
                        //之前是否计算了这个单据的时间
                        var isExistBillNo = mainData.Where(o => o.BillNo == item.BillNo);
                        if (!isExistBillNo.Any())
                        {
                            //求出这个单据流程的上一级审批时间
                            var nowBillNoInfo = await _sqlserverSql.Select<ApprovalComments>().Where(c => c.BillNo == item.BillNo).OrderBy(c => c.ApprovalDate).ToListAsync();

                            //zhege wanyi shifou zai diyige shenpiguo
                            if (nowBillNoInfo[0].ApprovalID == item.ApprovalID)
                            {
                                DateTime beginTime = new DateTime();
                                //求开始时间
                                if (item.BillNo.ToLower().StartsWith("cl"))
                                {
                                    beginTime = await _sqlserverSql.Select<ExpeTrav>().Where(o => o.BillNo == item.BillNo).FirstAsync(o => o.BillDate);
                                }
                                else if (item.BillNo.ToLower().StartsWith("zdf"))
                                {
                                    beginTime = await _sqlserverSql.Select<ExpeEnteMent>().Where(o => o.BillNo == item.BillNo).FirstAsync(o => o.BillDate);
                                }
                                else
                                {
                                    beginTime = await _sqlserverSql.Select<ExpeOther>().Where(o => o.BillNo == item.BillNo).FirstAsync(o => o.BillDate);
                                }
                                timespanUsedTime = item.ApprovalDate.Value - beginTime;
                                UsedTime = CommonHelper.GetUsedTime(timespanUsedTime);

                            }
                            else //不是第一个流程
                            {
                                //求出上一个流程的审批时间
                                var tempComment = nowBillNoInfo.Where(o => o.CommentsId == item.CommentsId).First();
                                int sygLc = nowBillNoInfo.IndexOf(tempComment);
                                timespanUsedTime = tempComment.ApprovalDate.Value - nowBillNoInfo[sygLc - 1].ApprovalDate.Value;
                                UsedTime = CommonHelper.GetUsedTime(timespanUsedTime);
                            }

                            // I am Very irritable, do not want to get a result that this bill 是否是这个人发起又审批的了
                            // if you see this,good luck to you
                            if (timespanUsedTime.TotalSeconds > 2)
                            {
                                mainData.Add(new Approver_BillInfo
                                {
                                    ApproverType = item.AType,
                                    BillType = (await _sqlserverSql.Select<BillClass>().Where(o => o.BillClassid == item.BillClassid).FirstAsync()).BillName.Trim(),
                                    BillNo = item.BillNo,
                                    UsedTime = UsedTime,
                                    TimeSpanUsed = timespanUsedTime,
                                    ApprovalState = 1
                                });
                            }

                        }
                        else//那就是一个单据多个审批角色，时间忽略不计，角色上当前的
                        {
                            var existIndex = mainData.IndexOf(isExistBillNo.FirstOrDefault());
                            mainData[existIndex].ApproverType = $"{mainData[existIndex].ApproverType},{item.AType}";
                        }

                    }

                    if (timespanUsedTime.TotalSeconds > 2)
                    {
                        allTimeSpanUsed = allTimeSpanUsed + timespanUsedTime;
                        allUsedTime = CommonHelper.GetUsedTime(allTimeSpanUsed);
                    }

                }
                if (mainData.Count == 0)
                {
                    result.code = -1;
                    result.message = "获取数据失败";
                    return result;
                }
                result.code = 0;
                result.message = "获取数据成功";
                result.data = new Approver_BillInfoData
                {
                    Total = mainData.Count,
                    Items = mainData,
                    AllTimeSpanUsed = allTimeSpanUsed,
                    AllTimeUsed = allUsedTime
                };

                return result;
            }
            catch (Exception ex)
            {
                result.code = -1;
                result.message = "获取数据失败" + ex.Message;
                return result;
            }
        }


        private async Task<List<Approver>> GetApproverByNameAsync(ApproverRequest approverRequest)
        {
            var mainData = new List<Approver>();
            var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments>().Distinct().Where(a =>
                    a.AType == approverRequest.LinkDetailName).Page(approverRequest.Page, approverRequest.Limit)
          ;
            var approvalCommentsdata = await approvalCommentsmain.ToListAsync(a => new { a.ApprovalID, a.ApprovalName });

            //循环这些审批意见，添加到list中，如果有重复的 增加usedtime 得出总的消耗时间
            for (int i = 0; i < approvalCommentsdata.Count; i++)
            {
                var DeptCode = await _sqlserverSql.Select<FlowEmployee>().Where(f =>
                    f.employeecode == approvalCommentsdata[i].ApprovalID
                 ).ToListAsync(a => a.orgcode);
                var deptcodeinfo = string.Join(",", (await _sqlserverSql.Select<Organization>().Where(o => DeptCode.Contains(o.Guid)).ToListAsync(a => a.Name)));

                var isExistJobNumber = mainData.Where(o => o.JobNumber == approvalCommentsdata[i].ApprovalID);
                if (!isExistJobNumber.Any())
                {
                    //求出这个人的总单据
                    var billinfo = await GetBillInfoMain(new Approver_BillInfoRequest
                    {
                        BeginDate = approverRequest.BeginDate,
                        EndDate = approverRequest.EndDate,
                        JobNumber = approvalCommentsdata[i].ApprovalID,
                        Limit = 0,
                        Page = 0
                    });

                    if (billinfo.data != null)
                    {
                        mainData.Add(new Approver
                        {
                            AllUsedTime = billinfo.data.AllTimeUsed,
                            ApproverName = approvalCommentsdata[i].ApprovalName,
                            AvgTimeSpanTime = billinfo.data.AllTimeSpanUsed / billinfo.data.Total,
                            AvgUsedTime = CommonHelper.GetUsedTime(billinfo.data.AllTimeSpanUsed / billinfo.data.Total),
                            JobNumber = approvalCommentsdata[i].ApprovalID,
                            DeptInfo = deptcodeinfo,
                            AllTimeSpanUsed = billinfo.data.AllTimeSpanUsed,
                            BillCount = billinfo.data.Total
                        });
                    }
                }
            }

            return mainData;
        }

        private async Task<List<Approver>> GetAllApproverAsync(ApproverRequest approverRequest)
        {
            var mainData = new List<Approver>();

            var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments>().Distinct();
            var approvalCommentsdata = await approvalCommentsmain.ToListAsync(a=> new { a.ApprovalID,a.ApprovalName });
            
            
            //循环这些审批意见，添加到list中，如果有重复的 增加usedtime 得出总的消耗时间
            for (int i = 0; i < approvalCommentsdata.Count; i++)
            {
               var DeptCode = await _sqlserverSql.Select<FlowEmployee>().Where(f=>
                   f.employeecode == approvalCommentsdata[i].ApprovalID
                ).ToListAsync(a =>  a.orgcode);
                var deptcodeinfo = string.Join(",", (await _sqlserverSql.Select<Organization>().Where(o => DeptCode.Contains(o.Guid)).ToListAsync(a => a.Name)));

                var isExistJobNumber = mainData.Where(o => o.JobNumber == approvalCommentsdata[i].ApprovalID);
                if (!isExistJobNumber.Any())
                {
                    //求出这个人的总单据
                    var billinfo = await GetBillInfoMain(new Approver_BillInfoRequest
                    {
                        BeginDate = approverRequest.BeginDate,
                        EndDate = approverRequest.EndDate,
                        JobNumber = approvalCommentsdata[i].ApprovalID,
                        Limit = 0,
                        Page = 0
                    });
                    if (billinfo.data != null)
                    {
                        mainData.Add(new Approver
                        {
                            AllUsedTime = billinfo.data.AllTimeUsed,
                            ApproverName = approvalCommentsdata[i].ApprovalName,
                            AvgTimeSpanTime = billinfo.data.AllTimeSpanUsed / billinfo.data.Total,
                            AvgUsedTime = CommonHelper.GetUsedTime(billinfo.data.AllTimeSpanUsed / billinfo.data.Total),
                            JobNumber = approvalCommentsdata[i].ApprovalID,
                            DeptInfo = deptcodeinfo,
                            AllTimeSpanUsed = billinfo.data.AllTimeSpanUsed,
                            BillCount = billinfo.data.Total
                        });
                    }
                   
                }
            }

            return mainData;
        }

        
    }
}
