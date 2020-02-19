using Sistem2.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2.ViewModels
{
	public interface IHaveAPattern
	{
		Image<Rgba32> PatternImage { get; set; }
		string PatternImageFileName { get; set; }
		ImageSharpImageSource<Rgba32> PatternImageSource { get; }
	}

}
