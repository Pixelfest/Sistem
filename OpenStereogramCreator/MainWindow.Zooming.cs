using System.Windows;

namespace OpenStereogramCreator
{
    public partial class MainWindow
    {
        private void ResetZoom(object sender, RoutedEventArgs e)
        {
            ZoomBorder.SetFitToWindow();
        }

        private void SetPixelPerfect(object sender, RoutedEventArgs e)
        {
            ZoomBorder.SetPerPixel();
        }
    }
}
