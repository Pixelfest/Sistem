using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenStereogramCreator.Dtos;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator.ViewModels
{
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		public DocumentLayer Document { get; set; }

		public LayersViewModel() : base()
		{
			InitializeDocument();
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
			Document.Width = 0;
			Document.Height = 0;
			Document.Oversampling = 0;
		}

        public LayersDto Export()
        {
            var result = new LayersDto();

            result.Document = Document.Export();
            result.Layers = new List<string>();
            
            foreach (var layerBase in Items)
            {
                switch (layerBase)
                {
					case RandomDotStereogramLayer l:
						result.Layers.Add($"{nameof(RandomDotStereogramLayerDto)}|{Convert.ToBase64String(l.Export())}");
                        break;
                }
            }

            return result;
        }

        public void Import(LayersDto layersDto)
        {
            this.Document = DocumentLayer.Import(layersDto.Document);

            foreach (var dto in layersDto.Layers)
            {
                var data = dto.Split("|", StringSplitOptions.RemoveEmptyEntries);

                switch (data[0])
                {
					case nameof(RandomDotStereogramLayerDto):
						Items.Add(RandomDotStereogramLayer.Import(Convert.FromBase64String(data[1])));
                        break;
                }
            }

            foreach (var layer in Items)
            {
				layer.PropertyChanged
            }

        }
	}
}