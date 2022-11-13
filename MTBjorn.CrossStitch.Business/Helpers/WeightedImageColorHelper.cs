using MTBjorn.CrossStitch.Business.Extensions;
using Newtonsoft.Json;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Helpers
{
	public static class WeightedImageColorHelper
	{
		public static List<WeightedPixel<TPixel>> GetAllColors<TPixel>(IS.Image<TPixel> image) where TPixel : unmanaged, IPixel<TPixel>
		{
			var colorMap = new Dictionary<int, (TPixel color, int count)>();

			foreach (var pixel in GetAllPixels(image))
			{
				var pixelHash = pixel.GetHashCode();
				if (!colorMap.ContainsKey(pixelHash))
				{
					colorMap.Add(pixelHash, (pixel, 1));
				}
				else
				{
					var (color, count) = colorMap[pixelHash];
					colorMap[pixelHash] = (color, count + 1);
				}
			}

			return colorMap.Values.Select(data => new WeightedPixel<TPixel>(data.color, data.count)).ToList();
		}

		public static List<ColorGroup<Rgb24>> GetReducedColorSet<TPixel>(List<WeightedPixel<Rgb24>> pixels, int reducedColorCount)
		{
			return WeightedDistanceColorReductionAlgorithm(pixels, reducedColorCount)
				.Select(g => (ColorGroup<Rgb24>)g)
				.ToList();
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
		private static List<RgbWeightedColorGroup> WeightedDistanceColorReductionAlgorithm(List<WeightedPixel<Rgb24>> pixels, int reducedColorCount)
		{
			if (reducedColorCount <= 0)
				return null;
			if (reducedColorCount >= pixels.Count)
				return pixels.Select(p => new RgbWeightedColorGroup(new[] { p })).ToList();
			if (reducedColorCount == 1)
				return new List<RgbWeightedColorGroup>
				{
					new RgbWeightedColorGroup(pixels)
				};

			return NColorAlgorithmWeighted(pixels, reducedColorCount);
		}

		private static List<RgbWeightedColorGroup> NColorAlgorithmWeighted(List<WeightedPixel<Rgb24>> pixels, int numberOfPoints)
		{
			var initialGroups = GetWeightedGroupsAroundNFurthestPoints(pixels, numberOfPoints);
			//var updatedGroups = WeightedRebalanceIterative(initialGroups);
			var updatedGroups = WeightedRebalanceIterativeWithDiagnostics(initialGroups);

			return updatedGroups.Select(g => new RgbWeightedColorGroup(g)).ToList();
		}

		private static List<List<WeightedPixel<Rgb24>>> GetWeightedGroupsAroundNFurthestPoints(List<WeightedPixel<Rgb24>> pixels, int numberOfPoints)
		{
			var nonWeightedPixels = pixels.Select(p => p.Value).ToList();
			var correlationMatrix = GetDistanceCorrelationMatrix(nonWeightedPixels); // NOTE: use the distance between colors to identify the initial groups
			var referenceIndices = GetReferenceIndexes(correlationMatrix, numberOfPoints);
			var groupMap = referenceIndices.ToDictionary(r => r, r => new List<WeightedPixel<Rgb24>>
			{
				pixels[r]
			});

			// fill groups with closest pixels
			foreach (var i in Enumerable.Range(0, pixels.Count).Except(referenceIndices))
			{
				var point = pixels[i];
				var closestReferencePointIndex = referenceIndices
					.Select(r => (Index: r, Distance: Math.Abs(correlationMatrix[i, r])))
					.OrderBy(t => t.Distance)
					.First().Index;

				groupMap[closestReferencePointIndex].Add(point);

				// NOTE: a decision must be made if a point is equidistant from multiple groups
			}

			return groupMap.Values.ToList();
		}

		private static List<int> GetReferenceIndexes(double[,] correlationMatrix, int numberOfPoints)
		{
			var referenceIndexes = GetInitialReferenceIndexes(correlationMatrix).ToList();

			while (referenceIndexes.Count < numberOfPoints)
				referenceIndexes.Add(FindPixelFurthestFromAllReferenceIndexes(correlationMatrix, referenceIndexes));

			return referenceIndexes;
		}

		// i.e. maximize the closest distance each pixel is from each reference index
		// the final selection is as far away from each reference as possible, without being too close to any other references indexes... in theory
		// try using the average distance from each reference...?
		private static int FindPixelFurthestFromAllReferenceIndexes(double[,] correlationMatrix, IEnumerable<int> referenceIndexes)
		{
			var numberOfPixels = correlationMatrix.GetLength(0); // dimension doesn't matter since it's a square matrix

			return Enumerable.Range(0, numberOfPixels)
				.Select(pixelIndex =>
				{
					if (referenceIndexes.Contains(pixelIndex))
						return (pixelIndex, 0);

					return (PixelIndex: pixelIndex, AverageDistance: referenceIndexes.Select(r => Math.Abs(correlationMatrix[r, pixelIndex])).Average());
				})
				.OrderByDescending(t => t.AverageDistance)
				.First().PixelIndex;
		}

		private static int[] GetInitialReferenceIndexes(double[,] correlationMatrix)
		{
			var numberOfPixels = correlationMatrix.GetLength(0); // dimension doesn't matter since it's a square matrix
			var greatestDistance = -1.0d;
			var initialReferenceIndices = new int[2];

			for (var col = 0; col < numberOfPixels; col++)
			{
				for (var row = col; row < numberOfPixels; row++)
				{
					var distance = Math.Abs(correlationMatrix[col, row]);
					if (distance > greatestDistance)
					{
						greatestDistance = distance;
						initialReferenceIndices[0] = col;
						initialReferenceIndices[1] = row;
					}
				}
			}

			return initialReferenceIndices;
		}

		/// <summary>
		/// Rebalance each grouping such that every point within a group is closer to that group's centroid than the centroid of any other group
		/// 1. Build a distance correlation matrix with each actual pixel and each group's centroid
		/// 2. Move pixels into whichever group they're closest to the centroid of
		/// 3. Repeat steps 1 & 2 (until no pixels change group)
		///
		/// TODO: test that this behavior is truly necessary
		/// </summary>
		private static List<List<WeightedPixel<Rgb24>>> WeightedRebalanceIterative(List<List<WeightedPixel<Rgb24>>> groupings)
		{
			var balancedGroups = groupings;
			var aPixelShifted = true;

			while (aPixelShifted)
				balancedGroups = Rebalance(balancedGroups, out aPixelShifted);

			return balancedGroups;
		}

		// NOTE: the "centroids" in this (diagnostics) context are actually centers of mass
		// TODO: consider updating the diagnostic visualization to include the weight of each pixel
		private static List<List<WeightedPixel<Rgb24>>> WeightedRebalanceIterativeWithDiagnostics(List<List<WeightedPixel<Rgb24>>> groupings, string diagnosticsFilePath = @"D:\Chris\Projects\MTBjorn.CrossStitch\MTBjorn.CrossStitch.Business.Test\Resources\contrast-test\contrast-test-image-3colors-weighted-reduction-diagnostics.json")
		{
			const int maxIterations = 3;

			var balancedGroups = groupings;
			var aPixelShifted = true;
			var numberOfIterations = 0;
			var pixelColorGroupHistory = balancedGroups.Select((g, i) => (group: g, groupIndex: i))
				.SelectMany(gi => gi.group.Select(p => (pixel: p.Value, groupIndex: gi.groupIndex)))
				.ToDictionary(pi => pi.pixel.GetHashCode(), pi => (pixel: pi.pixel, groupIndices: new List<int> { pi.groupIndex }));
			var groupCentroidHistory = balancedGroups.Select(g => new List<Rgb24> { g.GetCentroid() }).ToList();
			var groupCenterOfMassHistory = balancedGroups.Select(g => new List<Rgb24> { g.GetCenterOfMass().Value }).ToList();
			var stopWatch = new Stopwatch();

			stopWatch.Start();
			while (aPixelShifted && numberOfIterations < maxIterations)
			{
				balancedGroups = Rebalance(balancedGroups, out aPixelShifted);
				numberOfIterations++;

				var pixelGroupData = balancedGroups.Select((g, i) => (group: g, groupIndex: i))
					.SelectMany(gi => gi.group.Select(p => (pixel: p.Value, groupIndex: gi.groupIndex)));
				foreach (var (pixel, groupIndex) in pixelGroupData)
				{
					var pixelHash = pixel.GetHashCode();
					if (pixelColorGroupHistory[pixelHash].groupIndices.Last() != groupIndex)
						pixelColorGroupHistory[pixelHash].groupIndices.Add(groupIndex);
				}

				for (var i = 0; i < balancedGroups.Count; i++)
				{
					groupCentroidHistory[i].Add(balancedGroups[i].GetCentroid());
					groupCenterOfMassHistory[i].Add(balancedGroups[i].GetCenterOfMass().Value);
				}
			}
			stopWatch.Stop();

			//var pixelsThatMovedAtLeastOnce = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 1).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastTwice = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 2).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastThrice = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 3).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastFource = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 4).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastFifths = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 5).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastSixise = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 6).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastSeptise = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 7).Select(h => h.pixel).ToList();

			var rebalanceHistory = new WeightedRebalanceHistory
			{
				Pixels = pixelColorGroupHistory.Values.Select(h => new PixelHistory
				{
					Pixel = new Rgb24
					{
						R = h.pixel.R,
						G = h.pixel.G,
						B = h.pixel.B
					},
					Groups = h.groupIndices
				}).ToArray(),
				Centroids = groupCentroidHistory.Select(g => g.Select(c => new Rgb24
				{
					R = c.R,
					G = c.G,
					B = c.B
				}).ToArray()).ToArray(),
				CentersOfMass = groupCenterOfMassHistory.Select(g => g.Select(c => new Rgb24
				{
					R = c.R,
					G = c.G,
					B = c.B
				}).ToArray()).ToArray(),
				NumberOfIterations = numberOfIterations,
				Completed = !aPixelShifted,
				ElapsedTime = $"{stopWatch.Elapsed.Hours}:{stopWatch.Elapsed.Minutes}:{stopWatch.Elapsed.Seconds}.{stopWatch.Elapsed.Milliseconds}"
			};
			var diagnosticsJson = JsonConvert.SerializeObject(rebalanceHistory);
			System.IO.File.WriteAllText(diagnosticsFilePath, diagnosticsJson);

			return balancedGroups;
		}

		private static List<List<WeightedPixel<Rgb24>>> Rebalance(List<List<WeightedPixel<Rgb24>>> groupings, out bool aPixelShifted)
		{
			var centersOfMass = groupings.Select(g => g.GetCenterOfMass());
			var pixels = groupings.SelectMany(g => g).ToList();
			var correlationMatrix = GetAdjustedDistanceCorrelationMatrix(centersOfMass.Concat(pixels).ToList());
			var newGroups = GetInitializedGroupList(groupings.Count);
			aPixelShifted = false;

			for (var pixelIndex = 0; pixelIndex < pixels.Count; pixelIndex++)
			{
				var currentLeastForce = double.MaxValue;
				var leastForceGroupIndex = -1;
				for (var groupIndex = 0; groupIndex < groupings.Count; groupIndex++)
				{
					var rowIndex = pixelIndex + groupings.Count;
					var force = correlationMatrix[groupIndex, rowIndex];
					if (force < currentLeastForce)
					{
						currentLeastForce = force;
						leastForceGroupIndex = groupIndex;
					}
				}
				newGroups[leastForceGroupIndex].Add(pixels[pixelIndex]);

				if (!groupings[leastForceGroupIndex].Contains(pixels[pixelIndex]))
					aPixelShifted = true;
			}

			return newGroups.ToList();
		}

		private static List<WeightedPixel<Rgb24>>[] GetInitializedGroupList(int groupCount)
		{
			var groups = new List<WeightedPixel<Rgb24>>[groupCount];
			for (var i = 0; i < groupCount; i++)
				groups[i] = new List<WeightedPixel<Rgb24>>();

			return groups;
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

				// "walk" behind the first iterator of the pixel array, measuring distance between each prior pixel  & 'currentPixel', skipping a comparison of 'currentPixel' to itself
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

		/// <summary>
		/// A variation of the distance correlation matrix, with each color weighted by how many pixels exist of that color
		/// The distance is adjusted by the ratio of each pixel's weight, giving the "adjusted distance" that we want to minimize for each color group
		/// A group with large "gravitational force" should appear MORE distant & less appealing for the regroup process...
		/// In theory, this should even out the size of each group, whilst ensuring each group contains the closest colors, preserving contrast
		/// </summary>
		private static double[,] GetAdjustedDistanceCorrelationMatrix(List<WeightedPixel<Rgb24>> pixels)
		{
			var matrix = new double[pixels.Count, pixels.Count];
			for (var i = 0; i < pixels.Count; i++)
			{
				var currentPixel = pixels[i];

				// "walk" behind the first iterator of the pixel array, measuring distance between each prior pixel  & 'currentPixel', skipping a comparison of 'currentPixel' to itself
				for (var j = 0; j < i; j++)
				{
					var rawDistance = GetAdjustedDistance(currentPixel, pixels[j]);
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

		private static double GetAdjustedDistance(WeightedPixel<Rgb24> first, WeightedPixel<Rgb24> second)
		{
			var distance = GetDistance(first.Value, second.Value);
			if (distance == 0 || first.Weight == 0 || second.Weight == 0)
				return 0;

			var weightRatio = first.Weight < second.Weight ? second.Weight / first.Weight : first.Weight / second.Weight;

			return distance * weightRatio;
		}
	}
}
