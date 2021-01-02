using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator
{
	public partial class StereogramLayerProperties
	{
		private StereogramLayer StereogramLayer => DataContext as StereogramLayer;

		public StereogramLayerProperties()
		{
			InitializeComponent();
		}

		private void MinimumSeparationMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			StereogramLayer.MinimumSeparation += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}

		private void MaximumSeparationMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			StereogramLayer.MaximumSeparation += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}
	}
}
