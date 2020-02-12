using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sistem2.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for ImageLayerProperties.xaml
	/// </summary>
	public partial class ImageLayerProperties : UserControl
	{
		/// <summary>
		/// Constructor
		/// </summary>
		public ImageLayerProperties()
		{
			InitializeComponent();
		}


		/// <summary>
		/// Handle the event to load an image
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void LoadImageButtonClick(object sender, RoutedEventArgs e)
		{
			if (!(DataContext is ImageLayer imageLayer))
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
					imageLayer.Image = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);
					imageLayer.FileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}
	}
}
