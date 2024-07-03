using System.Windows.Media;
using System.Text.RegularExpressions;

namespace SakuraLauncher.Model
{
    public class LogModel
    {
        public static readonly Regex Pattern = new(@"(?<Time>\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) \[(?<Level>[DIWET])\] (?:\[[a-zA-Z0-9\-_\.]+:\d+\] )?(?<Content>.+)", RegexOptions.Compiled | RegexOptions.Singleline);

        public static readonly SolidColorBrush BrushInfo = new(Colors.White),
            BrushWarning = new(Colors.Orange),
            BrushError = new(Color.FromRgb(220, 80, 54));

        public string Source { get; set; }
        public string Time { get; set; }
        public string Level { get; set; }
        public string Data { get; set; }

        public SolidColorBrush LevelColor { get; set; } = BrushInfo;

        public override string ToString() => string.Format("{0} {1} {2} {3}", Time, Level, Source, Data);
    }
}
