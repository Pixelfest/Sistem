using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sistem2.LayerTypes
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

		public ImageLayer(Image<Rgba32> target) : base(target)
		{
		}

		public override void DrawPreview()
		{
			var preview = Image.Clone(context => context.Resize(150, 150));
			Preview = new ImageSharpImageSource<Rgba32>(preview);
			OnPropertyChanged(nameof(Preview));  
		}

		public override void Draw()
		{
			var location = new Point(0, 0);

			if(Image != null)
				Target.Mutate(t => t.DrawImage(Image, location, 1));
		}
	}
}
