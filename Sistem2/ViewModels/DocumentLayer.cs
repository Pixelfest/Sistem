using System;
using Sistem2.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2.ViewModels
{
	/// <summary>
	/// Document layer, the bottommost layer and document-wide properties
	/// </summary>
	public class DocumentLayer : LayerBase
	{
		public event EventHandler AutoSize;

		public void OnAutoSize()
		{
			var handler = AutoSize;
			if (null != handler)
				handler(this, EventArgs.Empty);
		}

		/// <summary>
		/// Background color
		/// </summary>
		public Color BackgroundColor { get; set; }

		/// <summary>
		/// Background color in text, for input
		/// </summary>
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
		
		/// <summary>
		/// Constructor
		/// </summary>
		/// <param name="target"></param>
		public DocumentLayer(Image<Rgba32> target) : base(target)
		{
		}

		/// <summary>
		/// Draw a preview of the layer
		/// </summary>
		public override void DrawPreview()
		{
			var preview = new Image<Rgba32>(150, 150).Clone(context => context.BackgroundColor(BackgroundColor));
			Preview = new ImageSharpImageSource<Rgba32>(preview);

			OnPropertyChanged(nameof(Preview));  
		}

		/// <summary>
		/// Draw the Background
		/// </summary>
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
