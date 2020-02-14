using Sistem.Core;
using Sistem2.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2.ViewModels
{
	/// <summary>
	/// Single Image Random Dot Stereogram Layer
	/// </summary>
	public abstract class StereogramLayer : LayerBase
	{
		private Image<Rgb48> _depthImage;
		private string _depthImageFileName;
		private float _minimumSeparation;
		private float _maximumSeparation;
		private float _origin;
		private bool _drawDepthImage;


		private float _eyeDistanceCentimeter;

		/// <summary>
		/// Gets ot sets the middle of the stereogram, where the pattern starts
		///
		/// Usually this is the middle of the stereogram
		/// </summary>
		public float Origin
		{
			get => _origin;
			set
			{
				_origin = value;
				OnPropertyChanged(nameof(Origin));
			}
		}

		/// <summary>
		/// The Depth image
		/// </summary>
		public Image<Rgb48> DepthImage
		{
			get => _depthImage;
			set
			{
				_depthImage = value;

				MinimumSeparation = _depthImage.Width / 8f;
				MaximumSeparation = _depthImage.Width / 7f;

				Width = _depthImage.Width;
				Height = _depthImage.Height;

				Origin = (value.Width - MaximumSeparation) / 2;

				DrawPreview();

				OnPropertyChanged(nameof(DepthImage));  
				OnPropertyChanged(nameof(DepthImageSource));  
			}
		}
		
		/// <summary>
		/// The Filename for the depth image
		/// </summary>
		public string DepthImageFileName
		{
			get => _depthImageFileName;
			set
			{
				_depthImageFileName = value;
				OnPropertyChanged(nameof(DepthImageFileName));  
			}
		}

		/// <summary>
		/// The Depth Image source
		/// </summary>
		public ImageSharpImageSource<Rgb48> DepthImageSource
		{
			get
			{
				if (DepthImage != null) 
					return new ImageSharpImageSource<Rgb48>(DepthImage);

				return null;
			}
		}

		/// <summary>
		/// Gets or sets the MinimumSeparation for the pattern
		/// </summary>
		public float MinimumSeparation
		{
			get => _minimumSeparation;
			set
			{
				_minimumSeparation = value;
				OnPropertyChanged(nameof(MinimumSeparation));
				OnPropertyChanged(nameof(MinimumDepthCentimeter));
				OnPropertyChanged(nameof(MinimumDepthInch));
			}
		}

		/// <summary>
		/// Gets or sets the MaximumSeparation for the pattern
		/// </summary>
		public float MaximumSeparation
		{
			get => _maximumSeparation;
			set
			{
				_maximumSeparation = value;
				OnPropertyChanged(nameof(MaximumSeparation));
				OnPropertyChanged(nameof(MaximumDepthCentimeter));
				OnPropertyChanged(nameof(MaximumDepthInch));
			}
		}

		/// <summary>
		/// Gets or sets the Distance from the eye to the screen/print in centimeters
		/// </summary>
		public float EyeDistanceCentimeter
		{
			get => _eyeDistanceCentimeter;
			set
			{
				_eyeDistanceCentimeter = value; OnPropertyChanged(nameof(EyeDistanceCentimeter));
				OnPropertyChanged(nameof(MinimumSeparation));
				OnPropertyChanged(nameof(MaximumSeparation));
				OnPropertyChanged(nameof(MinimumDepthCentimeter));
				OnPropertyChanged(nameof(MaximumDepthCentimeter));
				OnPropertyChanged(nameof(MinimumDepthInch));
				OnPropertyChanged(nameof(MaximumDepthInch));
			}
		}

		/// <summary>
		/// Gets or sets the minimum depth in centimeters
		/// </summary>
		public float MinimumDepthCentimeter
		{
			get => Utilities.EyeDistance * Dpc * EyeDistanceCentimeter / (Utilities.EyeDistance * Dpc - MinimumSeparation);
			set => MinimumSeparation = Utilities.EyeDistance * Dpc * ((value - EyeDistanceCentimeter) / value);
		}

		/// <summary>
		/// Gets or sets the maximum depth in centimeters
		/// </summary>
		public float MaximumDepthCentimeter
		{
			get => Utilities.EyeDistance * Dpc * EyeDistanceCentimeter / (Utilities.EyeDistance * Dpc - MaximumSeparation);
			set => MaximumSeparation = Utilities.EyeDistance * Dpc * ((value - EyeDistanceCentimeter) / value);
		}

		/// <summary>
		/// Gets or sets the Distance from the eye to the screen/print in inches
		/// </summary>
		public float EyeDistanceInch
		{
			get => Utilities.CMToInch(EyeDistanceCentimeter);
			set => EyeDistanceCentimeter = Utilities.InchToCM(value);
		}


		/// <summary>
		/// Gets or sets the minimum depth in inches
		/// </summary>
		public float MinimumDepthInch
		{
			get => Utilities.CMToInch(MinimumDepthCentimeter);
			set => MinimumDepthCentimeter = Utilities.InchToCM(value);
		}

		/// <summary>
		/// Gets or sets the maximum depth in inches
		/// </summary>
		public float MaximumDepthInch
		{
			get => Utilities.CMToInch(MaximumDepthCentimeter);
			set => MaximumDepthCentimeter = Utilities.InchToCM(value);
		}

		public bool DrawDepthImage
		{
			get => _drawDepthImage;
			set
			{
				_drawDepthImage = value;
				OnPropertyChanged(nameof(DrawDepthImage));
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target"></param>
		protected StereogramLayer(Image<Rgba32> target) : base(target)
		{
			EyeDistanceCentimeter = 50f;
		}

		/// <summary>
		/// Draw the depth image for the preview
		/// </summary>
		public override void DrawPreview()
		{
			if (DepthImage == null)
				return;

			var preview48 = DepthImage.CloneAs<Rgba32>();
			var preview = preview48.Clone(context => context.Resize(200, 150));
			Preview = new ImageSharpImageSource<Rgba32>(preview);
			OnPropertyChanged(nameof(Preview));  
		}

		/// <summary>
		/// Create a Stereogram object with the base properties
		/// </summary>
		/// <returns>A Stereogram</returns>
		public Stereogram CreateStereogram()
		{
			var stereogram = new Stereogram
			{
				DepthMap = DepthImage
			};

			//Don't use object initializer
			stereogram.MinSeparation = (int)MinimumSeparation;
			stereogram.MaxSeparation = (int)MaximumSeparation;
			stereogram.Origin = (int) Origin;

			return stereogram;
		}
	}
}
