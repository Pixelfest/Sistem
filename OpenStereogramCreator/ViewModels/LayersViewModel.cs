using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text.Json;
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
				if (layer.CachedImage == null)
					layer.Render();

				if (layer.CachedImage != null)
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
			Document.Oversampling = 1;
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
			Reset();

			this.Document.Import2(layersDto.Document);

			//this.Document = DocumentLayer.Import(layersDto.Document);
			
			foreach (var dto in layersDto.Layers)
			{
				var data = dto.Split("|", StringSplitOptions.RemoveEmptyEntries);
				
				switch (data[0])
				{
					case nameof(RandomDotStereogramLayerDto):
						Insert(0, new RandomDotStereogramLayer());
						(this[0] as RandomDotStereogramLayer).Import2(Read<RandomDotStereogramLayerDto>(data[1]));
						break;
					case nameof(FullImageStereogramLayerDto):

				}
			}

			this.Document.OnPropertyChanged(LayerBase.ImportName);

			foreach (var layer in Items)
			{
				layer.OnPropertyChanged(LayerBase.ImportName);
				//layer.PropertyChanged
				//var handler = layer.PropertyChanged;
				//if (handler != null) handler(this, new PropertyChangedEventArgs("TimeStamp"));
			}

		}

		private TDto Read<TDto>(string importString)
			where TDto : new()
		{
			var import = Convert.FromBase64String(importString);
			var reader = new Utf8JsonReader(import);
			var deserializedDto = JsonSerializer.Deserialize<TDto>(ref reader);

			return deserializedDto;
		}
	}
}