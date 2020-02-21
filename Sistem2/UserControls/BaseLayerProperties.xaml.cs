using System;
using System.Windows.Controls;
using Sistem2.ViewModels;

namespace Sistem2
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
