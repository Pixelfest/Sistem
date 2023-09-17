// See https://aka.ms/new-console-template for more information

using Sistem.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

Console.WriteLine("Hello!");

var image = Image.Load<Rgb48>("Simpsons.png");

var result = ImageProcessing.GenerateShadows(image);

result.Save("output.png");

Console.WriteLine("Done!");