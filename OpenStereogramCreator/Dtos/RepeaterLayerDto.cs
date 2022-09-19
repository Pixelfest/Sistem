using System;

namespace OpenStereogramCreator.Dtos;

[Serializable]
public class RepeaterLayerDto : ImageLayerDto
{
	public float Zoom { get; set; }

	public int TotalWidth { get; set; }

	public int Start { get; set; }

	public int Y { get; set; }

	public string RepeatPattern { get; set; }
}