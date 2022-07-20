using System;
using System.Collections.Generic;

namespace OpenStereogramCreator.Dtos;

public interface ILayerBaseDto
{
    int Top { get; set; }
    int Left { get; set; }
    int Width { get; set; }
    int Height { get; set; }
    float Opacity { get; set; }
    int Oversampling { get; set; }
    string Name { get; set; }
    bool Visible { get; set; }
}

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
}

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

[Serializable]
public class RandomDotStereogramLayerDto : StereogramLayerDto
{
    public bool ColoredNoise { get; set; }
    public int Density { get; set; }
}

[Serializable]
public class DocumentLayerDto : LayerBaseDto
{
    public string BackgroundColorText { get; set; }
}

[Serializable]
public class LayersDto
{
    public DocumentLayerDto Document { get; set; }
    
    public List<string> Layers { get; set; }
}