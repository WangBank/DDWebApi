using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.Model.Response
{

    /// <summary>
    /// response
    /// </summary>
    public class LinkInfoResponse
    {
        /// <summary>
        /// 0 成功
        /// </summary>
        public int code { get; set; }

        /// <summary>
        /// 错误信息
        /// </summary>
        public string message { get; set; }

        /// <summary>
        /// 主数据
        /// </summary>
        public List<LinkInfoData> data { get; set; }
    }

    /// <summary>
    /// 包含明细及总条数
    /// </summary>
    public class LinkInfoData
    {

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// id
        /// </summary>
        public string Id { get; set; }
    }
}

