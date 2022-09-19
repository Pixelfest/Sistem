using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class PatternStereogramLayerDto : StereogramLayerDto
{
	public int PatternStart { get; set; }
	public int PatternEnd { get; set; }
	public string PatternImageBase64 { get; set; }
	public string PatternImageFileName { get; set; }
	public float Zoom { get; set; }
	public int PatternYShift { get; set; }
}