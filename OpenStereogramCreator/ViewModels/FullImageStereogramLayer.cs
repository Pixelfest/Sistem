using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;

namespace OpenStereogramCreator.ViewModels
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

		public override void Render()
		{
			if (DepthImage == null || PatternImage == null || DepthImage.Width > PatternImage.Width)
				return;

			var location = new Point(0, 0);
			

			if (CachedImage != null)
				return;

			if (DrawDepthImage)
			{
				CachedImage = DepthImage.CloneAs<Rgba32>();

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
					result.Mutate(t => t.DrawImage(stereogram.Result, location, Opacity));
				}

				start += (int)MaximumSeparation;
			}

			CachedImage = result.Clone();
		}

		private Image<Rgba32> RenderPatternImage(int start)
		{
			// Take only part of the pattern
			var patternWidth = (int)MaximumSeparation;

			var patternImage = PatternImage.Clone(context => context.Crop(new Rectangle(start, 0, patternWidth, PatternImage.Height)));

			return patternImage;
		}

		[NotifyPropertyChangedInvocator]
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Shift):
					CachedImage = null;
					break;
				default:
					break;
			}

			base.OnPropertyChanged(propertyName);
		}
	}
}