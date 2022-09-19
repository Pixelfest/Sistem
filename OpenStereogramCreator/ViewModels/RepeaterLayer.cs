using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator.ViewModels;

public class RepeaterLayer : ImageLayer
{    
    //private Image<Rgba32> _image;
    //private string _imageFileName;
    private float _zoom = 1;
    private int _start = 0;
    private int _width;
    private int _height;
    private List<int> _repeatPattern = new List<int>();
        
    private int _y;

    public float Zoom
    {
        get => _zoom;
        set
        {
            _zoom = value;

            OnPropertyChanged();
        }
    }

    public int TotalWidth
    {
        get => _width;
        set
        {
            _width = value;

            OnPropertyChanged(nameof(Width));
        }
    }

    public int Start
    {
        get => _start;
        set
        {
            _start = value;

            OnPropertyChanged();
        }
    }

    public int Y
    {
        get => _y;
        set
        {
            _y = value;

            OnPropertyChanged();
        }
    }

    public string RepeatPattern
    {
        get => string.Join(',', _repeatPattern);
        set
        {
            try
            {
                _repeatPattern = value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(v => int.Parse((string)v)).ToList();
            }
            catch (Exception e)
            {
                _repeatPattern = new List<int>();
            }

            OnPropertyChanged();
        }
    }

    public override void Render()
    {
        if (Image == null || _width == 0 || _zoom == 0 || _repeatPattern.Count == 0)
            return;

        CachedImage = new Image<Rgba32>(_width, _height);

        var pattern = Image.Clone();
        pattern.Mutate(x => x.Resize((int)(Image.Width * _zoom), (int)(Image.Height * _zoom)));

        var x = _start;
        var repeatPatternIndex = 0;

        while (x < _width)
        {
            CachedImage.Mutate(i => i.DrawImage(pattern, new Point(x, _y), 1));

            x += _repeatPattern[repeatPatternIndex % _repeatPattern.Count];
            repeatPatternIndex++;
        }
    }

    [NotifyPropertyChangedInvocator]
	public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        switch (propertyName)
        {
			case nameof(Image):
            case nameof(TotalWidth):
            case nameof(Start):
            case nameof(RepeatPattern):
            case nameof(Zoom):
            case nameof(Y):
                CachedImage = null;

                if(Image != null)
                    _height = _y + (int)(Image.Height * _zoom);

                break;
        }

        base.OnPropertyChanged(propertyName);
    }

	public new T Export<T>() where T : RepeaterLayerDto, new()
	{
		var export = base.Export<T>();

		export.Zoom = Zoom;
		export.TotalWidth = TotalWidth;
		export.Start = Start;
		export.Y = Y;
		export.RepeatPattern = RepeatPattern;

		return export;
	}

	public new void Import<TSource>(TSource source)
		where TSource : RepeaterLayerDto, new()
	{
		this.Zoom = source.Zoom;
		this.TotalWidth = source.TotalWidth;
		this.Start = source.Start;
		this.Y = source.Y;
		this.RepeatPattern = source.RepeatPattern;
		base.Import(source);
	}
}