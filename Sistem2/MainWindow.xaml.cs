using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Win32;
using Sistem2.LayerTypes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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
		public Image<Rgba32> Image { get; set; }
		public ImageSharpImageSource<Rgba32> ImageSource { get; set; }

		public MainWindow()
		{
			Image = new Image<Rgba32>(800,600);
			Layers = new LayersViewModel();

			InitializeComponent();
			
			LayersListBox.DataContext = Layers;

			Layers.Add(new BackgroundLayer(Image) { Name = "Background", Visible = true, Color = Color.Black, Order = 0 });

		}

		private void AddImageLayerClick(object sender, RoutedEventArgs e)
		{
			Layers.Add(new ImageLayer(Image) { Name = $"Layer {Layers.Count}", Visible = true});
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
			}
		}

		private void DrawClick(object sender, RoutedEventArgs e)
		{
			Draw();
		}

		private void Draw()
		{
			Layers.Draw();
			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(Image);
		}

	}
}
