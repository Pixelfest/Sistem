using System;
using System.Collections.Generic;
using System.IO;
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
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class PatternStereogramLayerProperties : UserControl
	{
		private PatternStereogramLayer _patternStereogramLayer => DataContext as PatternStereogramLayer;

		public PatternStereogramLayerProperties()
		{
			InitializeComponent();
		}

		private void LoadPatternImageButtonClick(object sender, RoutedEventArgs e)
		{
			var openFileDialog = new OpenFileDialog
			{
				Title = "Open image",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					_patternStereogramLayer.PatternImage = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);

					_patternStereogramLayer.PatternImageFileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}
	}
}
