using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Win32;
using SixLabors.ImageSharp.PixelFormats;
using Brushes = System.Windows.Media.Brushes;
using Image = System.Windows.Controls.Image;
using Point = System.Windows.Point;
using Size = System.Windows.Size;

namespace Sistem
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// </summary>
	public partial class MainWindow
	{
		// Layer names
		private const string StereogramLayerName = "Stereogram";
		private const string CopyrightLayerName = "Copyright";
		private const string IndicatorLeftLayerName = "IndicatorLeft";
		private const string IndicatorRightLayerName = "IndicatorRight";
		private const double FloatingPointTolerance = 0.01;

		/// <summary>
		/// The main Stereogram
		/// </summary>
		public StereogramWrapper StereogramWrapper { get; }
		
		/// <summary>
		/// The layers
		/// </summary>
		public Layers LayersData { get; set;}

		/// <summary>
		/// The image the stereogram will be placed in
		/// </summary>
		private readonly Image _image;
		
		private string _copyrightMessage = $"© Pixelfest {DateTime.Now.Year}";

		private readonly DispatcherTimer _autoUpdateTimer = new DispatcherTimer();
		private readonly string[]  _updateTextureProperties;
		private bool _fitWindow;
		
		private Point _scrollMousePoint;
		private double _horizontalOffset = 1;
		private double _verticalOffset = 1;

		protected double IndicatorHeight => HelpersCheckBox?.IsChecked == true ? LayersData.Get<Image>(IndicatorLeftLayerName)?.Height ?? 0 : 0;
		protected double CopyrightHeight => LayersData.Get<TextBlock>(CopyrightLayerName)?.Height ?? 0;
		protected double DepthMapHeight => StereogramWrapper.DepthMap?.Height ?? 0;
		
		private static double DefaultMargin => 4;
		
		public MainWindow()
		{
			_updateTextureProperties = new[]
			{
				nameof(StereogramWrapper.Texture),
				nameof(StereogramWrapper.TextureWidth),
				nameof(StereogramWrapper.MaxSeparation),
				nameof(StereogramWrapper.MinSeparation),
			};

			StereogramWrapper = new StereogramWrapper();
			LayersData = new Layers();

			InitializeComponent();

			StereogramWrapper.PropertyChanged += Stereogram_PropertyChanged;
			DataContext = StereogramWrapper;

			LayersListView.DataContext = LayersData;

			_image = new Image { Name = StereogramLayerName };
			MainCanvas.Children.Add(_image);
			LayersData.Add(new Layer { Name = StereogramLayerName, Element = _image });

			// We need these layers to be built as well
			SetIndicators();
			SetCopyrightMessage();

			var paddingZoomValue = ZoomValueLabel.Padding;
			paddingZoomValue.Right = 0;
			ZoomValueLabel.Padding = paddingZoomValue;
			var paddingZoomValuePercentage = ZoomValuePercentageLabel.Padding;
			paddingZoomValuePercentage.Left = 0;
			ZoomValuePercentageLabel.Padding = paddingZoomValuePercentage;

			UpdateValidationMessages();
		}

		#region Events

		private void Stereogram_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
		{
			Dispatcher.Invoke(() =>
			{
				if (_updateTextureProperties.Contains(e.PropertyName))
				{
					SetTextureMask();
				}

				if (e.PropertyName == nameof(StereogramWrapper.StereogramType))
				{
					switch (StereogramWrapper.StereogramType)
					{
						case StereogramType.FastRandomDot:
							TypeQuickRandomDot.IsChecked = true;
							break;
						case StereogramType.RandomDotTextured:
							TypeRandomDot.IsChecked = true;
							break;
						case StereogramType.Textured:
							TypeTextured.IsChecked = true;
							break;
					}
				}
			});
		}

		private void LoadDepthMap_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = "Open depth image",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				StereogramWrapper.DepthMap = SixLabors.ImageSharp.Image.Load<Rgb48>(openFileDialog.FileName);
				
				var depthMap = (Bitmap)LoadImage(openFileDialog.FileName);
				DepthMapImage.Source = BitmapToImageSource(depthMap);
			}
		}

		private void LoadPattern_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = "Open texture image",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				StereogramWrapper.Texture = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);
				
				var pattern = (Bitmap)LoadImage(openFileDialog.FileName);
				StereogramWrapper.StereogramType = StereogramType.Textured;

				TextureImage.Source = BitmapToImageSource(pattern);
			}
		}

		private void SaveButton_Click(object sender, RoutedEventArgs e)
		{
			if (StereogramWrapper.Result == null)
				return;

			var saveFileDialog = new SaveFileDialog
			{
				Title = "Save result",
				Filter = "PNG Image|*.png;"
			};

			if (saveFileDialog.ShowDialog() == true)
				ExportToPng(saveFileDialog.FileName, MainCanvas);
		}

		private void LeftHelperButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = "Open left helper image - Image should be square",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				var bitmap = (Bitmap)LoadImage(openFileDialog.FileName);
				var imageSourceLeft = BitmapToImageSource(bitmap);
				LeftHelperButton.Content = new Image { Source = imageSourceLeft, Width = 20, Height = 20 };
			}
		}

		private void RightHelperButton_Click(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = "Open right helper image - Image should be square",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				var bitmap = (Bitmap)LoadImage(openFileDialog.FileName);
				var imageSourceRight = BitmapToImageSource(bitmap);
				RightHelperButton.Content = new Image { Source = imageSourceRight, Width = 20, Height = 20 };
			}
		}

		private void Generate_Click(object sender, RoutedEventArgs e)
		{
			Generate();

			if (_fitWindow)
				FitWindow();
		}

		private void TextBox_OnlyAllowNumbers(object sender, TextCompositionEventArgs e)
		{
			if (!(sender is TextBox textBox))
				return;

			var text = textBox.Text.Insert(textBox.SelectionStart, e.Text);

			// If parsing is successful, set Handled to false
			e.Handled = !int.TryParse(text, out _);
		}

		private void ZoomSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
		{
			_fitWindow = false;

			Zoom(ZoomSlider.Value / 100);
		}

		private void ZoomMatchWindow_Click(object sender, RoutedEventArgs e)
		{
			_fitWindow = true;
			FitWindow();
		}

		private void TypeQuickRandomDot_Checked(object sender, RoutedEventArgs e)
		{
			StereogramWrapper.StereogramType = StereogramType.FastRandomDot;
		}

		private void TypeRandomDot_Checked(object sender, RoutedEventArgs e)
		{
			StereogramWrapper.StereogramType = StereogramType.RandomDotTextured;
		}

		private void TypeTextured_Checked(object sender, RoutedEventArgs e)
		{
			StereogramWrapper.StereogramType = StereogramType.Textured;
		}

		private void ParallelViewType_Checked(object sender, RoutedEventArgs e)
		{
			StereogramWrapper.ViewType = ViewType.Parallel;
		}

		private void CrossViewType_Checked(object sender, RoutedEventArgs e)
		{
			StereogramWrapper.ViewType = ViewType.CrossView;
		}
		
		private void ResetHelperButton_Click(object sender, RoutedEventArgs e)
		{
			LeftHelperButton.Content = "\uf03e";
			RightHelperButton.Content = "\uf03e";
		}

		private void AutoUpdateButton_Click(object sender, RoutedEventArgs e)
		{
			if (_autoUpdateTimer?.IsEnabled == true)
			{
				_autoUpdateTimer.Stop();
				AutoUpdateButton.Background = null;
			}
			else
			{
				_autoUpdateTimer?.Start();
				AutoUpdateButton.Background = Brushes.PaleVioletRed;
			}
		}

		private void ZoomOutButton_Click(object sender, RoutedEventArgs e)
		{
			_fitWindow = false;

			if(Math.Abs(ZoomSlider.Value - ZoomSlider.Minimum) > FloatingPointTolerance)
			{
				var closestValue = Math.Round(ZoomSlider.Value / 25) * 25;
				ZoomSlider.Value = closestValue - ZoomSlider.SmallChange;
			}
		}

		private void ZoomInButton_Click(object sender, RoutedEventArgs e)
		{
			_fitWindow = false;

			if(Math.Abs(ZoomSlider.Value - ZoomSlider.Maximum) > FloatingPointTolerance)
			{
				var closestValue = Math.Round(ZoomSlider.Value / 25) * 25;
				ZoomSlider.Value = closestValue + ZoomSlider.SmallChange;
			}
		}
		
		private void Window_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if(_fitWindow)
				FitWindow();
		}
		private void ScrollViewer_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			object original = e.OriginalSource;

			if (original.GetType() != typeof(ScrollViewer))
			{
				_scrollMousePoint = e.GetPosition(ScrollViewer);
				_horizontalOffset = ScrollViewer.HorizontalOffset;
				_verticalOffset = ScrollViewer.VerticalOffset;

				ScrollViewer.CaptureMouse();
			}
		}

		private void ScrollViewer_PreviewMouseMove(object sender, MouseEventArgs e)
		{
			if(ScrollViewer.IsMouseCaptured)
			{
				ScrollViewer.ScrollToHorizontalOffset(_horizontalOffset + (_scrollMousePoint.X - e.GetPosition(ScrollViewer).X));
				ScrollViewer.ScrollToVerticalOffset(_verticalOffset + (_scrollMousePoint.Y - e.GetPosition(ScrollViewer).Y));
			}
		}

		private void ScrollViewer_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			ScrollViewer.ReleaseMouseCapture();
		}

		private void ScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
			{
				if (e.Delta > 0 && Math.Abs(ZoomSlider.Value - ZoomSlider.Maximum) > FloatingPointTolerance)
					ZoomSlider.Value = ZoomSlider.Value + ZoomSlider.SmallChange;
				else if(Math.Abs(ZoomSlider.Value - ZoomSlider.Minimum) > FloatingPointTolerance)
					ZoomSlider.Value = ZoomSlider.Value - ZoomSlider.SmallChange;
			}
		}

		private void AboutButton_Click(object sender, RoutedEventArgs e)
		{
			About about = new About();

			about.ShowDialog();
		}
		
		private void CopyrightMessageButton_Click(object sender, RoutedEventArgs e)
		{
			CopyrightMessage copyrightMessage = new CopyrightMessage(_copyrightMessage);

			if (copyrightMessage.ShowDialog() == true)
				_copyrightMessage = copyrightMessage.Message;
		}

		private void AddLayerButton_Click(object sender, RoutedEventArgs e)
		{
			LayersData.Add(new Layer { Name = Guid.NewGuid().ToString() });
		}

		#endregion

		#region Helper methods
		
		private void Generate()
		{
			Task.Run(() =>
			{
				var success = StereogramWrapper.Generate();

				SetResult(success);
			});
		}

		private void Zoom(double scale)
		{
			if (MainCanvas == null)
				return;

			var scaleTransform = new ScaleTransform
			{
				ScaleX = scale,
				ScaleY = scale,
				CenterX = 0,
				CenterY = 0
			};

			if ((int)(scaleTransform.ScaleX * 100) == 100)
				RenderOptions.SetBitmapScalingMode(MainCanvas, BitmapScalingMode.NearestNeighbor);
			else
				RenderOptions.SetBitmapScalingMode(MainCanvas, BitmapScalingMode.HighQuality);

			MainCanvas.LayoutTransform = scaleTransform;
		}

		private static void ExportToPng(string path, FrameworkElement canvas)
		{
			if (path == null) return;

			// Save current canvas transform
			var transform = canvas.LayoutTransform;
			// reset current transform (in case it is scaled or rotated)
			canvas.LayoutTransform = null;

			// Get the size of canvas
			var size = new Size(canvas.ActualWidth, canvas.ActualHeight);
			// Measure and arrange the canvas
			// VERY IMPORTANT
			canvas.Measure(size);
			canvas.Arrange(new Rect(size));

			// Create a render bitmap and push the canvas to it
			var renderBitmap = new RenderTargetBitmap((int)size.Width, (int)size.Height, 96d, 96d, PixelFormats.Pbgra32);
			renderBitmap.Render(canvas);

			// Create a file stream for saving image
			using (var outStream = new FileStream(path, FileMode.Create))
			{
				// Use png encoder for our data
				var encoder = new PngBitmapEncoder();
				// push the rendered bitmap to it
				encoder.Frames.Add(BitmapFrame.Create(renderBitmap));
				// save the data to the stream
				encoder.Save(outStream);
			}

			// Restore previously saved layout
			canvas.LayoutTransform = transform;
		}

		private static BitmapImage BitmapToImageSource(System.Drawing.Image bitmap)
		{
			using (var memory = new MemoryStream())
			{
				bitmap.Save(memory, ImageFormat.Png);
				memory.Position = 0;
				var bitmapImage = new BitmapImage();
				bitmapImage.BeginInit();
				bitmapImage.StreamSource = memory;
				bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
				bitmapImage.EndInit();

				return bitmapImage;
			}
		}

		private void SetResult(bool updated)
		{
			Dispatcher.Invoke(() =>
			{
				if (updated)
				{
					MainCanvas.Width = StereogramWrapper.Result.Width;

					SetIndicators();
					SetCopyrightMessage();
					
					_image.Width = StereogramWrapper.Result.Width;
					_image.Height = StereogramWrapper.Result.Height;
					_image.Source = BitmapToImageSource(StereogramWrapper.Result);

					var margin = IndicatorHeight > 0 ? IndicatorHeight + DefaultMargin * 2 : 0;
					_image.Margin = new Thickness(0,margin,0,0);

					SetMainCanvasHeight();
				}

				UpdateValidationMessages();
			});
		}

		private void SetCopyrightMessage()
		{
			if(string.IsNullOrWhiteSpace(_copyrightMessage) || StereogramWrapper.DepthMap == null)
				return;
			
			var elementCopyright = LayersData.Get<TextBlock>(CopyrightLayerName);

			if (elementCopyright == null)
			{
				elementCopyright = new TextBlock { Name = CopyrightLayerName };
				RenderOptions.SetBitmapScalingMode(elementCopyright, BitmapScalingMode.HighQuality);
				MainCanvas.Children.Add(elementCopyright);
				LayersData.Add(new Layer { Name = CopyrightLayerName, Element = elementCopyright });
			}

			// Margin * 1 or 3 when there are indicators
			var margin = Math.Abs(IndicatorHeight) < FloatingPointTolerance ? 0 : DefaultMargin * 2;

			elementCopyright.Text = _copyrightMessage;
			elementCopyright.FontSize = DepthMapHeight / 50;
			elementCopyright.Height = DepthMapHeight / 45 + DefaultMargin;
			elementCopyright.Margin = new Thickness(DefaultMargin, IndicatorHeight + DepthMapHeight + margin, 0, 0);
		}

		private void SetIndicators()
		{
			if (double.IsNaN(MainCanvas.Width))
				return;

			var elementLeft = LayersData.Get<Image>(IndicatorLeftLayerName);
			var elementRight = LayersData.Get<Image>(IndicatorRightLayerName);

			if (HelpersCheckBox.IsChecked != true)
			{
				if (elementLeft != null)
				{
					MainCanvas.Children.Remove(elementLeft);
					LayersData.Remove(IndicatorLeftLayerName);
				}

				if (elementRight != null)
				{
					MainCanvas.Children.Remove(elementRight);
					LayersData.Remove(IndicatorRightLayerName);
				}

				return;
			}

			if (elementLeft == null)
			{
				elementLeft = new Image { Name = IndicatorLeftLayerName };
				RenderOptions.SetBitmapScalingMode(elementLeft, BitmapScalingMode.HighQuality);
				MainCanvas.Children.Add(elementLeft);
				LayersData.Add(new Layer { Name = IndicatorLeftLayerName, Element = elementLeft });
			}

			if (elementRight == null)
			{
				elementRight = new Image { Name = IndicatorRightLayerName };
				RenderOptions.SetBitmapScalingMode(elementRight, BitmapScalingMode.HighQuality);
				MainCanvas.Children.Add(elementRight);
				LayersData.Add(new Layer { Name = IndicatorRightLayerName, Element = elementRight });
			}

			// Only if both helpers are custom do we use them, otherwise use default helpers 
			if(LeftHelperButton.Content is Image leftHelper && RightHelperButton.Content is Image rightHelper)
			{
				elementLeft.Source = leftHelper.Source;
				elementRight.Source = rightHelper.Source;
			}
			else
			{
				Uri uriLeft;
				if (StereogramWrapper.ViewType == ViewType.Parallel)
					uriLeft = new Uri("pack://application:,,,/Indicators/Arrows-Parallel-Right.png");
				else
					uriLeft = new Uri("pack://application:,,,/Indicators/Arrows-Crossview-Right.png");
				var imageSourceLeft = new BitmapImage(uriLeft);
				
				elementLeft.Source = imageSourceLeft;

				Uri uriRight;
				if (StereogramWrapper.ViewType == ViewType.Parallel)
					uriRight = new Uri("pack://application:,,,/Indicators/Arrows-Parallel-Left.png");
				else
					uriRight = new Uri("pack://application:,,,/Indicators/Arrows-Crossview-Left.png");
				var imageSourceRight = new BitmapImage(uriRight);

				elementRight.Source = imageSourceRight;
			}
			
			elementLeft.Width = StereogramWrapper.MaxSeparation / 3f;
			elementLeft.Height = StereogramWrapper.MaxSeparation / 3f;
			elementRight.Width = StereogramWrapper.MaxSeparation / 3f;
			elementRight.Height = StereogramWrapper.MaxSeparation / 3f;
			
			var separation = StereogramWrapper.ViewType == ViewType.Parallel ? StereogramWrapper.MaxSeparation / 2 : StereogramWrapper.MinSeparation / 2;
			var leftX = MainCanvas.Width / 2 - separation - elementLeft.Width / 2;
			var rightX = MainCanvas.Width / 2 + separation - elementLeft.Width / 2;

			elementLeft.Margin = new Thickness(StereogramWrapper.ViewType == ViewType.Parallel ? leftX : rightX, DefaultMargin, 0, 0);
			elementRight.Margin = new Thickness(StereogramWrapper.ViewType == ViewType.Parallel ? rightX : leftX, DefaultMargin, 0, 0);
			
			var stereogram = LayersData.Get<Image>(StereogramLayerName);

			if (stereogram != null)
				stereogram.Margin = new Thickness(0, elementRight.Height + DefaultMargin * 2, 0,0);
		}

		private void SetMainCanvasHeight()
		{
			var margin = 0d;

			if(IndicatorHeight > 0)
				margin += DefaultMargin * 2;

			if(CopyrightHeight > 0)
				margin += DefaultMargin;

			MainCanvas.Height = DepthMapHeight + IndicatorHeight + CopyrightHeight + margin;
		}

		/// <summary>
		/// Load a file into a bitmap
		/// </summary>
		/// <param name="path"></param>
		/// <returns></returns>
		private static System.Drawing.Image LoadImage(string path)
		{
			System.Drawing.Image image;

			using (var bmpTemp = new Bitmap(path))
				image = new Bitmap(bmpTemp);

			return image;
		}

		private void UpdateValidationMessages()
		{
			ValidationErrorMessages.Items.Clear();
			ValidationWarningMessages.Items.Clear();

			foreach (var error in StereogramWrapper.ValidationErrors)
				ValidationErrorMessages.Items.Add(error);

			if(StereogramWrapper.ValidationErrors.Any())
				ValidationErrorMessages.Visibility = Visibility.Visible;
			else
				ValidationErrorMessages.Visibility = Visibility.Collapsed;

			foreach (var warning in StereogramWrapper.ValidationWarnings)
				ValidationWarningMessages.Items.Add(warning);

			if(StereogramWrapper.ValidationWarnings.Any())
				ValidationWarningMessages.Visibility = Visibility.Visible;
			else
				ValidationWarningMessages.Visibility = Visibility.Collapsed;

			NoMessagesLabel.Visibility = (StereogramWrapper.ValidationErrors.Any() || StereogramWrapper.ValidationWarnings.Any()) ? Visibility.Collapsed : Visibility.Visible;
		}
		
		private void FitWindow()
		{
			var scaleX = CanvasBorder.ActualWidth / MainCanvas.ActualWidth;
			var scaleY = CanvasBorder.ActualHeight / MainCanvas.ActualHeight;
			var scale = scaleX > scaleY ? scaleY : scaleX;

			var zoomSliderValue = (int) (scale * 100);
			if (zoomSliderValue < ZoomSlider.Minimum)
				ZoomSlider.Value = ZoomSlider.Minimum;
			else if (zoomSliderValue > ZoomSlider.Maximum)
				ZoomSlider.Value = ZoomSlider.Maximum;
			else
				ZoomSlider.Value = zoomSliderValue;

			Zoom(ZoomSlider.Value / 100);
		}
		
		private void SetTextureMask()
		{
			if (StereogramWrapper.Texture?.Width > 0)
			{
				var textureImageWidth = TextureImage.Width;
				double visiblePart;

				if(StereogramWrapper.ViewType == ViewType.Parallel)
					visiblePart = StereogramWrapper.MaxSeparation / (double) StereogramWrapper.TextureWidth;
				else
					visiblePart = StereogramWrapper.MinSeparation / (double) StereogramWrapper.TextureWidth;

				if (visiblePart <= 1)
				{
					var maskStartingPoint = textureImageWidth * visiblePart;
					var maskWidth = textureImageWidth - maskStartingPoint;

					TextureImageMask.Width = maskWidth;
					TextureImageMask.Margin = new Thickness(maskStartingPoint, 0, 0, 0);
				}
				else
				{
					TextureImageMask.Width = 0;
					TextureImageMask.Margin = new Thickness(0, 0, 0, 0);
				}
			}
		}

		#endregion
	}
}
