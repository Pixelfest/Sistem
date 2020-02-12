using System.Collections.ObjectModel;
using System.Linq;

namespace Sistem2.ViewModels
{
	/// <summary>
	/// The viewmodel that contains all layers
	/// </summary>
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		/// <summary>
		/// Draw the layers
		/// </summary>
		public void Draw()
		{
			foreach (var layer in this.Where(layer => layer.Visible).Reverse())
			{
				layer.Draw();
			}
		}
	}
}