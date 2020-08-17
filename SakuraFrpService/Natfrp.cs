using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using fastJSON;

namespace SakuraFrpService
{
    public static class Natfrp
    {
        public static bool BypassProxy = false;

        public static string Endpoint = "https://api.natfrp.com/launcher/";
        public static string UserAgent = "SakuraFrpService/" + Assembly.GetExecutingAssembly().GetName().Version;

        public static async Task<dynamic> Request(string action, string query = null)
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var request = WebRequest.CreateHttp(string.Format("{0}{1}?token={2}{3}{4}", Endpoint, action, "/* TODO: Token */", query == null ? null : "&", query));
                request.Method = "GET";
                request.Timeout = 10 * 1000;
                request.UserAgent = UserAgent;
                request.AllowAutoRedirect = true;

                if (BypassProxy)
                {
                    request.Proxy = null;
                }

                using (var response = await request.GetResponseAsync() as HttpWebResponse)
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                    {
                        throw new NatfrpException("API 状态异常: " + response.StatusCode + " " + response.StatusDescription);
                    }
                    using (var reader = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                    {
                        var json = JSON.ToObject<Dictionary<string, dynamic>>(await reader.ReadToEndAsync());
                        if (!json.ContainsKey("success"))
                        {
                            throw new NatfrpException("API 返回数据异常");
                        }
                        else if (!json["success"])
                        {
                            throw new NatfrpException(json.ContainsKey("message") ? json["message"] : "API 请求失败, 未知错误");
                        }
                        return json;
                    }
                }
            }
            catch (Exception e) when (!(e is NatfrpException))
            {
                throw new NatfrpException("出现未知错误", e);
            }
        }
    }

    public class NatfrpException : Exception
    {
        public NatfrpException(string message) : base(message) { }

        public NatfrpException(string message, Exception inner) : base(message, inner) { }
    }
}
