using Sistem2.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2.ViewModels
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

		/// <summary>
		/// Draw the Background
		/// </summary>
		/// <param name="useOversampling">Use oversampling (unused)</param>
		public override void Draw(bool useOversampling)
		{
			// Clear the image first
			var options = new GraphicsOptions
			{
				AlphaCompositionMode = PixelAlphaCompositionMode.Clear
			};

			Target.Mutate(context => context.Fill(options, new SolidBrush(new Color(new Rgba32(0, 0, 0, 0)))));


			Target.Mutate(image => image.BackgroundColor(Color));
		}

	}
}
