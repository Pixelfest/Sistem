using System.Windows.Controls;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
{
	public partial class RandomDotStereogramLayerProperties
	{
		private RandomDotStereogramLayer RandomDotStereogramLayer => DataContext as RandomDotStereogramLayer;

		public RandomDotStereogramLayerProperties()
		{
			InitializeComponent();
		}
	}
}
