using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Helpers
{
	public static class ImageColorHelper
	{
		public static List<TPixel> GetAllColors<TPixel>(IS.Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
		{
			var colorMap = new Dictionary<int, TPixel>();

			foreach (var pixel in GetAllPixels(image))
			{
				var pixelHash = pixel.GetHashCode();
				if (!colorMap.ContainsKey(pixelHash))
					colorMap.Add(pixelHash, pixel);
			}

			return colorMap.Values.ToList();
		}

		private static IEnumerable<TPixel> GetAllPixels<TPixel>(IS.Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
		{
			for (var rowIndex = 0; rowIndex < image.Height; rowIndex++)
				for (var columnIndex = 0; columnIndex < image.Width; columnIndex++)
					yield return image[rowIndex, columnIndex];
		}
	}
}
