using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Sistem2.ViewModels;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for BackgroundLayerProperties.xaml
	/// </summary>
	public partial class BackgroundLayerProperties : UserControl
	{
		Dictionary<string, Measurements> measurements = new Dictionary<string, Measurements>
		{
			{ "Pixels", Measurements.Pixels },
			{ "Centimeters", Measurements.Centimeters },
			{ "Inches", Measurements.Inches },
		}; 

		public BackgroundLayerProperties()
		{
			InitializeComponent();
		}

		private void SelectorSelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			var backgroundLayer = (BackgroundLayer) DataContext;

			if (backgroundLayer == null)
				return;

			backgroundLayer.Measurements = measurements[(MeasurementsTab.SelectedItem as TabItem).Name];
		}
	}
}
