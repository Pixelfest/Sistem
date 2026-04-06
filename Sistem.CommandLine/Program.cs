using System.CommandLine;

namespace Sistem.CommandLine;

internal static class Program
{
	private static int Main(string[] args)
	{
		var rootCommand = new RootCommand("Generate singe image stereograms");

		foreach (var option in CommandLineOptions.GetAll())
		{
			rootCommand.Add(option);
		}

		rootCommand.SetAction(parseResult =>
		{
			var options = CommandLineOptions.FromParseResult(parseResult);
			return SistemCommandHandler.Run(options);
		});

		return rootCommand.Parse(args).Invoke();
	}
}
