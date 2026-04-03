using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using System.Threading.Tasks;

namespace Sistem.Core.Generation;

/// <summary>
/// Default stereogram generator. Validates options, selects the appropriate
/// algorithm (random-dot or pattern-based), and produces the result.
/// </summary>
public sealed class StereogramGenerator : IStereogramGenerator
{
	private static readonly RandomDotAlgorithm RandomDot = new();
	private static readonly PatternAlgorithm Pattern = new();
	private static readonly OptimizedPatternAlgorithm OptimizedPattern = new();
	private static readonly NewPatternAlgorithm NewPattern = new();
	private static readonly BidirectionalAveragingAlgorithm BidirectionalAveraging = new();

	/// <inheritdoc />
	public StereogramResult Generate(StereogramOptions options)
	{
		var (errors, warnings) = StereogramValidator.Validate(options);

		if (errors.Count > 0)
		{
			return new StereogramResult
			{
				Errors = errors,
				Warnings = warnings,
			};
		}

		var context = OversamplingContext.From(options);

		var algorithm = SelectAlgorithm(options, context);

		RunLines(context, algorithm);

		var image = BuildResult(context);

		return new StereogramResult
		{
			Image = image,
			Warnings = warnings,
		};
	}

	/// <summary>
	/// Select the algorithm based on options.
	/// Random-dot is used only when there is no pattern and oversampling is 1.
	/// </summary>
	private static IStereogramAlgorithm SelectAlgorithm(StereogramOptions options, OversamplingContext context)
	{
		if (options.Pattern is null && context.Factor == 1)
			return RandomDot;

		//return BidirectionalAveraging;
		return OptimizedPattern;
		//return Pattern;
	}

	/// <summary>
	/// Process all lines, optionally in parallel.
	/// </summary>
	private static void RunLines(OversamplingContext context, IStereogramAlgorithm algorithm)
	{
		if (context.ParallelProcessing)
		{
			Parallel.For(0, context.Height, y => algorithm.ProcessLine(y, context));
		}
		else
		{
			for (var y = 0; y < context.Height; y++)
				algorithm.ProcessLine(y, context);
		}
	}

	/// <summary>
	/// Downsample the result when post-processing oversampling was used.
	/// </summary>
	private static Image<Rgba32> BuildResult(OversamplingContext context)
	{
		if (context.Factor == 1 || !context.PostProcessingOversampling)
			return context.ResultImage;

		return OversamplingContext.Resize(context.ResultImage, context.Width, context.Height);
	}
}
