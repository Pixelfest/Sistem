using System;
using System.Linq;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator
{
	public partial class BaseLayerProperties
	{
		public BaseLayerProperties()
		{
			InitializeComponent();

			BlendingModeComboBox.ItemsSource = Enum.GetValues(typeof(PixelColorBlendingMode)).Cast<PixelColorBlendingMode>();
		}
	}
}
