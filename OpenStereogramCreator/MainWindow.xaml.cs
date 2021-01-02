using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using OpenStereogramCreator.Tools;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = SixLabors.ImageSharp.Color;

namespace OpenStereogramCreator
{
	public partial class MainWindow
	{
		private bool _drawing = false;
		private bool _drawRequested = false;

		public delegate void UpdateImageCallback(Image<Rgba32> image);

		public LayersViewModel Layers  { get; set; }
		public DocumentLayer DocumentLayer { get; set; }

        private Image<Rgba32> _image;
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
			
			Image = new Image<Rgba32>(1920, 1080);

			DocumentLayer = new DocumentLayer
			{
				Name = "Document", 
				Visible = true, 
				BackgroundColor = Color.Black, 
				Width = 1920,
				Height = 1080,
				Dpi = 100,
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

			Draw();
		}
		
		private void LayerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Draw();
		}

		private void UpdateImage(Image<Rgba32> image)
		{
			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(image);
		}

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var saveFileDialog = new SaveFileDialog
            {
                Title = "Save image",
                Filter = "Image File|*.png"
            };

            if (saveFileDialog.ShowDialog() == true)
            {
                try
                {
                    using (var stream = new FileStream(saveFileDialog.FileName, FileMode.Create, FileAccess.ReadWrite))
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

		// Fine tuning

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
