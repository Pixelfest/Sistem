using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator
{
	public partial class PatternProperties : UserControl
	{
		private IHaveAPattern _dataContext => DataContext as IHaveAPattern;

		public PatternProperties()
		{
			InitializeComponent();
		}

		private void LoadPatternImageButtonClick(object sender, RoutedEventArgs e)
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
					_dataContext.PatternImage = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);

					_dataContext.PatternImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}

	}
}
