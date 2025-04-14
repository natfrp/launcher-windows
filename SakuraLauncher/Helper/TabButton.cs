using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace SakuraLauncher.Helper
{
    public class TabButton : Button
    {
        public static readonly DependencyProperty IsSelectedProperty = DependencyProperty.Register("IsSelected", typeof(bool), typeof(TabButton), new FrameworkPropertyMetadata(false, OnIsSelectedChanged));

        public bool IsSelected
        {
            get { return (bool)GetValue(IsSelectedProperty); }
            set { SetValue(IsSelectedProperty, value); }
        }

        private static void OnIsSelectedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TabButton tb || e.NewValue is not bool sel) return;
            if (sel)
            {
                tb.StartSelectedAnimation();
            }
            else
            {
                tb.StartDeselectedAnimation();
            }
        }

        static TabButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TabButton), new FrameworkPropertyMetadata(typeof(TabButton)));
        }

        public TabButton()
        {
            MouseEnter += TabButton_MouseEnter;
            MouseLeave += TabButton_MouseLeave;
        }

        private void TabButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsSelected) Background = FindResource("TabGradientHover") as Brush;
        }

        private void TabButton_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            if (!IsSelected) Background = Brushes.Transparent;
        }

        public void RefreshTheme()
        {
            if (IsSelected) Background = (FindResource("TabGradientSelected") as LinearGradientBrush);
        }

        private void StartDeselectedAnimation()
        {
            var brush = (FindResource("TabGradientTransparent") as LinearGradientBrush).Clone();
            Background = brush;

            Duration duration = new(TimeSpan.FromSeconds(0.1));

            brush.BeginAnimation(LinearGradientBrush.EndPointProperty, new PointAnimation { Duration = duration, From = new Point(1, 0), To = new Point(0, 0) });

            BeginAnimation(OpacityProperty, new DoubleAnimation { Duration = duration, To = 0.76 });
        }

        private void StartSelectedAnimation()
        {
            var brush = (FindResource("TabGradientSelected") as LinearGradientBrush).Clone();
            Background = brush;

            Duration duration = new(TimeSpan.FromSeconds(0.25)), durationEnd = new(TimeSpan.FromSeconds(0.1));

            brush.GradientStops[0].BeginAnimation(GradientStop.OffsetProperty, new DoubleAnimation { Duration = duration, From = 0, To = 0.02 });
            brush.GradientStops[1].BeginAnimation(GradientStop.OffsetProperty, new DoubleAnimation { Duration = duration, From = 0, To = 0.02 });
            brush.GradientStops[2].BeginAnimation(GradientStop.OffsetProperty, new DoubleAnimation { Duration = duration, From = 0, To = 0.1 });
            brush.GradientStops[3].BeginAnimation(GradientStop.OffsetProperty, new DoubleAnimation { Duration = durationEnd, From = 0.5, To = 1 });
            brush.GradientStops[4].BeginAnimation(GradientStop.ColorProperty, new ColorAnimation { Duration = durationEnd, From = (Color)FindResource("TabGradient_EndTransparent"), To = (Color)FindResource("TabGradient_End") });

            BeginAnimation(OpacityProperty, new DoubleAnimation { Duration = duration, To = 1 });
        }
    }
}
