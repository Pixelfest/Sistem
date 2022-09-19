using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class StereogramLayerDto : LayerBaseDto
{
	public string DepthImageBase64 { get; set; }
	public string DepthImageFileName { get; set; }
	public float MinimumSeparation { get; set; }
	public float MaximumSeparation { get; set; }
	public float Origin { get; set; }
	public bool DrawDepthImage { get; set; }
}