using SakuraLauncher.Model;
using System;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

using TextDecorationsCol = System.Windows.TextDecorations;

namespace SakuraLauncher.Helper
{
    public class LogTextBlock : TextBlock
    {
        public static readonly DependencyProperty LogProperty = DependencyProperty.Register("Log", typeof(LogModel), typeof(LogTextBlock), new FrameworkPropertyMetadata(null, PropChanged));

        public LogModel Log
        {
            get => (LogModel)GetValue(LogProperty);
            set => SetValue(LogProperty, value);
        }

        public static readonly DependencyProperty HighlightConnProperty = DependencyProperty.Register("HighlightConn", typeof(bool), typeof(LogTextBlock), new FrameworkPropertyMetadata(true, PropChanged));

        public bool HighlightConn
        {
            get => (bool)GetValue(HighlightConnProperty);
            set => SetValue(HighlightConnProperty, value);
        }

        private static void PropChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is LogTextBlock textBlock) textBlock.RefreshDisplay();
        }

        public static Brush TimeBrush = new SolidColorBrush(Color.FromRgb(80, 141, 220));

        public static readonly Regex HighLightRegex = new(@">>(.*?)<<", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        static LogTextBlock()
        {
            FocusableProperty.OverrideMetadata(typeof(LogTextBlock), new FrameworkPropertyMetadata(true));
            TextEditorHelper.RegisterCommandHandlers(typeof(LogTextBlock), true, true, true);
            FocusVisualStyleProperty.OverrideMetadata(typeof(LogTextBlock), new FrameworkPropertyMetadata((object)null));
        }

        public LogTextBlock() => TextEditorHelper.MakeEditable(this);

        private void RefreshDisplay()
        {
            if (!string.IsNullOrEmpty(Text)) return;

            Inlines.Clear();

            var log = Log;
            if (log == null) return;

            addNormalInline(log.Time + " ", TimeBrush);
            addNormalInline(log.Level + " ", log.LevelColor);
            addNormalInline(log.Source + " ", Brushes.Wheat);

            if (!HighlightConn)
            {
                addNormalInline(log.Data, Brushes.Silver);
                return;
            }

            var lastIndex = 0;
            foreach (Match match in HighLightRegex.Matches(log.Data))
            {
                var startIndex = match.Index + 2;
                var length = match.Length - 4;

                if (startIndex > lastIndex)
                {
                    addNormalInline(log.Data.Substring(lastIndex, startIndex - lastIndex), Brushes.Silver);
                }
                Inlines.Add(new ClickCopyRun()
                {
                    Text = match.Groups[1].Value,
                    FontWeight = FontWeights.ExtraBold,
                    Foreground = Brushes.Khaki,
                    TextDecorations = TextDecorationsCol.Underline
                });

                lastIndex = startIndex + length;
            }
            if (lastIndex < log.Data.Length)
            {
                addNormalInline(log.Data.Substring(lastIndex), Brushes.Silver);
            }
        }

        private void addNormalInline(string text, Brush color)
        {
            Inlines.Add(new Run { Text = text, Foreground = color });
        }
    }

    public class ClickCopyRun : Run
    {
        public static readonly Brush HighlightBackground = new SolidColorBrush(Color.FromArgb(90, 255, 240, 150));

        private bool isHover = false;
        private Brush originalBackground = null;

        public ClickCopyRun() : base()
        {
            MouseEnter += OnMouseEnter;
            MouseLeave += OnMouseLeave;
            MouseLeftButtonDown += OnMouseLeftButtonDown;
            Loaded += (s, e) => TrySaveBackground();
        }

        private void TrySaveBackground()
        {
            if (!isHover && Background != HighlightBackground)
            {
                originalBackground = Background;
            }
        }

        private void OnMouseEnter(object sender, MouseEventArgs e)
        {
            isHover = true;
            TrySaveBackground();

            Background = HighlightBackground;
            Cursor = Cursors.Hand;
        }

        private void OnMouseLeave(object sender, MouseEventArgs e)
        {
            isHover = false;

            Background = originalBackground;
            Cursor = null;
        }

        private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (string.IsNullOrEmpty(Text)) return;
            try
            {
                Clipboard.SetText(Text);
                LauncherViewModel.Instance?.SnackMessageQueue.Enqueue("已复制 " + Text, null, null, null, false, false, TimeSpan.FromSeconds(1.5));
            }
            catch
            {
                LauncherViewModel.Instance?.SnackMessageQueue.Enqueue("复制失败, 请检查是否有杀毒软件拦截");
            }
        }
    }

    // Reference: https://stackoverflow.com/a/45627524
    public class TextEditorHelper
    {
        private static readonly Type TextEditorType = Type.GetType("System.Windows.Documents.TextEditor, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly MethodInfo RegisterMethod = TextEditorType.GetMethod("RegisterCommandHandlers", BindingFlags.Static | BindingFlags.NonPublic, null, new[] { typeof(Type), typeof(bool), typeof(bool), typeof(bool) }, null);

        private static readonly PropertyInfo TextViewProp = TextEditorType.GetProperty("TextView", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo IsReadOnlyProp = TextEditorType.GetProperty("IsReadOnly", BindingFlags.Instance | BindingFlags.NonPublic);

        private static readonly Type TextContainerType = Type.GetType("System.Windows.Documents.ITextContainer, PresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
        private static readonly PropertyInfo TextContainerTextViewProp = TextContainerType.GetProperty("TextView");

        private static readonly PropertyInfo TextContainerProp = typeof(TextBlock).GetProperty("TextContainer", BindingFlags.Instance | BindingFlags.NonPublic);

        public static void RegisterCommandHandlers(Type controlType, bool acceptsRichContent, bool readOnly, bool registerEventListeners) => RegisterMethod.Invoke(null, new object[] { controlType, acceptsRichContent, readOnly, registerEventListeners });

        public static void MakeEditable(TextBlock block)
        {
            var container = TextContainerProp.GetValue(block);
            var _editor = Activator.CreateInstance(TextEditorType, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.CreateInstance, null, new[] { container, block, false }, null);
            IsReadOnlyProp.SetValue(_editor, true);
            TextViewProp.SetValue(_editor, TextContainerTextViewProp.GetValue(container));
        }
    }
}
