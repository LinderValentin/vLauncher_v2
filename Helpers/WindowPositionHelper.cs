using System.Windows;

namespace vLauncher.Helpers
{
    public static class WindowPositionHelper
    {
        public static void CenterToOwner(Window child, Window owner, double offsetX = 0, double offsetY = 0)
        {
            child.Owner = owner;
            child.WindowStartupLocation = WindowStartupLocation.Manual;

            child.Loaded += (s, e) =>
            {
                child.Left = owner.Left + (owner.Width - child.ActualWidth) / 2 + offsetX;
                child.Top = owner.Top + (owner.Height - child.ActualHeight) / 2 + offsetY;
            };
        }
    }
}