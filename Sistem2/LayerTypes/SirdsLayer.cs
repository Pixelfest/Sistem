using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Media.Imaging;
using Sistem.Core;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace Sistem2.LayerTypes
{
	/// <summary>
	/// Single Image Random Dot Stereogram Layer
	/// </summary>
	public class SirdsLayer : LayerBase
	{
		private Image<Rgb48> _depthImage;
		private string _depthImageFileName;

		/// <summary>
		/// The Depth image
		/// </summary>
		public Image<Rgb48> DepthImage
		{
			get => _depthImage;
			set
			{
				_depthImage = value;

				DrawPreview();

				OnPropertyChanged(nameof(DepthImage));  
				OnPropertyChanged(nameof(DepthImageSource));  
			}
		}
		
		/// <summary>
		/// The Filename for the depth image
		/// </summary>
		public string DepthImageFileName
		{
			get => _depthImageFileName;
			set
			{
				_depthImageFileName = value;
				OnPropertyChanged(nameof(DepthImageFileName));  
			}
		}

		/// <summary>
		/// The Depth Image source
		/// </summary>
		public ImageSharpImageSource<Rgb48> DepthImageSource
		{
			get
			{
				if (DepthImage != null) 
					return new ImageSharpImageSource<Rgb48>(DepthImage);

				return null;
			}
		}

		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target"></param>
		public SirdsLayer(Image<Rgba32> target) : base(target)
		{
		}

		/// <summary>
		/// Draw the depth image for the preview
		/// </summary>
		public override void DrawPreview()
		{
			var preview48 = DepthImage.CloneAs<Rgba32>();
			var preview = preview48.Clone(context => context.Resize(150, 150));
			Preview = new ImageSharpImageSource<Rgba32>(preview);
			OnPropertyChanged(nameof(Preview));  
		}

		/// <summary>
		/// Draw the stereogram
		/// </summary>
		public override void Draw()
		{
			if (DepthImage == null)
				return;

			Stereogram stereogram = new Stereogram();

			stereogram.DepthMap = DepthImage;

			if (stereogram.Generate())
			{
				var location = new Point(0, 0);

				if(stereogram.Result != null)
					Target.Mutate(t => t.DrawImage(stereogram.Result, location, 1));
			}

		}
	}
}
