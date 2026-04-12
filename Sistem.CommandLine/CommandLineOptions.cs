using System.CommandLine;

namespace Sistem.CommandLine;

internal sealed class CommandLineOptions
{
	public required string DepthMap { get; init; }
	public string? Pattern { get; init; }
	public string? ResultFile { get; init; }
	public int? MinSeparation { get; init; }
	public int? MaxSeparation { get; init; }
	public int? Origin { get; init; }
	public int? YShift { get; init; }
	public int? NoiseReductionRadius { get; init; }
	public int? NoiseReductionThreshold { get; init; }
	public int? Oversampling { get; init; }
	public bool DisablePostProcessingOverSampling { get; init; }
	public bool CrossView { get; init; }
	public bool ColoredNoise { get; init; }
	public bool NoParallelProcessing { get; init; }
	public int? NoiseDensity { get; init; }
	public bool SaveMetadata { get; init; } = true;
	public string ParameterOverlayMode { get; init; } = "none";

	public static readonly Option<string> DepthMapOption = CreateRequiredOption<string>("-d", "--depth-map", "The depth map (png, gif, jpg or bmp)");
	public static readonly Option<string?> PatternOption = CreateOption<string?>("-p", "--pattern", "The pattern map (png, gif, jpg or bmp)");
	public static readonly Option<string?> ResultFileOption = CreateOption<string?>("-f", "--result", "The result filename");
	public static readonly Option<int?> MinSeparationOption = CreateOption<int?>("-i", "--min-separation", "The minimum pattern size in pixels");
	public static readonly Option<int?> MaxSeparationOption = CreateOption<int?>("-a", "--max-separation", "The maximum pattern size in pixels");
	public static readonly Option<int?> OriginOption = CreateOption<int?>("-b", "--pattern-origin", "The pattern origin, default to center");
	public static readonly Option<int?> YShiftOption = CreateOption<int?>("-y", "--y-shift", "The number of pixels to shift on y-axis, to fix echoes");
	public static readonly Option<int?> NoiseReductionRadiusOption = CreateOption<int?>("-r", "--noise-reduction-radius", "Fix echo noise in the resulting image (radius, default 3)");
	public static readonly Option<int?> NoiseReductionThresholdOption = CreateOption<int?>("-t", "--noise-reduction-threshold", "Fix echo noise in the resulting image (threshold, default 10)");
	public static readonly Option<int?> OversamplingOption = CreateOption<int?>("-o", "--oversampling", "Amount of oversampling (1-8)");
	public static readonly Option<bool> DisablePostProcessingOverSamplingOption = CreateOption<bool>("-z", "--disable-ppo", "Disable post processing oversampling (for working with really, really big images)");
	public static readonly Option<bool> CrossViewOption = CreateOption<bool>("-x", "--crossview", "Use crossview instead of parallel");
	public static readonly Option<bool> ColoredNoiseOption = CreateOption<bool>("-c", "--use-color", "Use color for random dot stereogram");
	public static readonly Option<bool> NoParallelProcessingOption = CreateOption<bool>("-m", "--no-parallel-processing", "Disable parallel processing");
	public static readonly Option<int?> NoiseDensityOption = CreateOption<int?>("-n", "--noise-density", "Noise density for monochrome random dot stereogram (1-99)");
	public static readonly Option<bool?> SaveMetadataOption = CreateOption<bool?>("-s", "--save-metadata", "Save used stereogram options in EXIF metadata when supported by the output format.");
	public static readonly Option<string?> ParameterOverlayModeOption = CreateOption<string?>("-e", "--embed-parameters", "Embed parameter text in output image: command, detailed.");

	public static Option[] GetAll() =>
	[
		DepthMapOption,
		PatternOption,
		ResultFileOption,
		MinSeparationOption,
		MaxSeparationOption,
		OriginOption,
		YShiftOption,
		NoiseReductionRadiusOption,
		NoiseReductionThresholdOption,
		OversamplingOption,
		DisablePostProcessingOverSamplingOption,
		CrossViewOption,
		ColoredNoiseOption,
		NoParallelProcessingOption,
		NoiseDensityOption,
		SaveMetadataOption,
		ParameterOverlayModeOption,
	];

	public static CommandLineOptions FromParseResult(ParseResult parseResult) => new()
	{
		DepthMap = parseResult.GetValue(DepthMapOption)!,
		Pattern = parseResult.GetValue(PatternOption),
		ResultFile = parseResult.GetValue(ResultFileOption),
		MinSeparation = parseResult.GetValue(MinSeparationOption),
		MaxSeparation = parseResult.GetValue(MaxSeparationOption),
		Origin = parseResult.GetValue(OriginOption),
		YShift = parseResult.GetValue(YShiftOption),
		NoiseReductionRadius = parseResult.GetValue(NoiseReductionRadiusOption),
		NoiseReductionThreshold = parseResult.GetValue(NoiseReductionThresholdOption),
		Oversampling = parseResult.GetValue(OversamplingOption),
		DisablePostProcessingOverSampling = parseResult.GetValue(DisablePostProcessingOverSamplingOption),
		CrossView = parseResult.GetValue(CrossViewOption),
		ColoredNoise = parseResult.GetValue(ColoredNoiseOption),
		NoParallelProcessing = parseResult.GetValue(NoParallelProcessingOption),
		NoiseDensity = parseResult.GetValue(NoiseDensityOption),
		SaveMetadata = parseResult.GetValue(SaveMetadataOption) ?? true,
		ParameterOverlayMode = parseResult.GetValue(ParameterOverlayModeOption) ?? "none",
	};

	private static Option<T> CreateOption<T>(string shortAlias, string longAlias, string description)
	{
		var option = string.IsNullOrWhiteSpace(shortAlias)
			? new Option<T>(longAlias)
			: new Option<T>(longAlias, shortAlias);

		option.Description = description;
		return option;
	}

	private static Option<T> CreateRequiredOption<T>(string shortAlias, string longAlias, string description)
	{
		var option = CreateOption<T>(shortAlias, longAlias, description);
		option.Required = true;
		return option;
	}
}
