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
using Sistem2.LayerTypes;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class SirdsLayerProperties : UserControl
	{
		public SirdsLayerProperties()
		{
			InitializeComponent();
		}

		private void LoadImageButtonClick(object sender, RoutedEventArgs e)
		{
			if (!(DataContext is SirdsLayer sirdsLayer))
				return;

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

					sirdsLayer.DepthImage = image;
					sirdsLayer.DepthImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}
	}
}
