using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media.Imaging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sistem2.LayerTypes
{
	class ImageLayer : LayerBase, INotifyPropertyChanged
	{
		private Image<Rgba32> _image;
		private string _fileName;

		public Image<Rgba32> Image
		{
			get => _image;
			set
			{
				_image = value;
				OnPropertyChanged("Image");  
			}
		}

		public string FileName
		{
			get => _fileName;
			set
			{
				_fileName = value;
				OnPropertyChanged("FileName");  
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

		public override void Draw()
		{
			var location = new Point(0, 0);
		
			Target.Mutate(t => t.DrawImage(Image, location, 1));
		}
	}
}
