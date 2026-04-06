// See https://aka.ms/new-console-template for more information

using Sistem.Core;
using Sistem.Core.Generation;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

Console.WriteLine("Hello!");

var outputFolder = @"d:\test\";
var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

string OutputPath(string folderName, string filename)
{
    var name = Path.GetFileNameWithoutExtension(filename);
    var ext = Path.GetExtension(filename);
    var folderPath = Path.Combine(outputFolder, folderName);
    Directory.CreateDirectory(folderPath);
    return Path.Combine(folderPath, $"{name}_{timestamp}{ext}");
}

string TestFilePath(string filename)
{
    return filename;
}

var image = Image.Load<Rgb48>("Simpsons.png");

var result = ImageProcessing.GenerateShadows(image);
result.Save(OutputPath("Common", "output-Simpsons-outline.png"));

var tests = new List<(string, string, string)>
{
	("Test01", "Test01-Depthmap.png", "Test01-Pattern.png"),
	("Test02", "Test02-Depthmap.png", "Test01-Pattern.png"),
	("Test03", "Test03-Depthmap.png", "Test02-Pattern.png")
};

foreach (var tuple in tests)
{
	var bla = new StereogramGenerator();
	var options = new StereogramOptions
	{
		DepthMap = ImageIO.LoadDepthMap(TestFilePath(tuple.Item2)),
		Pattern = ImageIO.LoadPattern(TestFilePath(tuple.Item3))
	};
	var stereogramResult = bla.Generate(options);
	ImageIO.SaveResult(stereogramResult.Image, OutputPath(tuple.Item1, $"output-{tuple.Item1}-default.png"));

	options = new StereogramOptions
	{
		DepthMap = ImageIO.LoadDepthMap(TestFilePath(tuple.Item2)),
		Pattern = ImageIO.LoadPattern(TestFilePath(tuple.Item3)),
		Oversampling = 8
	};
	stereogramResult = bla.Generate(options);
	stereogramResult = bla.Generate(options);
	ImageIO.SaveResult(stereogramResult.Image, OutputPath(tuple.Item1, $"output-{tuple.Item1}-over-8.png"));
}

Console.WriteLine("Done!");
