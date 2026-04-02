// See https://aka.ms/new-console-template for more information

using Sistem.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

Console.WriteLine("Hello!");

var outputFolder = @"d:\test\";
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

string OutputPath(string filename)
{
    var name = Path.GetFileNameWithoutExtension(filename);
    var ext = Path.GetExtension(filename);
    return Path.Combine(outputFolder, $"{name}_{timestamp}{ext}");
}

var image = Image.Load<Rgb48>("Simpsons.png");

var result = ImageProcessing.GenerateShadows(image);
result.Save(OutputPath("output-Simpsons-outline.png"));

var tests = new List<(string, string, string)>
{
	("Test01", "Test01-Depthmap.png", "Test01-Pattern.png"),
	("Test02", "Test02-Depthmap.png", "Test01-Pattern.png")
};

foreach (var tuple in tests)
{
	var stereogram = new Stereogram();
	stereogram.DepthMap = Image.Load<Rgb48>(tuple.Item2);
	stereogram.Pattern = Image.Load<Rgba32>(tuple.Item3);
	//stereogram.MinSeparation = 120;
	//stereogram.MaxSeparation = 160;
	stereogram.YShift = 0;
	stereogram.Generate();
	stereogram.SaveResult(OutputPath($"output-{tuple.Item1}-default.png"));

	stereogram.Oversampling = 2;
	stereogram.Generate();
	stereogram.SaveResult(OutputPath($"output-{tuple.Item1}-over-2.png"));

	stereogram.Oversampling = 8;
	stereogram.Generate();
	stereogram.SaveResult(OutputPath($"output-{tuple.Item1}-over-8.png"));
}
Console.WriteLine("Done!");
