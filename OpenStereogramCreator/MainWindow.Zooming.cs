﻿using System;
using System.Collections.Generic;
using System.Text;
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
            ZoomBorder.SetActualSize((int)DocumentLayer.Dpi, 109, (int)DocumentLayer.WidthInch);
        }
    }
}