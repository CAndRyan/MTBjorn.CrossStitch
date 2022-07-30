﻿using MTBjorn.CrossStitch.Business.Extensions;
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
					pixels.GetCentroid()
				};

			if (reducedColorCount == 2)
				return TwoColorAlgorithm(pixels);

			throw new NotImplementedException("TODO: implement");
		}

		private static List<Rgb24> TwoColorAlgorithm(List<Rgb24> pixels)
		{
			var (firstGroup, secondGroup) = GetTwoInitialGroups(pixels);

			// 2. Reassess each group after computing their new centers
			//   - NOTE: check distance of each point in a group from each group's centroid
			Reassess(ref firstGroup, ref secondGroup);
			Reassess(ref secondGroup, ref firstGroup);

			return new List<Rgb24>()
			{
				firstGroup.GetCentroid(),
				secondGroup.GetCentroid()
			};
		}

		private static void Reassess(ref List<Rgb24> firstGroup, ref List<Rgb24> secondGroup)
		{
			var firstGroupCorrelation = GetDistanceCorrelationMatrix(Enumerable.Repeat(firstGroup.GetCentroid(), 1)
				.Concat(firstGroup)
				.Concat(Enumerable.Repeat(secondGroup.GetCentroid(), 1))
				.ToList());
			for (var i = 1; i < firstGroupCorrelation.GetLength(0) - 2; i++)
			{
				var distanceFromFirstGroupCenter = Math.Abs(firstGroupCorrelation[0, i]);
				var distanceFromSecondGroupCenter = Math.Abs(firstGroupCorrelation[firstGroupCorrelation.GetLength(0) - 1, i]);
				if (distanceFromFirstGroupCenter > distanceFromSecondGroupCenter)
				{
					secondGroup.Add(firstGroup[i - 1]);
					firstGroup.RemoveAt(i - 1);
					Reassess(ref firstGroup, ref secondGroup); // TODO: verify there's no trap with this iteration
				}
			}
		}

		private static (List<Rgb24>, List<Rgb24>) GetTwoInitialGroups(List<Rgb24> pixels)
		{
			var correlationMatrix = GetDistanceCorrelationMatrix(pixels);

			// 1. Grab initial 2 colors as those being the furthest apart
			//   - TODO: how to handle multiple pairs equally far apart?
			//   - NOTE: only need to search either the upper or lower half of correlation matrix
			var greatestDistance = -1.0d;
			var referenceIndices = new int[2];
			for (var i = 0; i < pixels.Count; i++)
			{
				for (var j = i; j < pixels.Count; j++)
				{
					var distance = Math.Abs(correlationMatrix[i, j]);
					if (distance > greatestDistance)
					{
						greatestDistance = distance;
						referenceIndices[0] = i;
						referenceIndices[1] = j;
					}
				}
			}
			var firstReference = pixels[referenceIndices[0]];
			var secondReference = pixels[referenceIndices[1]];
			var firstGroup = new List<Rgb24>
			{
				firstReference
			};
			var secondGroup = new List<Rgb24>
			{
				secondReference
			};
			var questionableGroup = new List<Rgb24>();

			// 2. Put remaining pixels in the group they each are closest to
			foreach (var i in Enumerable.Range(0, pixels.Count).Except(referenceIndices))
			{
				var point = pixels[i];
				//var distanceFromFirstReference = GetDistance(point, firstReference);
				//var distanceFromSecondReference = GetDistance(point, secondReference);
				var distanceFromFirstReference = Math.Abs(correlationMatrix[i, referenceIndices[0]]);
				var distanceFromSecondReference = Math.Abs(correlationMatrix[i, referenceIndices[1]]);

				if (distanceFromFirstReference < distanceFromSecondReference)
					firstGroup.Add(point);
				else if (distanceFromFirstReference > distanceFromSecondReference)
					secondGroup.Add(point);
				else
					questionableGroup.Add(point); // NOTE: a decision must be made if a point is equidistant from each group
			}
			foreach (var point in questionableGroup)
			{
				if (firstGroup.Count > secondGroup.Count)
					secondGroup.Add(point);
				else
					firstGroup.Add(point);
			}

			return (firstGroup, secondGroup);
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
	}
}
