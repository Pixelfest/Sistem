using System;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator.ViewModels
{
	public class DocumentLayer : LayerBase
	{
		public event EventHandler AutoSize;

		public void OnAutoSize()
		{
			AutoSize?.Invoke(this, EventArgs.Empty);
		}

		public Color BackgroundColor { get; set; }

		public string BackgroundColorText {
			get => BackgroundColor.ToHex();
			set
			{
				try
				{
					BackgroundColor = Color.FromHex(value);

					DrawPreview();

					OnPropertyChanged(nameof(BackgroundColor));
				}
				catch {}
			}
		}
		
		public DocumentLayer(Image<Rgba32> target) : base(target)
		{
		}

		public override void DrawPreview()
		{
			var preview = new Image<Rgba32>(150, 150).Clone(context => context.BackgroundColor(BackgroundColor));
			Preview = new ImageSharpImageSource<Rgba32>(preview);

			OnPropertyChanged(nameof(Preview));  
		}

		public override void Draw()
		{
			// Clear the image first
			var options = new GraphicsOptions
			{
				AlphaCompositionMode = PixelAlphaCompositionMode.Clear
			};

			Target.Mutate(context => context.Fill(options, new SolidBrush(new Color(new Rgba32(0, 0, 0, 0)))));


			Target.Mutate(image => image.BackgroundColor(BackgroundColor));
		}
	}
}
