using Sistem2.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace Sistem2.ViewModels
{
	/// <summary>
	/// Pattern stereogram
	/// </summary>
	public class PatternStereogramLayer : StereogramLayer
	{
		private Image<Rgba32> _patternImage;
		private string _patternImageFileName;
		private int _patternStart;
		private int _patternEnd;

		/// <summary>
		/// Gets ot sets the start location for taking the pattern from the pattern image
		///
		/// Usually this is 0, unless you only want to use part of the pattern
		/// </summary>
		public int PatternStart
		{
			get => _patternStart;
			set
			{
				if (value >= 0)
				{
					_patternStart = value;
				}
				else
				{
					_patternStart = 0;
				}

				OnPropertyChanged(nameof(PatternStart));
			}
		}

		/// <summary>
		/// Gets ot sets the end location for taking the pattern from the pattern image
		///
		/// Usually this is PatternImage.Width, unless you only want to use part of the pattern
		/// </summary>
		public int PatternEnd
		{
			get => _patternEnd;
			set
			{
				if (value <= PatternImage?.Width)
				{
					_patternEnd = value;
				}
				else
				{
					_patternEnd = PatternImage?.Width ?? 0;
				}

				OnPropertyChanged(nameof(PatternEnd));
			}
		}

		/// <summary>
		/// The Pattern image
		/// </summary>
		public Image<Rgba32> PatternImage
		{
			get => _patternImage;
			set
			{
				_patternImage = value;

				// Reset these to use the complete pattern
				PatternStart = 0;
				PatternEnd = _patternImage.Width;

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

			stereogram.Pattern = RenderPatternImage();
			stereogram.Oversampling = Oversampling;

			if (stereogram.Generate())
			{
				var location = new Point(0, 0);

				if (stereogram.Result != null)
					Target.Mutate(t => t.DrawImage(stereogram.Result, location, 1));
			}
		}

		/// <summary>
		/// Render the PatternImage to the desired crop and size
		/// </summary>
		/// <returns>The rendered pattern image</returns>
		private Image<Rgba32> RenderPatternImage()
		{
			Image<Rgba32> patternImage;
			int patternWidth;

			if (PatternStart != 0 || PatternEnd != PatternImage.Width)
			{
				// Take only part of the pattern
				patternWidth = PatternEnd - PatternStart;
				patternImage = PatternImage.Clone(context => context.Crop(new Rectangle(PatternStart, 0, patternWidth, PatternImage.Height)));
			}
			else
			{
				patternImage = PatternImage.Clone();
				patternWidth = PatternImage.Width;
			}

			var factor = MaximumSeparation / (float) patternWidth;

			if (!factor.EqualsWithTolerance(1))
				patternImage.Mutate(image => image.Resize((int)MaximumSeparation, (int) (PatternImage.Height * factor), new RobidouxSharpResampler()));

			return patternImage;
		}
	}
}