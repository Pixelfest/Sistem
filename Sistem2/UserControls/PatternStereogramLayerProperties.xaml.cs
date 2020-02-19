using System.Windows.Controls;
using System.Windows.Input;
using Sistem2.ViewModels;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class PatternStereogramLayerProperties : UserControl
	{
		private PatternStereogramLayer _patternStereogramLayer => DataContext as PatternStereogramLayer;

		/// <summary>
		/// Constructor
		/// </summary>
		public PatternStereogramLayerProperties()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handle the event when the mousewheel is used
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void PatternStartMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_patternStereogramLayer.PatternStart += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
			_patternStereogramLayer.MaximumSeparation = _patternStereogramLayer.PatternEnd - _patternStereogramLayer.PatternStart;
		}

		/// <summary>
		/// Handle the event when the mousewheel is used
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void PatternEndMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_patternStereogramLayer.PatternEnd += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
			_patternStereogramLayer.MaximumSeparation = _patternStereogramLayer.PatternEnd - _patternStereogramLayer.PatternStart;
		}

		/// <summary>
		/// Handle the event when the mousewheel is used
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void OriginMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_patternStereogramLayer.Origin += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}
	}
}
