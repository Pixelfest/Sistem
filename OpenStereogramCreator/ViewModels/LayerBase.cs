using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public abstract class LayerBase : INotifyPropertyChanged
	{
		public const string ImportName = "Import";

		public Image<Rgba32> CachedImage { get; set; }

		private int _top;
		private int _left;
		private int _width;
		private int _height;
		private float _opacity;
		private int _oversampling;
		private string _name;
		private PixelColorBlendingMode _blendingMode;
		private bool _visible;

		public float Opacity
		{
			get => _opacity;
			set
			{
				_opacity = value;
				OnPropertyChanged();
			}
		}

		public int Top
		{
			get => _top;
			set
			{
				_top = value;
				OnPropertyChanged();
			}
		}

		public int Left
		{
			get => _left;
			set
			{
				_left = value;
				OnPropertyChanged();
			}
		}

		public Point Location => new Point(Left, Top);

		public int Width
		{
			get => _width;
			set
			{
				_width = value;
				OnPropertyChanged();
			}
		}

		public int Height
		{
			get => _height;
			set
			{
				_height = value;
				OnPropertyChanged();
			}
		}

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

				OnPropertyChanged();
			}
		}

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				OnPropertyChanged();
			}
		}

		public PixelColorBlendingMode BlendingMode
		{
			get => _blendingMode;
			set
			{
				_blendingMode = value;
				OnPropertyChanged();
			}
		}

		public bool Visible
		{
			get => _visible;
			set
			{
				_visible = value;
				OnPropertyChanged();
			}
		}

		protected LayerBase()
		{
			CachedImage = null;
			Opacity = 1;
		}

		public abstract void Render();

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case ImportName:
				case nameof(Visible):
				case nameof(Oversampling):
					CachedImage = null;
					break;
				case nameof(Opacity):
					// Trigger update, don't clear the cached image.
					break;
			}

			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public void Import<TSource>(TSource dto)
			where TSource : LayerBaseDto, new()
		{
			this.Width = dto.Width;
			this.Height = dto.Height;
			this.Left = dto.Left;
			this.Top = dto.Top;
			this.Name = dto.Name;
			this.Opacity = dto.Opacity;
			this.Oversampling = dto.Oversampling;
			this.Visible = dto.Visible;

			try
			{
				this.BlendingMode = (PixelColorBlendingMode)dto.BlendingMode;
			}
			catch
			{
				this.BlendingMode = PixelColorBlendingMode.Normal;
			}

			this.OnPropertyChanged(nameof(Visible));
		}

		public T Export<T>() where T : LayerBaseDto, new()
		{
			return new T
			{
				Width = Width,
				Height = Height,
				Left = Left,
				Top = Top,
				Name = Name,
				Opacity = Opacity,
				Oversampling = Oversampling,
				Visible = Visible,
				BlendingMode = (int)BlendingMode
			};
		}
	}
}
