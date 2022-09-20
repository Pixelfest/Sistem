using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class LayerBaseDto : ILayerBaseDto
{
	public int Top { get; set; }
	public int Left { get; set; }
	public int Width { get; set; }
	public int Height { get; set; }
	public float Opacity { get; set; }
	public int Oversampling { get; set; }
	public string Name { get; set; }
	public bool Visible { get; set; }
	public int BlendingMode { get; set; }
}