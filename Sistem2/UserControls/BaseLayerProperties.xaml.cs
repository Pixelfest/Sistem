using System;
using System.Windows.Controls;
using Sistem2.ViewModels;

namespace Sistem2
{
	/// <summary>
	/// Interaction logic for SirdsLayerProperties.xaml
	/// </summary>
	public partial class BaseLayerProperties : UserControl
	{
		private LayerBase _baseLayer => DataContext as LayerBase;
		


		/// <summary>
		/// Constructor
		/// </summary>
		public BaseLayerProperties()
		{
			InitializeComponent();
		}
	}
}
