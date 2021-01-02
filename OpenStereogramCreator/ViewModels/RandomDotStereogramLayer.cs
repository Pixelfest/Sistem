using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public class RandomDotStereogramLayer : StereogramLayer
	{
		private bool _coloredNoise;
		private int _density;

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
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
	}
}