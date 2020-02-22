using System.Windows.Controls;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
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
