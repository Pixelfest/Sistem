using System;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem.Core.Generation;

/// <summary>
/// All configurable parameters for stereogram generation.
/// </summary>
public record StereogramOptions
{
	/// <summary>
	/// Sentinel value meaning the separation should be calculated from depth map width.
	/// </summary>
	public const int AutoSeparation = 0;

	/// <summary>
	/// The depth map image (required).
	/// </summary>
	public required Image<Rgb48> DepthMap { get; init; }

	/// <summary>
	/// Optional pattern image. When null, a random-dot stereogram is generated.
	/// </summary>
	public Image<Rgba32>? Pattern { get; init; }

	/// <summary>
	/// Minimum separation in pixels.
	/// Default = DepthMap.Width / 8 (auto).
	/// </summary>
	public int MinSeparation { get; init; } = AutoSeparation;

	/// <summary>
	/// Maximum separation in pixels.
	/// Default = DepthMap.Width / 6 (auto).
	/// </summary>
	public int MaxSeparation { get; init; } = AutoSeparation;

	/// <summary>
	/// Origin x-coordinate for the pattern. Null for auto-center.
	/// </summary>
	public int? Origin { get; init; }

	/// <summary>
	/// Oversampling factor (1–8) for smoother results.
	/// Default = 1
	/// </summary>
	public int Oversampling { get; init; } = 1;

	/// <summary>
	/// Y-shift in pixels to prevent echoes.
	/// Default = 16
	/// </summary>
	public int YShift { get; init; } = 16;

	/// <summary>
	/// Threshold for noise reduction.
	/// Default = 10
	/// </summary>
	public int NoiseReductionThreshold { get; init; } = 10;

	/// <summary>
	/// Radius in pixels for noise reduction.
	/// Default = 0 (disabled)
	/// </summary>
	public int NoiseReductionRadius { get; init; } = 0;

	/// <summary>
	/// When true, generate a cross-eyed stereogram instead of wall-eyed.
	/// </summary>
	public bool CrossView { get; init; }

	/// <summary>
	/// Use colored noise instead of black and white for random-dot stereograms.
	/// </summary>
	public bool ColoredNoise { get; init; }

	/// <summary>
	/// Noise density (1–99) for random-dot stereograms.
	/// Default = 50
	/// </summary>
	public int NoiseDensity { get; init; } = 50;

	/// <summary>
	/// Use parallel processing for line generation.
	/// Default = true
	/// </summary>
	public bool ParallelProcessing { get; init; } = true;

	/// <summary>
	/// Post-processing oversampling: higher memory, slightly blurrier but better-looking.
	/// Default = true
	/// </summary>
	public bool PostProcessingOversampling { get; init; } = true;

	internal int GetResolvedMinSeparation() => ResolveSeparations().Min;
	internal int GetResolvedMaxSeparation() => ResolveSeparations().Max;

	private (int Min, int Max) ResolveSeparations()
	{
		var min = MinSeparation;
		var max = MaxSeparation;

		if (min > 0 && max > 0)
			return (min, max);

		var depthMapBasedMin = Math.Max(10, DepthMap.Width / 6);
		var depthMapBasedMax = Math.Max(10, DepthMap.Width / 8);

		var autoMin = Math.Min(depthMapBasedMin, depthMapBasedMax);
		var autoMax = Math.Max(depthMapBasedMin, depthMapBasedMax);

		if (min <= 0)
			min = autoMin;

		if (max <= 0)
			max = autoMax;

		return (min, max);
	}
}
