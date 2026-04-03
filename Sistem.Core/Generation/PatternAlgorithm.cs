using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Sistem.Core.Generation;

/// <summary>
/// Pattern-based stereogram algorithm. Handles both user-supplied patterns
/// and synthetic random-dot patterns when oversampling > 1.
/// </summary>
internal sealed class PatternAlgorithm : IStereogramAlgorithm
{
	/// <inheritdoc />
	public void ProcessLine(int y, OversamplingContext context)
	{
		var virtualWidth = context.VirtualWidth;
		var oversampling = context.Factor;
		var maxSep = context.VirtualMaxSeparation;
		var minSep = context.VirtualMinSeparation;
		var startingPoint = context.VirtualStartingPoint;
		var patternHeight = context.PatternHeight;
		var yShift = context.YShift;
		var noiseThreshold = context.NoiseReductionThreshold;
		var noiseRadius = context.VirtualNoiseReductionRadius;
		var postProcessing = context.PostProcessingOversampling;
		var depthMap = context.DepthMap;
		var pattern = context.PreparedPattern!;
		var resultImage = context.ResultImage;
		var width = context.Width;

		var colors = new Rgba32[virtualWidth];
		var lookLeft = new int[virtualWidth];
		var lookRight = new int[virtualWidth];
		var setLeft = new int[virtualWidth];
		var setRight = new int[virtualWidth];
		var separations = new int[virtualWidth];

		for (var x = 0; x < virtualWidth; x++)
		{
			lookLeft[x] = x;
			lookRight[x] = x;
			setLeft[x] = 0;
			setRight[x] = 0;
		}

		var sep = 0;

		for (var x = 0; x < virtualWidth; x++)
		{
			FillLookArrays(y, x, lookLeft, setLeft, lookRight, setRight, ref sep,
				oversampling, depthMap, maxSep, minSep, virtualWidth);
			separations[x] = sep;
		}

		if (noiseThreshold > 0 && noiseRadius > 0)
			ApplyNoiseReduction(lookLeft, lookRight, virtualWidth, noiseThreshold, noiseRadius);

		var phases = new double[virtualWidth];

		// Right of starting point
		var phase = 0.0;
		for (var x = startingPoint; x < virtualWidth; x++)
		{
			if (lookLeft[x] == x || lookLeft[x] < startingPoint)
			{
				if (x > startingPoint)
					phase += 1.0 / Math.Max(separations[x], 1);
				phases[x] = phase;

				var calculatedY = y;

				if (yShift > 0)
					calculatedY = (y + (x - startingPoint) / maxSep * yShift) + patternHeight;

				var fractionalPhase = phase - Math.Floor(phase);
				var locationX = (int)(fractionalPhase * maxSep) / oversampling;
				var locationY = (calculatedY + patternHeight) % patternHeight;

				if (locationY < 0)
					locationY += patternHeight;

				colors[x] = pattern[locationX, locationY];
			}
			else if (lookLeft[x] == int.MinValue)
			{
				phase += 1.0 / Math.Max(separations[x], 1);
				phases[x] = phase;
				colors[x] = x > startingPoint ? colors[x - 1] : default;
			}
			else
			{
				colors[x] = colors[lookLeft[x]];
				phase = phases[lookLeft[x]] + 1.0;
				phases[x] = phase;
			}

			if (postProcessing)
				resultImage[x, y] = new Rgba32(colors[x].R, colors[x].G, colors[x].B, colors[x].A);
		}

		// Left of starting point
		phase = 0.0;
		for (var x = startingPoint - 1; x >= 0; x--)
		{
			if (lookRight[x] == x)
			{
				phase += 1.0 / Math.Max(separations[x], 1);
				phases[x] = phase;

				var calculatedY = y;

				if (yShift > 0)
					calculatedY = (y + (x - startingPoint) / maxSep * yShift) + patternHeight;

				var fractionalPhase = phase - Math.Floor(phase);
				var locationX = (maxSep - (int)(fractionalPhase * maxSep) % maxSep) % maxSep / oversampling;
				var locationY = (calculatedY + patternHeight) % patternHeight;

				if (locationY < 0)
					locationY += patternHeight;

				colors[x] = pattern[locationX, locationY];
			}
			else if (lookRight[x] == int.MinValue)
			{
				phase += 1.0 / Math.Max(separations[x], 1);
				phases[x] = phase;
				colors[x] = x < startingPoint - 1 ? colors[x + 1] : default;
			}
			else
			{
				colors[x] = colors[lookRight[x]];
				phase = phases[lookRight[x]] + 1.0;
				phases[x] = phase;
			}

			if (postProcessing)
				resultImage[x, y] = new Rgba32(colors[x].R, colors[x].G, colors[x].B, colors[x].A);
		}

		if (!postProcessing)
		{
			for (var x = 0; x < width; x++)
			{
				var red = 0;
				var green = 0;
				var blue = 0;
				var alpha = 0;

				for (var vx = 0; vx < oversampling; vx++)
				{
					var color = colors[(x * oversampling) + vx];
					red += color.R;
					green += color.G;
					blue += color.B;
					alpha += color.A;
				}

				resultImage[x, y] = new Rgba32(
					(byte)Math.Floor(red / (double)oversampling),
					(byte)Math.Floor(green / (double)oversampling),
					(byte)Math.Floor(blue / (double)oversampling),
					(byte)Math.Floor(alpha / (double)oversampling));
			}
		}
	}

	/// <summary>
	/// Fill lookleft and lookright arrays with the calculated values.
	/// </summary>
	private static void FillLookArrays(
		int y, int x,
		int[] lookLeft, int[] setLeft,
		int[] lookRight, int[] setRight,
		ref int separation,
		int oversampling, Image<Rgb48> depthMap,
		int maxSep, int minSep, int virtualWidth)
	{
		if (x % oversampling == 0)
		{
			var color = depthMap[x / oversampling, y];
			var relativeDepth = (color.R + color.G + color.B) / OversamplingContext.MaxCombinedPixelValue;
			separation = (int)Math.Floor(maxSep - relativeDepth * (maxSep - minSep));
		}

		var left = x - separation / 2;
		var right = left + separation;

		var visible = true;

		if (left >= 0 && right < virtualWidth)
		{
			if (lookLeft[right] != right)
			{
				if (lookLeft[right] < left)
				{
					lookRight[lookLeft[right]] = lookLeft[right];
					lookLeft[right] = right;
				}
				else
				{
					visible = false;
				}
			}

			if (lookRight[left] != left)
			{
				if (lookRight[left] > right)
				{
					lookLeft[lookRight[left]] = lookRight[left];
					lookRight[left] = left;
				}
				else
				{
					visible = false;
				}
			}

			if (visible)
			{
				lookLeft[right] = left;
				lookRight[left] = right;
				setLeft[right] = 1;
				setRight[left] = 1;
			}
		}
	}

	/// <summary>
	/// Apply manual noise reduction on the look-arrays.
	/// </summary>
	private static void ApplyNoiseReduction(
		int[] lookLeft, int[] lookRight,
		int virtualWidth, int threshold, int radius)
	{
		for (var x = 1; x < virtualWidth; x++)
		{
			if (Math.Abs(lookLeft[x] - lookLeft[x - 1]) > threshold)
			{
				for (var lookAhead = x + 1; lookAhead < x + radius; lookAhead++)
				{
					if (lookAhead >= lookLeft.Length)
						break;

					if (Math.Abs(lookLeft[lookAhead] - lookLeft[x - 1]) < threshold)
						break;
				}
			}

			var invertX = virtualWidth - x - 1;

			if (Math.Abs(lookRight[invertX] - lookRight[invertX + 1]) > threshold)
			{
				for (var lookAhead = invertX; lookAhead > invertX - radius; lookAhead--)
				{
					if (lookAhead < 0)
						break;

					if (Math.Abs(lookRight[lookAhead] - lookRight[invertX + 1]) < threshold)
						break;
				}
			}
		}
	}
}
