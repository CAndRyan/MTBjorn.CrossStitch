using SixLabors.ImageSharp.PixelFormats;
using System;
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

		public static List<TPixel> GetReducedColorSet<TPixel>(List<TPixel> pixels, int reducedColorCount) where TPixel : unmanaged, IPixel<TPixel>
		{
			if (typeof(TPixel) == typeof(Rgb24))
				return VectorDistanceColorReductionAlgorithm(pixels.Cast<Rgb24>().ToList(), reducedColorCount)
					.Cast<TPixel>()
					.ToList();

			throw new NotImplementedException($"No algorithm implemented to reduce pixel type: {typeof(TPixel)}");
		}

		/// <summary>
		/// Reduce a color set by identifying x new colors which most closely represent each original color within their grouping
		///   - a numerical method requiring multiple iterations?
		///   - or a single matrix computation?
		/// 1. Consider each RGB pixel as a 3D vector
		/// 2. Compute the "distance" between each pixel in this 3D vector space
		///   - the upper half of a sphere: 0<=r<=255, 0<=g<=255, 0<=b<=255
		/// 3. *Identify weights for each pixel such that there exist only x unique colors
		///   - or consider grouping similar colors (i.e. those closest to each other)
		///     - how to identify where to draw the boundary for each group?
		///   - then assese each group's average RGB value
		///     - how to account for each grouping having a correlation with one another?
		///       - i.e. clusters near one another may be pulled, as if by gravity, towards one another if one is 'denser' than the other
		/// 4. ...
		/// </summary>
		private static List<Rgb24> VectorDistanceColorReductionAlgorithm(List<Rgb24> pixels, int reducedColorCount)
		{
			if (reducedColorCount <= 0)
				return null;
			if (reducedColorCount >= pixels.Count)
				return pixels;
			if (reducedColorCount == 1)
				return new List<Rgb24> {
					GetCentroid(pixels)
				};

			var correlationMatrix = GetDistanceCorrelationMatrix(pixels);
			var centroid = GetCentroid(pixels);

			throw new NotImplementedException("TODO: implement");
		}

		private static IEnumerable<TPixel> GetAllPixels<TPixel>(IS.Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
		{
			for (var rowIndex = 0; rowIndex < image.Height; rowIndex++)
				for (var columnIndex = 0; columnIndex < image.Width; columnIndex++)
					yield return image[columnIndex, rowIndex];
		}

		private static double[,] GetDistanceCorrelationMatrix(List<Rgb24> pixels)
		{
			var matrix = new double[pixels.Count, pixels.Count];
			for (var i = 0; i < pixels.Count; i++)
			{
				var currentPixel = pixels[i];
				for (var j = 0; j < i; j++)
				{
					var rawDistance = GetDistance(currentPixel, pixels[j]);
					matrix[j, i] = rawDistance;
					matrix[i, j] = rawDistance * -1;
				}

				matrix[i, i] = 0;
			}

			return matrix;
		}

		private static double GetDistance(Rgb24 first, Rgb24 second)
		{
			var redPortion = Math.Pow(first.R - second.R, 2);
			var greenPortion = Math.Pow(first.G - second.G, 2);
			var bluePortion = Math.Pow(first.B - second.B, 2);

			return Math.Sqrt(redPortion + greenPortion + bluePortion);
		}

		private static Rgb24 GetCentroid(List<Rgb24> pixels)
		{
			var (redValues, greenValues, blueValues) = GetValuesFromEachDimension(pixels);
			var redAverage = (byte)GetAverage(redValues.ToArray());
			var greenAverage = (byte)GetAverage(greenValues.ToArray());
			var blueAverage = (byte)GetAverage(blueValues.ToArray());

			return new Rgb24(redAverage, greenAverage, blueAverage);
		}

		private static (IEnumerable<int> redValues, IEnumerable<int> greenValues, IEnumerable<int> blueValues) GetValuesFromEachDimension(IEnumerable<Rgb24> pixels) => (
			pixels.Select(p => (int)p.R),
			pixels.Select(p => (int)p.G),
			pixels.Select(p => (int)p.B)
		);

		private static int GetAverage(params int[] values) => (int)Math.Round((double)values.Sum() / values.Length);
	}
}
