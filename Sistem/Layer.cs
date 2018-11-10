using System.Windows;

namespace Sistem
{
	public class Layer
	{
		public FrameworkElement Element { get; set; }

		public string Name { get; set; }

		public bool Visible
		{
			get { return Element.Visibility == Visibility.Visible; }
			set
			{
				if (value)
					Element.Visibility = Visibility.Visible;
				else
					Element.Visibility = Visibility.Collapsed;
			}
		}
	}
}
