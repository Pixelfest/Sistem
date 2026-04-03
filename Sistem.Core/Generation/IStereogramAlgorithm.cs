namespace Sistem.Core.Generation;

/// <summary>
/// Strategy interface for stereogram line-rendering algorithms.
/// </summary>
internal interface IStereogramAlgorithm
{
	/// <summary>
	/// Render a single horizontal line of the stereogram.
	/// </summary>
	/// <param name="y">The line index to process.</param>
	/// <param name="context">Pre-computed dimensions and shared images.</param>
	void ProcessLine(int y, OversamplingContext context);
}
