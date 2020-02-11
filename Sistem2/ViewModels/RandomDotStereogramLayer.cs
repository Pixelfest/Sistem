using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sistem2.ViewModels
{
	public class RandomDotStereogramLayer : StereogramLayer
	{
		private bool _coloredNoise;

		/// <summary>
		/// The MinimumSeparation for the pattern
		/// </summary>
		public bool ColoredNoise
		{
			get => _coloredNoise;
			set
			{
				_coloredNoise = value;
				OnPropertyChanged(nameof(ColoredNoise));
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target"></param>
		public RandomDotStereogramLayer(Image<Rgba32> target) : base(target)
		{
		}

		/// <summary>
		/// Draw the stereogram
		/// </summary>
		/// <param name="useOversampling">Use oversampling</param>
		public override void Draw(bool useOversampling)
		{
			if (DepthImage == null)
				return;

			var stereogram = CreateStereogram();
			
			if (useOversampling)
				stereogram.Oversampling = 4;

			stereogram.ColoredNoise = ColoredNoise;

			if (stereogram.Generate())
			{
				var location = new Point(0, 0);

				if (stereogram.Result != null)
					Target.Mutate(t => t.DrawImage(stereogram.Result, location, 1));
			}
		}
	}
}