using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2.LayerTypes
{
	public class BackgroundLayer : LayerBase
	{
		public Color Color { get; set; }

		public BackgroundLayer(Image<Rgba32> target) : base(target)
		{
		}

		public override void Draw()
		{
			Target.Mutate(image => image.BackgroundColor(Color));
		}

	}
}
