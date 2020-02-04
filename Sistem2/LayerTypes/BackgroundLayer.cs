using System;
using System.Collections.Generic;
using System.Text;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2.LayerTypes
{
	public class BackgroundLayer : LayerBase
	{
		public Color Color { get; set; }

		public string ColorText {
			get => Color.ToHex();
			set
			{
				try
				{
					Color = Color.FromHex(value);

					DrawPreview();

					OnPropertyChanged(nameof(Color));
				}
				catch {}
			}
		}

		public new bool IsDeleteEnabled => false;

		public BackgroundLayer(Image<Rgba32> target) : base(target)
		{
		}

		public override void DrawPreview()
		{
			var preview = new Image<Rgba32>(150, 150).Clone(context => context.BackgroundColor(Color));
			Preview = new ImageSharpImageSource<Rgba32>(preview);
			OnPropertyChanged(nameof(Preview));  
		}

		public override void Draw()
		{
			Target.Mutate(image => image.BackgroundColor(Color));
		}

	}
}
