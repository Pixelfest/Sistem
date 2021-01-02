using System.Windows.Controls;
using System.Windows.Input;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
{
	public partial class PatternStereogramLayerProperties
	{
		private PatternStereogramLayer PatternStereogramLayer => DataContext as PatternStereogramLayer;

		public PatternStereogramLayerProperties()
		{
			InitializeComponent();
		}

		private void PatternStartMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			PatternStereogramLayer.PatternStart += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
			PatternStereogramLayer.MaximumSeparation = PatternStereogramLayer.PatternEnd - PatternStereogramLayer.PatternStart;
		}

		private void PatternEndMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			PatternStereogramLayer.PatternEnd += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
			PatternStereogramLayer.MaximumSeparation = PatternStereogramLayer.PatternEnd - PatternStereogramLayer.PatternStart;
		}

		private void OriginMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			PatternStereogramLayer.Origin += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}
	}
}
