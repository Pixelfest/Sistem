﻿using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public interface IHaveADepthImage
	{
		Image<Rgb48> DepthImage { get; set; }
		string DepthImageFileName { get; set; }
	}
}
