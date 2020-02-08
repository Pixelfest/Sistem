using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sistem2.LayerTypes
{
	public class PatternStereogramLayer : StereogramLayer
	{
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
			if (DepthImage == null)
				return;

			var stereogram = CreateStereogram();

			if (stereogram.Generate())
			{
				var location = new Point(0, 0);

				if (stereogram.Result != null)
					Target.Mutate(t => t.DrawImage(stereogram.Result, location, 1));
			}
		}
	}
}