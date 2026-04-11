using System;
using System.IO;

namespace Sistem.CommandLine;

internal static class CommandLineUtilities
{
	public static void WriteBanner()
	{
		WriteLine("**************************************************************");
		WriteLine("********** Welcome to Sistem - Stereogram Generator **********");
		WriteLine("**************************************************************");
	}

	public static string? FindFile(string file)
	{
		if (File.Exists(file))
		{
			return file;
		}

		var localPath = Path.Combine(Directory.GetCurrentDirectory(), file);
		return File.Exists(localPath) ? localPath : null;
	}

	public static void WriteError(string message, params object[] args) => WriteLine(ConsoleColor.Red, message, args);
	public static void WriteWarning(string message, params object[] args) => WriteLine(ConsoleColor.DarkYellow, message, args);
	public static void WriteSuccess(string message, params object[] args) => WriteLine(ConsoleColor.Green, message, args);
	public static void WriteLine(string message) => Console.WriteLine(message);

	private static void WriteLine(ConsoleColor color, string message, params object[] args)
	{
		var defaultColor = Console.ForegroundColor;
		Console.ForegroundColor = color;
		Console.WriteLine(message, args);
		Console.ForegroundColor = defaultColor;
	}
}
