using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
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
		private float _opacity;
		private int _oversampling;
		private string _name;
		private bool _visible;
		protected bool _importing;

		public float Opacity
		{
			get => _opacity;
			set
			{
				_opacity = value;
				OnPropertyChanged(nameof(Opacity));
			}
		}

		public int Top
		{
			get => _top;
			set
			{
				_top = value;
				OnPropertyChanged(nameof(Top));
			}
		}

		public int Left
		{
			get => _left;
			set
			{
				_left = value;
				OnPropertyChanged(nameof(Left));
			}
		}

		public Point Location => new Point(Left, Top);

		public int Width
		{
			get => _width;
			set
			{
				_width = value;
				OnPropertyChanged(nameof(Width));
			}
		}

		public int Height
		{
			get => _height;
			set
			{
				_height = value;
				OnPropertyChanged(nameof(Height));
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

		public string Name
		{
			get => _name;
			set
			{
				_name = value;
				OnPropertyChanged(nameof(Name));
			}
		}

		public bool Visible
		{
			get => _visible;
			set
			{
				_visible = value;
				OnPropertyChanged(nameof(Visible));
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
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			if (!_importing)
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
				}

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
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
                Visible = Visible
            };
        }

        public static TResult Import<TSource, TResult>(TSource dto)
		    where TSource : LayerBaseDto
		    where TResult : LayerBase, new()
        {
            var target = new TResult();

            target.Width = dto.Width;
            target.Height = dto.Height;
            target.Left = dto.Left;
            target.Top = dto.Top;
            target.Name = dto.Name;
            target.Opacity = dto.Opacity;
            target.Oversampling = dto.Oversampling;
            target.Visible = dto.Visible;

            return target;
        }
	}
}
