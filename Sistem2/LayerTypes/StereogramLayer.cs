using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media.Imaging;
using Sistem.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2.LayerTypes
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

		/// <summary>
		/// The Depth image
		/// </summary>
		public Image<Rgb48> DepthImage
		{
			get => _depthImage;
			set
			{
				_depthImage = value;

				MinimumSeparation = value.Width / 8f;
				MaximumSeparation = value.Width / 7f;

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
		/// The MinimumSeparation for the pattern
		/// </summary>
		public float MinimumSeparation
		{
			get => _minimumSeparation;
			set
			{
				_minimumSeparation = value;
				OnPropertyChanged(nameof(MinimumSeparation));
				OnPropertyChanged(nameof(MinimumDepthCentimeter));
			}
		}

		/// <summary>
		/// The MinimumSeparation for the pattern
		/// </summary>
		public float MaximumSeparation
		{
			get => _maximumSeparation;
			set
			{
				_maximumSeparation = value;
				OnPropertyChanged(nameof(MaximumSeparation));
				OnPropertyChanged(nameof(MaximumDepthCentimeter));
			}
		}
		private float _eyeDistanceCentimeter;

		public float EyeDistanceCentimeter
		{
			get => _eyeDistanceCentimeter;
			set
			{
				_eyeDistanceCentimeter = value; OnPropertyChanged(nameof(EyeDistanceCentimeter));
				OnPropertyChanged(nameof(MinimumSeparation));
				OnPropertyChanged(nameof(MinimumDepthCentimeter));
				OnPropertyChanged(nameof(MinimumDepthCentimeter));
				OnPropertyChanged(nameof(MaximumSeparation));
				OnPropertyChanged(nameof(MaximumDepthCentimeter));
			}
		}

		public float MinimumDepthCentimeter
		{
			get => Tools.EyeDistance * Dpc * EyeDistanceCentimeter / (Tools.EyeDistance * Dpc - MinimumSeparation);
			set => MinimumSeparation = Tools.EyeDistance * Dpc * ((value - EyeDistanceCentimeter) / value);
		}

		public float MaximumDepthCentimeter
		{
			get => Tools.EyeDistance * Dpc * EyeDistanceCentimeter / (Tools.EyeDistance * Dpc - MaximumSeparation);
			set => MaximumSeparation = Tools.EyeDistance * Dpc * ((value - EyeDistanceCentimeter) / value);
		}

		public float EyeDistanceInch
		{
			get => Tools.CMToInch(EyeDistanceCentimeter);
			set => EyeDistanceCentimeter = Tools.InchToCM(value);
		}

		public float MinimumDepthInch
		{
			get => Tools.CMToInch(MinimumDepthCentimeter);
			set => MinimumDepthCentimeter = Tools.InchToCM(value);
		}

		public float MaximumDepthInch
		{
			get => Tools.CMToInch(MaximumDepthCentimeter);
			set => MaximumDepthCentimeter = Tools.InchToCM(value);
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target"></param>
		public StereogramLayer(Image<Rgba32> target) : base(target)
		{
			EyeDistanceCentimeter = 50f;
		}

		/// <summary>
		/// Draw the depth image for the preview
		/// </summary>
		public override void DrawPreview()
		{
			var preview48 = DepthImage.CloneAs<Rgba32>();
			var preview = preview48.Clone(context => context.Resize(200, 150));
			Preview = new ImageSharpImageSource<Rgba32>(preview);
			OnPropertyChanged(nameof(Preview));  
		}

		public Stereogram CreateStereogram()
		{
			var stereogram = new Stereogram
			{
				DepthMap = DepthImage,
			};

			//Don't use object initializer
			stereogram.MinSeparation = (int)MinimumSeparation;
			stereogram.MaxSeparation = (int)MaximumSeparation;

			return stereogram;
		}
	}
}
