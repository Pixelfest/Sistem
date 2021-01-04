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

        private void ResetActualSize(object sender, RoutedEventArgs e)
        {
            ZoomBorder.SetActualSize((int)Layers.Document.Dpi, 109, (int)Layers.Document.WidthInch);
        }
    }
}
