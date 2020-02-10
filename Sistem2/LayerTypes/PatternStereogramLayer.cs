using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace Sistem2.LayerTypes
{
	public class PatternStereogramLayer : StereogramLayer
	{
		private Image<Rgba32> _patternImage;
		private string _patternImageFileName;

		/// <summary>
		/// The Pattern image
		/// </summary>
		public Image<Rgba32> PatternImage
		{
			get => _patternImage;
			set
			{
				_patternImage = value;

				MinimumSeparation = value.Width / 8f;
				MaximumSeparation = value.Width / 7f;

				DrawPreview();

				OnPropertyChanged(nameof(PatternImage));
				OnPropertyChanged(nameof(PatternImageSource));
			}
		}

		/// <summary>
		/// The Filename for the Pattern image
		/// </summary>
		public string PatternImageFileName
		{
			get => _patternImageFileName;
			set
			{
				_patternImageFileName = value;
				OnPropertyChanged(nameof(PatternImageFileName));
			}
		}

		/// <summary>
		/// The Pattern Image source
		/// </summary>
		public ImageSharpImageSource<Rgb24> PatternImageSource
		{
			get
			{
				if (PatternImage != null)
					return new ImageSharpImageSource<Rgb24>(PatternImage.CloneAs<Rgb24>());

				return null;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target"></param>
		public PatternStereogramLayer(Image<Rgba32> target) : base(target)
		{
		}

		/// <summary>
		/// Draw the stereogram
		/// </summary>
		public override void Draw()
		{
			if (DepthImage == null || PatternImage == null)
				return;

			var stereogram = CreateStereogram();

			var factor = stereogram.PatternWidth / (float) PatternImage.Width;
			stereogram.Pattern = PatternImage.Clone(image => image.Resize(stereogram.PatternWidth, (int)(PatternImage.Height * factor), new RobidouxSharpResampler()));
			
			if (stereogram.Generate())
			{
				var location = new Point(0, 0);

				if (stereogram.Result != null)
					Target.Mutate(t => t.DrawImage(stereogram.Result, location, 1));
			}
		}
	}
}