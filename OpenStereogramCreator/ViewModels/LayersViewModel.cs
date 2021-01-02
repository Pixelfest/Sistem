using System.Collections.ObjectModel;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator.ViewModels
{
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		public void Draw(Image<Rgba32> image)
		{
			foreach (var layer in this.Where(layer => layer.Visible && layer.Opacity > 0).Reverse())
			{
				if(layer.CachedImage == null)
					layer.Render();

				if(layer.CachedImage != null)
					image.Mutate(t => t.DrawImage(layer.CachedImage, new Point(0, 0), layer.Opacity));
			}
		}
	}
}