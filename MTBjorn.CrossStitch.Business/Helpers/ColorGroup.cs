using MTBjorn.CrossStitch.Business.Extensions;
using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;

namespace MTBjorn.CrossStitch.Business.Helpers
{
	public abstract class ColorGroup<TPixel> where TPixel : unmanaged, IPixel<TPixel>
	{
		private TPixel? centroid;

		public ColorGroup(IEnumerable<TPixel> pixels)
		{
			Pixels = pixels.ToList();
		}

		public List<TPixel> Pixels { get; }

		public TPixel Centroid
		{
			get
			{
				if (centroid is null)
					centroid = GetCentroid();

				return (TPixel)centroid;
			}
		}

		public bool Contains(TPixel reference) => Pixels.Contains(reference);

		protected abstract TPixel GetCentroid();
	}

	public class RgbColorGroup : ColorGroup<Rgb24>
	{
		public RgbColorGroup(IEnumerable<Rgb24> pixels) : base(pixels) { }

		protected override Rgb24 GetCentroid() => Pixels.GetCentroid();
	}

	public abstract class WeightedColorGroup<TPixel> where TPixel : unmanaged, IPixel<TPixel>
	{
		private WeightedPixel<TPixel>? centerOfMass;

		public WeightedColorGroup(IEnumerable<WeightedPixel<TPixel>> pixels)
		{
			Pixels = pixels.ToList();
		}

		public List<WeightedPixel<TPixel>> Pixels { get; }

		public WeightedPixel<TPixel> CenterOfMass {
			get {
				if (centerOfMass is null)
					centerOfMass = GetCenterOfMass();

				return centerOfMass;
			}
		}

		public bool Contains(TPixel reference) => Pixels.Select(p => p.Value).Contains(reference);

		protected abstract WeightedPixel<TPixel> GetCenterOfMass();
	}

	public class RgbWeightedColorGroup : WeightedColorGroup<Rgb24>
	{
		public RgbWeightedColorGroup(IEnumerable<WeightedPixel<Rgb24>> pixels) : base(pixels) { }

		protected override WeightedPixel<Rgb24> GetCenterOfMass() => Pixels.GetCenterOfMass();

		public static implicit operator ColorGroup<Rgb24>(RgbWeightedColorGroup d) => new RgbColorGroup(d.Pixels.Select(p => p.Value));
		public static explicit operator RgbWeightedColorGroup(ColorGroup<Rgb24> b) => new RgbWeightedColorGroup(b.Pixels.Select(p => new WeightedPixel<Rgb24>(p, 1)));
	}
}
