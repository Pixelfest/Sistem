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