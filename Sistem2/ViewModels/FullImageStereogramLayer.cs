using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;

namespace Sistem2.ViewModels
{
	public class FullImageStereogramLayer : PatternStereogramLayer
	{
		private int _shift;

		public int Shift
		{
			get => _shift;
			set
			{
				_shift = value;

				DrawPreview();

				OnPropertyChanged(nameof(Shift));
			}
		}

		public FullImageStereogramLayer(Image<Rgba32> target) : base(target)
		{

		}
		public override void Draw()
		{
			if (DepthImage == null || PatternImage == null || DepthImage.Width > PatternImage.Width)
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

			var start = 0;

			var result = new Image<Rgba32>(DepthImage.Width, DepthImage.Height);

			while (start < DepthImage.Width - MaximumSeparation)
			{
				var stereogram = CreateStereogram();

				stereogram.Pattern = RenderPatternImage(start);
				stereogram.Oversampling = Oversampling;
				stereogram.Origin = start - (int) Math.Floor(MaximumSeparation / 2f) + Shift;

				if (stereogram.Generate() && stereogram.Result != null)
				{
					CachedImage = stereogram.Result;
					result.Mutate(t => t.DrawImage(stereogram.Result, location, 1));
				}

				start += (int)MaximumSeparation;
			}

			CachedImage = result.Clone();
			Target.Mutate(context => context.DrawImage(CachedImage, location, opacity: Opacity));
		}

		private Image<Rgba32> RenderPatternImage(int start)
		{
			// Take only part of the pattern
			var patternWidth = (int)MaximumSeparation;

			var patternImage = PatternImage.Clone(context => context.Crop(new Rectangle(start, 0, patternWidth, PatternImage.Height)));

			return patternImage;
		}
	}
}