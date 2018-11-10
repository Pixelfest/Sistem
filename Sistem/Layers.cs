using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;

namespace Sistem
{
	public class Layers : BindableBase
	{
		public ObservableCollection<Layer> List { get; private set; }

		public Layers()
		{
			List = new ObservableCollection<Layer>();
		}

		public void Add(Layer layer)
		{
			var element = List.FirstOrDefault(item => item.Name == layer.Name);

			if (element == null)
			{
				List.Add(layer);
				OnPropertyChanged(nameof(List));
			}
		}

		public Layer Get(string name)
		{
			var element = List.FirstOrDefault(item => item.Name == name);

			return element;
		}

		public T Get<T>(string name) where T : FrameworkElement
		{
			var element = List.FirstOrDefault(item => item.Name == name);

			return element?.Element as T;
		}

		public void Remove(string name)
		{
			var element = List.FirstOrDefault(item => item.Name == name);

			if (element != null)
			{
				List.Remove(element);
				OnPropertyChanged(nameof(List));
			}
		}
	}
}
