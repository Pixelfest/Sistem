using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Sistem2.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class StereogramLayerProperties : UserControl
	{
		private StereogramLayer _stereogramLayer => DataContext as StereogramLayer;

		/// <summary>
		/// Constructor
		/// </summary>
		public StereogramLayerProperties()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Handle the event to load a pattern image
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void LoadDepthImageButtonClick(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = "Open image",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					var image = SixLabors.ImageSharp.Image.Load<Rgb48>(openFileDialog.FileName);

					_stereogramLayer.DepthImage = image;
					_stereogramLayer.DepthImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// Handle the event when the mousewheel is used
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void MimimumSeparationMouseWheel(object sender, MouseWheelEventArgs e)
		{
			int multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_stereogramLayer.MinimumSeparation += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}

		/// <summary>
		/// Handle the event when the mousewheel is used
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void MaximumSeparationMouseWheel(object sender, MouseWheelEventArgs e)
		{
			int multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_stereogramLayer.MaximumSeparation += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}

		/// <summary>
		/// Handle the event when the mousewheel is used
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void OriginMouseWheel(object sender, MouseWheelEventArgs e)
		{
			int multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_stereogramLayer.Origin += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}
	}
}
