using System.Windows;
using Microsoft.Win32;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator
{
	public partial class ImageLayerProperties
	{
		public ImageLayerProperties()
		{
			InitializeComponent();
		}

		private void LoadImageButtonClick(object sender, RoutedEventArgs e)
		{
			if (!(DataContext is ImageLayer imageLayer))
				return;

			var openFileDialog = new OpenFileDialog
			{
				Title = Text.OpenImage,
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					imageLayer.Image = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);
				}
				catch
				{
					MessageBox.Show(Text.ErrorLoadingImage);
				}
			}
		}
	}
}
