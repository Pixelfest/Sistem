using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json;
using OpenStereogramCreator.Dtos;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using static OpenStereogramCreator.ViewModels.LayerBase;

namespace OpenStereogramCreator.ViewModels
{
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		public DocumentLayer Document { get; set; }

		public LayersViewModel()
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
					image.Mutate(t => t.DrawImage(layer.CachedImage, layer.Location, layer.BlendingMode, layer.Opacity));
			}
		}

		public void Reset()
		{
			this.Clear();

			InitializeDocument();
		}

		private void InitializeDocument()
		{
			Document ??= new DocumentLayer();

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

			foreach (var layerBase in Items.Reverse())
			{
				switch (layerBase)
				{
					case RandomDotStereogramLayer l:
						result.Layers.Add($"{nameof(RandomDotStereogramLayerDto)}|{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(l.Export<RandomDotStereogramLayerDto>()))}");
						break;
					case FullImageStereogramLayer l:
						result.Layers.Add($"{nameof(FullImageStereogramLayerDto)}|{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(l.Export<FullImageStereogramLayerDto>()))}");
						break;
					case PatternStereogramLayer l:
						result.Layers.Add($"{nameof(PatternStereogramLayerDto)}|{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(l.Export<PatternStereogramLayerDto>()))}");
						break;
					case RepeaterLayer l:
						result.Layers.Add($"{nameof(RepeaterLayerDto)}|{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(l.Export<RepeaterLayerDto>()))}");
						break;
					case ReversePatternLayer l:
						result.Layers.Add($"{nameof(ReversePatternLayerDto)}|{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(l.Export<ReversePatternLayerDto>()))}");
						break;
					case ImageLayer l:
						result.Layers.Add($"{nameof(ImageLayerDto)}|{Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(l.Export<ImageLayerDto>()))}");
						break;
				}
			}

			return result;
		}

		public void Import(LayersDto layersDto)
		{
			Reset();

			this.Document.Import(layersDto.Document);
		
			foreach (var dto in layersDto.Layers)
			{
				var data = dto.Split("|", StringSplitOptions.RemoveEmptyEntries);

				switch (data[0])
				{
					case nameof(RandomDotStereogramLayerDto):
						Insert(0, new RandomDotStereogramLayer());
						(this[0] as RandomDotStereogramLayer).Import(Read<RandomDotStereogramLayerDto>(data[1]));
						break;
					case nameof(FullImageStereogramLayerDto):
						Insert(0, new FullImageStereogramLayer());
						(this[0] as FullImageStereogramLayer).Import(Read<FullImageStereogramLayerDto>(data[1]));
						break;
					case nameof(ImageLayerDto):
						Insert(0, new ImageLayer());
						(this[0] as ImageLayer).Import(Read<ImageLayerDto>(data[1]));
						break;
					case nameof(PatternStereogramLayerDto):
						Insert(0, new PatternStereogramLayer());
						(this[0] as PatternStereogramLayer).Import(Read<PatternStereogramLayerDto>(data[1]));
						break;
					case nameof(RepeaterLayerDto):
						Insert(0, new RepeaterLayer());
						(this[0] as RepeaterLayer).Import(Read<RepeaterLayerDto>(data[1]));
						break;
					case nameof(ReversePatternLayerDto):
						Insert(0, new ReversePatternLayer());
						(this[0] as ReversePatternLayer).Import(Read<ReversePatternLayerDto>(data[1]));
						break;
				}
			}

			this.Document.OnPropertyChanged(ImportName);

			foreach (var layer in Items)
			{
				layer.OnPropertyChanged(ImportName);
			}

		}

		private static TDto Read<TDto>(string importString)
			where TDto : new()
		{
			var import = Convert.FromBase64String(importString);
			var reader = new Utf8JsonReader(import);
			var deserializedDto = JsonSerializer.Deserialize<TDto>(ref reader);

			return deserializedDto;
		}
	}
}