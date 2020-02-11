using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using Microsoft.Win32;
using Sistem2.Tools;
using Sistem2.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using Image = System.Windows.Controls.Image;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow : Window
	{
		private bool useOversampling => UseOversampling?.IsChecked ?? false;

		public LayersViewModel Layers  { get; set; }
		public BackgroundLayer BackgroundLayer { get; set; }

		public Image<Rgba32> Image { get; set; }
		public ImageSharpImageSource<Rgba32> ImageSource { get; set; }

		public MainWindow()
		{
			Layers = new LayersViewModel();

			InitializeComponent();
			
			LayersListBox.DataContext = Layers;
			
			Image = new Image<Rgba32>(800, 600);

			BackgroundLayer = new BackgroundLayer(Image)
			{
				Name = "Document", 
				Visible = true, 
				Color = Color.Black, 
				Order = 0,
				Width = 800,
				Height = 600,
				Dpi = 100,
				Target = Image
			};
			BackgroundLayer.PropertyChanged += BackgroundLayerPropertyChanged;
			BackgroundLayerProperties.DataContext = BackgroundLayer;
		}

		private void BackgroundLayerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(BackgroundLayer.Width):
				case nameof(BackgroundLayer.Height):
					Image.Mutate(context => context.Resize(BackgroundLayer.Width, BackgroundLayer.Height));
					break;
				case nameof(BackgroundLayer.Dpi):
				{
					foreach (var layer in Layers.Where(layer => !(layer is BackgroundLayer)))
						layer.Dpi = BackgroundLayer.Dpi;

					break;
				}
				case nameof(BackgroundLayer.Measurements):
				{
					var layers = Layers.Where(layer => !(layer is BackgroundLayer));

					foreach (var layer in layers)
						layer.Measurements = BackgroundLayer.Measurements;

					break;
				}
			}
		}

		private void AddImageLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new ImageLayer(Image)
			{
				Name = $"Image Layer {Layers.Count}",
				Dpi = BackgroundLayer.Dpi,
				Visible = true
			};

			Layers.Insert(0, layer);
			layer.DrawPreview();
			Layers.Draw(useOversampling);
		}

		private void AddRandomDotStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new RandomDotStereogramLayer(Image)
			{
				Name = $"Random Dot Layer {Layers.Count}", 
				Dpi = BackgroundLayer.Dpi,
				Visible = true

			};

			Layers.Insert(0, layer);
			layer.DrawPreview();
			Layers.Draw(useOversampling);
		}

		private void AddPatternStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new PatternStereogramLayer(Image)
			{
				Name = $"Pattern Layer {Layers.Count}",
				Dpi = BackgroundLayer.Dpi,
				Visible = true

			};

			Layers.Insert(0, layer);
			layer.DrawPreview();
			Layers.Draw(useOversampling);
		}

		private void DeleteLayerClick(object sender, RoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			Layers.Remove(element.DataContext as LayerBase);
			Layers.Draw(useOversampling);
		}

		private void LayerUpClick(object sender, RoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			var layer = element.DataContext as LayerBase;

			var layerIndex = Layers.IndexOf(layer);

			if(layerIndex > 0)
				Layers.Swap(layerIndex, layerIndex - 1);
		}

		private void LayerDownClick(object sender, RoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			var layer = element.DataContext as LayerBase;

			var layerIndex = Layers.IndexOf(layer);

			if (layerIndex < Layers.Count - 2)
				Layers.Swap(layerIndex, layerIndex + 1);
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
				case PatternStereogramLayer patternStereogramLayer:
					PatternStereogramLayerProperties.Visibility = Visibility.Visible;
					PatternStereogramLayerProperties.DataContext = patternStereogramLayer;
					break;
			}
		}

		private void DrawClick(object sender, RoutedEventArgs e)
		{
			Draw();
		}

		private void Draw()
		{
			// Clear the image first
			BackgroundLayer.Draw(useOversampling);
			Layers.Draw(useOversampling);

			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(Image);
		}

		private void ResetZoom(object sender, RoutedEventArgs e)
		{
			ZoomBorder.Reset();
		}

		private void SetPixelPerfect(object sender, RoutedEventArgs e)
		{
			ZoomBorder.Reset100();
		}

		private void ResetActualSize(object sender, RoutedEventArgs e)
		{
			ZoomBorder.SetActualSize((int)BackgroundLayer.Dpi, (int)BackgroundLayer.WidthInch);
		}
	}
}
