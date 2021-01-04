using System.Linq;
using System.Threading;
using System.Windows;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator
{
	public partial class MainWindow
	{
		// Drawing

		private void DrawClick(object sender, RoutedEventArgs e)
		{
			Draw();
		}

		private void Draw()
		{
			if (_drawing)
			{
				_drawRequested = true;
				return;
			}

			_drawing = true;

			var thread = new Thread(() =>
			{
				Layers.Document.Render();

				if (Layers.Document.CachedImage != null)
				{
					var result = Layers.Document.CachedImage.Clone();

					Layers.Draw(result);

					Image.Replace(result);

					PreviewImage.Dispatcher.Invoke(
						new UpdateImageCallback(this.UpdateImage),
						new object[] {Image});
				}

				_drawing = false;

				if (_drawRequested)
				{
					_drawRequested = false;
					Draw();
				}
			});

			thread.Start();
		}
	}
}
