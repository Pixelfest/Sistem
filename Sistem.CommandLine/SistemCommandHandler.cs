using Sistem.Core.Generation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Diagnostics;

namespace Sistem.CommandLine;

internal static class SistemCommandHandler
{
	public static int Run(CommandLineOptions arguments, string? commandText = null)
	{
		CommandLineUtilities.WriteBanner();

		var result = 0;
		var depthMapFile = CommandLineUtilities.FindFile(arguments.DepthMap);
		if (string.IsNullOrEmpty(depthMapFile))
		{
			CommandLineUtilities.WriteError("Depthmap file could not be found");
			result = 2;
		}

		var patternFile = string.Empty;
		if (!string.IsNullOrWhiteSpace(arguments.Pattern))
		{
			patternFile = CommandLineUtilities.FindFile(arguments.Pattern) ?? string.Empty;
			if (string.IsNullOrEmpty(patternFile))
			{
				CommandLineUtilities.WriteError("Pattern file could not be found");
				result = 2;
			}
		}

		if (arguments.Oversampling is < 0 or > 8)
		{
			CommandLineUtilities.WriteError("--oversampling must be between 0 and 8.");
			result = 2;
		}

		if (arguments.NoiseDensity is < 1 or > 99)
		{
			CommandLineUtilities.WriteError("--noise-density must be between 1 and 99.");
			result = 2;
		}

		if (!TryCreateOverlayOptions(arguments, commandText, out var overlayOptions))
		{
			CommandLineUtilities.WriteError("--embed-parameters must be one of: command, detailed.");
			result = 2;
		}

		if (result == 0)
		{
			Image<Rgb48> depthMap;
			Image<Rgba32>? pattern = null;

			try
			{
				depthMap = ImageIO.LoadDepthMap(depthMapFile!);
			}
			catch (NotSupportedException)
			{
				CommandLineUtilities.WriteError("Depthmap should be png, gif, jpg or bmp.");
				return 3;
			}

			try
			{
				if (!string.IsNullOrWhiteSpace(patternFile))
				{
					try
					{
						pattern = ImageIO.LoadPattern(patternFile);
					}
					catch (NotSupportedException)
					{
						CommandLineUtilities.WriteError("Pattern should be png, gif, jpg or bmp.");
						return 3;
					}
				}

				var options = new StereogramOptions
				{
					DepthMap = depthMap,
					Pattern = pattern,
					MinSeparation = arguments.MinSeparation ?? StereogramOptions.AutoSeparation,
					MaxSeparation = arguments.MaxSeparation ?? StereogramOptions.AutoSeparation,
					Origin = arguments.Origin,
					YShift = arguments.YShift ?? 16,
					NoiseReductionRadius = arguments.NoiseReductionRadius ?? 0,
					NoiseReductionThreshold = arguments.NoiseReductionThreshold ?? 10,
					Oversampling = arguments.Oversampling ?? 1,
					CrossView = arguments.CrossView,
					ColoredNoise = arguments.ColoredNoise,
					NoiseDensity = arguments.NoiseDensity ?? 50,
					PostProcessingOversampling = !arguments.DisablePostProcessingOverSampling,
					ParallelProcessing = !arguments.NoParallelProcessing,
				};

				var keeper = Stopwatch.StartNew();
				var generator = new StereogramGenerator();
				var stereogramResult = generator.Generate(options);
				CommandLineUtilities.WriteSuccess($"The stereogram was generated in {keeper.ElapsedMilliseconds}ms.");

				foreach (var message in stereogramResult.Warnings)
				{
					CommandLineUtilities.WriteWarning(message);
				}

				if (!stereogramResult.Success)
				{
					foreach (var message in stereogramResult.Errors)
					{
						CommandLineUtilities.WriteError(message);
					}

					result = 3;
				}
				else
				{
					CommandLineUtilities.WriteSuccess("The stereogram was successfully generated. Saving...");

					using var image = stereogramResult.Image;
					var fileName = ImageIO.SaveResult(image!, options, arguments.ResultFile ?? string.Empty, arguments.SaveMetadata, overlayOptions);

					CommandLineUtilities.WriteSuccess("The stereogram was saved as '{0}'", fileName);
				}
			}
			finally
			{
				depthMap.Dispose();
				pattern?.Dispose();
			}
		}

		if (result > 0)
		{
			CommandLineUtilities.WriteWarning("Use --help to view command options.");
		}

		return result;
	}

	private static bool TryCreateOverlayOptions(CommandLineOptions arguments, string? commandText, out ResultImageOverlayOptions overlayOptions)
	{
		var normalizedMode = arguments.ParameterOverlayMode.Trim().ToLowerInvariant();
		switch (normalizedMode)
		{
			case null:
			case "":
			case "none":
				overlayOptions = ResultImageOverlayOptions.None;
				return true;
			case "command":
				overlayOptions = new ResultImageOverlayOptions
				{
					Mode = ResultImageParametersMode.Command,
					CommandText = commandText,
				};
				return true;
			case "detailed":
				overlayOptions = new ResultImageOverlayOptions
				{
					Mode = ResultImageParametersMode.Detailed,
				};
				return true;
			default:
				overlayOptions = ResultImageOverlayOptions.None;
				return false;
		}
	}
}
