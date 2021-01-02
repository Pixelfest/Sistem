using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator.ViewModels
{
	public class ImageLayer : LayerBase
	{
		private Image<Rgba32> _image;
		private string _fileName;

		public Image<Rgba32> Image
		{
			get => _image;
			set
			{
				_image = value;

				Width = value.Width;
				Height = value.Height;

				DrawPreview();

				OnPropertyChanged(nameof(Image));  
				OnPropertyChanged(nameof(ImageSource));  
			}
		}

		public string FileName
		{
			get => _fileName;
			set
			{
				_fileName = value;
				OnPropertyChanged(nameof(FileName));  
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

		public override void DrawPreview()
		{
			if (Image == null)
				return;

			var preview = Image.Clone(context => context.Resize(150, 150));
			Preview = new ImageSharpImageSource<Rgba32>(preview);
			OnPropertyChanged(nameof(Preview));  
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
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Image):
					CachedImage = null;
					break;
				default:
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

	}
}
