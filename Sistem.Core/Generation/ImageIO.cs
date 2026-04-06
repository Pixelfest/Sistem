using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.IO;

namespace Sistem.Core.Generation;

/// <summary>
/// Helper methods for loading depth maps, patterns, and saving results.
/// </summary>
public static class ImageIO
{
	/// <summary>
	/// Load a depth map image from a file path.
	/// </summary>
	/// <param name="filePath">Path to the image file (png, gif, jpg, bmp).</param>
	/// <returns>The loaded depth map image.</returns>
	/// <exception cref="NotSupportedException">The file format is not supported.</exception>
	public static Image<Rgb48> LoadDepthMap(string filePath) =>
		Image.Load<Rgb48>(filePath);

	/// <summary>
	/// Load a pattern image from a file path.
	/// </summary>
	/// <param name="filePath">Path to the image file (png, gif, jpg, bmp).</param>
	/// <returns>The loaded pattern image.</returns>
	/// <exception cref="NotSupportedException">The file format is not supported.</exception>
	public static Image<Rgba32> LoadPattern(string filePath) =>
		Image.Load<Rgba32>(filePath);

	/// <summary>
	/// Save a result image to disk.
	/// </summary>
	/// <param name="image">The image to save.</param>
	/// <param name="path">The file path. If empty, a timestamped filename is generated.</param>
	/// <returns>The path the file was saved to.</returns>
	public static string SaveResult(Image<Rgba32> image, string path = "") =>
		SaveResult(image, options: null, path, saveMetadata: false);

	/// <summary>
	/// Save a result image to disk and optionally include stereogram options in EXIF metadata.
	/// </summary>
	/// <param name="image">The image to save.</param>
	/// <param name="options">The used stereogram options.</param>
	/// <param name="path">The file path. If empty, a timestamped filename is generated.</param>
	/// <param name="saveMetadata">When true, write applied options to EXIF metadata.</param>
	/// <returns>The path the file was saved to.</returns>
	public static string SaveResult(Image<Rgba32> image, StereogramOptions? options, string path = "", bool saveMetadata = true)
	{
		ArgumentNullException.ThrowIfNull(image);

		if (string.IsNullOrWhiteSpace(path))
			path = $"result-{DateTime.Now:yyyyMMdd.HH.mm.ss}.png";

		if (saveMetadata && options is not null)
		{
			AttachStereogramMetadata(image, options, path);
		}

		image.Save(path);
		return path;
	}

	private static void AttachStereogramMetadata(Image<Rgba32> image, StereogramOptions options, string path)
	{
		var metadataValue = BuildStereogramMetadataValue(options);

		if (string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase))
		{
			var pngMetadata = image.Metadata.GetPngMetadata();
			pngMetadata.TextData.Add(new PngTextData("sistem:stereogram-options", metadataValue, null, null));
			return;
		}

		var exif = image.Metadata.ExifProfile ?? new ExifProfile();
		exif.SetValue(ExifTag.Software, "Sistem");
		exif.SetValue(ExifTag.ImageDescription, metadataValue);
		image.Metadata.ExifProfile = exif;
	}

	private static string BuildStereogramMetadataValue(StereogramOptions options)
	{
		var resolvedMin = options.GetResolvedMinSeparation();
		var resolvedMax = options.GetResolvedMaxSeparation();

		return string.Join(';',
			"generator=sistem",
			"schema=stereogram-options-v1",
			$"depth-map-width={options.DepthMap.Width}",
			$"depth-map-height={options.DepthMap.Height}",
			$"has-pattern={options.Pattern is not null}",
			$"min-separation={resolvedMin}",
			$"max-separation={resolvedMax}",
			$"origin={(options.Origin?.ToString() ?? "auto")}",
			$"oversampling={options.Oversampling}",
			$"y-shift={options.YShift}",
			$"noise-reduction-threshold={options.NoiseReductionThreshold}",
			$"noise-reduction-radius={options.NoiseReductionRadius}",
			$"cross-view={options.CrossView}",
			$"colored-noise={options.ColoredNoise}",
			$"noise-density={options.NoiseDensity}",
			$"parallel-processing={options.ParallelProcessing}",
			$"post-processing-oversampling={options.PostProcessingOversampling}");
	}
}
