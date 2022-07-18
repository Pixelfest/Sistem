using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace OpenStereogramCreator.ViewModels;

public class RepeaterLayer : LayerBase
{
    private Image<Rgba32> _image;
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

            OnPropertyChanged(nameof(Zoom));
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

            OnPropertyChanged(nameof(Start));
        }
    }

    public int Y
    {
        get => _y;
        set
        {
            _y = value;

            OnPropertyChanged(nameof(Y));
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

            OnPropertyChanged(nameof(RepeatPattern));
        }
    }

    public Image<Rgba32> Image
    {
        get => _image;
        set
        {
            _image = value;

            Width = value.Width;
            Height = value.Height;

            OnPropertyChanged(nameof(Image));
            OnPropertyChanged(nameof(ImageSource));
        }
    }

    public ImageSharpImageSource<Rgba32> ImageSource
    {
        get
        {
            if (Image != null)
                return new ImageSharpImageSource<Rgba32>(Image);

            return null;
        }
    }

    public override void Render()
    {
        if (_image == null || _width == 0 || _zoom == 0 || _repeatPattern.Count == 0)
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
    protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
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
}