using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace G3WebApiCore.JWT
{
    /// <summary>
    /// token工具类的接口，方便使用依赖注入，很简单提供两个常用的方法
    /// </summary>
    public interface ITokenHelper
    {
        /// <summary>
        /// 根据一个对象通过反射提供负载生成token
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="user"></param>
        /// <returns></returns>
        TnToken CreateToken<T>(T user) where T : class;
        /// <summary>
        /// 根据键值对提供负载生成token
        /// </summary>
        /// <param name="keyValuePairs"></param>
        /// <returns></returns>
        TnToken CreateToken(Dictionary<string, string> keyValuePairs);

        /// <summary>
        /// Token验证
        /// </summary>
        /// <param name="encodeJwt">token</param>
        /// <param name="validatePayLoad">自定义各类验证； 是否包含那种申明，或者申明的值</param>
        /// <returns></returns>
        bool ValiToken(string encodeJwt, Func<Dictionary<string, string>, bool> validatePayLoad = null);
        /// <summary>
        /// 带返回状态的Token验证
        /// </summary>
        /// <param name="encodeJwt">token</param>
        /// <param name="validatePayLoad">自定义各类验证； 是否包含那种申明，或者申明的值</param>
        /// <param name="action"></param>
        /// <returns></returns>
        TokenType ValiTokenState(string encodeJwt, Func<Dictionary<string, string>, bool> validatePayLoad, Action<Dictionary<string, string>> action);
    }
}
