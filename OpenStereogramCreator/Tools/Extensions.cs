using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using Color = SixLabors.ImageSharp.Color;

namespace OpenStereogramCreator.Tools
{
	public static class Extensions
	{
		public static void Clear(this Image<Rgba32> source, Color color)
		{
            Parallel.For(0, source.Height, (i) => { source.GetPixelRowSpan(i).Fill(color); });
        }

		public static void Replace(this Image<Rgba32> source, Image<Rgba32> replace)
		{
			if (source.Width != replace.Width || source.Height != replace.Height)
				return;
			
			int width = source.Width;

			Parallel.For(0, source.Height, (i) => { for(int j = 0; j < width; j++) { source[j, (int)i] = replace[j, (int)i]; } });
		}
    }
}
