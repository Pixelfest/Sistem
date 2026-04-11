using System;
using System.CommandLine;
using System.Linq;

namespace Sistem.CommandLine;

internal static class Program
{
	private const string CommandDisplayName = "sis.exe";

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
			return SistemCommandHandler.Run(options, BuildDisplayCommand(args));
		});

		return rootCommand.Parse(args).Invoke();
	}

	private static string BuildDisplayCommand(string[] args)
	{
		if (args.Length == 0)
		{
			return CommandDisplayName;
		}

		return $"{CommandDisplayName} {string.Join(' ', args.Select(EscapeArgument))}";
	}

	private static string EscapeArgument(string value)
	{
		if (string.IsNullOrEmpty(value))
		{
			return "\"\"";
		}

		if (!value.Any(char.IsWhiteSpace) && !value.Contains('"'))
		{
			return value;
		}

		return $"\"{value.Replace("\\", "\\\\").Replace("\"", "\\\"")}\"";
	}
}
