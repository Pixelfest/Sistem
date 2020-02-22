using System;
using System.Windows.Controls;
using OpenStereogramCreator.ViewModels;

namespace OpenStereogramCreator
{
	public partial class BaseLayerProperties : UserControl
	{
		private LayerBase _baseLayer => DataContext as LayerBase;

		public BaseLayerProperties()
		{
			InitializeComponent();
		}
	}
}
