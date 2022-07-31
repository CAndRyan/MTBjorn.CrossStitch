using MTBjorn.CrossStitch.Business.Extensions;
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
}
