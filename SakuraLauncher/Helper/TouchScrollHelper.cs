using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace SakuraLauncher.Helper
{
    public class TouchScrollHelper
    {
        public ScrollViewer view = null;
        public double CurrentPosition = 0;

        public TouchScrollHelper() { }

        public TouchScrollHelper(ScrollViewer view) => AttachTo(view, false);

        public void AttachTo(ScrollViewer view, bool autoDetach = true)
        {
            Detach();

            if (view == null)
            {
                return;
            }
            this.view = view;

            view.PreviewTouchDown += OnPreviewTouchDown;
            view.PreviewTouchMove += OnPreviewTouchMove;

            if (autoDetach)
            {
                view.Unloaded += View_Unloaded;
            }
        }

        public void Detach()
        {
            if (view != null)
            {
                view.PreviewTouchDown -= OnPreviewTouchDown;
                view.PreviewTouchMove -= OnPreviewTouchMove;
                view.Unloaded -= View_Unloaded;

                view = null;
            }
        }

        public void View_Unloaded(object sender, RoutedEventArgs e) => Detach();

        public void OnPreviewTouchDown(object sender, TouchEventArgs e)
        {
            if (sender is IInputElement s)
            {
                CurrentPosition = e.GetTouchPoint(s).Position.Y;
            }
        }

        public void OnPreviewTouchMove(object sender, TouchEventArgs e)
        {
            if (sender is ScrollViewer s)
            {
                var newPos = e.GetTouchPoint(s).Position.Y;
                s.ScrollToVerticalOffset(s.VerticalOffset - (int)(newPos - CurrentPosition));
                CurrentPosition = newPos;

                e.Handled = true;
            }
        }
    }
}
