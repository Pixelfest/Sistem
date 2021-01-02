using System.Windows;
using System.Windows.Controls;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
{
	public partial class DocumentLayerProperties
	{
		private DocumentLayer DocumentLayer => DataContext as DocumentLayer;

		public DocumentLayerProperties()
		{
			InitializeComponent();
		}

		private void SelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var backgroundLayer = (DocumentLayer) DataContext;

			if (backgroundLayer == null)
				return;

			backgroundLayer.MeasurementsTabIndex = MeasurementsTab.SelectedIndex;
		}

		private void AutoSizeClick(object sender, RoutedEventArgs e)
		{
			DocumentLayer.OnAutoSize();
		}
	}
}
