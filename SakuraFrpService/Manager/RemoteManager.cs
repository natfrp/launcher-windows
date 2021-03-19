using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Runtime.InteropServices;

using Newtonsoft.Json;

using SakuraLibrary;
using SakuraLibrary.Helper;

using SakuraFrpService.Data;
using SakuraFrpService.Sodium;
using SakuraFrpService.WebSocketShim;

namespace SakuraFrpService.Manager
{
    public class RemoteManager : IAsyncManager
    {
        public const string Tag = "Service/RemoteManager";

        public const int OVERHEAD = 3;
        public const string REMOTE_VERSION = "SAKURA_1";
        public static readonly byte[] SALT = new byte[]
        {
            0xc5, 0x23, 0xa3, 0xfe, 0x56, 0xee, 0x5b, 0x76, 0x77, 0x30, 0x99, 0x8d, 0x7c, 0xb6, 0x22, 0xc8
        };

        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        public bool Enabled = false;
        public byte[] EncryptKey = null;
        protected string Identifier = Environment.MachineName;

        protected WebSocket Socket = null;
        protected CancellationTokenSource Source = null;

        public RemoteManager(MainService main)
        {
            Main = main;
            AsyncManager = new AsyncManager(Run);

            try
            {
                var sodium = RuntimeInformation.ProcessArchitecture.ToString().ToLower() + "\\libsodium.dll";
                if (!File.Exists(sodium))
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "未找到架构匹配的 libsodium, 当前系统可能不支持远程管理");
                }
                else if (NTAPI.LoadLibraryEx(Path.GetFullPath(sodium), IntPtr.Zero, 0) == IntPtr.Zero)
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "libsodium 加载失败, 远程管理无法正常工作");
                }
            }
            catch (Exception e)
            {
                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "libsodium 加载失败, 远程管理无法正常工作: " + e.ToString());
            }
        }

        protected async Task Connect()
        {
            Socket = SystemClientWebSocket.ConnectAsync(new Uri("ws://remote.natfrp.com:2333"), Source.Token).Result;
            await Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Dictionary<string, object>
            {
                { "version", REMOTE_VERSION },
                { "type", "launcher" },
                { "token", Natfrp.Token },
                { "identifier", Identifier }
            }))), WebSocketMessageType.Text, true, Source.Token);

            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "远程管理已连接");

            var remote = new RemotePipeConnection();

            byte[] buffer = new byte[8192];
            while (!Source.IsCancellationRequested)
            {
                // Ensure message is complete
                int length = 0;
                WebSocketReceiveResult result = null;
                while (true)
                {
                    result = await Socket.ReceiveAsync(new ArraySegment<byte>(buffer, length, buffer.Length - length), Source.Token);
                    length += result.Count;
                    if (result.EndOfMessage)
                    {
                        break;
                    }
                    else if (length >= buffer.Length)
                    {
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "接收到过长消息, 已断开服务器连接, 将在稍后重连");
                        await Socket.CloseAsync(WebSocketCloseStatus.MessageTooBig, "消息过长", Source.Token);
                        return;
                    }
                }

                // Handle close message
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    switch (result.CloseStatus.Value)
                    {
                    case WebSocketCloseStatus.NormalClosure:
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "服务端正常断开 [" + result.CloseStatusDescription + "] 将在稍后重连");
                        break;
                    case WebSocketCloseStatus.PolicyViolation:
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "服务器拒绝请求, 已停止远程管理功能: " + result.CloseStatusDescription);
                        Stop();
                        return;
                    case WebSocketCloseStatus.InternalServerError:
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "服务器内部错误, " + result.CloseStatusDescription);
                        break;
                    default:
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "未知错误 [" + result.CloseStatus + "], " + result.CloseStatusDescription);
                        break;
                    }
                    await Socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, Source.Token);
                    return;
                }

                // Hmm, ensure something unexpected won't crash the socket
                if (length < OVERHEAD)
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "收到过短的消息");
                    return;
                }

                // Process payload
                using (var ms = new MemoryStream())
                {
                    ms.Write(buffer, 0, OVERHEAD);
                    switch (buffer[0])
                    {
                    case 0x01: // Heartbeat
                        if (length != OVERHEAD)
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "心跳包长度异常");
                            continue;
                        }
                        break;
                    case 0x02: // Remote Command
                        if (length < 24 + OVERHEAD)
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "收到过短的指令");
                            continue;
                        }

                        byte[] nonce = new byte[24], data = new byte[length - nonce.Length - OVERHEAD];

                        Buffer.BlockCopy(buffer, OVERHEAD, nonce, 0, nonce.Length);
                        Buffer.BlockCopy(buffer, nonce.Length + OVERHEAD, data, 0, data.Length);

                        try
                        {
                            data = SecretBox.Open(data, nonce, EncryptKey);
                        }
                        catch
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "指令解密失败, 原因可能为密钥错误, 如果您无故看到此错误请检查账户是否被盗");
                            break;
                        }
                        remote.Buffer = data;
                        Main.Pipe_DataReceived(remote, data.Length);

                        nonce = SecretBox.GenerateNonce();
                        ms.Write(nonce, 0, nonce.Length);

                        data = SecretBox.Create(remote.Buffer, nonce, EncryptKey);
                        ms.Write(data, 0, data.Length);
                        break;
                    default:
                        Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "收到未知消息");
                        continue;
                    }
                    await Socket.SendAsync(new ArraySegment<byte>(ms.ToArray()), WebSocketMessageType.Binary, true, Source.Token);
                }
            }
        }

        protected void Run()
        {
            do
            {
                try
                {
                    Connect().Wait();
                }
                catch (Exception e)
                {
                    if (e is AggregateException ae && ae.InnerExceptions.Count == 1)
                    {
                        e = ae.InnerExceptions[0];
                    }
                    if (e is TaskCanceledException || e is ThreadAbortException)
                    {
                        continue;
                    }
                    if (e is WebSocketException wse)
                    {
                        if (wse.InnerException is SocketException se)
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, Tag, "WebSocket 断开, 将在稍后重连: " + se.Message);
                        }
                        else
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "WebSocket 出现错误: " + wse.Message);
                        }
                        continue;
                    }
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, Tag, "未知错误, " + e.ToString());
                }
                finally
                {
                    Socket?.Dispose();
                    Socket = null;
                }
            }
            while (!AsyncManager.StopEvent.WaitOne(60 * 1000));
        }

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start()
        {
            if (!Enabled || Running || EncryptKey == null || EncryptKey.Length == 0)
            {
                return;
            }
            Source = new CancellationTokenSource();
            AsyncManager.Start();
        }

        public void Stop(bool kill = false)
        {
            if (!Running)
            {
                return;
            }
            AsyncManager.StopEvent.Set();
            try
            {
                Socket?.CloseAsync(WebSocketCloseStatus.NormalClosure, null, Source.Token).Wait(500);
            }
            catch { }
            Source.Cancel();
            AsyncManager.Stop(kill);
            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_INFO, Tag, "远程管理正常退出");
        }

        #endregion
    }
}
