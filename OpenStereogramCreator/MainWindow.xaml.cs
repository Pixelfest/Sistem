using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using OpenStereogramCreator.Tools;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;
using Image = System.Windows.Controls.Image;

namespace OpenStereogramCreator
{
	public partial class MainWindow
	{
		private bool _drawing = false;
		private bool _drawRequested = false;
		private Image<Rgba32> _image;

		public delegate void UpdateImageCallback(Image<Rgba32> image);

		public LayersViewModel Layers  { get; set; }
		public DocumentLayer DocumentLayer { get; set; }
		public Image<Rgba32> Image { 
			get => _image;
			set
			{
				_image = value;
				PreviewImage.Source = new ImageSharpImageSource<Rgb24>(Image.CloneAs<Rgb24>());
			}
		}

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

		private void DocumentLayerAutoSize(object sender, System.EventArgs e)
		{
			if (!Layers.Any())
				return;

			var width = Layers.OrderByDescending(item => item.Width).FirstOrDefault()?.Width ?? 1920;
			var height = Layers.OrderByDescending(item => item.Height).FirstOrDefault()?.Height ?? 1080;

			DocumentLayer.Width = width;
			DocumentLayer.Height = height;
		}

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
				case nameof(DocumentLayer.MeasurementsTabIndex):
				{
					foreach (var layer in Layers)
						layer.MeasurementsTabIndex = DocumentLayer.MeasurementsTabIndex;

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

		private void AddFullImageStereogramLayerMenuClick(object sender, RoutedEventArgs e)
		{
			var layer = new FullImageStereogramLayer(Image)
			{
				Name = $"Full Image Layer {Layers.Count}",
				Dpi = DocumentLayer.Dpi,
				Origin = 0,
				Visible = true

			};

			Layers.Insert(0, layer);
			layer.DrawPreview();
			Layers.Draw();
		}

		// Layer events

		private void DeleteLayerClick(object sender, RoutedEventArgs e)
		{
			var layer = LayersListBox.SelectedItem as LayerBase;

			if(layer == null)
				return;

			Layers.Remove(layer);
			Draw();
		}

		private void LayerUpClick(object sender, RoutedEventArgs e)
		{
			var layer = LayersListBox.SelectedItem as LayerBase;

			if (layer == null)
				return;

			var layerIndex = Layers.IndexOf(layer);

			if(layerIndex > 0)
				Layers.Swap(layerIndex, layerIndex - 1);
		}

		private void LayerDownClick(object sender, RoutedEventArgs e)
		{
			var layer = LayersListBox.SelectedItem as LayerBase;

			if (layer == null)
				return;

			var layerIndex = Layers.IndexOf(layer);

			if (layerIndex < Layers.Count - 1)
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

		// Drawing

		private void DrawClick(object sender, RoutedEventArgs e)
		{
			Draw();
		}

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

		private void UpdateImage(Image<Rgba32> image)
		{
			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(image);
		}

		// Zoom buttons

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
			ZoomBorder.SetActualSize((int)DocumentLayer.Dpi, (int)DocumentLayer.WidthInch);
		}

		// Fumbling

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

		private void SaveButtonClick(object sender, RoutedEventArgs e)
		{
			var saveFileDialog = new SaveFileDialog
			{
				Title = "Open image",
				Filter = "Image File|*.png"
			};

			if (saveFileDialog.ShowDialog() == true)
			{
				try
				{
					using(var stream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite))
					{ 
						Image.SaveAsPng(stream);
					}
				}
				catch
				{
					MessageBox.Show("Something went wrong");
				}
			}
		}
	}
}
