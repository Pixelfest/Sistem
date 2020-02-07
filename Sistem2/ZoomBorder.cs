using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Sistem2
{
	/// <summary>
	/// https://stackoverflow.com/questions/741956/pan-zoom-image
	/// 
	/// </summary>
	public class ZoomBorder : Border
	{
		private UIElement _child;
		private Point _origin;
		private Point _start;

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
				this.MouseWheel += child_MouseWheel;
				this.MouseLeftButtonDown += child_MouseLeftButtonDown;
				this.MouseLeftButtonUp += child_MouseLeftButtonUp;
				this.MouseMove += child_MouseMove;
				this.PreviewMouseRightButtonDown += new MouseButtonEventHandler(
				  child_PreviewMouseRightButtonDown);
			}
		}

		public void Reset()
		{
			if (_child != null)
			{
				// reset zoom
				var st = GetScaleTransform(_child);
				st.ScaleX = 1.0;
				st.ScaleY = 1.0;

				// reset pan
				var tt = GetTranslateTransform(_child);
				tt.X = 0.0;
				tt.Y = 0.0;
			}
		}

		#region Child Events

		private void child_MouseWheel(object sender, MouseWheelEventArgs e)
		{
			if (_child != null)
			{
				var st = GetScaleTransform(_child);
				var tt = GetTranslateTransform(_child);

				var zoom = e.Delta > 0 ? .2 : -.2;
				if (!(e.Delta > 0) && (st.ScaleX < .4 || st.ScaleY < .4))
					return;

				var relative = e.GetPosition(_child);
				double absoluteX;
				double absoluteY;

				absoluteX = relative.X * st.ScaleX + tt.X;
				absoluteY = relative.Y * st.ScaleY + tt.Y;

				st.ScaleX += zoom;
				st.ScaleY += zoom;

				tt.X = absoluteX - relative.X * st.ScaleX;
				tt.Y = absoluteY - relative.Y * st.ScaleY;
			}
		}

		private void child_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

		private void child_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			if (_child != null)
			{
				_child.ReleaseMouseCapture();
				this.Cursor = Cursors.Arrow;
			}
		}

		void child_PreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
		{
			this.Reset();
		}

		private void child_MouseMove(object sender, MouseEventArgs e)
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

		#endregion
	}
}
