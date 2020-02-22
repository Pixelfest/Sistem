using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
{
	public partial class DocumentLayerProperties : UserControl
	{
		private DocumentLayer _documentLayer => DataContext as DocumentLayer;

		readonly Dictionary<string, Measurements> _measurements = new Dictionary<string, Measurements>
		{
			{ "Pixels", Measurements.Pixels },
			{ "Centimeters", Measurements.Centimeters },
			{ "Inches", Measurements.Inches },
		}; 

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
			_documentLayer.OnAutoSize();
		}
	}
}
