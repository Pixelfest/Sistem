using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Png.Chunks;
using SixLabors.ImageSharp.Metadata.Profiles.Exif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sistem.Core.Generation;

/// <summary>
/// Helper methods for loading depth maps, patterns, and saving results.
/// </summary>
public static class ImageIO
{
	private const int OverlayPadding = 12;
	private const float OverlayFontSize = 16;
	private const int OverlayLineHeight = 24;
	private const int OverlayMinimumWidthForWrap = 120;

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
		SaveResult(image, options: null, path, saveMetadata: false, overlayOptions: null);

	/// <summary>
	/// Save a result image to disk and optionally include stereogram options in EXIF metadata.
	/// </summary>
	/// <param name="image">The image to save.</param>
	/// <param name="options">The used stereogram options.</param>
	/// <param name="path">The file path. If empty, a timestamped filename is generated.</param>
	/// <param name="saveMetadata">When true, write applied options to EXIF metadata.</param>
	/// <param name="overlayOptions">Controls whether parameter text is rendered under the saved image.</param>
	/// <returns>The path the file was saved to.</returns>
	public static string SaveResult(Image<Rgba32> image, StereogramOptions? options, string path = "", bool saveMetadata = true, ResultImageOverlayOptions? overlayOptions = null)
	{
		ArgumentNullException.ThrowIfNull(image);

		if (string.IsNullOrWhiteSpace(path))
			path = $"result-{DateTime.Now:yyyyMMdd.HH.mm.ss}.png";

		var imageToSave = BuildOverlayImageIfNeeded(image, options, overlayOptions);
		try
		{
			if (saveMetadata && options is not null)
			{
				AttachStereogramMetadata(imageToSave, options, path);
			}

			imageToSave.Save(path);
			return path;
		}
		finally
		{
			if (!ReferenceEquals(imageToSave, image))
			{
				imageToSave.Dispose();
			}
		}
	}

	private static Image<Rgba32> BuildOverlayImageIfNeeded(Image<Rgba32> image, StereogramOptions? options, ResultImageOverlayOptions? overlayOptions)
	{
		if (overlayOptions is null || overlayOptions.Mode == ResultImageParametersMode.None)
		{
			return image;
		}

		var rawLines = BuildOverlayLines(options, overlayOptions);
		if (rawLines.Count == 0)
		{
			return image;
		}

		var maxContentWidth = Math.Max(OverlayMinimumWidthForWrap, image.Width - (OverlayPadding * 2));
		var maxCharactersPerLine = Math.Max(20, (int)Math.Floor(maxContentWidth / (OverlayFontSize * 0.55f)));
		var lines = WrapLines(rawLines, maxCharactersPerLine);
		var panelHeight = (OverlayPadding * 2) + (lines.Count * OverlayLineHeight);

		var output = new Image<Rgba32>(image.Width, image.Height + panelHeight, Color.Black);
		var font = CreateOverlayFont();
		var textY = image.Height + OverlayPadding;

		output.Mutate(context =>
		{
			context.DrawImage(image, new Point(0, 0), 1f);
			foreach (var line in lines)
			{
				context.DrawText(line, font, Color.White, new PointF(OverlayPadding, textY));
				textY += OverlayLineHeight;
			}
		});

		return output;
	}

	private static Font CreateOverlayFont()
	{
		if (SystemFonts.TryGet("Consolas", out var family))
		{
			return family.CreateFont(OverlayFontSize, FontStyle.Regular);
		}

		var fallbackFamily = SystemFonts.Families.First();
		return fallbackFamily.CreateFont(OverlayFontSize, FontStyle.Regular);
	}

	private static List<string> BuildOverlayLines(StereogramOptions? options, ResultImageOverlayOptions overlayOptions)
	{
		if (overlayOptions.Mode == ResultImageParametersMode.Command)
		{
			return
			[
				$"Command: {overlayOptions.GetCommandText()}",
			];
		}

		if (options is null)
		{
			return
			[
				"Detailed parameters are unavailable.",
			];
		}

		var resolvedMin = options.GetResolvedMinSeparation();
		var resolvedMax = options.GetResolvedMaxSeparation();

		return
		[
			$"Depthmap (w*h): {options.DepthMap.Width}*{options.DepthMap.Height}",
			$"Minimum separation: {resolvedMin}",
			$"Maximum separation: {resolvedMax}",
			$"Oversampling: {options.Oversampling}",
			$"Cross view: {options.CrossView}",
		];
	}

	private static List<string> WrapLines(IReadOnlyList<string> lines, int maxCharactersPerLine)
	{
		var wrapped = new List<string>();
		foreach (var line in lines)
		{
			if (line.Length <= maxCharactersPerLine)
			{
				wrapped.Add(line);
				continue;
			}

			var words = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			var current = string.Empty;

			foreach (var word in words)
			{
				if (string.IsNullOrEmpty(current))
				{
					if (word.Length <= maxCharactersPerLine)
					{
						current = word;
						continue;
					}

					var offset = 0;
					while (offset < word.Length)
					{
						var chunkLength = Math.Min(maxCharactersPerLine, word.Length - offset);
						wrapped.Add(word.Substring(offset, chunkLength));
						offset += chunkLength;
					}

					continue;
				}

				if ((current.Length + 1 + word.Length) <= maxCharactersPerLine)
				{
					current = $"{current} {word}";
				}
				else
				{
					wrapped.Add(current);
					current = word;
				}
			}

			if (!string.IsNullOrEmpty(current))
			{
				wrapped.Add(current);
			}
		}

		return wrapped;
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
