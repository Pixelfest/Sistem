using System.Collections.ObjectModel;
using System.Linq;
using Sistem2.LayerTypes;

namespace Sistem2
{
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		public void Draw()
		{
			foreach (var layer in this.Where(layer => layer.Visible))
			{
				layer.Draw();
			}
		}
	}
}