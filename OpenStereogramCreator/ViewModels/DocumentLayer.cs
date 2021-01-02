using System;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Processing.Processors.Drawing;

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
					BackgroundColor = Color.ParseHex(value);

					DrawPreview();

					OnPropertyChanged(nameof(BackgroundColor));
				}
				catch {}
			}
		}
		
		public override void DrawPreview()
		{
			var preview = new Image<Rgba32>(150, 150).Clone(context => context.BackgroundColor(BackgroundColor));
			Preview = new ImageSharpImageSource<Rgba32>(preview);

			OnPropertyChanged(nameof(Preview));  
		}

		public override void Render()
		{
			if (CachedImage != null)
				return;

			var result = new Image<Rgba32>(Width, Height);
			
            // Clear the image with the background color
			result.Clear(BackgroundColor);

			CachedImage = result;
		}

		[NotifyPropertyChangedInvocator]
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(Width):
				case nameof(Height):
				case nameof(BackgroundColor):
					CachedImage = null;
					break;
				default:
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

	}
}
