using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Transforms;
using SixLabors.Primitives;

namespace OpenStereogramCreator.ViewModels
{
	public class PatternStereogramLayer : StereogramLayer, IHaveAPattern
	{
		private Image<Rgba32> _patternImage;
		private string _patternImageFileName;
		private int _patternStart;
		private int _patternEnd;

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

		public string PatternImageFileName
		{
			get => _patternImageFileName;
			set
			{
				_patternImageFileName = value;
				OnPropertyChanged(nameof(PatternImageFileName));
			}
		}

		public ImageSharpImageSource<Rgba32> PatternImageSource
		{
			get
			{
				if (PatternImage != null)
					return new ImageSharpImageSource<Rgba32>(PatternImage);

				return null;
			}
		}

		public float Zoom { get; set; } = 1;

		public int PatternYShift { get; set; } = 1;

		public PatternStereogramLayer(Image<Rgba32> target) : base(target)
		{

		}

		public override void Draw()
		{
			if (DepthImage == null || PatternImage == null)
				return;

			var location = new Point(0, 0);
			
			if (DrawDepthImage)
			{
				Target.Mutate(t => t.DrawImage(DepthImage, location, Opacity));
				return;
			}

			if (CachedImage != null)
			{
				Target.Mutate(t => t.DrawImage(CachedImage, location, 1));
				return;
			}

			var stereogram = CreateStereogram();

			stereogram.Pattern = RenderPatternImage();
			stereogram.Oversampling = Oversampling;

			if (stereogram.Generate() && stereogram.Result != null)
			{
				CachedImage = stereogram.Result;
				Target.Mutate(t => t.DrawImage(stereogram.Result, location, 1));
			}
		}

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
				patternImage = PatternImage.CloneAs<Rgba32>();
				patternWidth = PatternImage.Width;
			}

			var factor = MaximumSeparation / (float) patternWidth;

			factor *= Zoom;
			

			if (!factor.EqualsWithTolerance(1))
				patternImage.Mutate(image => image.Resize((int)MaximumSeparation, (int) (PatternImage.Height * factor), new RobidouxSharpResampler()));

			return patternImage;
		}
	}
}