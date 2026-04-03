namespace Sistem.Core.Generation;

/// <summary>
/// Generates stereogram images from a depth map and optional pattern.
/// </summary>
public interface IStereogramGenerator
{
	/// <summary>
	/// Generate a stereogram from the given options.
	/// </summary>
	/// <param name="options">All parameters for generation.</param>
	/// <returns>A result containing the image, or validation errors.</returns>
	StereogramResult Generate(StereogramOptions options);
}
