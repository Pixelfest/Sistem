using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;

namespace Sistem.Core.Generation;

/// <summary>
/// Pre-computed dimensions for stereogram generation, incorporating oversampling.
/// Built once from <see cref="StereogramOptions"/> before rendering begins.
/// </summary>
internal sealed class OversamplingContext
{
	internal const double MaxCombinedPixelValue = 196607; // -1 to rule out 0

	/// <summary>
	/// Actual depth-map width.
	/// </summary>
	public int Width { get; private init; }

	/// <summary>
	/// Actual depth-map height.
	/// </summary>
	public int Height { get; private init; }

	/// <summary>
	/// Oversampling factor (1–8).
	/// </summary>
	public int Factor { get; private init; }

	/// <summary>
	/// Width × Factor.
	/// </summary>
	public int VirtualWidth { get; private init; }

	/// <summary>
	/// Max separation × Factor.
	/// </summary>
	public int VirtualMaxSeparation { get; private init; }

	/// <summary>
	/// Min separation × Factor.
	/// </summary>
	public int VirtualMinSeparation { get; private init; }

	/// <summary>
	/// Origin × Factor.
	/// </summary>
	public int VirtualStartingPoint { get; private init; }

	/// <summary>
	/// Noise reduction radius × Factor.
	/// </summary>
	public int VirtualNoiseReductionRadius { get; private init; }

	/// <summary>
	/// Height of the prepared pattern (or depth-map height when no pattern).
	/// </summary>
	public int PatternHeight { get; private init; }

	/// <summary>
	/// Resolved Y-shift.
	/// </summary>
	public int YShift { get; private init; }

	/// <summary>
	/// Noise density (1–99).
	/// </summary>
	public int NoiseDensity { get; private init; }

	/// <summary>
	/// Noise reduction threshold.
	/// </summary>
	public int NoiseReductionThreshold { get; private init; }

	/// <summary>
	/// Whether to use parallel line processing.
	/// </summary>
	public bool ParallelProcessing { get; private init; }

	/// <summary>
	/// Whether post-processing oversampling is active.
	/// </summary>
	public bool PostProcessingOversampling { get; private init; }

	/// <summary>
	/// Whether to generate colored noise.
	/// </summary>
	public bool ColoredNoise { get; private init; }

	/// <summary>
	/// The depth map reference.
	/// </summary>
	public Image<Rgb48> DepthMap { get; private init; }

	/// <summary>
	/// The prepared pattern (resized or synthetic). Null for non-oversampled random-dot.
	/// </summary>
	public Image<Rgba32>? PreparedPattern { get; internal set; }

	/// <summary>
	/// The result image to write pixels into.
	/// </summary>
	public Image<Rgba32> ResultImage { get; private init; }

	/// <summary>
	/// Build an <see cref="OversamplingContext"/> from the given options.
	/// </summary>
	public static OversamplingContext From(StereogramOptions options)
	{
		var oversampling = Math.Clamp(options.Oversampling, 1, 8);
		var noiseDensity = Math.Clamp(options.NoiseDensity, 1, 99);
		var depthMap = options.DepthMap;
		var width = depthMap.Width;
		var height = depthMap.Height;
		var postProcessingOversampling = options.PostProcessingOversampling;

		// Prepare pattern
		Image<Rgba32>? preparedPattern = null;
		var maxSep = options.MaxSeparation;
		var patternWidth = Math.Max(options.PatternWidth, maxSep);

		if (options.Pattern is not null)
		{
			preparedPattern = options.Pattern.Width != patternWidth
				? Resize(options.Pattern, Math.Max(patternWidth, maxSep))
				: options.Pattern;
		}
		else if (oversampling > 1)
		{
			preparedPattern = RandomDotPatternProvider.Create(
				width, height, noiseDensity, options.ColoredNoise);
		}

		// Result image dimensions
		var resultWidth = postProcessingOversampling && oversampling > 1
			? width * oversampling
			: width;

		var resultImage = new Image<Rgba32>(resultWidth, height);

		var patternHeight = preparedPattern?.Height ?? height;

		// Virtual dimensions
		var virtualWidth = width * oversampling;

		int virtualMinSep, virtualMaxSep;
		if (options.CrossView)
		{
			virtualMinSep = maxSep * oversampling;
			virtualMaxSep = options.MinSeparation * oversampling;
		}
		else
		{
			virtualMinSep = options.MinSeparation * oversampling;
			virtualMaxSep = maxSep * oversampling;
		}

		// Resolve origin
		var origin = options.Origin;
		if (!origin.HasValue)
			origin = width / 2 - maxSep / 2;
		else if (origin > width - maxSep)
			origin = width - maxSep;
		else if (origin < 0)
			origin = 0;

		return new OversamplingContext
		{
			Width = width,
			Height = height,
			Factor = oversampling,
			VirtualWidth = virtualWidth,
			VirtualMaxSeparation = virtualMaxSep,
			VirtualMinSeparation = virtualMinSep,
			VirtualStartingPoint = origin.Value * oversampling,
			VirtualNoiseReductionRadius = options.NoiseReductionRadius * oversampling,
			PatternHeight = patternHeight,
			YShift = options.YShift,
			NoiseDensity = noiseDensity,
			NoiseReductionThreshold = options.NoiseReductionThreshold,
			ParallelProcessing = options.ParallelProcessing,
			PostProcessingOversampling = postProcessingOversampling,
			ColoredNoise = options.ColoredNoise,
			DepthMap = depthMap,
			PreparedPattern = preparedPattern,
			ResultImage = resultImage,
		};
	}

	/// <summary>
	/// Resize an image. Height 0 means auto-calculated from aspect ratio.
	/// </summary>
	internal static Image<Rgba32> Resize(Image<Rgba32> source, int width, int height = 0)
	{
		if (height == 0)
			height = (int)(source.Height / (source.Width / (double)width));

		var resultImage = source.Clone();
		resultImage.Mutate(x => x.Resize(width, height));

		return resultImage;
	}
}
