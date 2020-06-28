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
                var totals = mainData.Count();
                var maindatafy = mainData.Skip((approverRequest.Page - 1) * approverRequest.Limit).Take(approverRequest.Limit).ToList();
                result.code = 0;
                result.message = "获取数据成功";
                result.data = new ApproverData
                {
                    Total = totals,
                    Items = maindatafy
                };
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计审批人出参", JsonConvert.SerializeObject(result));
                });
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
            try
            {
                CommonHelper.TxtLog("统计审批人单据入参", JsonConvert.SerializeObject(approver_BillInfoRequest));
                var mainData = new List<Approver_BillInfo>();
                var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments, ExpeTrav, ExpeOther, ExpeEnteMent, BillClass>()
                 .LeftJoin((a, b, c, d, e) => a.BillNo == b.BillNo)
                 .LeftJoin((a, b, c, d, e) => a.BillNo == c.BillNo)
                 .LeftJoin((a, b, c, d, e) => a.BillNo == d.BillNo)
                  .LeftJoin((a, b, c, d, e) => e.BillClassid == e.BillClassid)
                 .Where(
                   (a, b, c, d, e) =>
                     a.ApprovalID == approver_BillInfoRequest.JobNumber
                     &&
                    (
                    ((b.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.BillDate)
                    ||
                    (b.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= b.AuditingDate)
                    ||
                    (b.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= b.AuditingDate))
                    ||

                    ((c.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.BillDate)
                    ||
                    (c.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= c.AuditingDate)
                    ||
                    (c.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= c.AuditingDate))
                    ||

                    ((d.BillDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.BillDate)
                    ||
                    (d.AuditingDate <= approver_BillInfoRequest.EndDate && approver_BillInfoRequest.BeginDate <= d.AuditingDate)
                    ||
                    (d.AuditingDate <= approver_BillInfoRequest.BeginDate && approver_BillInfoRequest.EndDate <= d.AuditingDate))
                    )
                 );
                //var fydata = approvalCommentsmain.Skip((approver_BillInfoRequest.Page - 1) * approver_BillInfoRequest.Limit).Take(approver_BillInfoRequest.Limit);
                var approvalCommentsdata = await approvalCommentsmain.ToListAsync();
                foreach (var item in approvalCommentsdata)
                {
                    mainData.Add(new Approver_BillInfo { 
                        ApproverType = item.AType,
                        BillType = (await _sqlserverSql.Select<BillClass>().Where(o=>o.BillClassid==item.BillClassid).FirstAsync()).BillName,
                        BillNo = item.BillNo,
                        UsedTime = "暂未写完逻辑"
                    });
                }
                var totals = mainData.Count();
                var maindatafy = mainData.Skip((approver_BillInfoRequest.Page - 1) * approver_BillInfoRequest.Limit).Take(approver_BillInfoRequest.Limit).ToList();
                result.code = 0;
                result.message = "获取数据成功";
                result.data = new Approver_BillInfoData
                {
                    Total = totals,
                    Items = maindatafy
                };
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("统计审批人单据出参", JsonConvert.SerializeObject(result));
                });
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

        private async Task<List<Approver>> GetApproverByNameAsync(ApproverRequest approverRequest)
        {
            var mainData = new List<Approver>();
            var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments, ExpeTrav, ExpeOther, ExpeEnteMent>()
                 .LeftJoin((a, b, c, d) => a.BillNo == b.BillNo)
                 .LeftJoin((a, b, c, d) => a.BillNo == c.BillNo)
                 .LeftJoin((a, b, c, d) => a.BillNo == d.BillNo)
                 .Where(
                   (a, b, c, d) =>
                     a.AType==approverRequest.LinkDetailName   
                     &&
                    (
                    ((b.BillDate <= approverRequest.EndDate && approverRequest.BeginDate <= b.BillDate)
                    ||
                    (b.AuditingDate <= approverRequest.EndDate && approverRequest.BeginDate <= b.AuditingDate)
                    ||
                    (b.AuditingDate <= approverRequest.BeginDate && approverRequest.EndDate <= b.AuditingDate))
                    ||

                    ((c.BillDate <= approverRequest.EndDate && approverRequest.BeginDate <= c.BillDate)
                    ||
                    (c.AuditingDate <= approverRequest.EndDate && approverRequest.BeginDate <= c.AuditingDate)
                    ||
                    (c.AuditingDate <= approverRequest.BeginDate && approverRequest.EndDate <= c.AuditingDate))
                    ||

                    ((d.BillDate <= approverRequest.EndDate && approverRequest.BeginDate <= d.BillDate)
                    ||
                    (d.AuditingDate <= approverRequest.EndDate && approverRequest.BeginDate <= d.AuditingDate)
                    ||
                    (d.AuditingDate <= approverRequest.BeginDate && approverRequest.EndDate <= d.AuditingDate))
                    )
                 );
            var approvalCommentsdata = await approvalCommentsmain.ToListAsync();
            foreach (var item in approvalCommentsdata)
            {
                mainData.Add(new Approver
                {
                    AllUsedTime = "20天15小时30分钟20秒",
                    ApproverName = item.ApprovalName,
                    AvgUsedTime = "0天10小时30分钟20秒",
                    JobNumber = item.ApprovalID,
                    DeptInfo = "测试数据部门"
                });
            }


            return mainData;
        }

        private async Task<List<Approver>> GetAllApproverAsync(ApproverRequest approverRequest)
        {
            var mainData = new List<Approver>();

            var approvalCommentsmain = _sqlserverSql.Select<ApprovalComments, ExpeTrav, ExpeOther, ExpeEnteMent>()
                 .LeftJoin((a, b, c, d) => a.BillNo == b.BillNo)
                 .LeftJoin((a, b, c, d) => a.BillNo == c.BillNo)
                 .LeftJoin((a, b, c, d) => a.BillNo == d.BillNo)
                 .Where((a, b, c, d) =>
                    ((b.BillDate <= approverRequest.EndDate && approverRequest.BeginDate <= b.BillDate)
                    ||
                    (b.AuditingDate <= approverRequest.EndDate && approverRequest.BeginDate <= b.AuditingDate)
                    ||
                    (b.AuditingDate <= approverRequest.BeginDate && approverRequest.EndDate <= b.AuditingDate))
                    ||

                    ((c.BillDate <= approverRequest.EndDate && approverRequest.BeginDate <= c.BillDate)
                    ||
                    (c.AuditingDate <= approverRequest.EndDate && approverRequest.BeginDate <= c.AuditingDate)
                    ||
                    (c.AuditingDate <= approverRequest.BeginDate && approverRequest.EndDate <= c.AuditingDate))
                    ||

                    ((d.BillDate <= approverRequest.EndDate && approverRequest.BeginDate <= d.BillDate)
                    ||
                    (d.AuditingDate <= approverRequest.EndDate && approverRequest.BeginDate <= d.AuditingDate)
                    ||
                    (d.AuditingDate <= approverRequest.BeginDate && approverRequest.EndDate <= d.AuditingDate))
                 );
            var approvalCommentsdata = await approvalCommentsmain.ToListAsync();
            foreach (var item in approvalCommentsdata)
            {
                mainData.Add(new Approver {
                    AllUsedTime ="20天15小时30分钟20秒",
                    ApproverName = item.ApprovalName,
                    AvgUsedTime = "0天10小时30分钟20秒",
                    JobNumber = item.ApprovalID,
                    DeptInfo ="测试数据部门"
                });
            }
            return mainData;
        }
    }
}
