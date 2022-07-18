using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = SixLabors.ImageSharp.Color;

namespace OpenStereogramCreator.Tools
{
	public static class Extensions
	{
		public static void Clear(this Image<Rgba32> source, Color color)
		{
			source.ProcessPixelRows(a =>
            {
                for (var y = 0; y < a.Height; y++)
                {
                    var span = a.GetRowSpan(y);
					span.Fill(color);
                }
				
            });
        }

		public static void Replace(this Image<Rgba32> source, Image<Rgba32> replace)
		{
			if (source.Width != replace.Width || source.Height != replace.Height)
				return;
			
			var width = source.Width;

			Parallel.For(0, source.Height, (i) => { for(var j = 0; j < width; j++) { source[j, (int)i] = replace[j, (int)i]; } });
		}
    }
}
