using System;
using System.Runtime.CompilerServices;
using System.Windows;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

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

					OnPropertyChanged(nameof(BackgroundColor));
				}
				catch
				{
					MessageBox.Show("Could not parse background color use rgba Hex codes.");
				}
			}
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
			}

			base.OnPropertyChanged(propertyName);
		}

	}
}
