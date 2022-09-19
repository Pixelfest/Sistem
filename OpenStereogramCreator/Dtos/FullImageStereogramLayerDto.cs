using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class FullImageStereogramLayerDto : PatternStereogramLayerDto
{
	public int Shift { get; set; }
	public int Start { get; set; }
	public int End { get; set; }
}