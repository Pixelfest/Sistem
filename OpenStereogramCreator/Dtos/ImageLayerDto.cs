using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class ImageLayerDto : LayerBaseDto
{
	public string ImageBase64 { get; set; }
	public string ImageFileName { get; set; }
}