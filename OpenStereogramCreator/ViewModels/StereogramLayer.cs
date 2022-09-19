using System;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using OpenStereogramCreator.Annotations;
using OpenStereogramCreator.Dtos;
using Sistem.Core;
using OpenStereogramCreator.Tools;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;

namespace OpenStereogramCreator.ViewModels
{
	public abstract class StereogramLayer : LayerBase, IHaveADepthImage
	{
		private Image<Rgb48> _depthImage;
		private string _depthImageFileName;
		private float _minimumSeparation;
		private float _maximumSeparation;
		private float _origin;

		public float Origin
		{
			get => _origin;
			set
			{
				_origin = value;
				OnPropertyChanged();
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

				OnPropertyChanged();
				OnPropertyChanged(nameof(DepthImageSource));
			}
		}

		public string DepthImageFileName
		{
			get => _depthImageFileName;
			set
			{
				_depthImageFileName = value;
				OnPropertyChanged();
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
				OnPropertyChanged();
			}
		}

		public float MaximumSeparation
		{
			get => _maximumSeparation;
			set
			{
				_maximumSeparation = value;
				OnPropertyChanged();
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
		public override void OnPropertyChanged([CallerMemberName] string propertyName = null)
		{
			switch (propertyName)
			{
				case nameof(MinimumSeparation):
				case nameof(MaximumSeparation):
				case nameof(Origin):
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

			export.MaximumSeparation = MaximumSeparation;
			export.MinimumSeparation = MinimumSeparation;
			export.Origin = Origin;

			return export;
		}

		public new void Import<TSource>(TSource source)
			where TSource : StereogramLayerDto, new()
		{
			if (!string.IsNullOrEmpty(source.DepthImageBase64))
			{
				var bytes = Convert.FromBase64String(source.DepthImageBase64.Split(",")[1]);
				this.DepthImage = Image.Load<Rgb48>(bytes);
				this.DepthImageFileName = source.DepthImageFileName;
			}

			this.MaximumSeparation = source.MaximumSeparation;
			this.MinimumSeparation = source.MinimumSeparation;
			this.Origin = source.Origin;
			base.Import(source);
		}
	}
}
