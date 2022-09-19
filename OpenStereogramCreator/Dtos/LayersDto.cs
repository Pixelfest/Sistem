using System;
using System.Collections.Generic;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class LayersDto
{
	public DocumentLayerDto Document { get; set; }
	public List<string> Layers { get; set; }
}