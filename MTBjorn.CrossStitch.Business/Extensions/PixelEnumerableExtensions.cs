using MTBjorn.CrossStitch.Business.Helpers;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Linq;

namespace MTBjorn.CrossStitch.Business.Extensions
{
	public static class PixelEnumerableExtensions
	{
		public static Rgb24 GetCentroid(this IEnumerable<Rgb24> pixels)
		{
			var pixelsEnumerated = pixels.ToList();

			if (pixelsEnumerated.Count == 0)
				return new Rgb24(0, 0, 0);
			if (pixelsEnumerated.Count == 1)
				return pixelsEnumerated[0];

			var (redValues, greenValues, blueValues) = GetValuesFromEachDimension(pixelsEnumerated);
			var redAverage = (byte)redValues.GetAverage();
			var greenAverage = (byte)greenValues.GetAverage();
			var blueAverage = (byte)blueValues.GetAverage();

			return new Rgb24(redAverage, greenAverage, blueAverage);
		}

		/// <summary>
		/// Essentially a "weighted centroid" where the "mass" of each pixel is their prescribed weight, pulling the center of each color component towards them
		/// TODO: consider adding a sub-weight to each component to facilitate a red shift (or blue/green)
		/// </summary>
		public static WeightedPixel<Rgb24> GetCenterOfMass(this IEnumerable<WeightedPixel<Rgb24>> pixels) // TODO: add total weight to returned value...
		{
			var pixelsEnumerated = pixels.ToList();

			if (pixelsEnumerated.Count == 0)
				return new WeightedPixel<Rgb24>(new Rgb24(0, 0, 0), 0);
			if (pixelsEnumerated.Count == 1)
				return new WeightedPixel<Rgb24>(pixelsEnumerated[0].Value, 1);

			var (redValues, greenValues, blueValues, totalWeight) = GetWeightedValuesFromEachDimension(pixelsEnumerated);
			if (totalWeight == 0)
				throw new Exception("Pixels have a total weight of 0!");

			var redAverageWeighted = (byte)(redValues.GetAverage() / totalWeight);
			var greenAverageWeighted = (byte)(greenValues.GetAverage() / totalWeight);
			var blueAverageWeighted = (byte)(blueValues.GetAverage() / totalWeight);

			return new WeightedPixel<Rgb24>(new Rgb24(redAverageWeighted, greenAverageWeighted, blueAverageWeighted), totalWeight);
		}

		private static (List<int> redValues, List<int> greenValues, List<int> blueValues) GetValuesFromEachDimension(List<Rgb24> pixels)
		{
			var redValues = new List<int>();
			var greenValues = new List<int>();
			var blueValues = new List<int>();

			for (var i = 0; i < pixels.Count; i++)
			{
				redValues.Add(pixels[i].R);
				greenValues.Add(pixels[i].G);
				blueValues.Add(pixels[i].B);
			}

			return (redValues, greenValues, blueValues);
		}

		private static (List<int> redValues, List<int> greenValues, List<int> blueValues, int totalWeight) GetWeightedValuesFromEachDimension(List<WeightedPixel<Rgb24>> pixels)
		{
			var redValues = new List<int>();
			var greenValues = new List<int>();
			var blueValues = new List<int>();
			var totalWeight = 0;

			for (var i = 0; i < pixels.Count; i++)
			{
				var pixelWeight = pixels[i].Weight;
				var pixelValue = pixels[i].Value;

				redValues.Add(pixelValue.R * pixelWeight);
				greenValues.Add(pixelValue.G * pixelWeight);
				blueValues.Add(pixelValue.B * pixelWeight);
				totalWeight += pixelWeight;
			}

			return (redValues, greenValues, blueValues, totalWeight);
		}
	}
}
