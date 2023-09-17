using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem.Core
{
	public static class ImageProcessing
	{
		public static Image<Rgba32> GenerateShadows(Image<Rgb48> depthImage)
		{
			var range = 4;
			var threshold = 5000;

			var contourImage = new Image<Rgba32>(depthImage.Width, depthImage.Height);

			int SetPixelValue(int px, int py, Queue<int> queue)
			{
				var pixel = depthImage[px, py];
				var value = (pixel.R + pixel.G + pixel.B) / 3;

				// look back at values
				if (queue.Any())
				{
					var difference = value - queue.Average();
					var maxMultiplier = 8;
				
					for(var multiplier = maxMultiplier; multiplier > 0; multiplier--)
					{
						if (difference > threshold * multiplier)
						{
							var alpha = 1f / maxMultiplier * multiplier;
							contourImage[px, py] = new Rgba32(0, 0, 0, alpha);
							break;
						}
					}
				}

				return value;
			}

			Parallel.For(0, depthImage.Height, (y) =>
			{
				var tempStack = new Queue<int>();

				// Look back
				for (var x = 0; x < depthImage.Width; x++)
				{
					var value = SetPixelValue(x, y, tempStack);

					if (tempStack.Count >= range) tempStack.Dequeue();
					tempStack.Enqueue(value);
				}

				tempStack = new Queue<int>();

				// Look forward
				for (var x = depthImage.Width - 1; x >= 0; x--)
				{
					var value = SetPixelValue(x, y, tempStack);

					if (tempStack.Count >= range) tempStack.Dequeue();
					tempStack.Enqueue(value);
				}
			});

			return contourImage;
		}
	}
}