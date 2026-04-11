namespace Sistem.Core.Generation;

/// <summary>
/// Selects which parameter details should be embedded into the saved image.
/// </summary>
public enum ResultImageParametersMode
{
	/// <summary>
	/// Do not embed parameter text in the output image.
	/// </summary>
	None,

	/// <summary>
	/// Embed the command line that generated the image.
	/// </summary>
	Command,

	/// <summary>
	/// Embed a detailed parameter list defined in code.
	/// </summary>
	Detailed,
}
