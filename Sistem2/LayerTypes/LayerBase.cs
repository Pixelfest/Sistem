using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Printing;
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
		public ImageSharpImageSource<Rgba32> Preview { get; set; }
		public string Name { get; set; }
		public bool Visible { get; set; }
		public int Order { get; set; }

		public bool IsDeleteEnabled  => true;

		protected LayerBase(Image<Rgba32> target)
		{
			this.Target = target;
			this.Preview = new ImageSharpImageSource<Rgba32>(150,150);
		}

		public event PropertyChangedEventHandler PropertyChanged;

		[NotifyPropertyChangedInvocator]
		protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
		}

		public abstract void Draw();
		public abstract void DrawPreview();
	}
}
