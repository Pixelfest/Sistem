using System.Collections.ObjectModel;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator.ViewModels
{
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		public DocumentLayer Document { get; set; }

		//public Dictionary<string, byte[]> Assets;

		public LayersViewModel() : base()
		{
			InitializeDocument();
			//Assets = new Dictionary<string, byte[]>();
		}

		public void Draw(Image<Rgba32> image)
		{
			foreach (var layer in this.Where(layer => layer.Visible && layer.Opacity > 0).Reverse())
			{
				if(layer.CachedImage == null)
					layer.Render();

				if(layer.CachedImage != null)
					image.Mutate(t => t.DrawImage(layer.CachedImage, layer.Location, layer.Opacity));
			}
		}

		public void Reset()
		{
			this.Clear();

			InitializeDocument();
		}

		private void InitializeDocument()
		{
			if (Document == null)
				Document = new DocumentLayer();

			Document.Name = "Document";
			Document.BackgroundColor = Color.Black;
			Document.Visible = true;
			Document.Dpi = 100;
			Document.Width = 0;
			Document.Height = 0;
			Document.Oversampling = 0;
		}
	}
}