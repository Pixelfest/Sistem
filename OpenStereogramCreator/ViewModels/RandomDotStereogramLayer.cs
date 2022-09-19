using System;
using System.Runtime.CompilerServices;
using System.Text.Json;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
    [Serializable]
	public class RandomDotStereogramLayer : StereogramLayer
	{
		private bool _coloredNoise;
		private int _density = 50;

		public bool ColoredNoise
		{
			get => _coloredNoise;
			set
			{
				_coloredNoise = value;
				OnPropertyChanged(nameof(ColoredNoise));
			}
		}

		public int Density
		{
			get => _density;
			set
			{
				_density = value;
				OnPropertyChanged(nameof(Density));
			}
		}

		public override void Render()
		{
			if (DepthImage == null)
				return;
			
			if (CachedImage != null)
				return;

			if (DrawDepthImage)
			{
				CachedImage = DepthImage.CloneAs<Rgba32>();
				return;
			}

			var stereogram = CreateStereogram();
			stereogram.Oversampling = Oversampling;
			stereogram.ColoredNoise = ColoredNoise;
			stereogram.NoiseDensity = Density;

			if (stereogram.Generate() && stereogram.Result != null)
			{
				CachedImage = stereogram.Result; 
			}
		}

		[NotifyPropertyChangedInvocator]
		public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Density):
				case nameof(ColoredNoise):
					CachedImage = null;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

        public byte[] Export()
        {
            var export = base.Export<RandomDotStereogramLayerDto>();

            export.ColoredNoise = ColoredNoise;
            export.Density = Density;

            return JsonSerializer.SerializeToUtf8Bytes(export);
        }

		public new void Import2<TSource>(TSource source)
			where TSource : RandomDotStereogramLayerDto, new()
		{
			base.Import2(source);

			this.ColoredNoise = source.ColoredNoise;
			this.Density = source.Density;
		}

		public static RandomDotStereogramLayer Import(byte[] import)
        {
			var reader = new Utf8JsonReader(import);

            var dto =  JsonSerializer.Deserialize<RandomDotStereogramLayerDto>(ref reader);

            var result = Import<RandomDotStereogramLayerDto, RandomDotStereogramLayer>(dto);

            result.ColoredNoise = dto.ColoredNoise;
            result.Density = dto.Density;

            return result;
        }
	}
}