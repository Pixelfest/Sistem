using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Sistem2.LayerTypes;
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
				Name = "Background", 
				Visible = true, 
				Color = Color.Black, 
				Order = 0,
				Width = 800,
				Height = 600,
				Dpi = 100,
				Target = Image
			};
			BackgroundLayer.PropertyChanged += BackgroundLayerPropertyChanged;


			Layers.Add(BackgroundLayer);
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

		private void AddImageLayerClick(object sender, RoutedEventArgs e)
		{
			Layers.Add(new ImageLayer(Image) { Name = $"Image Layer {Layers.Count}", Visible = true});
			Layers.Draw();
		}

		private void AddSirdsLayerClick(object sender, RoutedEventArgs e)
		{
			Layers.Add(new SirdsLayer(Image) { Name = $"SIRDS Layer {Layers.Count}", Visible = true});
			Layers.Draw();
		}

		private void DeleteLayerClick(object sender, RoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			Layers.Remove(element.DataContext as LayerBase);
			Layers.Draw();
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
				case BackgroundLayer backgroundLayer:
					BackgroundLayerProperties.Visibility = Visibility.Visible;
					BackgroundLayerProperties.DataContext = backgroundLayer;
					break;
				case SirdsLayer sirdsLayer:
					SirdsLayerProperties.Visibility = Visibility.Visible;
					SirdsLayerProperties.DataContext = sirdsLayer;
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
			Image.Mutate(context => context.Fill(new SolidBrush(new Color(new Rgba32(0,0,0,0)))));

			Layers.Draw();
			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(Image);
		}
	}
}
