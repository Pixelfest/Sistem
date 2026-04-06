using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;

namespace Sistem.Core.Generation;

/// <summary>
/// Renders each line twice — once left-to-right and once right-to-left —
/// then averages the two results per pixel. Echoes from each direction
/// fall on opposite sides of depth transitions and cancel out when blended.
/// </summary>
internal sealed class BidirectionalAveragingAlgorithm : IStereogramAlgorithm
{
	/// <inheritdoc />
	public void ProcessLine(int y, OversamplingContext context)
	{
		var virtualWidth = context.VirtualWidth;
		var oversampling = context.Factor;
		var maxSep = context.VirtualMaxSeparation;
		var minSep = context.VirtualMinSeparation;
		var patternHeight = context.PatternHeight;
		var yShift = context.YShift;
		var noiseThreshold = context.NoiseReductionThreshold;
		var noiseRadius = context.VirtualNoiseReductionRadius;
		var postProcessing = context.PostProcessingOversampling;
		var depthMap = context.DepthMap;
		var pattern = context.PreparedPattern!;
		var resultImage = context.ResultImage;
		var width = context.Width;

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

		// Pass 1: left-to-right using lookLeft
		var colorsLR = new Rgba32[virtualWidth];
		var phasesLR = new double[virtualWidth];
		var phaseLR = 0.0;

		for (var x = 0; x < virtualWidth; x++)
		{
			if (lookLeft[x] == x)
			{
				if (x > 0)
					phaseLR += 1.0 / Math.Max(separations[x], 1);
				phasesLR[x] = phaseLR;

				colorsLR[x] = SamplePattern(x, y, phaseLR, maxSep, oversampling,
					yShift, patternHeight, pattern);
			}
			else if (lookLeft[x] == int.MinValue)
			{
				phaseLR += 1.0 / Math.Max(separations[x], 1);
				phasesLR[x] = phaseLR;
				colorsLR[x] = x > 0 ? colorsLR[x - 1] : default;
			}
			else
			{
				colorsLR[x] = colorsLR[lookLeft[x]];
				phaseLR = phasesLR[lookLeft[x]] + 1.0;
				phasesLR[x] = phaseLR;
			}
		}

		// Pass 2: right-to-left using lookRight
		var colorsRL = new Rgba32[virtualWidth];
		var phasesRL = new double[virtualWidth];
		var phaseRL = 0.0;

		for (var x = virtualWidth - 1; x >= 0; x--)
		{
			if (lookRight[x] == x)
			{
				phaseRL += 1.0 / Math.Max(separations[x], 1);
				phasesRL[x] = phaseRL;

				var fractionalPhase = phaseRL - Math.Floor(phaseRL);
				var locationX = (maxSep - (int)(fractionalPhase * maxSep) % maxSep) % maxSep / oversampling;

				var calculatedY = y;
				if (yShift > 0)
					calculatedY = (y + x / maxSep * yShift) + patternHeight;

				var locationY = (calculatedY + patternHeight) % patternHeight;
				if (locationY < 0)
					locationY += patternHeight;

				colorsRL[x] = pattern[locationX, locationY];
			}
			else if (lookRight[x] == int.MinValue)
			{
				phaseRL += 1.0 / Math.Max(separations[x], 1);
				phasesRL[x] = phaseRL;
				colorsRL[x] = x < virtualWidth - 1 ? colorsRL[x + 1] : default;
			}
			else
			{
				colorsRL[x] = colorsRL[lookRight[x]];
				phaseRL = phasesRL[lookRight[x]] + 1.0;
				phasesRL[x] = phaseRL;
			}
		}

		// Blend the two passes
		var colors = new Rgba32[virtualWidth];

		for (var x = 0; x < virtualWidth; x++)
		{
			var lr = colorsLR[x];
			var rl = colorsRL[x];

			colors[x] = new Rgba32(
				(byte)((lr.R + rl.R) / 2),
				(byte)((lr.G + rl.G) / 2),
				(byte)((lr.B + rl.B) / 2),
				(byte)((lr.A + rl.A) / 2));

			if (postProcessing)
				resultImage[x, y] = colors[x];
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
	/// Sample the pattern using phase-based tiling for the left-to-right pass.
	/// </summary>
	private static Rgba32 SamplePattern(
		int x, int y, double phase,
		int maxSep, int oversampling,
		int yShift, int patternHeight,
		Image<Rgba32> pattern)
	{
		var fractionalPhase = phase - Math.Floor(phase);
		var locationX = (int)(fractionalPhase * maxSep) / oversampling;

		var calculatedY = y;
		if (yShift > 0)
			calculatedY = (y + x / maxSep * yShift) + patternHeight;

		var locationY = (calculatedY + patternHeight) % patternHeight;
		if (locationY < 0)
			locationY += patternHeight;

		return pattern[locationX, locationY];
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
