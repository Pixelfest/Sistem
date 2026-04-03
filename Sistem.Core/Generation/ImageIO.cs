using SixLabors.ImageSharp;
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
	public static string SaveResult(Image<Rgba32> image, string path = "")
	{
		ArgumentNullException.ThrowIfNull(image);

		if (string.IsNullOrWhiteSpace(path))
			path = $"result-{DateTime.Now:yyyyMMdd.HH.mm.ss}.png";

		image.Save(path);
		return path;
	}
}
