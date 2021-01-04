using System.Windows;
using System.Windows.Controls;
using OpenStereogramCreator.Tools;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
{
    public partial class MainWindow
    {
		// Layer events

		private void AddImageLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new ImageLayer
			{
				Name = $"Image Layer {Layers.Count}",
				Dpi = Layers.Document.Dpi,
				Visible = true
			};

			layer.PropertyChanged += LayerPropertyChanged;

			Layers.Insert(0, layer);
		}

		private void AddRandomDotStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new RandomDotStereogramLayer
			{
				Name = $"Random Dot Layer {Layers.Count}",
				Dpi = Layers.Document.Dpi,
				Visible = true
			};

			layer.PropertyChanged += LayerPropertyChanged;

			Layers.Insert(0, layer);
		}

		private void AddPatternStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new PatternStereogramLayer
			{
				Name = $"Pattern Layer {Layers.Count}",
				Dpi = Layers.Document.Dpi,
				Visible = true
			};

			layer.PropertyChanged += LayerPropertyChanged;

			Layers.Insert(0, layer);
		}

		private void AddFullImageStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new FullImageStereogramLayer
			{
				Name = $"Full Image Layer {Layers.Count}",
				Dpi = Layers.Document.Dpi,
				Origin = 0,
				Visible = true
			};

			layer.PropertyChanged += LayerPropertyChanged;

			Layers.Insert(0, layer);
		}

		private void DeleteLayerClick(object sender, RoutedEventArgs e)
		{
			var layer = LayersListBox.SelectedItem as LayerBase;

			if (layer == null)
				return;

			layer.PropertyChanged -= LayerPropertyChanged;

			Layers.Remove(layer);

			Draw();
		}

		private void LayerUpClick(object sender, RoutedEventArgs e)
		{
			var layer = LayersListBox.SelectedItem as LayerBase;

			if (layer == null)
				return;

			var layerIndex = Layers.IndexOf(layer);

			if (layerIndex > 0)
				Layers.Swap(layerIndex, layerIndex - 1);

			LayersListBox.SelectedIndex = Layers.IndexOf(layer);

			Draw();
		}

		private void LayerDownClick(object sender, RoutedEventArgs e)
		{
			var layer = LayersListBox.SelectedItem as LayerBase;

			if (layer == null)
				return;

			var layerIndex = Layers.IndexOf(layer);

			if (layerIndex < Layers.Count - 1)
				Layers.Swap(layerIndex, layerIndex + 1);
			
			LayersListBox.SelectedIndex = Layers.IndexOf(layer);

			Draw();
		}

		private void LayersListBoxSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedItem = LayersListBox.SelectedItem;

			foreach (UIElement child in LayersStackPanel.Children)
			{
				child.Visibility = Visibility.Collapsed;
			}

			switch (selectedItem)
			{
				case ImageLayer imageLayer:
					ImageLayerProperties.Visibility = Visibility.Visible;
					ImageLayerProperties.DataContext = imageLayer;
					break;
				case RandomDotStereogramLayer randomDotStereogramLayer:
					RandomDotStereogramLayerProperties.Visibility = Visibility.Visible;
					RandomDotStereogramLayerProperties.DataContext = randomDotStereogramLayer;
					break;
				case FullImageStereogramLayer fullImageStereogramLayer:
					FullImageStereogramLayerProperties.Visibility = Visibility.Visible;
					FullImageStereogramLayerProperties.DataContext = fullImageStereogramLayer;
					break;
				case PatternStereogramLayer patternStereogramLayer:
					PatternStereogramLayerProperties.Visibility = Visibility.Visible;
					PatternStereogramLayerProperties.DataContext = patternStereogramLayer;
					break;
			}
		}

	}
}
