using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Sistem.Core.Generation;

/// <summary>
/// Creates a synthetic random-dot pattern for use when no user pattern is supplied
/// but oversampling is enabled (requiring the pattern-based algorithm).
/// </summary>
internal static class RandomDotPatternProvider
{
	/// <summary>
	/// Create a random-dot pattern image.
	/// </summary>
	/// <param name="width">Image width.</param>
	/// <param name="height">Image height.</param>
	/// <param name="noiseDensity">Density of dark pixels (1–99).</param>
	/// <param name="coloredNoise">True for colored noise, false for black/white.</param>
	public static Image<Rgba32> Create(int width, int height, int noiseDensity, bool coloredNoise)
	{
		var random = new Random();
		var result = new Image<Rgba32>(width, height);

		for (var x = 0; x < width; x++)
		{
			for (var y = 0; y < height; y++)
			{
				if (!coloredNoise)
				{
					result[x, y] = random.Next(100) > noiseDensity
						? Color.White
						: Color.Black;
				}
				else
				{
					result[x, y] = new Rgba32(
						(byte)random.Next(255),
						(byte)random.Next(255),
						(byte)random.Next(255));
				}
			}
		}

		return result;
	}
}
