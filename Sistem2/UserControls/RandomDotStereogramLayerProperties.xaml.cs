using System.Windows.Controls;
using Sistem2.ViewModels;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class RandomDotStereogramLayerProperties : UserControl
	{
		private RandomDotStereogramLayer _randomDotStereogramLayer => DataContext as RandomDotStereogramLayer;

		/// <summary>
		/// Constructor
		/// </summary>
		public RandomDotStereogramLayerProperties()
		{
			InitializeComponent();
		}
	}
}
