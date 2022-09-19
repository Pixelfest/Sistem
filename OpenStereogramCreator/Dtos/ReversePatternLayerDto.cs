using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class ReversePatternLayerDto : ImageLayerDto
{
	public int NumberOfColumns { get; set; }
}