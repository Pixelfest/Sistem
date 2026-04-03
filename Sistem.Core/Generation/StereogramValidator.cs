using System;
using System.Collections.Generic;

namespace Sistem.Core.Generation;

/// <summary>
/// Validates <see cref="StereogramOptions"/> before generation.
/// </summary>
internal static class StereogramValidator
{
	/// <summary>
	/// Validate the options and return errors and warnings.
	/// </summary>
	public static (List<string> Errors, List<string> Warnings) Validate(StereogramOptions options)
	{
		var errors = new List<string>();
		var warnings = new List<string>();

		var oversampling = Math.Clamp(options.Oversampling, 1, 8);
		var patternWidth = Math.Max(options.PatternWidth, options.MaxSeparation);

		if (patternWidth < options.MaxSeparation)
			errors.Add($"Pattern width ({patternWidth}) should be bigger or equal to maximum separation ({options.MaxSeparation}).");

		var depthMap = options.DepthMap;

		if (options.PostProcessingOversampling && (long)depthMap.Width * depthMap.Height * oversampling * 4 > int.MaxValue)
			errors.Add("The depthmap is too big. Try disabling Post Processing Oversampling. The depthmap is limited to 536MP / Oversampling when Post Processing Oversampling is enabled.");
		else if (!options.PostProcessingOversampling && (long)depthMap.Width * depthMap.Height * 4 > int.MaxValue)
			errors.Add("The depthmap is too big. The depthmap is limited to 536MP.");

		if (options.MaxSeparation < 10)
			errors.Add("Maximum separation is too small.");

		if (options.MinSeparation < 10)
			errors.Add("Minimum separation is too small.");

		var ratio = options.MaxSeparation / (double)options.MinSeparation;

		if (ratio < 1)
			errors.Add("Maximum separation must be bigger than minimum separation.");
		else if (ratio > 1.7)
			warnings.Add("Maximum and minimum separation are quite far apart, this may cause unwanted effects.");
		else if (ratio < 1.1)
			warnings.Add("Maximum and minimum separation are close, there will be barely any depth in the result.");

		if (options.Pattern is not null && patternWidth > options.Pattern.Width)
			warnings.Add("Pattern width is greater than the pattern image. It will be zoomed in.");

		return (errors, warnings);
	}
}
