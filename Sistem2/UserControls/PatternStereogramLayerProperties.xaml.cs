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
		/// Handle the event to load a pattern image
		/// </summary>
		/// <param name="sender">The event sender</param>
		/// <param name="e">The event arguments</param>
		private void LoadPatternImageButtonClick(object sender, RoutedEventArgs e)
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
					_patternStereogramLayer.PatternImage = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);

					_patternStereogramLayer.PatternImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
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
		private void PatternStartMouseWheel(object sender, MouseWheelEventArgs e)
		{
			int multiplier = 1;

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
			int multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			_patternStereogramLayer.PatternEnd += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
			_patternStereogramLayer.MaximumSeparation = _patternStereogramLayer.PatternEnd - _patternStereogramLayer.PatternStart;
		}
	}
}
