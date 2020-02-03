using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using Sistem2.LayerTypes;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
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

		private void AddImageLayer_Click(object sender, RoutedEventArgs e)
		{
			Layers.Add(new ImageLayer(Image) { Name = "Bami"});

			Layers.Draw();
		}

		private void LayersListBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var selectedItem = LayersListBox.SelectedItem;

			switch (selectedItem)
			{
				case ImageLayer imageLayer:
					ImageLayerProperties.Visibility = Visibility.Visible;
					ImageLayerProperties.DataContext = imageLayer;
					break;
			}
		}

		private void ImageLayer_LoadImage_OnClick(object sender, RoutedEventArgs e)
		{
			var imageLayer = LayersListBox.SelectedItem as ImageLayer;

			var openFileDialog = new OpenFileDialog
			{
				Title = "Open image",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					imageLayer.Image = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);
					imageLayer.FileName =
						openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}

		private void Draw_OnClick(object sender, RoutedEventArgs e)
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
