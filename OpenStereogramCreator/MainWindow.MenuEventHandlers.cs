using System.IO;
using System.Windows;
using Microsoft.Win32;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator
{
	public partial class MainWindow
	{
		private void MenuClickNew(object sender, RoutedEventArgs e)
		{
			Layers.Reset();
			Image = new Image<Rgba32>(1920, 1080);
		}

		private void MenuClickOpen(object sender, RoutedEventArgs e)
		{
				MessageBox.Show(Text.FunctionalityNotAvailable);

				return;

			var openFileDialog = new OpenFileDialog
			{
				Title = Text.OpenFile,
				Filter = Text.FilterProjectFile
			};

			if (openFileDialog.ShowDialog() == true)
			{
			}
		}

		private void MenuClickSave(object sender, RoutedEventArgs e)
		{
			MessageBox.Show(Text.FunctionalityNotAvailable);

			return;

			var saveFileDialog = new SaveFileDialog
			{
				Title = Text.SaveFile,
				Filter = Text.FilterProjectFile
			};

			if (saveFileDialog.ShowDialog() == true)
			{
                
			}
		}


		private void MenuClickExportToPng(object sender, RoutedEventArgs e)
		{
			var saveFileDialog = new SaveFileDialog
			{
				Title = Text.ExportPng,
				Filter = "Image File|*.png"
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				try
				{
					using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite))
					{
						Image.SaveAsPng(stream);
					}
				}
				catch
				{
					MessageBox.Show(Text.ErrorSavingImage);
				}
			}
		}

		private void MenuClickExit(object sender, RoutedEventArgs e)
		{
			throw new System.NotImplementedException();
		}
	}
}
