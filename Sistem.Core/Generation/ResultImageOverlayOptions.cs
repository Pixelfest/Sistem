using System;

namespace Sistem.Core.Generation;

/// <summary>
/// Controls text overlay content rendered below a saved stereogram image.
/// </summary>
public sealed record ResultImageOverlayOptions
{
	/// <summary>
	/// Gets the mode used to build parameter text.
	/// </summary>
	public ResultImageParametersMode Mode { get; init; }

	/// <summary>
	/// Gets the command line that generated the image.
	/// </summary>
	public string? CommandText { get; init; }

	public static ResultImageOverlayOptions None { get; } = new()
	{
		Mode = ResultImageParametersMode.None,
	};

	internal string GetCommandText() =>
		string.IsNullOrWhiteSpace(CommandText)
			? "(command unavailable)"
			: CommandText.Trim();
}
