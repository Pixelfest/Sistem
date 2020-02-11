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
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class RandomDotStereogramLayerProperties : UserControl
	{
		private RandomDotStereogramLayer _randomDotStereogramLayer => DataContext as RandomDotStereogramLayer;

		public RandomDotStereogramLayerProperties()
		{
			InitializeComponent();
		}
	}
}
