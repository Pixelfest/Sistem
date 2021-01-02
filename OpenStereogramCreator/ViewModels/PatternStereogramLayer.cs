using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

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

				PatternStart = 0;
				PatternEnd = _patternImage.Width;

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

		public override void Render()
		{
			if (DepthImage == null || PatternImage == null)
				return;

			if (CachedImage != null)
				return;

			if (DrawDepthImage)
			{
				CachedImage = DepthImage.CloneAs<Rgba32>();
				return;
			}

			var stereogram = CreateStereogram();

			stereogram.Pattern = RenderPatternImage();
			stereogram.Oversampling = Oversampling;

			if (stereogram.Generate() && stereogram.Result != null)
			{
				CachedImage = stereogram.Result;
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

			var factor = MaximumSeparation / patternWidth;

			factor *= Zoom;
			

			if (!factor.EqualsWithTolerance(1))
				patternImage.Mutate(image => image.Resize((int)MaximumSeparation, (int) (PatternImage.Height * factor), KnownResamplers.RobidouxSharp));

			return patternImage;
		}

		[NotifyPropertyChangedInvocator]
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(PatternStart):
				case nameof(PatternEnd):
				case nameof(PatternYShift):
				case nameof(PatternImage):
				case nameof(Zoom):
					CachedImage = null;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}
	}
}