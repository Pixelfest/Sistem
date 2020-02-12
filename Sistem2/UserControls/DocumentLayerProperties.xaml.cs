using System.Collections.Generic;
using System.Windows.Controls;
using Sistem2.ViewModels;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for DocumentLayerProperties.xaml
	/// </summary>
	public partial class DocumentLayerProperties : UserControl
	{
		readonly Dictionary<string, Measurements> _measurements = new Dictionary<string, Measurements>
		{
			{ "Pixels", Measurements.Pixels },
			{ "Centimeters", Measurements.Centimeters },
			{ "Inches", Measurements.Inches },
		}; 

		/// <summary>
		/// Constructor
		/// </summary>
		public DocumentLayerProperties()
		{
			InitializeComponent();
		}

		/// <summary>
		/// Update the documents Measurements when the tab is changed
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var backgroundLayer = (DocumentLayer) DataContext;

			if (backgroundLayer == null)
				return;

			backgroundLayer.Measurements = _measurements[(MeasurementsTab.SelectedItem as TabItem).Name];
		}
	}
}
