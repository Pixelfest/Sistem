using System.Windows.Controls;
using System.Windows.Input;
using Sistem2.ViewModels;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class FullImageStereogramLayerProperties : UserControl
	{
		private FullImageStereogramLayer _fullImageStereogramLayer => DataContext as FullImageStereogramLayer;

		/// <summary>
		/// Constructor
		/// </summary>
		public FullImageStereogramLayerProperties()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handle the event when the mousewheel is used
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void ShiftMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_fullImageStereogramLayer.Shift += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}
	}
}
