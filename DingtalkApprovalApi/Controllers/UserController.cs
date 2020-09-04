using DingtalkApprovalApi.Entities.Dtos;
using DingtalkApprovalApi.Entities.Models;
using DingtalkApprovalApi.Entities.Models.User;
using DingtalkApprovalApi.JWT;
using FreeSql;
using JWTToken.Filter;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.Controllers
{
    /// <summary>
    /// User Api
    /// </summary>
    
    public class UserController : CommonBaseController
    {
      
        BaseRepository<UserDto> resUser;
        private readonly ITokenHelper tokenHelper = null;
        /// <summary>
        /// ctor
        /// </summary>
        /// <param name="_tokenHelper"></param>
        /// <param name="_userDto"></param>
        public UserController(ITokenHelper _tokenHelper, BaseRepository<UserDto> _userDto)
        {
            tokenHelper = _tokenHelper;
            resUser = _userDto;
        }

        /// <summary>
        /// 登录测试
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<ActionResult<CommonResponse>> Login([FromBody] UserInfo user)
        {
            var ret = new CommonResponse();

            if (string.IsNullOrEmpty(user.Username) || string.IsNullOrEmpty(user.Password))
            {
                ret.errcode = -1;
                ret.errmsg = "用户名密码不能为空";
                return ret;
            }

            if (await resUser.Select.AnyAsync(u => u.UserName == user.Username && u.Password == user.Password))
            {
                Dictionary<string, object> keyValuePairs = new Dictionary<string, object>
                    {
                        { "username", user.Username },
                        { "createTime", DateTime.Now}
                    };
                ret.errcode = 0;
                ret.errmsg = "登录成功";
                ret.TokenInfo = tokenHelper.CreateToken(keyValuePairs);
                ret.Data = "";
            }

            return ret;
        }


        /// <summary>
        /// UserInfo
        /// </summary>
        /// <returns></returns>
        [ServiceFilter(typeof(TokenFilter))]
        [HttpGet]
        public async Task<IActionResult> Info(string username)
        {
            var userDto = await resUser.Select.Where(u => u.UserName == username).FirstAsync();
            var useInfo = new UserInfo
            {
                Id = userDto.Id,
                NickName = userDto.NickName,
                Avatar = userDto.Avatar,
                Roles = new string[] { "admin" }
            };
            return Ok(useInfo);
        }
    }
}


