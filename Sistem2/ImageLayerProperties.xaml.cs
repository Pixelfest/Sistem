﻿using System;
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
using Sistem2.LayerTypes;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for ImageLayerProperties.xaml
	/// </summary>
	public partial class ImageLayerProperties : UserControl
	{
		public ImageLayerProperties()
		{
			InitializeComponent();
		}

		private void ImageLayer_LoadImage_OnClick(object sender, RoutedEventArgs e)
		{
			if (!(DataContext is ImageLayer imageLayer))
				return;

			var openFileDialog = new OpenFileDialog
			{
				Title = "Open image",
				Filter = "Image File|*.bmp; *.gif; *.jpg; *.jpeg; *.png;"
			};

			if (openFileDialog.ShowDialog() == true)
			{
				try
				{
					imageLayer.Image = SixLabors.ImageSharp.Image.Load<Rgba32>(openFileDialog.FileName);
					imageLayer.FileName = openFileDialog.FileName.Substring(openFileDialog.FileName.LastIndexOf('\\') + 1);
				}
				catch
				{
				}
			}
		}
	}
}
