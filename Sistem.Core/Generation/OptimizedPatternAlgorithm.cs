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

	// ── Slope-shadow configuration (set SlopeShadingEnabled = false to disable entirely) ──
	private const bool SlopeShadingEnabled = false;
	private const float SlopeShadowMaxDarkening = 0.4f;  // 0 = no darkening, 1 = full black
	private const float SlopeShadowUpper = 0.02f;         // per-pixel slope >= this → full shadow
	private const float SlopeShadowLower = 0.01f;        // per-pixel slope <= this → no shadow
	private const float SlopeShadowDecay = 0.20f;         // per-pixel decay factor for shadow spread

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

			// ── Pass 2: Compute and reconcile slope shadow ──
			float[]? shadowArray = null;
			var shadow = Span<float>.Empty;

			if (SlopeShadingEnabled)
			{
				var rawArray = ArrayPool<float>.Shared.Rent(width);
				try
				{
					var rawShadow = ComputeRawShadow(depthRow, rawArray.AsSpan(0, width));

					shadowArray = ArrayPool<float>.Shared.Rent(virtualWidth);
					shadow = shadowArray.AsSpan(0, virtualWidth);

					ExpandAndReconcileShadow(rawShadow, shadow, lookLeft, depthRow,
						oversampling, maxSep, minSep);
				}
				finally
				{
					ArrayPool<float>.Shared.Return(rawArray);
				}
			}

			try
			{
				// ── Pass 3: Propagate colors, apply shadow, write output ──
				PropagateColors(
					colors, lookLeft, startingPoint, virtualWidth, step: 1,
					useLookRight: false, startingPointForFilter: startingPoint,
					y, yShift, maxSep, patternHeight, virtualPatternOffset, oversampling,
					pattern);

				PropagateColors(
					colors, lookRight, startingPoint - 1, virtualWidth, step: -1,
					useLookRight: true, startingPointForFilter: startingPoint,
					y, yShift, maxSep, patternHeight, virtualPatternOffset, oversampling,
					pattern);

				if (!shadow.IsEmpty)
					ApplyShadowToRow(colors, shadow);

				if (postProcessing)
				{
					for (var x = 0; x < virtualWidth; x++)
						resultImage[x, y] = colors[x];
				}
				else
				{
					DownsampleToResult(colors, resultImage, y, width, oversampling);
				}
			}
			finally
			{
				if (shadowArray is not null)
					ArrayPool<float>.Shared.Return(shadowArray);
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
		Image<Rgba32> pattern)
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
	/// Compute the raw shadow intensity at depth-map resolution.
	/// Uses per-pixel adjacent depth difference as the seed, then spreads
	/// shadow outward with exponential decay (<see cref="SlopeShadowDecay"/>)
	/// to produce a smooth gradient instead of hard lines.
	/// </summary>
	private static ReadOnlySpan<float> ComputeRawShadow(
		ReadOnlySpan<Rgb48> depthRow, Span<float> destination)
	{
		var width = destination.Length;
		if (width == 0)
			return ReadOnlySpan<float>.Empty;

		// Seed: per-pixel slope from adjacent depth difference
		var prevDepth = (float)((depthRow[0].R + depthRow[0].G + depthRow[0].B)
			/ OversamplingContext.MaxCombinedPixelValue);
		destination[0] = 0f;

		for (var px = 1; px < width; px++)
		{
			var depth = (float)((depthRow[px].R + depthRow[px].G + depthRow[px].B)
				/ OversamplingContext.MaxCombinedPixelValue);
			destination[px] = MathF.Abs(depth - prevDepth);
			prevDepth = depth;
		}

		// Forward decay pass: spread shadow to the right
		for (var px = 1; px < width; px++)
			destination[px] = MathF.Max(destination[px], destination[px - 1] * SlopeShadowDecay);

		// Backward decay pass: spread shadow to the left
		for (var px = width - 2; px >= 0; px--)
			destination[px] = MathF.Max(destination[px], destination[px + 1] * SlopeShadowDecay);

		return destination;
	}

	/// <summary>
	/// Scatter raw shadow to link endpoint positions in virtual space, then
	/// reconcile through the link structure so that every pixel in a linked
	/// chain shares the same (maximum) shadow value. This makes the shadow
	/// part of the repeated pattern: all linked copies see identical darkening.
	/// </summary>
	/// <remarks>
	/// Each depth-map pixel <c>px</c> creates a link pair at
	/// <c>left = px*oversampling - sep/2</c> and <c>right = left + sep</c>.
	/// Pattern distortion is visible at those endpoints, not at <c>px</c>,
	/// so shadow must be placed there to align with the 3-D image.
	/// </remarks>
	private static void ExpandAndReconcileShadow(
		ReadOnlySpan<float> rawShadow, Span<float> shadow,
		ReadOnlySpan<int> lookLeft, ReadOnlySpan<Rgb48> depthRow,
		int oversampling, int maxSep, int minSep)
	{
		var virtualWidth = shadow.Length;
		var width = rawShadow.Length;
		shadow.Clear();

		// Scatter raw shadow to link endpoint positions
		for (var px = 0; px < width; px++)
		{
			var s = rawShadow[px];
			if (s <= 0f) continue;

			var color = depthRow[px];
			var relativeDepth = (color.R + color.G + color.B) / OversamplingContext.MaxCombinedPixelValue;
			var sep = (int)(maxSep - relativeDepth * (maxSep - minSep));

			var vx = px * oversampling;
			var leftBase = vx - sep / 2;
			var rightBase = leftBase + sep;

			for (var k = 0; k < oversampling; k++)
			{
				var left = leftBase + k;
				if ((uint)left < (uint)virtualWidth)
					shadow[left] = MathF.Max(shadow[left], s);

				var right = rightBase + k;
				if ((uint)right < (uint)virtualWidth)
					shadow[right] = MathF.Max(shadow[right], s);
			}
		}

		// Forward pass (left -> right): propagate max through link chains
		for (var x = 0; x < virtualWidth; x++)
		{
			var l = lookLeft[x];
			if (l != x && l >= 0)
			{
				var max = Math.Max(shadow[x], shadow[l]);
				shadow[x] = max;
				shadow[l] = max;
			}
		}

		// Backward pass (right -> left): ensures max reaches the start of each chain
		for (var x = virtualWidth - 1; x >= 0; x--)
		{
			var l = lookLeft[x];
			if (l != x && l >= 0)
			{
				var max = Math.Max(shadow[x], shadow[l]);
				shadow[x] = max;
				shadow[l] = max;
			}
		}
	}

	/// <summary>
	/// Apply reconciled shadow to every pixel in the colour buffer.
	/// </summary>
	private static void ApplyShadowToRow(Span<Rgba32> colors, ReadOnlySpan<float> shadow)
	{
		for (var x = 0; x < colors.Length; x++)
		{
			var s = shadow[x];
			if (s > SlopeShadowLower)
				colors[x] = DarkenPixel(colors[x], s);
		}
	}

	/// <summary>
	/// Darken a single pixel with a linear ramp between
	/// <see cref="SlopeShadowLower"/> and <see cref="SlopeShadowUpper"/>.
	/// </summary>
	private static Rgba32 DarkenPixel(Rgba32 color, float slope)
	{
		var t = MathF.Min((slope - SlopeShadowLower) / (SlopeShadowUpper - SlopeShadowLower), 1f);
		var factor = 1f - t * SlopeShadowMaxDarkening;

		return new Rgba32(
			(byte)(color.R * factor),
			(byte)(color.G * factor),
			(byte)(color.B * factor),
			color.A);
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
