using System;
using System.Text;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Collections.Generic;

using Sodium;
using Newtonsoft.Json;

using SakuraLibrary.Helper;

using SakuraFrpService.Data;

namespace SakuraFrpService.Manager
{
    public class RemoteManager : IAsyncManager
    {
        public static readonly byte[] SALT = new byte[]
        {
            0xc5, 0x23, 0xa3, 0xfe, 0x56, 0xee, 0x5b, 0x76, 0x77, 0x30, 0x99, 0x8d, 0x7c, 0xb6, 0x22, 0xc8
        };

        public readonly MainService Main;
        public readonly AsyncManager AsyncManager;

        protected byte[] EncryptKey = null;
        protected string Identifier = "";

        protected ClientWebSocket Socket = null;
        protected CancellationTokenSource Source = null;

        public RemoteManager(MainService main)
        {
            Main = main;
            AsyncManager = new AsyncManager(Run);
        }

        protected void Run()
        {
            do
            {
                try
                {
                    Socket = new ClientWebSocket();
                    Socket.ConnectAsync(new Uri("ws://remote.natfrp.com:2333"), Source.Token).Wait();
                    Socket.SendAsync(new ArraySegment<byte>(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new Dictionary<string, object>
                    {
                        { "token", Natfrp.Token },
                        { "identifier", Identifier }
                    }))), WebSocketMessageType.Text, true, Source.Token).Wait();

                    byte[] nonce = new byte[24], buffer = new byte[8192], data = null;
                    var remote = new RemotePipeConnection();
                    while (true)
                    {
                        var result = Socket.ReceiveAsync(new ArraySegment<byte>(buffer), Source.Token).WaitResult();
                        if (result.CloseStatus != null)
                        {
                            switch (result.CloseStatus.Value)
                            {
                            case WebSocketCloseStatus.PolicyViolation:
                                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, "Service", "RemoteManager: 服务器拒绝请求, 已禁用远程控制功能: " + result.CloseStatusDescription);
                                Stop();
                                return;
                            case WebSocketCloseStatus.InternalServerError:
                                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, "Service", "RemoteManager: 服务器内部错误, " + result.CloseStatusDescription);
                                break;
                            default:
                                Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, "Service", "RemoteManager: 未知错误 [" + result.CloseStatus + "], " + result.CloseStatusDescription);
                                break;
                            }
                            break;
                        }

                        if (result.Count < 24)
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, "Service", "RemoteManager: 数据长度异常");
                            continue;
                        }
                        data = new byte[result.Count - 24];
                        Buffer.BlockCopy(buffer, 0, nonce, 0, 24);
                        Buffer.BlockCopy(buffer, 24, data, 0, data.Length);

                        try
                        {
                            data = SecretBox.Open(data, nonce, EncryptKey);
                        }
                        catch
                        {
                            Main.LogManager.Log(LogManager.CATEGORY_SERVICE_WARNING, "Service", "RemoteManager: 数据解密失败, 原因可能为密钥错误");
                            Socket.SendAsync(new ArraySegment<byte>(new byte[0]), WebSocketMessageType.Binary, true, Source.Token).Wait();
                            continue;
                        }

                        remote.Buffer = data;
                        Main.Pipe_DataReceived(remote, data.Length);

                        nonce = SecretBox.GenerateNonce();
                        data = SecretBox.Create(remote.Buffer, nonce, EncryptKey);

                        var encrypted = new byte[data.Length + nonce.Length];
                        Buffer.BlockCopy(nonce, 0, encrypted, 0, 24);
                        Buffer.BlockCopy(data, 24, encrypted, 0, data.Length);

                        Socket.SendAsync(new ArraySegment<byte>(encrypted), WebSocketMessageType.Binary, true, Source.Token).Wait();
                    }
                }
                catch (Exception e) when (e is TaskCanceledException || e is ThreadAbortException)
                {
                    return;
                }
                catch (Exception e)
                {
                    Main.LogManager.Log(LogManager.CATEGORY_SERVICE_ERROR, "Service", "RemoteManager: 未知错误, " + e.ToString());
                }
            }
            while (!AsyncManager.StopEvent.WaitOne(60 * 1000));
        }

        #region IAsyncManager

        public bool Running => AsyncManager.Running;

        public void Start()
        {
            var settings = Properties.Settings.Default;
            if (!settings.EnableRemote)
            {
                return;
            }
            Identifier = settings.RemoteIdentifier;
            EncryptKey = PasswordHash.ArgonHashBinary(Encoding.UTF8.GetBytes(settings.RemoteKey), SALT, 3, 268435456, 32);

            Source = new CancellationTokenSource();
            AsyncManager.Start();
        }

        public void Stop(bool kill = false)
        {
            Source.Cancel();
            AsyncManager.Stop(kill);
        }

        #endregion
    }
}
