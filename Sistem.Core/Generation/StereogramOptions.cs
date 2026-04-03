using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem.Core.Generation;

/// <summary>
/// All configurable parameters for stereogram generation.
/// </summary>
public record StereogramOptions
{
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
	/// Default = 60
	/// </summary>
	public int MinSeparation { get; init; } = 60;

	/// <summary>
	/// Maximum separation in pixels.
	/// Default = 90
	/// </summary>
	public int MaxSeparation { get; init; } = 90;

	/// <summary>
	/// Pattern width in pixels. Should be >= MaxSeparation.
	/// Default = 90
	/// </summary>
	public int PatternWidth { get; init; } = 90;

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
}
