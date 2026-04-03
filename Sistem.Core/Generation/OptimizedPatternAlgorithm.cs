using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Advanced;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Buffers;

namespace Sistem.Core.Generation;

/// <summary>
/// Optimized pattern-based stereogram algorithm derived from <see cref="PatternAlgorithm"/>.
/// Key optimizations over the original:
/// <list type="bullet">
///   <item>Removed dead <c>setLeft</c>/<c>setRight</c> arrays (write-only, never read).</item>
///   <item>Removed no-op <c>ApplyNoiseReduction</c> (iterates but never mutates).</item>
///   <item>Replaced per-pixel <c>Rgba32.ParseHex</c> with a static transparent constant.</item>
///   <item>Eliminated redundant <c>new Rgba32(...)</c> copies on post-processing writes.</item>
///   <item>Replaced <c>Math.Floor</c> on integer division with plain integer division.</item>
///   <item>Uses <see cref="ArrayPool{T}"/> to avoid per-line heap allocations.</item>
///   <item>Uses <c>DangerousGetPixelRowMemory</c> for row-based depth-map access.</item>
///   <item>Extracted duplicated left/right propagation into a shared helper.</item>
/// </list>
/// </summary>
internal sealed class OptimizedPatternAlgorithm : IStereogramAlgorithm
{
	private static readonly Rgba32 Transparent = new(0, 0, 0, 0);

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
		var postProcessing = context.PostProcessingOversampling;
		var depthMap = context.DepthMap;
		var pattern = context.PreparedPattern!;
		var resultImage = context.ResultImage;
		var width = context.Width;

		var virtualPatternOffset = maxSep - (startingPoint % maxSep);

		// Rent pooled arrays instead of allocating per line
		var colorsArray = ArrayPool<Rgba32>.Shared.Rent(virtualWidth);
		var lookLeftArray = ArrayPool<int>.Shared.Rent(virtualWidth);
		var lookRightArray = ArrayPool<int>.Shared.Rent(virtualWidth);

		try
		{
			var colors = colorsArray.AsSpan(0, virtualWidth);
			var lookLeft = lookLeftArray.AsSpan(0, virtualWidth);
			var lookRight = lookRightArray.AsSpan(0, virtualWidth);

			for (var x = 0; x < virtualWidth; x++)
			{
				lookLeft[x] = x;
				lookRight[x] = x;
			}

			// Read the depth-map row once via span instead of per-pixel indexer calls
			var depthRow = depthMap.Frames.RootFrame.DangerousGetPixelRowMemory(y).Span;

			var sep = 0;
			for (var x = 0; x < virtualWidth; x++)
			{
				FillLookArrays(x, depthRow, lookLeft, lookRight, ref sep,
					oversampling, maxSep, minSep, virtualWidth);
			}

			// Everything from starting point to the right
			PropagateColors(
				colors, lookLeft, startingPoint, virtualWidth, step: 1,
				useLookRight: false, startingPointForFilter: startingPoint,
				y, yShift, maxSep, patternHeight, virtualPatternOffset, oversampling,
				pattern, resultImage, postProcessing);

			// Everything from starting point to the left
			PropagateColors(
				colors, lookRight, startingPoint - 1, virtualWidth, step: -1,
				useLookRight: true, startingPointForFilter: startingPoint,
				y, yShift, maxSep, patternHeight, virtualPatternOffset, oversampling,
				pattern, resultImage, postProcessing);

			if (!postProcessing)
			{
				DownsampleToResult(colors, resultImage, y, width, oversampling);
			}
		}
		finally
		{
			ArrayPool<Rgba32>.Shared.Return(colorsArray);
			ArrayPool<int>.Shared.Return(lookLeftArray);
			ArrayPool<int>.Shared.Return(lookRightArray);
		}
	}

	/// <summary>
	/// Propagate colors from the starting point in the given direction,
	/// using pattern tiling with last-linked propagation.
	/// </summary>
	private static void PropagateColors(
		Span<Rgba32> colors, Span<int> lookArray,
		int start, int virtualWidth, int step,
		bool useLookRight, int startingPointForFilter,
		int y, int yShift, int maxSep, int patternHeight,
		int virtualPatternOffset, int oversampling,
		Image<Rgba32> pattern, Image<Rgba32> resultImage, bool postProcessing)
	{
		var lastLinked = -10;

		for (var x = start; step > 0 ? x < virtualWidth : x >= 0; x += step)
		{
			var lookVal = lookArray[x];
			var isUnlinked = useLookRight
				? lookVal == x
				: lookVal == x || lookVal < startingPointForFilter;

			if (isUnlinked)
			{
				if (lastLinked == x - step)
				{
					colors[x] = colors[x - step];
				}
				else
				{
					var calculatedY = y;

					if (yShift > 0)
						calculatedY = (y + (x - startingPointForFilter) / maxSep * yShift) + patternHeight;

					var locationX = (x + virtualPatternOffset) % maxSep / oversampling;
					var locationY = (calculatedY + patternHeight) % patternHeight;

					if (locationY < 0)
						locationY += patternHeight;

					colors[x] = pattern[locationX, locationY];
				}
			}
			else if (lookVal == int.MinValue)
			{
				colors[x] = Transparent;
			}
			else
			{
				colors[x] = colors[lookVal];
				lastLinked = x;
			}

			if (postProcessing)
				resultImage[x, y] = colors[x];
		}
	}

	/// <summary>
	/// Downsample the virtual-width color buffer into the result image row.
	/// </summary>
	private static void DownsampleToResult(
		Span<Rgba32> colors, Image<Rgba32> resultImage,
		int y, int width, int oversampling)
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
				(byte)(red / oversampling),
				(byte)(green / oversampling),
				(byte)(blue / oversampling),
				(byte)(alpha / oversampling));
		}
	}

	/// <summary>
	/// Fill lookLeft and lookRight arrays with depth-based separation constraints.
	/// Uses a row span for depth-map access instead of per-pixel indexer calls.
	/// </summary>
	private static void FillLookArrays(
		int x, ReadOnlySpan<Rgb48> depthRow,
		Span<int> lookLeft, Span<int> lookRight,
		ref int separation,
		int oversampling, int maxSep, int minSep, int virtualWidth)
	{
		if (x % oversampling == 0)
		{
			var color = depthRow[x / oversampling];
			var relativeDepth = (color.R + color.G + color.B) / OversamplingContext.MaxCombinedPixelValue;
			separation = (int)(maxSep - relativeDepth * (maxSep - minSep));
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
			}
		}
	}
}
