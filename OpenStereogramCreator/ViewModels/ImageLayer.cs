using System;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
    public class ImageLayer : LayerBase
	{
		private Image<Rgba32> _image;
		private string _imageFileName;

		public Image<Rgba32> Image
		{
			get => _image;
			set
			{
				_image = value;

				Width = value.Width;
				Height = value.Height;

				OnPropertyChanged();  
				OnPropertyChanged(nameof(ImageSource));  
			}
		}

		public string ImageFileName
		{
			get => _imageFileName;
			set
			{
				_imageFileName = value;
				OnPropertyChanged();
			}
		}

		public ImageSharpImageSource<Rgba32> ImageSource
		{
			get
			{
				if (Image != null) 
					return new ImageSharpImageSource<Rgba32>(Image);

				return null;
			}
		}

		public override void Render()
		{
			if (Image == null)
				return;

			if (CachedImage != null)
				return;

			CachedImage = Image;
		}

		[NotifyPropertyChangedInvocator]
		public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Image):
					CachedImage = null;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}
		public new T Export<T>() where T : ImageLayerDto, new()
		{
			var export = base.Export<T>();

			if (Image != null)
			{
				export.ImageBase64 = Image.ToBase64String(PngFormat.Instance);
				export.ImageFileName = ImageFileName;
			}

			return export;
		}

		public new void Import<TSource>(TSource source)
			where TSource : ImageLayerDto, new()
		{
			if (!string.IsNullOrEmpty(source.ImageBase64))
			{
				var bytes = Convert.FromBase64String(source.ImageBase64.Split(",")[1]);
				this.Image = SixLabors.ImageSharp.Image.Load<Rgba32>(bytes);
				this.ImageFileName = source.ImageFileName;
			}
			base.Import(source);
		}
	}
}
