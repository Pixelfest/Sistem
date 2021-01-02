using System.Windows;
using Microsoft.Win32;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator
{
	public partial class DepthMapProperties
	{
		private IHaveADepthImage DepthMap => DataContext as IHaveADepthImage;

		public DepthMapProperties()
		{
			InitializeComponent();
		}

		private void LoadDepthImageButtonClick(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = Text.OpenImage,
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					var image = SixLabors.ImageSharp.Image.Load<Rgb48>(openFileDialog.FileName);

					DepthMap.DepthImage = image;
					DepthMap.DepthImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
					MessageBox.Show(Text.ErrorLoadingImage);
				}
			}
		}

		private void InvertButtonClick(object sender, RoutedEventArgs e)
		{
			DepthMap.DepthImage = DepthMap.DepthImage.Clone(context => context.Invert());
		}
	}
}
