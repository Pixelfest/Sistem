using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Sistem.Core.Generation;

/// <summary>
/// Fast random-dot stereogram algorithm.
/// Used when no pattern is supplied and oversampling is 1.
/// </summary>
internal sealed class RandomDotAlgorithm : IStereogramAlgorithm
{
	/// <inheritdoc />
	public void ProcessLine(int y, OversamplingContext context)
	{
		var random = Random.Shared;
		var width = context.Width;
		var depthMap = context.DepthMap;
		var resultImage = context.ResultImage;
		var maxSep = context.VirtualMaxSeparation;
		var minSep = context.VirtualMinSeparation;
		var noiseDensity = context.NoiseDensity;
		var coloredNoise = context.ColoredNoise;

		var lookLeft = new int[width];

		for (var x = 0; x < width; x++)
			lookLeft[x] = x;

		for (var x = 0; x < width; x++)
		{
			var color = depthMap[x, y];
			var relativeDepth = (color.R + color.G + color.B) / OversamplingContext.MaxCombinedPixelValue;
			var separation = maxSep - relativeDepth * (maxSep - minSep);

			var left = (int)(x - separation / 2);
			var right = (int)(left + separation);

			if (0 <= left && right < width)
			{
				var linkedLeft = lookLeft[left];

				while (linkedLeft != left && linkedLeft != right)
				{
					if (linkedLeft < right)
						left = linkedLeft;
					else
					{
						left = right;
						right = linkedLeft;
					}

					linkedLeft = lookLeft[left];
				}

				lookLeft[left] = right;
			}
		}

		for (var x = width - 1; x >= 0; x--)
		{
			if (lookLeft[x] == x)
			{
				if (!coloredNoise)
				{
					resultImage[x, y] = random.Next(100) > noiseDensity
						? Color.White
						: Color.Black;
				}
				else
				{
					resultImage[x, y] = new Rgba32(
						(byte)random.Next(255),
						(byte)random.Next(255),
						(byte)random.Next(255));
				}
			}
			else
			{
				resultImage[x, y] = resultImage[lookLeft[x], y];
			}
		}
	}
}
