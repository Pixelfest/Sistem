using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public abstract class LayerBase : INotifyPropertyChanged
	{
		public Image<Rgba32> CachedImage { get; set; }

		private int _top;
		private int _left;
		private int _width;
		private int _height;
		private float _dpi;
		private float _opacity;
		private int _oversampling;
		private Measurements _measurements;
		private int _measurementsTabIndex;
		private string _name;
		private bool _visible;

		public int MeasurementsTabIndex
		{
			get => _measurementsTabIndex;
			set { _measurementsTabIndex = value; OnPropertyChanged(nameof(MeasurementsTabIndex)); }
		}

		public Measurements Measurements
		{
			get => _measurements;
			set { _measurements = value; OnPropertyChanged(nameof(Measurements)); }
		}

		public float Opacity
		{
			get => _opacity;
			set { _opacity = value; OnPropertyChanged(nameof(Opacity)); }
		}

		public int Top
		{
			get => _top;
			set { _top = value; OnPropertyChanged(nameof(Top));}
		}

		public int Left
		{
			get => _left;
			set { _left = value; OnPropertyChanged(nameof(Left));}
		}

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

		public float TopInch
		{
			get => Top / Dpi;
			set => Top = (int) (value * Dpi);
		}

		public float LeftInch
		{
			get => Left / Dpi;
			set => Left = (int) (value * Dpi);
		}

		public float WidthInch
		{
			get => Width / Dpi;
			set => Width = (int) (value * Dpi);
		}

		public float HeightInch
		{
			get => Height / Dpi;
			set => Height = (int) (value * Dpi);
		}
		
		public float Dpc
		{
			get => Utilities.CMToInch(Dpi);
			set => Dpi = Utilities.InchToCM(value);
		}

		public float TopCentimeter
		{
			get => Top / Dpc;
			set => Top = (int) (value * Dpc);
		}

        public float LeftCentimeter
		{
			get => Left / Dpc;
			set => Left = (int) (value * Dpc);
		}

		public float WidthCentimeter
		{
			get => Width / Dpc;
			set => Width = (int) (value * Dpc);
		}

		public float HeightCentimeter
		{
			get => Height / Dpc;
			set => Height = (int) (value * Dpc);
		}

		public ImageSharpImageSource<Rgba32> Preview { get; set; }

		public string Name 
		{ 
			get => _name; 
			set	{ _name = value; OnPropertyChanged(nameof(Name));}
		}

		public bool Visible 
		{ 
			get => _visible; 
			set { _visible = value; OnPropertyChanged(nameof(Visible)); } 
		}

		protected LayerBase()
		{
			CachedImage = null;
			Opacity = 1;
			Preview = new ImageSharpImageSource<Rgba32>(150,150);
		}

		public abstract void Render();

		public abstract void DrawPreview();

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Visible):
				case nameof(Oversampling):
					CachedImage = null;
					break;
				case nameof(Opacity):
					// Trigger update, don't clear the cached image.
					break;
				default:
					break;
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}
    }
}
