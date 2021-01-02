using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public interface IHaveAPattern
	{
		Image<Rgba32> PatternImage { get; set; }
		string PatternImageFileName { get; set; }
		//ImageSharpImageSource<Rgba32> PatternImageSource { get; }
	}

}
