using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DingtalkApprovalApi.Entities.Models.User
{
    /// <summary>
    /// 动态路由
    /// </summary>
    public class UserMenus
    {
        //@"[{path: '/test',component: 'Layout',children: [{path: 'index',name: 'Test',component: 'test/index',meta: { title: 'test', icon: 'dashboard' }},{ path: '*', redirect: '/404', hidden: true }]}]"

        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }

        /// <summary>
        /// 路由匹配名
        /// </summary>
        public string Path { get; set; }

        /// <summary>
        /// 组件名称
        /// </summary>
        public string Component { get; set; }

        /// <summary>
        /// 图标及标题
        /// </summary>
        public Meta Meta { get; set; }

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 重定向路由匹配名
        /// </summary>
        public string Redirect { get; set; }

        /// <summary>
        /// 是否在侧边显示
        /// </summary>
        public bool Hidden { get; set; }

        /// <summary>
        /// 父id
        /// </summary>
        public string FatherId { get; set; }

        /// <summary>
        /// 子
        /// </summary>
        public List<UserMenus> Children { get; set; }

    }
}
