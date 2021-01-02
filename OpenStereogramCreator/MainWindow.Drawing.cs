using System.Threading;
using System.Windows;
using OpenStereogramCreator.Tools;

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

            //if (Layers.Any(layer => layer.CachedImage == null) || force)
            //{
	            _drawing = true;
	            var thread = new Thread(() =>
	            {
		            // Clear the image first
		            //DocumentLayer.Draw(Image);

                    //Layers.Draw(Image);

                    DocumentLayer.Render();
                    var result = DocumentLayer.CachedImage.Clone();

                    Layers.Draw(result);

                    Image.Replace(result);

                    //Image.Mutate(t => t.DrawImage(result, ));

                    //PreviewImage.Source = new ImageSharpImageSource<Rgba32>(Image);
                    PreviewImage.Dispatcher.Invoke(
			            new UpdateImageCallback(this.UpdateImage),
			            new object[] {Image});

		            _drawing = false;

		            if (_drawRequested)
		            {
			            _drawRequested = false;
			            Draw();
		            }
	            });

	            thread.Start();
            //}
            //else
            //{
	       //     _drawRequested = false;
           //}
        }
    }
}
