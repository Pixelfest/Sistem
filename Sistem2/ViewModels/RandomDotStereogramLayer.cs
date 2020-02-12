using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sistem2.ViewModels
{
	/// <summary>
	/// Random dot stereogram
	/// </summary>
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
		public override void Draw()
		{
			if (DepthImage == null)
				return;

			var stereogram = CreateStereogram();
			stereogram.Oversampling = Oversampling;
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