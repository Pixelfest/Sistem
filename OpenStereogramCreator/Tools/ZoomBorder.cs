using System.ComponentModel;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;

namespace OpenStereogramCreator.Tools
{
	using System.Linq;
    using System.Windows;
	using System.Windows.Controls;
	using System.Windows.Input;
	using System.Windows.Media;

	public class ZoomBorder : Border, INotifyPropertyChanged
	{
		private UIElement _child;
		private Point _origin;
		private Point _start;
		private double? _windowsScaling;

		protected double WindowsScaling => _windowsScaling ?? LoadWindowsScaling() ?? 1;

		public Point Start => _start;

		private double? LoadWindowsScaling()
		{
			var source = PresentationSource.FromVisual(this);
			if (source != null)
			{
				_windowsScaling = source.CompositionTarget.TransformToDevice.M11;
				return _windowsScaling;
			}

			return null;
		}

		private TranslateTransform GetTranslateTransform(UIElement element)
		{
			return (TranslateTransform)((TransformGroup)element.RenderTransform)
			  .Children.First(tr => tr is TranslateTransform);
		}

		private ScaleTransform GetScaleTransform(UIElement element)
		{
			return (ScaleTransform)((TransformGroup)element.RenderTransform)
			  .Children.First(tr => tr is ScaleTransform);
		}

		public string Scale
		{
			get
			{
				var st = GetScaleTransform(_child);
				var image = _child as Image;
				var imageWidth = image.Source.Width;
				var renderWidth = image.RenderSize.Width;

				var scale = renderWidth / imageWidth * WindowsScaling * st.ScaleX;

				return $"{100 * scale:0}%";
			}
		}

		public override UIElement Child
		{
			get => base.Child;
			set
			{
				if (value != null && value != this.Child)
					this.Initialize(value);
				base.Child = value;
			}
		}

		public void Initialize(UIElement element)
		{
			this._child = element;

			if (_child != null)
			{
				var group = new TransformGroup();
				var st = new ScaleTransform();
				group.Children.Add(st);
				var tt = new TranslateTransform();
				group.Children.Add(tt);
				_child.RenderTransform = group;
				_child.RenderTransformOrigin = new Point(0.0, 0.0);
				this.MouseWheel += ChildMouseWheel;
				this.MouseLeftButtonDown += ChildMouseLeftButtonDown;
				this.MouseLeftButtonUp += ChildMouseLeftButtonUp;
				this.MouseMove += ChildMouseMove;
				this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(ChildPreviewMouseRightButtonDown);
			}
		}

		public void SetFitToWindow()
		{
			if (_child == null)
				return;

			SetScale(1);
			ResetPanning();
		}

		public void SetPerPixel()
		{
			var viewPortFactor = GetViewPortFactor();

			SetScale(viewPortFactor / WindowsScaling);
			ResetPanning();
		}

		public void SetActualSize(int dpiImage, int dpiMonitor, int targetWidthInch)
		{
			var viewPortFactor = GetViewPortFactor();

			var dpiFactor = dpiMonitor / (double)dpiImage;

			SetScale(viewPortFactor * dpiFactor / WindowsScaling);
			ResetPanning();
		}

		private void ChildMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var zoomInterval = 0.20000000000000;
			zoomInterval *= GetViewPortFactor();

			if (_child != null)
			{
				var st = GetScaleTransform(_child);
				var tt = GetTranslateTransform(_child);

				var zoom = e.Delta > 0 ? zoomInterval : -zoomInterval;
				if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
					return;

				var relative = e.GetPosition(_child);

				var absoluteX = relative.X * st.ScaleX + tt.X;
				var absoluteY = relative.Y * st.ScaleY + tt.Y;

				SetScale(st.ScaleX + zoom);

				tt.X = absoluteX - relative.X * st.ScaleX;
				tt.Y = absoluteY - relative.Y * st.ScaleY;
			}

			OnPropertyChanged(nameof(Scale));
		}

		private void ChildMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (_child != null)
			{
				var tt = GetTranslateTransform(_child);
				_start = e.GetPosition(this);
				_origin = new Point(tt.X, tt.Y);
				this.Cursor = Cursors.Hand;
				_child.CaptureMouse();
			}
		}

		private void ChildMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_child != null)
			{
				_child.ReleaseMouseCapture();
				this.Cursor = Cursors.Arrow;
			}
		}

		void ChildPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.SetFitToWindow();
		}

		private void ChildMouseMove(object sender, MouseEventArgs e)
		{
			if (_child != null)
			{
				if (_child.IsMouseCaptured)
				{
					var tt = GetTranslateTransform(_child);
					var v = _start - e.GetPosition(this);
					tt.X = _origin.X - v.X;
					tt.Y = _origin.Y - v.Y;
				}
			}
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		private void SetScale(double scale)
		{
			var st = GetScaleTransform(_child);
			st.ScaleX = scale;
			st.ScaleY = scale;

			OnPropertyChanged(nameof(Scale));
		}

		private void ResetPanning()
		{
			// reset pan
			var tt = GetTranslateTransform(_child);

			if (_child is FrameworkElement element)
			{
				var st = GetScaleTransform(_child);

				var imageWidth = element.ActualWidth * st.ScaleX;
				var renderWidth = element.RenderSize.Width;

				var imageHeight = element.ActualHeight * st.ScaleY;
				var renderHeight = element.RenderSize.Height;

				tt.X = (renderWidth - imageWidth) / 2;
				tt.Y = (renderHeight - imageHeight) / 2;

				return;
			}

			tt.X = 0.0;
			tt.Y = 0.0;
		}

		private double GetViewPortFactor()
		{
			var image = _child as Image;
			if (image == null)
				return 1;

			var imageWidth = image.Source.Width;
			var actualWidthViewPort = _child.RenderSize.Width;
			var viewPortFactor = imageWidth / actualWidthViewPort;

			return viewPortFactor;
		}
	}
}
