using System.Runtime.CompilerServices;
using System.Windows;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using static OpenStereogramCreator.ViewModels.LayerBase;

namespace OpenStereogramCreator.ViewModels
{
	public class DocumentLayer : LayerBase
	{
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

			if (Width == 0 || Height == 0)
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

        public new DocumentLayerDto Export()
        {
            var export = base.Export<DocumentLayerDto>();

            export.BackgroundColorText = BackgroundColorText;

            return export;
        }

        public static DocumentLayer Import(DocumentLayerDto dto)
        {
            var target = new DocumentLayer();

            var targetNew = Import<DocumentLayerDto, DocumentLayer>(dto);

            targetNew.BackgroundColorText = dto.BackgroundColorText;

            return targetNew;
        }
    }
}
