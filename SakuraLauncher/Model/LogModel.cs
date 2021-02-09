using System.Windows.Media;
using System.Text.RegularExpressions;

namespace SakuraLauncher.Model
{
    public class LogModel
    {
        public static readonly Regex Pattern = new Regex(@"(?<Time>\d{4}/\d{2}/\d{2} \d{2}:\d{2}:\d{2}) \[(?<Level>[DIWE])\] (?:\[[a-zA-Z0-9\-_\.]+:\d+\] )?(?<Content>.+)", RegexOptions.Compiled | RegexOptions.Singleline);

        public static readonly SolidColorBrush BrushInfo = new SolidColorBrush(Colors.White),
            BrushWarning = new SolidColorBrush(Colors.Orange),
            BrushError = new SolidColorBrush(Color.FromRgb(220, 80, 54));

        public string Source { get; set; }
        public string Time { get; set; }
        public string Level { get; set; }
        public string Data { get; set; }

        public SolidColorBrush LevelColor { get; set; } = BrushInfo;
    }
}
