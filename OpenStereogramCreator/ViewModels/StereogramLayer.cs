using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using Sistem.Core;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	[Serializable]
	public abstract class StereogramLayer : LayerBase, IHaveADepthImage
	{
		private Image<Rgb48> _depthImage;
		private string _depthImageFileName;
		private float _minimumSeparation;
		private float _maximumSeparation;
		private float _origin;
		private bool _drawDepthImage;

        public string DepthImageExport
        {
            get => DepthImage.ToBase64String(PngFormat.Instance);
            set
            {
                var bytes = Convert.FromBase64String(value);
                DepthImage = Image.Load<Rgb48>(bytes);
            }
        }

        public float Origin
		{
			get => _origin;
			set
			{
				_origin = value;
				OnPropertyChanged(nameof(Origin));
			}
		}

		[IgnoreDataMember]
		public Image<Rgb48> DepthImage
		{
			get => _depthImage;
			set
			{
				_depthImage = value;

				MinimumSeparation = _depthImage.Width / 8f;
				MaximumSeparation = _depthImage.Width / 7f;

				Width = _depthImage.Width;
				Height = _depthImage.Height;

				if (this is PatternStereogramLayer || this is RandomDotStereogramLayer)
					Origin = (value.Width - MaximumSeparation) / 2;

				OnPropertyChanged(nameof(DepthImage));
				OnPropertyChanged(nameof(DepthImageSource));
			}
		}

		public string DepthImageFileName
		{
			get => _depthImageFileName;
			set
			{
				_depthImageFileName = value;
				OnPropertyChanged(nameof(DepthImageFileName));
			}
		}

        [IgnoreDataMember]
		public ImageSharpImageSource<Rgb48> DepthImageSource
		{
			get
			{
				if (DepthImage != null)
					return new ImageSharpImageSource<Rgb48>(DepthImage);

				return null;
			}
		}

		public float MinimumSeparation
		{
			get => _minimumSeparation;
			set
			{
				_minimumSeparation = value;
				OnPropertyChanged(nameof(MinimumSeparation));
			}
		}

		public float MaximumSeparation
		{
			get => _maximumSeparation;
			set
			{
				_maximumSeparation = value;
				OnPropertyChanged(nameof(MaximumSeparation));
			}
		}

		public bool DrawDepthImage
		{
			get => _drawDepthImage;
			set
			{
				_drawDepthImage = value;
				OnPropertyChanged(nameof(DrawDepthImage));
			}
		}

		public Stereogram CreateStereogram()
		{
			var stereogram = new Stereogram
			{
				DepthMap = DepthImage
			};

			// Don't use object initializer, constructor initializes values that are otherwise overwritten.
			stereogram.MinSeparation = (int)MinimumSeparation;
			stereogram.MaxSeparation = (int)MaximumSeparation;
			stereogram.Origin = (int)Origin;

			return stereogram;
		}

		[NotifyPropertyChangedInvocator]
		protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(MinimumSeparation):
				case nameof(MaximumSeparation):
				case nameof(Origin):
				case nameof(DrawDepthImage):
				case nameof(DepthImage):
					CachedImage = null;
					break;
			}

			base.OnPropertyChanged(propertyName);
		}

        public new T Export<T>() where T : StereogramLayerDto, new()
        {
            var export = base.Export<T>();

            if (DepthImage != null)
            {
                export.DepthImageBase64 = DepthImage.ToBase64String(PngFormat.Instance);
                export.DepthImageFileName = DepthImageFileName;
            }

            export.DrawDepthImage = DrawDepthImage;
            export.MaximumSeparation = MaximumSeparation;
            export.MinimumSeparation = MinimumSeparation;
            export.Origin = Origin;

            return export;
        }

		public static TResult Import<TSource, TResult>(TSource dto)
            where TSource : StereogramLayerDto 
            where TResult : StereogramLayer, new()
        {
            var targetNew = LayerBase.Import<TSource, TResult>(dto);

            if (!string.IsNullOrEmpty(dto.DepthImageBase64))
            {
                var bytes = Convert.FromBase64String(dto.DepthImageBase64.Split(",")[1]);
                targetNew.DepthImage = Image.Load<Rgb48>(bytes);
                targetNew.DepthImageFileName = dto.DepthImageFileName;
            }

            targetNew.DrawDepthImage = dto.DrawDepthImage;
            targetNew.MaximumSeparation = dto.MaximumSeparation;
            targetNew.MinimumSeparation = dto.MinimumSeparation;
			targetNew.Origin = dto.Origin;

			return targetNew;
        }
	}
}
