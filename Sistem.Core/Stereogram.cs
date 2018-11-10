using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Sistem.Core
{
	/// <summary>
	/// Create stereograms
	/// </summary>
	public class Stereogram : IDisposable
	{
		private readonly object _lock = new object();
		private const double MaxCombinedPixelValue = 196607; // -1 to rule out 0;

		private int _noiseDensity = 50;
		private int _oversampling = 1;
		private int _maxSeparation = 90;

		private Image<Rgb48> _depthMap;

		private Image<Rgb48> _directDepthMap;
		private Image<Rgba32> _directPattern;
		private Image<Rgba32> _directResultMap;

		// Once we start generating, we copy all properties and setting to these fields to prevent issues when properties are changed during generation.
		private int _currentPatternHeight;
		private int _currentHeight;
		private int _currentWidth;
		private int _currentYShift;
		private int _currentNoiseDensity;
		private int _currentOversampling;
		private bool _currentParallelProcessing;
		private bool _currentPostProcessingOversampling;

		// Virtual measurements for oversampling. For example the stereogram will be rendered at 2 times the width for Oversampling = 2
		private int _virtualWidth;
		private int _virtualMaxSeparation;
		private int _virtualMinSeparation;
		private int _virtualStartingPoint;
		private int _virtualPatternOffset;


		/// <summary>
		/// Gets a list of errors, if no stereogram is generated, this is where to look for clues as to why
		/// </summary>
		public List<string> ValidationErrors { get; } = new List<string>();

		/// <summary>
		/// Gets a list of warnings, the stereogram will be generated, but there may be some unexpected results
		/// </summary>
		public List<string> ValidationWarnings { get; } = new List<string>();

		/// <summary>
		/// Gets the depth map
		/// </summary>
		public Image<Rgb48> DepthMap
		{
			get => _depthMap;
			set
			{
				if (value == null)
					return;
				
				var width = value.Width;
				var height = value.Height;

				MaxSeparation = width / 10;
				MinSeparation = width / 16;
				PatternWidth = MaxSeparation;

				// Set YShift to a fraction of the height
				YShift = height / 32;

				_depthMap = value;
			}
		}

		/// <summary>
		/// Gets the pattern to use
		/// </summary>
		public Image<Rgba32> Pattern { get; set; }

		/// <summary>
		/// Gets the result image of the stereogram
		/// </summary>
		public Image<Rgba32> Result { get; private set; }

		/// <summary>
		/// Gets or sets the minimum separation to use in pixels, limited by the maximum width of the used pattern
		/// Default = 60
		/// </summary>
		public int MinSeparation { get; set; } = 60;

		/// <summary>
		/// Gets or sets the maximum separation to use in pixels, limited by the maximum width of the used pattern
		/// Default = 90
		/// </summary>
		public int MaxSeparation
		{
			get => _maxSeparation;
			set
			{
				_maxSeparation = value;

				if (value > PatternWidth)
					PatternWidth = value;
			}
		}

		/// <summary>
		/// Gets or sets the pattern width to use in pixels
		/// Default = 90
		/// </summary>
		public int PatternWidth { get; set; } = 90;

		/// <summary>
		/// Gets or sets the oversampling factor, for smoother results.
		/// Default = 1
		/// </summary>
		public int Oversampling
		{
			get => _oversampling;
			set
			{
				if (value < 1)
					_oversampling = 1;
				else if (value > 8)
					_oversampling = 8;
				else
					_oversampling = value;
			}
		}

		/// <summary>
		/// Gets or sets the Y-shift, shift the pattern this amount of pixels to prevent echoes
		/// </summary>
		public int YShift { get; set; } = 16;

		/// <summary>
		/// Gets or sets the view type to cross eyed
		/// </summary>
		public bool CrossView { get; set; }

		/// <summary>
		/// Gets or sets the random dot stereogram to colored noise instead of black and white
		/// </summary>
		public bool ColoredNoise { get; set; }

		/// <summary>
		/// Gets or sets the noise density (1-99)
		/// Default = 50
		/// </summary>
		public int NoiseDensity
		{
			get => _noiseDensity;
			set
			{
				if (value < 1)
					_noiseDensity = 1;
				else if (value > 99)
					_noiseDensity = 99;
				else _noiseDensity = value;
			}
		}

		/// <summary>
		/// Gets or sets parallel processing
		/// Default = true
		/// </summary>
		public bool ParallelProcessing { get; set; } = true;

		/// <summary>
		/// Gets ot sets the post processing oversampling: higher memory requirements, a bit more blurry but better looking images
		/// </summary>
		public bool PostProcessingOversampling { get; set; } = true;

		/// <summary>
		/// Load a depth map from a path
		/// </summary>
		/// <param name="filePath">The path of the file to load</param>
		public bool LoadDepthMap(string filePath)
		{
			try
			{
				DepthMap = Image.Load<Rgb48>(filePath);
				return true;
			}
			catch (NotSupportedException)
			{
				ValidationErrors.Add("Depthmap should be png, gif, jpg or bmp.");
			}

			return false;
		}

		/// <summary>
		/// Load a pattern from a path
		/// </summary>
		/// <param name="filePath">The path of the file to load</param>
		public bool LoadPattern(string filePath)
		{
			try
			{
				Pattern = Image.Load(filePath);
				return true;
			}
			catch (NotSupportedException)
			{
				ValidationErrors.Add("Pattern should be png, gif, jpg or bmp.");
			}

			return false;
		}

		/// <summary>
		/// Load a pattern from a path
		/// </summary>
		/// <param name="stream">The stream of the file to load</param>
		public bool LoadPattern(Stream stream)
		{
			try
			{
				Pattern = Image.Load(stream);
				return true;
			}
			catch (NotSupportedException)
			{
				ValidationErrors.Add("Pattern should be png, gif, jpg or bmp.");
			}

			return false;
		}

		/// <summary>
		/// Save the result
		/// </summary>
		/// <param name="path">The path to save the result to</param>
		public string SaveResult(string path = "")
		{
			if (string.IsNullOrWhiteSpace(path))
				path = string.Format("result-{0}.png", DateTime.Now.ToString("yyyyMMdd.HH.mm.ss"));

			Result?.Save(path);

			return path;
		}

		/// <summary>
		/// Validate all parameters
		/// </summary>
		/// <returns>True if all parameters are valid</returns>
		public bool Validate()
		{
			// Errors break functionality, any error will prevent a stereogram from being generated
			ValidationErrors.Clear();

			// Warnings do not break the process, they merely warn the user that unusual result may follow
			ValidationWarnings.Clear();

			if (PatternWidth < MaxSeparation)
				ValidationErrors.Add($"Pattern width ({PatternWidth}) should be bigger or equal to maximum separation ({MaxSeparation}).");

			if (_depthMap != null && PostProcessingOversampling && (long)_depthMap.Width * _depthMap.Height * _oversampling * 4 > int.MaxValue)
				ValidationErrors.Add("The depthmap is too big. Try disabling Post Processing Oversampling. The depthmap is limited to 536MP / Oversampling when Post Processing Oversampling is enabled.");
			else if (_depthMap != null && !PostProcessingOversampling && (long)_depthMap.Width * _depthMap.Height * 4 > int.MaxValue)
				ValidationErrors.Add("The depthmap is too big. The depthmap is limited to 536MP.");

			if (_depthMap == null)
				ValidationErrors.Add("No depthmap is set.");

			if (MaxSeparation < 10)
				ValidationErrors.Add("Maximum separation is too small.");

			if (MinSeparation < 10)
				ValidationErrors.Add("Minimum separation is too small.");

			if (MaxSeparation / (double)MinSeparation < 1)
				ValidationErrors.Add("Maximum separation must be bigger than minimum separation.");
			else if (MaxSeparation / (double)MinSeparation > 1.7)
				ValidationWarnings.Add("Maximum and minimum separation are quite far apart, this may cause unwanted effects.");
			else if (MaxSeparation / (double)MinSeparation < 1.1)
				ValidationWarnings.Add("Maximum and minimum separation are close, there will be barely any depth in the result.");

			// Warnings, no showstoppers but worth mentioning
			if (Pattern != null && PatternWidth > Pattern?.Width)
				ValidationWarnings.Add("Pattern width is greater than the pattern image. It will be zoomed in.");

			return ValidationErrors.Count == 0;
		}

		/// <summary>
		/// Generate the stereogram
		/// </summary>
		public bool Generate()
		{
			if (!Validate())
			{
				return false;
			}

			lock (_lock)
			{
				PrepareParameters();

				GenerateStereogram();

				SetResult();

				return true;
			}
		}

		/// <summary>
		/// Setup all the required parameters
		/// </summary>
		private void PrepareParameters()
		{
			_currentWidth = _depthMap.Width;
			_currentHeight = _depthMap.Height;
			_currentOversampling = Oversampling;
			_currentParallelProcessing = ParallelProcessing;
			_currentYShift = YShift;
			_currentNoiseDensity = NoiseDensity;
			_currentPostProcessingOversampling = PostProcessingOversampling;

			_directDepthMap = _depthMap;

			if (Pattern != null)
			{
				if (Pattern.Width != PatternWidth)
					_directPattern = Resize(Pattern, Math.Max(PatternWidth, MaxSeparation));
				else
					_directPattern = Pattern;
			}
			else if (_currentOversampling > 1)
				_directPattern = PrepareRandomDotPattern();

			_directResultMap?.Dispose();

			if (_currentPostProcessingOversampling && _currentOversampling > 1)
				_directResultMap = new Image<Rgba32>(_currentWidth * _currentOversampling, _currentHeight);
			else
				_directResultMap = new Image<Rgba32>(_currentWidth, _currentHeight);

			_currentPatternHeight = _directPattern?.Height ?? _currentHeight;

			if (Pattern != null && MaxSeparation > _directPattern?.Width && _directPattern.Width > 0)
				MinSeparation = _directPattern.Width;

			// Calculate values for oversampling
			_virtualWidth = _currentWidth * _currentOversampling;

			if (CrossView)
			{
				_virtualMinSeparation = MaxSeparation * _currentOversampling;
				_virtualMaxSeparation = MinSeparation * _currentOversampling;
			}
			else
			{
				_virtualMinSeparation = MinSeparation * _currentOversampling;
				_virtualMaxSeparation = MaxSeparation * _currentOversampling;
			}

			_virtualStartingPoint = _virtualWidth / 2 - _virtualMaxSeparation / 2;
			_virtualPatternOffset = _virtualMaxSeparation - (_virtualStartingPoint % _virtualMaxSeparation);
		}

		/// <summary>
		/// Generate the stereogram
		/// </summary>
		private void GenerateStereogram()
		{
			if (_currentParallelProcessing)
				Parallel.For(0, _currentHeight, y => { ProcessLine(y); });
			else
				for (var y = 0; y < _currentHeight; y++)
					ProcessLine(y);
		}

		/// <summary>
		/// Process a single line of the stereogram
		/// </summary>
		/// <param name="y">The line to process</param>
		private void ProcessLine(int y)
		{
			// No pattern and no oversampling => Use the "fast" random dot algorithm
			if (Pattern == null && _currentOversampling == 1)
				MakeLineRandomDot(y);
			else
				MakeLinePattern(y);
		}

		/// <summary>
		/// Load the result in the "Result" property.
		/// </summary>
		private void SetResult()
		{
			if (_currentOversampling == 1 || !_currentPostProcessingOversampling)
				Result = _directResultMap;
			else
				Result = Resize(_directResultMap, _currentWidth, _currentHeight);
		}

		/// <summary>
		/// Resize a bitmap to the width and height given in the parameters
		/// 
		/// If the height is 0, it will be calculated with the image's ratio
		/// </summary>
		/// <param name="source">The source image</param>
		/// <param name="width">The target width</param>
		/// <param name="height">The target height, 0 for auto calculate</param>
		/// <returns>A resized bitmap</returns>
		private static Image<Rgba32> Resize(Image<Rgba32> source, int width, int height = 0)
		{
			if (height == 0)
				height = (int)(source.Height / (source.Width / (double)width));

			Image<Rgba32> resultImage = source.Clone();
			resultImage.Mutate(x => x.Resize(width, height));

			return resultImage;
		}

		/// <summary>
		/// Render a line for a random dot Stereogram
		/// </summary>
		/// <param name="y">The line to render</param>
		private void MakeLineRandomDot(int y)
		{
			var random = new Random(Guid.NewGuid().GetHashCode());

			var lookLeft = new int[_currentWidth];

			for (var x = 0; x < _currentWidth; x++)
				lookLeft[x] = x;

			for (var x = 0; x < _currentWidth; x++)
			{
				// Get color from depth map
				var color = _directDepthMap[x, y];

				// Get the color's brightness in a range from 0..1
				var relativeDepth = (color.R + color.G + color.B) / MaxCombinedPixelValue;

				// 90 - 0.1 * (90 - 60)
				var separation = _virtualMaxSeparation - relativeDepth * (_virtualMaxSeparation - _virtualMinSeparation);

				var left = (int)(x - separation / 2);
				var right = (int)(left + separation);

				if (0 <= left && right < _currentWidth)
				{
					var kkk = lookLeft[left];

					while (kkk != left && kkk != right)
					{
						if (kkk < right)
							left = kkk;
						else
						{
							left = right;
							right = kkk;
						}

						kkk = lookLeft[left];
					}
					lookLeft[left] = right;
				}
			}

			for (var x = _currentWidth - 1; x >= 0; x--)
			{
				if (lookLeft[x] == x)
				{
					if (!ColoredNoise)
					{
						if (random.Next(100) > _currentNoiseDensity)
							_directResultMap[x, y] = Rgba32.White;
						else
							_directResultMap[x, y] = Rgba32.Black;
					}
					else
						_directResultMap[x, y] = new Rgba32((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
				}
				else
					_directResultMap[x, y] = _directResultMap[lookLeft[x], y];
			}
		}

		/// <summary>
		/// Render a line for a pattern Stereogram
		/// </summary>
		/// <param name="y">The line to render</param>
		private void MakeLinePattern(int y)
		{
			var colors = new Rgba32[_virtualWidth];
			var lookLeft = new int[_virtualWidth];
			var lookRight = new int[_virtualWidth];

			if (_currentParallelProcessing)
			{
				Parallel.For(0, _virtualWidth, x =>
				{
					lookLeft[x] = x;
					lookRight[x] = x;
				});
			}
			else
			{
				for (var x = 0; x < _virtualWidth; x++)
				{
					lookLeft[x] = x;
					lookRight[x] = x;
				}
			}

			// The separation in pixels
			double sep = 0;

			for (var x = 0; x < _virtualWidth; x++)
			{
				FillLookArrays(y, x, lookLeft, lookRight, ref sep);
			}

			// Everything from starting point to the right
			var lastLinked = -10;
			for (var x = _virtualStartingPoint; x < _virtualWidth; x++)
			{
				if (lookLeft[x] == x || lookLeft[x] < _virtualStartingPoint)
				{
					if (lastLinked == (x - 1))
						colors[x] = colors[x - 1];
					else
					{
						var calculatedY = y;

						if (YShift > 0)
							calculatedY = (y + (x - _virtualStartingPoint) / _virtualMaxSeparation * _currentYShift) + _currentPatternHeight;

						colors[x] = _directPattern[(x + _virtualPatternOffset) % _virtualMaxSeparation / _currentOversampling, calculatedY % _currentPatternHeight];
					}
				}
				else
				{
					colors[x] = colors[lookLeft[x]];
					lastLinked = x;
				}

				if (_currentPostProcessingOversampling)
					_directResultMap[x, y] = new Rgba32(colors[x].R, colors[x].G, colors[x].B);
			}

			// Everything from starting point to the left
			lastLinked = -10;
			for (var x = _virtualStartingPoint - 1; x >= 0; x--)
			{
				if (lookRight[x] == x)
				{
					if (lastLinked == x + 1)
						colors[x] = colors[x + 1];
					else
					{
						var calculatedY = y;

						if (YShift > 0)
							calculatedY = y + (x - _virtualStartingPoint) / _virtualMaxSeparation * _currentYShift + _currentPatternHeight;

						var locationX = (x + _virtualPatternOffset) % _virtualMaxSeparation / _currentOversampling;
						var locationY = (calculatedY + _currentPatternHeight) % _currentPatternHeight;

						colors[x] = _directPattern[locationX, locationY];
					}
				}
				else
				{
					colors[x] = colors[lookRight[x]];
					lastLinked = x;
				}


				if (_currentPostProcessingOversampling)
					_directResultMap[x, y] = new Rgba32(colors[x].R, colors[x].G, colors[x].B);
			}

			if (!_currentPostProcessingOversampling)
				for (var x = 0; x < _currentWidth; x++)
				{
					int red = 0;
					int green = 0;
					int blue = 0;

					for (var vx = 0; vx < _currentOversampling; vx++)
					{
						var color = colors[(x * _currentOversampling) + vx];
						red += color.R;
						green += color.G;
						blue += color.B;
					}

					_directResultMap[x, y] = new Rgba32((byte)Math.Floor(red / (double)_currentOversampling), (byte)Math.Floor(green / (double)_currentOversampling), (byte)Math.Floor(blue / (double)_currentOversampling));
				}
		}

		/// <summary>
		/// Fill lookleft and lookright arrays with the calculated values
		/// </summary>
		/// <param name="y">The y-coordinate</param>
		/// <param name="x">The x-coordinate</param>
		/// <param name="separation">The separation</param>
		/// <param name="lookLeft">The lookleft array</param>
		/// <param name="lookRight">The lookright array</param>
		/// <returns>The new separation value</returns>
		private void FillLookArrays(int y, int x, int[] lookLeft, int[] lookRight, ref double separation)
		{

			if (x % _currentOversampling == 0)
			{
				// Get color from depth map
				var color = _directDepthMap[x / _currentOversampling, y];

				// Get the color's brightness in a range from 0..1
				var relativeDepth = (color.R + color.G + color.B) / MaxCombinedPixelValue;

				separation = _virtualMaxSeparation - relativeDepth * (_virtualMaxSeparation - _virtualMinSeparation);
			}

			var left = (int)(x - separation / 2);
			var right = (int)(left + separation);

			var visible = true;

			if (left >= 0 && right < _virtualWidth)
			{
				if (lookLeft[right] != right)
				{
					if (lookLeft[right] < left)
					{
						lookRight[lookLeft[right]] = lookLeft[right];
						lookLeft[right] = right;
					}
					else
						visible = false;
				}

				if (lookRight[left] != left)
				{
					if (lookRight[left] > right)
					{
						lookLeft[lookRight[left]] = lookRight[left];
						lookRight[left] = left;
					}
					else
						visible = false;
				}

				if (visible)
				{
					lookLeft[right] = left;
					lookRight[left] = right;
				}
			}
		}

		/// <summary>
		/// Create a pattern from random dots
		/// </summary>
		private Image<Rgba32> PrepareRandomDotPattern()
		{
			var random = new Random();

			var result = new Image<Rgba32>(_currentWidth, _currentHeight);

			for (var x = 0; x < result.Width; x++)
			{
				for (var y = 0; y < result.Height; y++)
				{
					if (!ColoredNoise)
						if (random.Next(100) > _noiseDensity)
							result[x, y] = Rgba32.White;
						else
							result[x, y] = Rgba32.Black;
					else
						result[x, y] = new Rgba32((byte)random.Next(255), (byte)random.Next(255), (byte)random.Next(255));
				}
			}

			return result;
		}

		#region IDisposable members

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposing)
				return;

			_directPattern?.Dispose();
			_directResultMap?.Dispose();
			_directDepthMap?.Dispose();

			Pattern?.Dispose();
			_depthMap?.Dispose();


			Result?.Dispose();
		}

		#endregion
	}
}
