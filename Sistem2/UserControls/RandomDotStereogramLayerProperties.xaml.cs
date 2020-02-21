using System.Windows.Controls;
using Sistem2.ViewModels;

namespace Sistem2
{
	public partial class RandomDotStereogramLayerProperties : UserControl
	{
		private RandomDotStereogramLayer _randomDotStereogramLayer => DataContext as RandomDotStereogramLayer;

		public RandomDotStereogramLayerProperties()
		{
			InitializeComponent();
		}
	}
}
