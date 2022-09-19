using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
    public class ImageLayer : LayerBase
	{
		private Image<Rgba32> _image;

		public Image<Rgba32> Image
		{
			get => _image;
			set
			{
				_image = value;

				Width = value.Width;
				Height = value.Height;

				OnPropertyChanged(nameof(Image));  
				OnPropertyChanged(nameof(ImageSource));  
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
	}
}
