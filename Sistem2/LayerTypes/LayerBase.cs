using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Printing;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Sistem2.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2.LayerTypes
{
	public abstract class LayerBase : INotifyPropertyChanged
	{
		private int _top;
		private int _left;
		private int _width;
		private int _height;
		private float _dpi;
		private Measurements _measurements;

		public Measurements Measurements
		{
			get => _measurements;
			set { _measurements = value; OnPropertyChanged(nameof(Measurements)); }
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
			get => Tools.InchToCM(Dpi);
			set => Dpi = Tools.CMToInch(value);
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

		public Image<Rgba32> Target { get; set; }
		public ImageSharpImageSource<Rgba32> Preview { get; set; }
		public string Name { get; set; }

		public bool Visible { get; set; }
		public int Order { get; set; }

		public bool IsDeleteEnabled  => true;

		protected LayerBase(Image<Rgba32> target)
		{
			this.Target = target;
			this.Preview = new ImageSharpImageSource<Rgba32>(150,150);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public abstract void Draw();
		public abstract void DrawPreview();
	}
}
