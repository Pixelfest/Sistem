using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using Sistem2.Tools;
using Sistem2.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		/// <summary>
		/// The Layers ViewModel
		/// </summary>
		public LayersViewModel Layers  { get; set; }

		/// <summary>
		/// The Document
		/// </summary>
		public DocumentLayer DocumentLayer { get; set; }

		/// <summary>
		/// The main image
		/// </summary>
		public Image<Rgba32> Image { get; set; }

		//public ImageSharpImageSource<Rgba32> ImageSource { get; set; }

		/// <summary>
		/// Constructor
		/// </summary>
		public MainWindow()
		{
			Layers = new LayersViewModel();

			InitializeComponent();
			
			LayersListBox.DataContext = Layers;
			
			Image = new Image<Rgba32>(800, 600);

			DocumentLayer = new DocumentLayer(Image)
			{
				Name = "Document", 
				Visible = true, 
				BackgroundColor = Color.Black, 
				Width = 800,
				Height = 600,
				Dpi = 100,
				Target = Image
			};
			DocumentLayer.PropertyChanged += DocumentPropertyChanged;
			BackgroundLayerProperties.DataContext = DocumentLayer;
		}

		/// <summary>
		/// Event handler for changes on the document
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void DocumentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(DocumentLayer.Width):
				case nameof(DocumentLayer.Height):
					Image.Mutate(context => context.Resize(DocumentLayer.Width, DocumentLayer.Height));
					break;
				case nameof(DocumentLayer.Dpi):
				{
					foreach (var layer in Layers)
						layer.Dpi = DocumentLayer.Dpi;

					break;
				}
				case nameof(DocumentLayer.Measurements):
				{
					foreach (var layer in Layers)
						layer.Measurements = DocumentLayer.Measurements;

					break;
				}
				case nameof(DocumentLayer.Oversampling):
				{
					foreach (var layer in Layers)
						layer.Oversampling = DocumentLayer.Oversampling;

					break;
				}
			}
		}

		/// <summary>
		/// Add image layer event handler
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void AddImageLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new ImageLayer(Image)
			{
				Name = $"Image Layer {Layers.Count}",
				Dpi = DocumentLayer.Dpi,
				Visible = true
			};

			Layers.Insert(0, layer);
			layer.DrawPreview();
			Layers.Draw();
		}

		/// <summary>
		/// Add random dot layer event handler
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void AddRandomDotStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new RandomDotStereogramLayer(Image)
			{
				Name = $"Random Dot Layer {Layers.Count}", 
				Dpi = DocumentLayer.Dpi,
				Visible = true

			};

			Layers.Insert(0, layer);
			layer.DrawPreview();
			Layers.Draw();
		}

		/// <summary>
		/// Add pattern stereogram layer event handler
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void AddPatternStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new PatternStereogramLayer(Image)
			{
				Name = $"Pattern Layer {Layers.Count}",
				Dpi = DocumentLayer.Dpi,
				Visible = true

			};

			Layers.Insert(0, layer);
			layer.DrawPreview();
			Layers.Draw();
		}

		/// <summary>
		/// Delete a layer
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void DeleteLayerClick(object sender, RoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;

			if (element == null)
				return;

			Layers.Remove(element.DataContext as LayerBase);
			Layers.Draw();
		}

		/// <summary>
		/// Move a layer up
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void LayerUpClick(object sender, RoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;

			if (element == null)
				return;

			var layer = element.DataContext as LayerBase;
			var layerIndex = Layers.IndexOf(layer);

			if(layerIndex > 0)
				Layers.Swap(layerIndex, layerIndex - 1);
		}

		/// <summary>
		/// Move a layer down
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void LayerDownClick(object sender, RoutedEventArgs e)
		{
			var element = e.OriginalSource as FrameworkElement;
			
			if (element == null)
				return;
			
			var layer = element.DataContext as LayerBase;
			var layerIndex = Layers.IndexOf(layer);

			if (layerIndex < Layers.Count - 1)
				Layers.Swap(layerIndex, layerIndex + 1);
		}

		/// <summary>
		/// Select a layer
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
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

		/// <summary>
		/// Draw the image event handler
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void DrawClick(object sender, RoutedEventArgs e)
		{
			Draw();
		}

		/// <summary>
		/// Draw the image
		/// </summary>
		private void Draw()
		{
			// Clear the image first
			DocumentLayer.Draw();
			Layers.Draw();

			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(Image);
		}

		/// <summary>
		/// Reset zoom on zoomborder
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void ResetZoom(object sender, RoutedEventArgs e)
		{
			ZoomBorder.Reset();
		}

		/// <summary>
		/// Set zoomborder 1:1 on screen pixels
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void SetPixelPerfect(object sender, RoutedEventArgs e)
		{
			ZoomBorder.Reset100();
		}

		/// <summary>
		/// Set zoomborder 1:1 on actual size, as far as we know because the actual monitor DPI is unknown
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void ResetActualSize(object sender, RoutedEventArgs e)
		{
			ZoomBorder.SetActualSize((int)DocumentLayer.Dpi, (int)DocumentLayer.WidthInch);
		}
	}
}
