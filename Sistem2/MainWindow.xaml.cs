using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
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
	public partial class MainWindow
	{
		private bool _drawing = false;
		private bool _drawRequested = false;
		private Image<Rgba32> _image;

		public delegate void UpdateImageCallback(Image<Rgba32> image);

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
		public Image<Rgba32> Image { 
			get => _image;
			set
			{
				_image = value;
				PreviewImage.Source = new ImageSharpImageSource<Rgba32>(Image);
			}
		}

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
				Width = 1920,
				Height = 1080,
				Dpi = 100,
				Target = Image
			};

			DocumentLayer.AutoSize += DocumentLayerAutoSize;
			DocumentLayer.PropertyChanged += DocumentPropertyChanged;
			BackgroundLayerProperties.DataContext = DocumentLayer;
		}

		/// <summary>
		/// Event handler for then the AutoSize button is clicked on Document
		/// </summary>
		/// <param name="sender">Event sender</param>
		/// <param name="e">Event arguments</param>
		private void DocumentLayerAutoSize(object sender, System.EventArgs e)
		{
			if (!Layers.Any())
				return;

			var width = Layers.OrderByDescending(item => item.Width).FirstOrDefault()?.Width ?? 1920;
			var height = Layers.OrderByDescending(item => item.Height).FirstOrDefault()?.Height ?? 1080;

			DocumentLayer.Width = width;
			DocumentLayer.Height = height;
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
			if (_drawing)
			{
				_drawRequested = true;
				return;
			}
			
			_drawing = true;
			var thread = new Thread(() =>
			{
				// Clear the image first
				DocumentLayer.Draw();
				Layers.Draw();

				//PreviewImage.Source = new ImageSharpImageSource<Rgba32>(Image);
				PreviewImage.Dispatcher.Invoke(
					new UpdateImageCallback(this.UpdateImage),
					new object[] {Image});

				_drawing = false;

				if (_drawRequested)
				{
					_drawRequested = false;
					Draw();
				}
			});

			thread.Start();
		}

		/// <summary>
		/// Method for updating the image in the UI
		/// </summary>
		/// <param name="image">The image to set</param>
		private void UpdateImage(Image<Rgba32> image)
		{
			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(image);
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

		private new void KeyDownEvent(object sender, KeyEventArgs e)
		{
			if (!(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl)))
				return;

			var layer = LayersListBox.SelectedItem as PatternStereogramLayer;

			if (layer == null)
				return;

			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;
			
			switch (e.Key)
			{
				case Key.Left:
					layer.Origin -= multiplier;
					break;
				case Key.Right:
					layer.Origin += multiplier;
					break;
				case Key.Up:
					layer.PatternYShift += multiplier;
					break;
				case Key.Down:
					layer.PatternYShift -= multiplier;
					break;
				case Key.Q:
					layer.PatternStart -= multiplier;
					layer.MaximumSeparation += multiplier;
					break;
				case Key.W:
					layer.PatternStart += multiplier;
					layer.MaximumSeparation -= multiplier;
					break;
				case Key.O:
					layer.PatternEnd -= multiplier;
					layer.MaximumSeparation -= multiplier;
					break;
				case Key.P:
					layer.PatternEnd += multiplier;
					layer.MaximumSeparation += multiplier;
					break;
				case Key.OemPeriod:
					layer.Zoom += 0.01f * multiplier;
					break;
				case Key.OemComma:
					layer.Zoom -= 0.01f * multiplier;
					break;
			}

			Draw();
		}
	}
}
