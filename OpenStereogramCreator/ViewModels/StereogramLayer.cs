using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using Sistem.Core;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public abstract class StereogramLayer : LayerBase, IHaveADepthImage
	{
		private Image<Rgb48> _depthImage;
		private string _depthImageFileName;
		private float _minimumSeparation;
		private float _maximumSeparation;
		private float _origin;
		private bool _drawDepthImage;


		public float Origin
		{
			get => _origin;
			set
			{
				_origin = value;
				OnPropertyChanged(nameof(Origin));
			}
		}

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

				if (this is PatternStereogramLayer || this is RandomDotStereogramLayer)
					Origin = (value.Width - MaximumSeparation) / 2;

				OnPropertyChanged(nameof(DepthImage));
				OnPropertyChanged(nameof(DepthImageSource));
			}
		}

		public string DepthImageFileName
		{
			get => _depthImageFileName;
			set
			{
				_depthImageFileName = value;
				OnPropertyChanged(nameof(DepthImageFileName));
			}
		}

		public ImageSharpImageSource<Rgb48> DepthImageSource
		{
			get
			{
				if (DepthImage != null)
					return new ImageSharpImageSource<Rgb48>(DepthImage);

				return null;
			}
		}

		public float MinimumSeparation
		{
			get => _minimumSeparation;
			set
			{
				_minimumSeparation = value;
				OnPropertyChanged(nameof(MinimumSeparation));
			}
		}

		public float MaximumSeparation
		{
			get => _maximumSeparation;
			set
			{
				_maximumSeparation = value;
				OnPropertyChanged(nameof(MaximumSeparation));
			}
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

		public Stereogram CreateStereogram()
		{
			var stereogram = new Stereogram
			{
				DepthMap = DepthImage
			};

			// Don't use object initializer, constructor initializes values that are otherwise overwritten.
			stereogram.MinSeparation = (int)MinimumSeparation;
			stereogram.MaxSeparation = (int)MaximumSeparation;
			stereogram.Origin = (int)Origin;

			return stereogram;
		}

		[NotifyPropertyChangedInvocator]
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(MinimumSeparation):
				case nameof(MaximumSeparation):
				case nameof(Origin):
				case nameof(DrawDepthImage):
				case nameof(DepthImage):
					CachedImage = null;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}
	}
}
