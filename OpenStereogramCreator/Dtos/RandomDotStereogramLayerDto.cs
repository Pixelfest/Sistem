using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class RandomDotStereogramLayerDto : StereogramLayerDto
{
	public bool ColoredNoise { get; set; }
	public int Density { get; set; }
}