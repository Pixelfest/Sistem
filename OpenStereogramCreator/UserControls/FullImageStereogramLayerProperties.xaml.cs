﻿using System.Windows.Input;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
{
	public partial class FullImageStereogramLayerProperties
	{
		private FullImageStereogramLayer FullImageStereogramLayer => DataContext as FullImageStereogramLayer;

		public FullImageStereogramLayerProperties()
		{
			InitializeComponent();
		}

		private void ShiftMouseWheel(object sender, MouseWheelEventArgs e)
		{
			var multiplier = 1;

			if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
				multiplier = 10;

			FullImageStereogramLayer.Shift += e.Delta < 0 ? -1 * multiplier : 1 * multiplier;
		}
	}
}
