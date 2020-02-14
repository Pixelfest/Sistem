using System.ComponentModel;
using System.Runtime.CompilerServices;
using Sistem2.Annotations;
using Sistem2.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2.ViewModels
{
	/// <summary>
	/// Base class for layers
	/// </summary>
	public abstract class LayerBase : INotifyPropertyChanged
	{
		protected Image<Rgba32> CachedImage = null;
		private int _top;
		private int _left;
		private int _width;
		private int _height;
		private float _dpi;
		private float _opacity;
		private int _oversampling;
		private Measurements _measurements;
		
		/// <summary>
		/// The measurements to use
		/// </summary>
		public Measurements Measurements
		{
			get => _measurements;
			set { _measurements = value; OnPropertyChanged(nameof(Measurements)); }
		}

		/// <summary>
		/// Gets or sets the opacity of this layer
		/// </summary>
		public float Opacity
		{
			get => _opacity;
			set { _opacity = value; OnPropertyChanged(nameof(Opacity)); }
		}

		/// <summary>
		/// Draw this layer this amount of pixels from the top
		/// </summary>
		public int Top
		{
			get => _top;
			set { _top = value; OnPropertyChanged(nameof(Top));}
		}

		/// <summary>
		/// Draw this layer this amount of pixels from the left
		/// </summary>
		public int Left
		{
			get => _left;
			set { _left = value; OnPropertyChanged(nameof(Left));}
		}

		/// <summary>
		/// Width of the layer in pixels
		/// </summary>
		public int Width
		{
			get => _width;
			set
			{
				_width = value;
				OnPropertyChanged(nameof(Width));
				OnPropertyChanged(nameof(WidthInch));
				OnPropertyChanged(nameof(WidthCentimeter));
			}
		}

		/// <summary>
		/// Height of the layer in pixels
		/// </summary>
		public int Height
		{
			get => _height;
			set
			{
				_height = value; 
				OnPropertyChanged(nameof(Height));
				OnPropertyChanged(nameof(HeightInch));
				OnPropertyChanged(nameof(HeightCentimeter));
			}
		}

		/// <summary>
		/// DPI to use
		/// </summary>
		public float Dpi
		{
			get => _dpi;
			set 
			{ 
				_dpi = value;
				OnPropertyChanged(nameof(Dpi));
				OnPropertyChanged(nameof(Dpc));
				OnPropertyChanged(nameof(Width));
				OnPropertyChanged(nameof(WidthInch));
				OnPropertyChanged(nameof(WidthCentimeter));
				OnPropertyChanged(nameof(Height));
				OnPropertyChanged(nameof(HeightInch));
				OnPropertyChanged(nameof(HeightCentimeter));
			}
		}

		/// <summary>
		/// The amount of oversampling to use if applicable
		/// 0 = none
		/// 1-8 = Oversampling level
		/// </summary>
		public int Oversampling
		{
			get => _oversampling;
			set
			{
				if (value < 0)
					_oversampling = 0;
				else if (value > 8)
					_oversampling = 8;
				else
					_oversampling = value;

				OnPropertyChanged(nameof(Oversampling));
			}
		}

		/// <summary>
		/// Draw this layer this amount of inches from the top
		/// </summary>
		public float TopInch
		{
			get => Top / Dpi;
			set => Top = (int) (value * Dpi);
		}

		/// <summary>
		/// Draw this layer this amount of inches from the left
		/// </summary>
		public float LeftInch
		{
			get => Left / Dpi;
			set => Left = (int) (value * Dpi);
		}

		/// <summary>
		/// Width of the layer in inches
		/// </summary>
		public float WidthInch
		{
			get => Width / Dpi;
			set => Width = (int) (value * Dpi);
		}

		/// <summary>
		/// Height of the layer in inches
		/// </summary>
		public float HeightInch
		{
			get => Height / Dpi;
			set => Height = (int) (value * Dpi);
		}
		
		/// <summary>
		/// DPC (Dots per Centimeter) to use
		/// </summary>
		public float Dpc
		{
			get => Utilities.InchToCM(Dpi);
			set => Dpi = Utilities.CMToInch(value);
		}

		/// <summary>
		/// Draw this layer this amount of cm from the top
		/// </summary>
		public float TopCentimeter
		{
			get => Top / Dpc;
			set => Top = (int) (value * Dpc);
		}


		/// <summary>
		/// Draw this layer this amount of cm from the left
		/// </summary>
		public float LeftCentimeter
		{
			get => Left / Dpc;
			set => Left = (int) (value * Dpc);
		}

		/// <summary>
		/// Width of the layer in cm
		/// </summary>
		public float WidthCentimeter
		{
			get => Width / Dpc;
			set => Width = (int) (value * Dpc);
		}

		/// <summary>
		/// Height of the layer in cm
		/// </summary>
		public float HeightCentimeter
		{
			get => Height / Dpc;
			set => Height = (int) (value * Dpc);
		}

		/// <summary>
		/// The target image to draw to
		/// </summary>
		public Image<Rgba32> Target { get; set; }

		/// <summary>
		/// A preview of this layer
		/// </summary>
		public ImageSharpImageSource<Rgba32> Preview { get; set; }

		/// <summary>
		/// The name of the layer
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Is the layer visible?
		/// </summary>
		public bool Visible { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target">The target image to draw to</param>
		protected LayerBase(Image<Rgba32> target)
		{
			CachedImage = null;
			Target = target;
			Opacity = 1;
			Preview = new ImageSharpImageSource<Rgba32>(150,150);
		}
		/// <summary>
		/// Draw the layer
		/// </summary>
		public abstract void Draw();

		/// <summary>
		/// Draw a preview
		/// </summary>
		public abstract void DrawPreview();

		/// <summary>
		/// Event for property changed
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Event trigger for property changed
		/// </summary>
		/// <param name="propertyName">The name of the property that was changed</param>
		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			CachedImage = null;

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

	}
}
