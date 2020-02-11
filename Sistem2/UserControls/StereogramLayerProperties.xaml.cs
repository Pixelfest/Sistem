using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Sistem2.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class StereogramLayerProperties : UserControl
	{
		private StereogramLayer _stereogramLayer => DataContext as StereogramLayer;

		public StereogramLayerProperties()
		{
			InitializeComponent();
		}

		private void LoadDepthImageButtonClick(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = "Open image",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					var image = SixLabors.ImageSharp.Image.Load<Rgb48>(openFileDialog.FileName);

					_stereogramLayer.DepthImage = image;
					_stereogramLayer.DepthImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}
	}
}
