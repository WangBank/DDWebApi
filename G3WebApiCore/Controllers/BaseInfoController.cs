using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using G3WebApiCore.Model.FreeSqlHelper;
using G3WebApiCore.Model.Response;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using G3WebApiCore.Model.ApiDtos;
using G3WebApiCore.Commom;
using Newtonsoft.Json;

namespace G3WebApiCore.Controllers
{
    /// <summary>
    /// 基础信息api
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class BaseInfoController : ControllerBase
    {

        /// <summary>
        /// 主db操作
        /// </summary>
        public IFreeSql _sqlserverSql;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="sqlserverSql"></param>
        public BaseInfoController(IFreeSql<SqlServerFlag> sqlserverSql)
        {
            _sqlserverSql = sqlserverSql;
        }


        /// <summary>
        /// 获取审批流程id及名称
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<LinkInfoResponse> GetLinkInfo()
        {
            var result = new  LinkInfoResponse();
            try
            {
                List<LinkInfoData> data = new List<LinkInfoData>();
                data.Add(new LinkInfoData { 
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "直接主管"
                });

                data.Add(new LinkInfoData
                {
                    Id = Guid.NewGuid().ToString("N"),
                    Name = "二级主管"
                });
                
                var grouplist = await _sqlserverSql.Select<RoleGroup>().Where(r => r.Status == 1).ToListAsync();
                foreach (var item in grouplist)
                {
                    data.Add(new LinkInfoData { 
                        Id = item.RoleGroupId,
                        Name = item.RoleGroupName
                    });
                }
                return new LinkInfoResponse
                {
                    code =0,
                    data = data,
                    message = ""
                };
            }
            catch (Exception ex)
            {
                result.code = -1;
                result.message = "获取信息出错!";
                _ = Task.Run(() =>
                {
                    CommonHelper.TxtLog("获取审批流程", JsonConvert.SerializeObject(result));
                });
                return result;
            }
           
        }
    }
}
