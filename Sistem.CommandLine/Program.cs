using McMaster.Extensions.CommandLineUtils;
using Sistem.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.IO;

namespace Sistem.CommandLine
{
	[Command(Name = "sis", Description = "Generate singe image stereograms", ThrowOnUnexpectedArgument = false)]
	[HelpOption("-?|-h|--help")]
	class Program
	{
		[Option(CommandOptionType.SingleValue, Description = "The depth map (png, gif, jpg or bmp)", Template = "-d|--depth-map")]
		[Required]
		protected string DepthMap { get; set; }

		[Option(CommandOptionType.SingleValue, Description = "The pattern map (png, gif, jpg or bmp)", Template = "-p|--pattern")]
		protected string Pattern { get; set; }

		[Option(CommandOptionType.SingleValue, Description = "The result filename", Template = "-r|--result")]
		protected string ResultFile { get; set; }

		[Option(CommandOptionType.SingleValue, Description = "The minimum pattern size in pixels", Template = "-i|--min-separation")]
		protected int? MinSeparation { get; set; }

		[Option(CommandOptionType.SingleValue, Description = "The maximum pattern size in pixels", Template = "-a|--max-separation")]
		protected int? MaxSeparation { get; set; }

		[Option(CommandOptionType.SingleValue, Description = "The pattern size in pixels, match with max-separation for seamless results", Template = "-w|--pattern-width")]
		protected int? PatternWidth { get; set; }

		[Option(CommandOptionType.SingleValue, Description = "Amount of oversampling (1-8)", Template = "-o|--oversampling")]
		[Range(0, 8)]
		protected int? Oversampling { get; set; }
		
		[Option(CommandOptionType.NoValue, Description = "Disable post processing oversampling (for working with really, really big images)", Template = "-z|--disable-ppo")]
		protected bool? DisablePostProcessingOverSampling { get; set; }

		[Option(CommandOptionType.NoValue, Description = "Use crossview instead of parallel", Template = "-x|--crossview")]
		protected bool? CrossView { get; set; }

		[Option(CommandOptionType.NoValue, Description = "Use color for random dot stereogram", Template = "-c|--use-color")]
		protected bool? ColoredNoise { get; set; }

		[Option(CommandOptionType.SingleValue, Description = "Noise density for monochrome random dot stereogram (1-99)", Template = "-n|--noise-density")]
		[Range(1, 99)]
		protected int? NoiseDensity { get; set; }


		private static ConsoleColor _defaultColor;

		private static int Main(string[] args)
		{
			var result = CommandLineApplication.Execute<Program>(args);

#if DEBUG
			WriteLine("Press enter to exit.");
			Console.ReadLine();
#endif

			return result;
		}

		// ReSharper disable once UnusedMember.Local
		private int OnExecute(CommandLineApplication app)
		{
			_defaultColor = Console.ForegroundColor;

			WriteLine("**************************************************************");
			WriteLine("********** Welcome to Sistem - Stereogram Generator **********");
			WriteLine("**************************************************************");

			var result = 0;

			string depthMapFile = FindFile(DepthMap);
			if (string.IsNullOrEmpty(depthMapFile))
			{
				WriteError($"Depthmap file could not be found");
				result = 2;
			}

			string patternFile = string.Empty;

			if (!string.IsNullOrEmpty(Pattern))
			{
				patternFile = FindFile(Pattern);
				if (string.IsNullOrEmpty(patternFile))
				{
					WriteError($"Pattern file could not be found");
					result = 2;
				}
			}

			if (result == 0)
			{
				using (var stereogram = new Stereogram())
				{
					bool success = true;

					// Load image files
					success &= stereogram.LoadDepthMap(depthMapFile);

					if (!string.IsNullOrWhiteSpace(patternFile))
						success &= stereogram.LoadPattern(patternFile);

					if (success)
					{
						// Set parameters
						if (MinSeparation.HasValue)
							stereogram.MinSeparation = MinSeparation.Value;

						if (MaxSeparation.HasValue)
							stereogram.MaxSeparation = MaxSeparation.Value;

						if (PatternWidth.HasValue)
							stereogram.PatternWidth = PatternWidth.Value;

						if (Oversampling.HasValue)
							stereogram.Oversampling = Oversampling.Value;

						if (CrossView.HasValue)
							stereogram.CrossView = true;

						if (ColoredNoise.HasValue)
							stereogram.ColoredNoise = true;

						if (NoiseDensity.HasValue)
							stereogram.NoiseDensity = NoiseDensity.Value;

						if(DisablePostProcessingOverSampling.HasValue)
							stereogram.PostProcessingOversampling = false;

						// Generate the stereogram
						success = stereogram.Generate();
					}

					if (!success)
					{
						foreach (var message in stereogram.ValidationErrors)
							WriteError(message);

						result = 3;
					}

					// Always write warnings
					foreach (var message in stereogram.ValidationWarnings)
						WriteWarning(message);

					if (success)
					{
						WriteSuccess("The stereogram was successfully generated. Saving...");

						var fileName = stereogram.SaveResult(ResultFile);

						WriteSuccess("The stereogram was saved as '{0}'", fileName);
					}
				}
			}

			if (result > 0)
				app.ShowHelp();

			return result;
		}

		private static string FindFile(string file)
		{
			if (!File.Exists(file))
				file = Path.Combine(Directory.GetCurrentDirectory(), file);

			if (!File.Exists(file))
				file = null;

			return file;
		}

		private static void WriteLine(ConsoleColor color, string message, params object[] args)
		{
			Console.ForegroundColor = color;
			Console.WriteLine(message, args);
			Console.ForegroundColor = _defaultColor;
		}
		private static void WriteError(string message, params object[] args) => WriteLine(ConsoleColor.Red, message, args);
		private static void WriteWarning(string message, params object[] args) => WriteLine(ConsoleColor.DarkYellow, message, args);
		private static void WriteSuccess(string message, params object[] args) => WriteLine(ConsoleColor.Green, message, args);
		private static void WriteLine(string message) => Console.WriteLine(message);
	}
}
