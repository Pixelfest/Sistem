using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Sistem2.Annotations;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Sistem2.LayerTypes
{
	public abstract class LayerBase : INotifyPropertyChanged
	{
		//public RenderTargetBitmap Target { get; set; }
		public Image<Rgba32> Target { get; set; }
		public string Name { get; set; }
		public bool Visible { get; set; }
		public int Order { get; set; }

		protected LayerBase(Image<Rgba32> target)
		{
			this.Target = target;
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public abstract void Draw();
	}
}
