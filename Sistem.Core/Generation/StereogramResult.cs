using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace Sistem.Core.Generation;

/// <summary>
/// The outcome of a stereogram generation attempt.
/// </summary>
public sealed class StereogramResult
{
	/// <summary>
	/// The generated stereogram image, or null when validation failed.
	/// </summary>
	public Image<Rgba32>? Image { get; init; }

	/// <summary>
	/// Validation errors that prevented generation.
	/// </summary>
	public IReadOnlyList<string> Errors { get; init; } = [];

	/// <summary>
	/// Warnings that did not prevent generation but may affect quality.
	/// </summary>
	public IReadOnlyList<string> Warnings { get; init; } = [];

	/// <summary>
	/// Whether generation succeeded.
	/// </summary>
	public bool Success => Errors.Count == 0 && Image is not null;
}
