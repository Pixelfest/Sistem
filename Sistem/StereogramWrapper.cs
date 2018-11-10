using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem
{
	public class StereogramWrapper : BindableBase
	{
		private int _minSeparation = 60;
		private int _maxSeparation = 90;
		private int _textureWidth = 90;
		private int _noiseDensity = 50;
		private StereogramType _stereoType = StereogramType.FastRandomDot;
		private ViewType _viewType = ViewType.Parallel;
		private bool _randomDotUseColor;
		private bool _postProcessingOversampling = true;
		private bool _parallelProcessing = true;

		private int _yShift = 16;
		private int _oversampling = 1;

		private Image<Rgb48> _depthMap;
		private Image<Rgba32> _texture;

		private string _progress = "Hey, man. Welcome to sistem.";

		/// <summary>
		/// If no stereogram is generated, this list will contain the reasons why
		/// </summary>
		public List<string> ValidationErrors { get; } = new List<string>();

		/// <summary>
		/// Show warnings
		/// </summary>
		public List<string> ValidationWarnings { get; } = new List<string>();

		/// <summary>
		/// The depth map
		/// </summary>
		public Image<Rgb48> DepthMap
		{
			get => _depthMap;
			set
			{
				var width = value.Width;

				MaxSeparation = width / 10;
				MinSeparation = width / 16;
				TextureWidth = MaxSeparation;

				SetProperty(ref _depthMap, value);
			}
		}

		/// <summary>
		/// The pattern to use
		/// </summary>
		public Image<Rgba32> Texture { get => _texture; set => SetProperty(ref _texture, value); }

		/// <summary>
		/// The result of the stereogram
		/// </summary>
		public Bitmap Result { get; set; }

		/// <summary>
		/// The minimum separation to use in pixels, limited by the maximum width of the used pattern
		/// Default = 60
		/// </summary>
		public int MinSeparation { get => _minSeparation; set => SetProperty(ref _minSeparation, value); }

		/// <summary>
		/// The maximum separation to use in pixels, limited by the maximum width of the used pattern
		/// Default = 90
		/// </summary>
		public int MaxSeparation { get => _maxSeparation; set => SetProperty(ref _maxSeparation, value); }

		/// <summary>
		/// The pattern width to use in pixels
		/// Default = 90
		/// </summary>
		public int TextureWidth { get => _textureWidth; set => SetProperty(ref _textureWidth, value); }

		/// <summary>
		/// Oversampling factor, for smoother results.
		/// Default = 1
		/// </summary>
		public int Oversampling { get => _oversampling; set => SetProperty(ref _oversampling, value); }

		/// <summary>
		/// Shift pattern this amount of pixels to prevent echoes
		/// </summary>
		public int YShift { get => _yShift; set => SetProperty(ref _yShift, value); }

		/// <summary>
		/// The viewtype can be parallel or cross eyed
		/// </summary>
		public ViewType ViewType { get => _viewType; set => SetProperty(ref _viewType, value); }

		/// <summary>
		/// Stereogram type
		/// Default = StereogramType.FastRandomDot
		/// </summary>
		public StereogramType StereogramType
		{
			get => _stereoType; set
			{
				Oversampling = 1;
				SetProperty(ref _stereoType, value);
			}
		}

		/// <summary>
		/// Type of random dot (color/monochrome)
		/// Default = RandomDotColorType.Monochrome
		/// </summary>
		public bool RandomDotUseColor { get => _randomDotUseColor; set => SetProperty(ref _randomDotUseColor, value); }

		/// <summary>
		/// The noise density 1-99
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

				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Allow parallel processing
		/// </summary>
		public bool ParallelProcessing { get => _parallelProcessing; set => SetProperty(ref _parallelProcessing, value); }

		/// <summary>
		/// Use post processing oversampling, higher memory requirements, a bit more blurry but better looking images
		/// </summary>
		public bool PostProcessingOversampling { get => _postProcessingOversampling; set => SetProperty(ref _postProcessingOversampling, value); }

		/// <summary>
		/// Progress status
		/// </summary>
		public string Progress { get => _progress; set => SetProperty(ref _progress, value); }

		public bool Generate()
		{
			Progress = "Initializing...";

			ValidationErrors.Clear();
			ValidationWarnings.Clear();

			using (var stereogram = new Core.Stereogram())
			{
				Progress = "Loading depth map..";
				stereogram.DepthMap = DepthMap.Clone();

				if (Texture != null && StereogramType == StereogramType.Textured)
				{
					Progress = "Loading pattern...";
					stereogram.Pattern = Texture.Clone();
				}

				Progress = "Loading properties...";
				stereogram.MaxSeparation = MaxSeparation;
				stereogram.MinSeparation = MinSeparation;
				stereogram.NoiseDensity = NoiseDensity;
				stereogram.Oversampling = Oversampling;
				stereogram.ParallelProcessing = ParallelProcessing;
				stereogram.PatternWidth = TextureWidth;
				stereogram.PostProcessingOversampling = PostProcessingOversampling;
				stereogram.YShift = YShift;
				stereogram.CrossView = ViewType.CrossView == ViewType;
				stereogram.ColoredNoise = RandomDotUseColor;

				Progress = "Generating, please wait...";
				if (stereogram.Generate())
				{
					using (var stream = new MemoryStream())
					{
						Progress = "Copying output to result...";
						stereogram.Result.SaveAsPng(stream);

						Result = (Bitmap) System.Drawing.Image.FromStream(stream);
					}

					ValidationWarnings.AddRange(stereogram.ValidationWarnings);

					if (ValidationWarnings.Any())
						Progress = "Done! There were some warnings though...";
					else
						Progress = "Done!";

					return true;
				}
				else
				{
					Progress = "Sorry, there were errors.";
					ValidationErrors.AddRange(stereogram.ValidationErrors);
					ValidationWarnings.AddRange(stereogram.ValidationWarnings);
					return false;
				}
			}
		}
	}
}