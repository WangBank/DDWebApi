using DingtalkApprovalApi.Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.Entities.Models.User
{
    /// <summary>
    /// UserInfo
    /// </summary>
    public class UserInfo
    {
        /// <summary>
        /// guid
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 昵称
        /// </summary>
        public string NickName { get; set; }
        /// <summary>
        /// 用户名
        /// </summary>
        public string Username { get; set; }
        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 头像
        /// </summary>
        public string Avatar { get; set; }

        /// <summary>
        /// 角色集合
        /// </summary>
        public string[] Roles { get; set; }

        /// <summary>
        /// 是否删除 0：未删除 1：已删除
        /// </summary>
        public int Deleted { get; set; }


        /// <summary>
        /// 用户路由
        /// </summary>
        public List<UserMenus> Menus { get; set; }

    }

   

}
