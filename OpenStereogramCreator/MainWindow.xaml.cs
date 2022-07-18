using System.ComponentModel;
using System.Linq;
using System.Windows.Input;
using OpenStereogramCreator.Tools;
using OpenStereogramCreator.ViewModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator
{
	public partial class MainWindow
	{
		private bool _drawing;
		private bool _drawRequested;

		public delegate void UpdateImageCallback(Image<Rgba32> image);

		public LayersViewModel Layers  { get; set; }

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

			Layers.Document.PropertyChanged += DocumentPropertyChanged;
			BackgroundLayerProperties.DataContext = Layers.Document;
		}

		private void DocumentPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			switch (e.PropertyName)
			{
				case nameof(DocumentLayer.Width):
				case nameof(DocumentLayer.Height):
					if(Layers.Document.Width != 0 && Layers.Document.Height != 0)
						Image.Mutate(context => context.Resize(Layers.Document.Width, Layers.Document.Height));
					break;
				case nameof(DocumentLayer.MeasurementsTabIndex):
				{
					foreach (var layer in Layers)
						layer.MeasurementsTabIndex = Layers.Document.MeasurementsTabIndex;

					break;
				}
				case nameof(DocumentLayer.Oversampling):
				{
					foreach (var layer in Layers)
						layer.Oversampling = Layers.Document.Oversampling;

					break;
				}
			}

			Draw();
		}
		
		private void LayerPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			DocumentAutoSize();

			Draw();
		}

		private void UpdateImage(Image<Rgba32> image)
		{
			PreviewImage.Source = new ImageSharpImageSource<Rgba32>(image);
		}

		// Fine tuning

        private new void KeyDownEvent(object sender, KeyEventArgs e)
		{
			if (!(Keyboard.IsKeyDown(Key.LeftCtrl) && !Keyboard.IsKeyDown(Key.RightCtrl)))
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

        private void DocumentAutoSize()
        {
	        if (!Layers.Any())
		        return;

	        var width = Layers.Select(item => item.Width + item.Left).Max();
	        var height = Layers.Select(item => item.Height + item.Top).Max();

	        if (Layers.Document.Width != width)
		        Layers.Document.Width = width;

	        if (Layers.Document.Height != height)
		        Layers.Document.Height = height;
        }
	}
}
