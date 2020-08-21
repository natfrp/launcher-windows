using System;
using System.Windows;
using System.Threading;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Collections.Generic;
using System.Text.RegularExpressions;

using SakuraLibrary.Proto;

using SakuraLauncher.Model;

// 这一坨先不重构了, 太麻烦
namespace SakuraLauncher.View
{
    /// <summary>
    /// LogTab.xaml 的交互逻辑
    /// </summary>
    public partial class LogTab : UserControl
    {
        public static readonly Regex LogPattern = new Regex(@"(?<Time>\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) (?<Level>\[[DIWE]\]) \[(?<Source>[a-zA-Z0-9\-_\.]+:\d+)\] (?<Content>.+)", RegexOptions.Compiled | RegexOptions.Singleline);
        public static readonly SolidColorBrush BrushInfo = new SolidColorBrush(Colors.White),
             BrushWarning = new SolidColorBrush(Colors.Orange),
             BrushError = new SolidColorBrush(Color.FromRgb(220, 80, 54)),
             BrushTime = new SolidColorBrush(Color.FromRgb(80, 141, 220)),
             BrushText = new SolidColorBrush(Colors.Silver),
             BrushTunnel = new SolidColorBrush(Colors.Wheat);

        private readonly LauncherModel Model;

        public Dictionary<string, string> failedData = new Dictionary<string, string>();

        public LogTab(LauncherModel main)
        {
            InitializeComponent();
            DataContext = Model = main;
        }

        public void Log(Log l, bool init = false)
        {
            if (!Dispatcher.CheckAccess())
            {
                Dispatcher.Invoke(() => Log(l));
                return;
            }
            bool bottom = ScrollViewerLog.ScrollableHeight - ScrollViewerLog.VerticalOffset < 1;
            if (TextBlockLog.Inlines.Count != 0)
            {
                TextBlockLog.Inlines.Add(new LineBreak());
            }
            AddRun(l.Source + " ", BrushTunnel);

            if (l.Category == 0) // CATEGORY_FRPC
            {
                var match = LogPattern.Match(l.Data);
                if (!match.Success)
                {
                    if (!init)
                    {
                        if (!failedData.ContainsKey(l.Source))
                        {
                            failedData[l.Source] = "";
                        }
                        failedData[l.Source] += l.Data + "\n";
                    }
                    AddRun(l.Data, BrushText);
                    AddRun("", BrushText); // Dirty Patch
                    AddRun("", BrushText);
                }
                else
                {
                    if (failedData.ContainsKey(l.Source))
                    {
                        if (Model.View.IsVisible && !Model.SuppressInfo)
                        {
                            string failedData_ = failedData[l.Source];
                            ThreadPool.QueueUserWorkItem(s => App.ShowMessage(failedData_, "隧道日志", MessageBoxImage.Information));
                        }
                        failedData.Remove(l.Source);
                    }
                    AddRun(match.Groups["Time"].Value + " ", BrushTime);
                    var levelColor = BrushInfo;
                    switch (match.Groups["Level"].Value)
                    {
                    case "W":
                        levelColor = BrushWarning;
                        break;
                    case "E":
                        levelColor = BrushError;
                        break;
                    }
                    AddRun(match.Groups["Level"].Value + ":" + match.Groups["Source"].Value + " ", levelColor);
                    AddRun(match.Groups["Content"].Value, BrushText);
                }
            }
            else
            {
                AddRun(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), BrushTime);
                switch (l.Category)
                {
                case 1:
                default:
                    AddRun(" INFO ", BrushInfo);
                    break;
                case 2:
                    AddRun(" WARNING ", BrushWarning);
                    break;
                case 3:
                    AddRun(" ERROR ", BrushError);
                    break;
                }
                AddRun(l.Data, BrushText);
            }

            while (TextBlockLog.Inlines.Count > 4 * 300 - 1)
            {
                TextBlockLog.Inlines.Remove(TextBlockLog.Inlines.FirstInline);
            }
            if (bottom)
            {
                ScrollViewerLog.ScrollToBottom();
            }
        }

        public void AddRun(string text, Brush color) => TextBlockLog.Inlines.Add(new Run(text)
        {
            Foreground = color
        });

        private void ButtonClear_Click(object sender, RoutedEventArgs e)
        {
            Model.Pipe.Request(new RequestBase()
            {
                Type = MessageID.LogClear
            });
            TextBlockLog.Inlines.Clear();
        }
    }
}
