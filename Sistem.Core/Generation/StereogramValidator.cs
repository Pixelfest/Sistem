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
		var minSeparation = options.GetResolvedMinSeparation();
		var maxSeparation = options.GetResolvedMaxSeparation();
		var depthMap = options.DepthMap;

		if (options.PostProcessingOversampling && (long)depthMap.Width * depthMap.Height * oversampling * 4 > int.MaxValue)
			errors.Add("The depthmap is too big. Try disabling Post Processing Oversampling. The depthmap is limited to 536MP / Oversampling when Post Processing Oversampling is enabled.");
		else if (!options.PostProcessingOversampling && (long)depthMap.Width * depthMap.Height * 4 > int.MaxValue)
			errors.Add("The depthmap is too big. The depthmap is limited to 536MP.");

		if (maxSeparation < 10)
			errors.Add("Maximum separation is too small.");

		if (minSeparation < 10)
			errors.Add("Minimum separation is too small.");

		var ratio = maxSeparation / (double)minSeparation;

		if (ratio < 1)
			errors.Add("Maximum separation must be bigger than minimum separation.");
		else if (ratio > 1.7)
			warnings.Add("Maximum and minimum separation are quite far apart, this may cause unwanted effects.");
		else if (ratio < 1.1)
			warnings.Add("Maximum and minimum separation are close, there will be barely any depth in the result.");

		return (errors, warnings);
	}
}
