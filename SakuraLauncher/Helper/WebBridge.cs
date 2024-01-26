using Google.Protobuf;
using SakuraLauncher.Model;
using SakuraLibrary.Model;
using SakuraLibrary.Proto;
using System;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;

namespace SakuraLauncher.Helper
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class SakuraLauncherBridge(LauncherViewModel model)
    {
        public static readonly JsonFormatter JsonNoDefault = new(new JsonFormatter.Settings(false)),
            JsonDefault = new(new JsonFormatter.Settings(true));

        public string GetLauncherVersion() => model.LauncherVersion;

        public string GetServiceVersion() => model.ServiceVersion;

        public string GetFrpcVersion() => model.FrpcVersion;

        public string GetUser() => JsonNoDefault.Format(model.UserInfo);

        public async Task<string> GetNodes()
        {
            try
            {
                await model.RequestReloadNodesAsync(false);
            }
            catch { }

            var nl = new NodeList();
            foreach (var kv in model.Nodes)
            {
                nl.Nodes[kv.Key] = kv.Value;
            }
            return JsonDefault.Format(nl);
        }

        public string GetNotifications()
        {
            var nl = new NotificationList();
            nl.Notifications.AddRange(model.Notifications);
            return JsonDefault.Format(nl);
        }

        public bool GetAdvancedMode() => model.AdvancedMode;
    }

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    public class CreateTunnelBridge(CreateTunnelWindow2 view, Tunnel edit)
    {
        public string GetEditTunnel() => edit == null ? "" : SakuraLauncherBridge.JsonNoDefault.Format(edit);

        public async Task EditTunnel(string payload)
        {
            try
            {
                await view.Launcher.RequestCreateTunnelAsync(JsonParser.Default.Parse<Tunnel>(payload), TunnelUpdate.Types.Action.Edit);
                view.Close();
            }
            catch (Exception e)
            {
                view.Launcher.ShowError(e);
            }
        }

        public async Task CreateTunnel(string payload)
        {
            try
            {
                var update = await view.Launcher.RequestCreateTunnelAsync(JsonParser.Default.Parse<Tunnel>(payload));
                if (view.Launcher.ShowMessage(
                    $"成功创建隧道 #{update.Tunnel.Id} {update.Tunnel.Name}\n\n按 \"取消\" 继续创建, \"确定\" 关闭创建隧道窗口",
                    "创建成功",
                    LauncherModel.MessageMode.OkCancel | LauncherModel.MessageMode.Confirm
                ) == LauncherModel.MessageResult.Ok)
                {
                    view.Close();
                }
            }
            catch (Exception e)
            {
                view.Launcher.ShowError(e);
            }
        }

        public string GetListening() => (view.Model.Loading ? "1" : "0") + JsonSerializer.Serialize(view.Model.Listening);

        public void ReloadListening() => view.Model.ReloadListening();
    }
}
