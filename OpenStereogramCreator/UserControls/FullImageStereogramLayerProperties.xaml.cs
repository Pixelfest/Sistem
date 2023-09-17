using System.Collections;
using System.Windows;
using System.Windows.Input;
using OpenStereogramCreator.ViewModels;
using Sistem.Core;

namespace OpenStereogramCreator
{
	public partial class FullImageStereogramLayerProperties
	{
		private FullImageStereogramLayer FullImageStereogramLayer => DataContext as FullImageStereogramLayer;

		public FullImageStereogramLayerProperties()
		{
			InitializeComponent();
		}

		private void ShiftMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			FullImageStereogramLayer.Shift += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}

		private void LessButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			FullImageStereogramLayer.Shift--;
		}

		private void MoreButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			FullImageStereogramLayer.Shift++;
		}

		private void GeneratePattern_Click(object sender, RoutedEventArgs e)
		{
			FullImageStereogramLayer.PatternImage = ImageProcessing.GenerateShadows(FullImageStereogramLayer.DepthImage);
		}
	}
}
