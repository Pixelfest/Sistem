using System.Windows;
using Microsoft.Win32;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator
{
	public partial class PatternProperties 
	{
		private IHaveAPattern Pattern => DataContext as IHaveAPattern;

		public PatternProperties()
		{
			InitializeComponent();
		}

		private void LoadPatternImageButtonClick(object sender, RoutedEventArgs e)
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
					Pattern.PatternImage = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);
					Pattern.PatternImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
					MessageBox.Show(Text.ErrorLoadingImage);
				}
			}
		}

	}
}
