using System;
using System.Linq;
using System.Reflection;
using System.Windows;
using OpenStereogramCreator.ViewModels;
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

		private void LayerUpClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var layer = DataContext as LayerBase;
			if (layer == null)
				return;

			var window = Window.GetWindow(this) as MainWindow;
			if (window == null)
				return;

			var layers = window.Layers;
			var index = layers.IndexOf(layer);

			if (index > 0)
			{
				layers.Swap(index, index - 1);
				// update selection
				try
				{
					window.LayersListBox.SelectedIndex = layers.IndexOf(layer);
				}
				catch
				{
					// ignored if access to control is not available
				}

				// call private Draw() on MainWindow to refresh preview
				var drawMethod = window.GetType().GetMethod("Draw", BindingFlags.Instance | BindingFlags.NonPublic);
				drawMethod?.Invoke(window, null);
			}
		}

		private void LayerDownClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var layer = DataContext as LayerBase;
			if (layer == null)
				return;

			var window = Window.GetWindow(this) as MainWindow;
			if (window == null)
				return;

			var layers = window.Layers;
			var index = layers.IndexOf(layer);

			if (index < layers.Count - 1 && index >= 0)
			{
				layers.Swap(index, index + 1);
				try
				{
					window.LayersListBox.SelectedIndex = layers.IndexOf(layer);
				}
				catch { }

				var drawMethod = window.GetType().GetMethod("Draw", BindingFlags.Instance | BindingFlags.NonPublic);
				drawMethod?.Invoke(window, null);
			}
		}

		private void DeleteLayerClick(object sender, System.Windows.RoutedEventArgs e)
		{
			var layer = DataContext as LayerBase;
			if (layer == null)
				return;

			var window = Window.GetWindow(this) as MainWindow;
			if (window == null)
				return;

			// Unsubscribe if possible
			//try
			//{
			//	layer.PropertyChanged -= window.LayerPropertyChanged;
			//}
			//catch { }

			window.Layers.Remove(layer);

			var drawMethod = window.GetType().GetMethod("Draw", BindingFlags.Instance | BindingFlags.NonPublic);
			drawMethod?.Invoke(window, null);
		}
	}
}
