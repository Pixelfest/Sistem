using System.Collections.ObjectModel;
using System.Linq;

namespace OpenStereogramCreator.ViewModels
{
	public class LayersViewModel : ObservableCollection<LayerBase>
	{
		public void Draw()
		{
			foreach (var layer in this.Where(layer => layer.Visible).Reverse())
			{
				layer.Draw();
			}
		}
	}
}