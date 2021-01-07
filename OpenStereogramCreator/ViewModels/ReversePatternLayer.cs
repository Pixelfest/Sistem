using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public class ReversePatternLayer : ImageLayer
	{
		private int _numberOfColumns;
		public int NumberOfColumns
		{
			get => _numberOfColumns;
			set
			{
				_numberOfColumns = value;

				OnPropertyChanged(nameof(NumberOfColumns));
			}
		}

		public override void Render()
		{
			if (Image == null)
				return;

			if (CachedImage != null)
				return;

			CachedImage = ReverseColumns(Image);
		}

		[NotifyPropertyChangedInvocator]
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(NumberOfColumns):
					CachedImage = null;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

		private Image<Rgba32> ReverseColumns(Image<Rgba32> source)
		{
			var result = new Image<Rgba32>(source.Width, source.Height);
			var columnWidth = source.Width / NumberOfColumns;

			void CalculateLine(int i)
			{
				for (var c = 0; c < NumberOfColumns; c++) 
				{
					for (var j = 0; j < columnWidth; j++)
					{
						result[j + (columnWidth * c), (int) i] = source[j + (columnWidth * (NumberOfColumns - 1 - c)), (int) i];
					}
				}
			}

			Parallel.For(0, source.Height, CalculateLine);

			return result;
		}

	}
}
