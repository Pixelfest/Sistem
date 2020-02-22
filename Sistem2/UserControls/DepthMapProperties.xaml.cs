using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sistem2.ViewModels;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2
{
	public partial class DepthMapProperties : UserControl
	{
		private IHaveADepthImage _dataContext => DataContext as IHaveADepthImage;

		public DepthMapProperties()
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

					_dataContext.DepthImage = image;
					_dataContext.DepthImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
					
					
				}
				catch
				{
				}
			}
		}

		private void InvertButtonClick(object sender, RoutedEventArgs e)
		{
			_dataContext.DepthImage = _dataContext.DepthImage.Clone(context => context.Invert());
		}
	}
}
