using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;

namespace MTBjorn.CrossStitch.Business.Extensions
{
	public static class PixelEnumerableExtensions
	{
		public static Rgb24 GetCentroid(this IEnumerable<Rgb24> pixels)
		{
			var pixelCount = pixels.Count();
			if (pixelCount == 0)
				return new Rgb24(0, 0, 0);
			if (pixelCount == 1)
				return pixels.First();

			var (redValues, greenValues, blueValues) = GetValuesFromEachDimension(pixels);
			var redAverage = (byte)redValues.GetAverage();
			var greenAverage = (byte)greenValues.GetAverage();
			var blueAverage = (byte)blueValues.GetAverage();

			return new Rgb24(redAverage, greenAverage, blueAverage);
		}

		private static (IEnumerable<int> redValues, IEnumerable<int> greenValues, IEnumerable<int> blueValues) GetValuesFromEachDimension(IEnumerable<Rgb24> pixels) => (
			pixels.Select(p => (int)p.R),
			pixels.Select(p => (int)p.G),
			pixels.Select(p => (int)p.B)
		);
	}
}
