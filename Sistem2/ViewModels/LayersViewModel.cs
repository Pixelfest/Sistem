using System.Collections.ObjectModel;
using System.Linq;

namespace Sistem2.ViewModels
{
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		public void Draw(bool useOversampling)
		{
			foreach (var layer in this.Where(layer => layer.Visible).Reverse())
			{
				layer.Draw(useOversampling);
			}
		}
	}
}