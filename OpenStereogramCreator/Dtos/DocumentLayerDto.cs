using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class DocumentLayerDto : LayerBaseDto
{
	public string BackgroundColorText { get; set; }
}