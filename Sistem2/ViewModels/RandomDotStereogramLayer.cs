using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sistem2.ViewModels
{
	public class RandomDotStereogramLayer : StereogramLayer
	{
		private bool _coloredNoise;

		public bool ColoredNoise
		{
			get => _coloredNoise;
			set
			{
				_coloredNoise = value;
				OnPropertyChanged(nameof(ColoredNoise));
			}
		}

		public RandomDotStereogramLayer(Image<Rgba32> target) : base(target)
		{
		}

		public override void Draw()
		{
			if (DepthImage == null)
				return;
			
			var location = new Point(0, 0);

			if (DrawDepthImage)
			{
				Target.Mutate(t => t.DrawImage(DepthImage, location, Opacity));
				return;
			}

			if (CachedImage != null)
			{
				Target.Mutate(t => t.DrawImage(CachedImage, location, Opacity));
				return;
			}

			var stereogram = CreateStereogram();
			stereogram.Oversampling = Oversampling;
			stereogram.ColoredNoise = ColoredNoise;

			if (stereogram.Generate() && stereogram.Result != null)
			{
				CachedImage = stereogram.Result; 
				Target.Mutate(t => t.DrawImage(stereogram.Result, location, Opacity));
			}
		}
	}
}