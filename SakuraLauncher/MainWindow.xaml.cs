using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Windows.Interop;
using System.Windows.Controls;
using System.Collections.Generic;
using System.Windows.Media.Animation;
using System.Collections.ObjectModel;

using fastJSON;
using MaterialDesignThemes.Wpf;

using SakuraLauncher.View;
using SakuraLauncher.Data;
using SakuraLauncher.Helper;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace SakuraLauncher
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public const int CONFIG_VERSION = 2;

        public static ulong Tick = 0;
        public static MainWindow Instance = null;

        public SnackbarMessageQueue snackbarMessageQueue { get; set; } = new SnackbarMessageQueue();

        public string ConfigPath = null;

        public bool CanEditToken => !LoggingIn.Value && !LoggedIn.Value;

        public Prop<bool> AutoRun { get; set; } = new Prop<bool>();
        public Prop<bool> SuppressInfo { get; set; } = new Prop<bool>();
        public Prop<bool> LoggedIn { get; set; } = new Prop<bool>();
        public Prop<bool> LoggingIn { get; set; } = new Prop<bool>();
        public Prop<string> UserToken { get; set; } = new Prop<string>();

        public UserControl[] Tabs = null;
        public TabIndexTester CurrentTabTester { get; set; }
        public Prop<int> CurrentTab { get; set; } = new Prop<int>();

        public Tunnel CurrentListener
        {
            get => this._currentListener;
            set
            {
                this._currentListener = value;
                foreach(var l in Tunnels)
                {
                    l.Real?.RaisePropertyChanged("Selected");
                }
            }
        }
        private Tunnel _currentListener = null;

        public List<string> AutoStart = null;
        public ObservableCollection<ITunnel> Tunnels { get; set; } = new ObservableCollection<ITunnel>();
        public ObservableCollection<ServerData> Servers { get; set; } = new ObservableCollection<ServerData>();

        public int LogoIndex = 0;

        public MainWindow(bool autorun)
        {
            Instance = this;

            AutoRun.Value = autorun;
            CurrentTabTester = new TabIndexTester(this);

            Tabs = new UserControl[] {
                new TunnelTab(this),
                new LogTab(this),
                new SettingsTab(this),
                new AboutTab()
            };

            InitializeComponent();

            LoggedIn.PropertyChanged += (s, e) => RaisePropertyChanged("CanEditToken");
            LoggingIn.PropertyChanged += (s, e) => RaisePropertyChanged("CanEditToken");

            #region Load Config

            if(File.Exists("config.json"))
            {
                var json = JSON.ToObject<Dictionary<string, dynamic>>(File.ReadAllText("config.json"));
                if(json.ContainsKey("token"))
                {
                    UserToken.Value = json["token"];
                }
                if(json.ContainsKey("logo"))
                {
                    SetLogo((int)json["logo"]);
                }
                if(json.ContainsKey("LogoIndex") && json["suppressinfo"])
                {
                    SuppressInfo.Value = true;
                }
                if(json.ContainsKey("enable_tunnels") && json["enable_tunnels"] is List<object> enable_tunnels)
                {
                    AutoStart = enable_tunnels.Select(s => s.ToString()).ToList();
                }
                if(json.ContainsKey("loggedin") && json["loggedin"])
                {
                    TryLogin();
                }
            }
            ConfigPath = "config.json";

            #endregion

            DataContext = this;
            SuppressInfo.PropertyChanged += (s, e) => Save();

            SwitchTab(LoggedIn.Value || LoggingIn.Value ? 0 : 2);
        }

        public void Log(string tunnel, string raw) => (Tabs[1] as LogTab).Log(tunnel, raw);

        public void Save()
        {
            if(ConfigPath == null)
            {
                return;
            }
            File.WriteAllText(ConfigPath, JSON.ToNiceJSON(new Dictionary<string, object>()
            {
                { "version", CONFIG_VERSION },
                { "logo", LogoIndex },
                { "token", UserToken.Value.Trim() },
                { "suppressinfo", SuppressInfo.Value },
                { "loggedin", LoggedIn.Value },
                { "enable_tunnels", Tunnels.Where(t => t.IsReal && t.Real.Enabled).Select(t => t.Real.Name) }
            }));
        }

        public void TryLogin()
        {
            LoggingIn.Value = true;
            App.ApiRequest("getuserproxylist", "showall").ContinueWith(t =>
            {
                var tunnels = t.Result;
                if(tunnels == null)
                {
                    LoggingIn.Value = false;
                    return;
                }
                Dispatcher.Invoke(() => App.ApiRequest("showserverlist", "showall").ContinueWith(t2 =>
                {
                    LoggingIn.Value = false;
                    var servers = t2.Result;
                    if(servers == null)
                    {
                        return;
                    }
                    Dispatcher.Invoke(() =>
                    {
                        Servers.Clear();
                        foreach(Dictionary<string, object> j in servers["nodes"])
                        {
                            Servers.Add(new ServerData()
                            {
                                ID = j["id"] as string,
                                Name = j["name"] as string
                            });
                        }
                        if(AutoStart == null)
                        {
                            AutoStart = new List<string>();
                            foreach(var tunnel in Tunnels)
                            {
                                if(tunnel.IsReal)
                                {
                                    if(tunnel.Real.Enabled)
                                    {
                                        AutoStart.Add(tunnel.Real.Name);
                                    }
                                    tunnel.Real.Stop();
                                }
                            }
                        }
                        Tunnels.Clear();
                        foreach(Dictionary<string, object> j in tunnels["proxy"])
                        {
                            // 全 员 字 符 串
                            var tunnel = new Tunnel()
                            {
                                Name = j["proxyname"] as string,
                                Type = (j["proxytype"] as string).ToUpper(),
                                ServerID = j["serverid"] as string,
                                RemotePort = j["remoteport"] as string,
                                ServerName = j["servername"] as string,
                                LocalAddress = j["localaddr"] as string + ":" + j["localport"] as string
                            };
                            tunnel.PropertyChanged += (s, e) =>
                            {
                                if(e.PropertyName == "Enabled")
                                {
                                    Save();
                                }
                            };
                            Tunnels.Add(tunnel);
                        }
                        if(AutoStart != null)
                        {
                            foreach(var tunnel in Tunnels)
                            {
                                if(tunnel.IsReal && AutoStart.Contains(tunnel.Real.Name))
                                {
                                    tunnel.Real.Enabled = true;
                                }
                            }
                            AutoStart = null;
                        }
                        Tunnels.Add(new FakeTunnel());
                    });
                    LoggedIn.Value = true;
                    Save();
                }));
            });
        }

        public void SetLogo(int index)
        {
            switch(index)
            {
            case 1:
                logo.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/debian.png")));
                break;
            case 2:
                logo.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/fishcake.png")));
                break;
            default:
                logo.Background = new ImageBrush(new BitmapImage(new Uri("pack://application:,,,/Resources/logo.png")));
                break;
            }
            LogoIndex = index;
            Save();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            ConfigPath = null;
            foreach(var l in Tunnels)
            {
                if(l.IsReal)
                {
                    l.Real.Stop();
                }
            }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            App.ReleaseCapture();
            App.SendMessage(new WindowInteropHelper(this).Handle, 0xA1, (IntPtr)0x2, IntPtr.Zero);
        }

        private void ButtonHide_Click(object sender, RoutedEventArgs e) => Hide();

        private void ButtonClose_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("确定要退出程序吗?\n退出后隧道会被关闭.", "Confirm", MessageBoxButton.OKCancel, MessageBoxImage.Asterisk) == MessageBoxResult.OK)
            {
                Close();
            }
        }

        private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            switch(e.ClickCount)
            {
            case 3:
                SetLogo(1);
                break;
            case 5:
                SetLogo(2);
                break;
            }
        }

        private void TrayIcon_TrayLeftMouseUp(object sender, RoutedEventArgs e)
        {
            Show();
            Topmost = true; // Still using this in 2020?
            Topmost = false;
        }

        #region Tab Switching

        public void SwitchTab(int id)
        {
            CurrentTab.Value = id;
            tabContents.BeginStoryboard(Resources["TabHideAnimation"] as Storyboard);
        }

        private void ButtonTab_Click(object sender, RoutedEventArgs e)
        {
            int id = int.Parse((sender as Button).Tag as string);
            if(CurrentTab.Value != id)
            {
                SwitchTab(id);
                RaisePropertyChanged("CurrentTabTester");
            }
        }

        private void StoryboardTabHideAnimation_Completed(object sender, EventArgs e)
        {
            tabContents.Child = Tabs[CurrentTab.Value];
            tabContents.BeginStoryboard(Resources["TabShowAnimation"] as Storyboard);
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public void RaisePropertyChanged(string name) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        #endregion
    }
}
