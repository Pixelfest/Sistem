using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using Sistem.Core.Generation;

namespace OpenStereogramCreator.ViewModels
{
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
				OnPropertyChanged();
			}
		}

		public int Density
		{
			get => _density;
			set
			{
				_density = value;
				OnPropertyChanged();
			}
		}

		public override void Render()
		{
			if (DepthImage == null)
				return;

			if (CachedImage != null)
				return;

			var options = CreateOptions() with
			{
				Oversampling = Oversampling,
				ColoredNoise = ColoredNoise,
				NoiseDensity = Density,
			};

			var generator = new StereogramGenerator();
			var result = generator.Generate(options);

			if (result.Success)
			{
				CachedImage = result.Image;
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

		public new T Export<T>() where T : RandomDotStereogramLayerDto, new()
		{
			var export = base.Export<T>();

			export.ColoredNoise = ColoredNoise;
			export.Density = Density;

			return export;
		}

		public new void Import<TSource>(TSource source)
			where TSource : RandomDotStereogramLayerDto, new()
		{
			this.ColoredNoise = source.ColoredNoise;
			this.Density = source.Density;
			base.Import(source);
		}
	}
}
