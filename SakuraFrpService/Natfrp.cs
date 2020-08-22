using System;
using System.IO;
using System.Net;
using System.Text;
using System.Reflection;
using System.Threading.Tasks;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace SakuraFrpService
{
    public static class Natfrp
    {
        public static bool BypassProxy = false;

        public static string Token = "";
        public static string Endpoint = "https://api.natfrp.com/launcher/";
        public static string UserAgent = "SakuraFrpService/" + Assembly.GetExecutingAssembly().GetName().Version;

        public static async Task<T> Request<T>(string action, string query = null) where T : ApiResponse
        {
            try
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                var request = WebRequest.CreateHttp(string.Format("{0}{1}?token={2}{3}{4}", Endpoint, action, Token, query == null ? null : "&", query));
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
                        var json = JsonConvert.DeserializeObject<T>(await reader.ReadToEndAsync());
                        if (!json.Success)
                        {
                            throw new NatfrpException(json.Message ?? "API 请求失败, 未知错误");
                        }
                        return json;
                    }
                }
            }
            catch (JsonException e)
            {
                throw new NatfrpException("API 返回数据异常", e);
            }
            catch (Exception e) when (!(e is NatfrpException))
            {
                throw new NatfrpException("出现未知错误", e);
            }
        }

        #region Api Data

        public class ApiUser
        {
            [JsonProperty("login")]
            public bool Login { get; set; }

            [JsonProperty("uid")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("meta")]
            public string Meta { get; set; }
        }

        public class ApiNode
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("accept_new")]
            public bool AcceptNew { get; set; }
        }

        public class ApiTunnel
        {
            [JsonProperty("id")]
            public int Id { get; set; }

            [JsonProperty("node")]
            public int Node { get; set; }

            [JsonProperty("name")]
            public string Name { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("note")]
            public string Note { get; set; }

            [JsonProperty("description")]
            public string Description { get; set; }
        }

        #endregion

        #region Api Response

        public class ApiResponse
        {
            public static string Path => null;

            [JsonProperty("success", Required = Required.Always)]
            public bool Success { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }
        }

        public class GetUser : ApiResponse
        {
            [JsonProperty("data")]
            public ApiUser Data { get; set; }
        }

        public class GetNodes : ApiResponse
        {
            [JsonProperty("data")]
            public List<ApiNode> Data { get; set; }
        }

        public class GetTunnels : ApiResponse
        {
            [JsonProperty("data")]
            public List<ApiTunnel> Data { get; set; }
        }

        public class CreateTunnel : ApiResponse
        {
            [JsonProperty("data")]
            public ApiTunnel Data { get; set; }
        }

        #endregion
    }

    public class NatfrpException : Exception
    {
        public NatfrpException(string message) : base(message) { }

        public NatfrpException(string message, Exception inner) : base(message, inner) { }

        public override string ToString() => Message + (InnerException == null ? "" : "\n" + InnerException.ToString());
    }
}
