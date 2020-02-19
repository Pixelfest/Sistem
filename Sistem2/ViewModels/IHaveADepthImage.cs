using Sistem2.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2.ViewModels
{
	public interface IHaveADepthImage
	{
		Image<Rgb48> DepthImage { get; set; }
		string DepthImageFileName { get; set; }
		ImageSharpImageSource<Rgb48> DepthImageSource { get; }
	}
}
