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

		public static List<ColorGroup<TPixel>> GetReducedColorSet<TPixel>(List<TPixel> pixels, int reducedColorCount) where TPixel : unmanaged, IPixel<TPixel>
		{
			if (typeof(TPixel) == typeof(Rgb24))
				return VectorDistanceColorReductionAlgorithm(pixels.Cast<Rgb24>().ToList(), reducedColorCount)
					.Cast<ColorGroup<TPixel>>()
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
		private static List<RgbColorGroup> VectorDistanceColorReductionAlgorithm(List<Rgb24> pixels, int reducedColorCount)
		{
			if (reducedColorCount <= 0)
				return null;
			if (reducedColorCount >= pixels.Count)
				return pixels.Select(p => new RgbColorGroup(new[] { p })).ToList();
			if (reducedColorCount == 1)
				return new List<RgbColorGroup>
				{
					new RgbColorGroup(pixels)
				};

			return NColorAlgorithm(pixels, reducedColorCount);
		}

		private static List<RgbColorGroup> NColorAlgorithm(List<Rgb24> pixels, int numberOfPoints)
		{
			var initialGroups = GetGroupsAroundNFurthestPoints(pixels, numberOfPoints);
			//var updatedGroups = RebalanceIterative(initialGroups);
			var updatedGroups = RebalanceIterativeWithDiagnostics(initialGroups);

			return updatedGroups.Select(g => new RgbColorGroup(g)).ToList();
		}

		private static List<List<Rgb24>> GetGroupsAroundNFurthestPoints(List<Rgb24> pixels, int numberOfPoints)
		{
			var correlationMatrix = GetDistanceCorrelationMatrix(pixels);

			var greatestDistance = -1.0d;
			var initialReferenceIndices = new int[2];
			for (var col = 0; col < pixels.Count; col++)
			{
				for (var row = col; row < pixels.Count; row++)
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
			var referenceIndices = new List<int>(initialReferenceIndices);

			while (referenceIndices.Count < numberOfPoints)
			{
				var greatestConsolidatedDistance = -1.0d;
				var currentReferenceIndex = -1;
				foreach (var index in Enumerable.Range(0, pixels.Count).Except(referenceIndices))
				{
					var distanceFromFirstReference = Math.Abs(correlationMatrix[index, referenceIndices[0]]);
					var distanceFromSecondReference = Math.Abs(correlationMatrix[index, referenceIndices[1]]);
					var consolidatedDistance = distanceFromFirstReference + distanceFromSecondReference;

					if (consolidatedDistance >= greatestConsolidatedDistance)
					{
						greatestConsolidatedDistance = consolidatedDistance;
						currentReferenceIndex = index;
					}
				}
				referenceIndices.Add(currentReferenceIndex);
			}

			var groupMap = referenceIndices.ToDictionary(r => r, r => new List<Rgb24>
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

		/// <summary>
		/// Rebalance each grouping such that every point within a group is closer to that group's centroid than the centroid of any other group
		/// 1. Build a distance correlation matrix with each actual pixel and each group's centroid
		/// 2. Move pixels into whichever group they're closest to the centroid of
		/// 3. Repeat steps 1 & 2 (until no pixels change group)
		///
		/// TODO: test that this behavior is truly necessary
		/// </summary>
		private static List<List<Rgb24>> RebalanceIterative(List<List<Rgb24>> groupings)
		{
			var balancedGroups = groupings;
			var aPixelShifted = true;

			while (aPixelShifted)
				balancedGroups = Rebalance(balancedGroups, out aPixelShifted);

			return balancedGroups;
		}

		private static List<List<Rgb24>> RebalanceIterativeWithDiagnostics(List<List<Rgb24>> groupings, string diagnosticsFilePath = @"D:\Chris\Downloads\cross-stitch-test-diagnostics.json")
		{
			const int maxIterations = 100;

			var balancedGroups = groupings;
			var aPixelShifted = true;
			var numberOfIterations = 0;
			var pixelColorGroupHistory = balancedGroups.Select((g, i) => (group: g, groupIndex: i))
				.SelectMany(gi => gi.group.Select(p => (pixel: p, groupIndex: gi.groupIndex)))
				.ToDictionary(pi => pi.pixel.GetHashCode(), pi => (pixel: pi.pixel, groupIndices: new List<int> { pi.groupIndex }));
			var groupCentroidHistory = balancedGroups.Select(g => new List<Rgb24> { g.GetCentroid() }).ToList();
			var stopWatch = new Stopwatch();

			stopWatch.Start();
			while (aPixelShifted && numberOfIterations < maxIterations)
			{
				balancedGroups = Rebalance(balancedGroups, out aPixelShifted);
				numberOfIterations++;

				var pixelGroupData = balancedGroups.Select((g, i) => (group: g, groupIndex: i))
					.SelectMany(gi => gi.group.Select(p => (pixel: p, groupIndex: gi.groupIndex)));
				foreach (var (pixel, groupIndex) in pixelGroupData)
				{
					var pixelHash = pixel.GetHashCode();
					if (pixelColorGroupHistory[pixelHash].groupIndices.Last() != groupIndex)
						pixelColorGroupHistory[pixelHash].groupIndices.Add(groupIndex);
				}

				for (var i = 0; i < balancedGroups.Count; i++)
					groupCentroidHistory[i].Add(balancedGroups[i].GetCentroid());
			}
			stopWatch.Stop();

			//var pixelsThatMovedAtLeastOnce = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 1).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastTwice = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 2).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastThrice = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 3).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastFource = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 4).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastFifths = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 5).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastSixise = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 6).Select(h => h.pixel).ToList();
			//var pixelsThatMovedAtLeastSeptise = pixelColorGroupHistory.Values.Where(h => h.groupIndices.Count > 7).Select(h => h.pixel).ToList();

			var rebalanceHistory = new RebalanceHistory
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
				NumberOfIterations = numberOfIterations,
				Completed = !aPixelShifted,
				ElapsedTime = $"{stopWatch.Elapsed.Hours}:{stopWatch.Elapsed.Minutes}:{stopWatch.Elapsed.Seconds}.{stopWatch.Elapsed.Milliseconds}"
			};
			var diagnosticsJson = JsonConvert.SerializeObject(rebalanceHistory);
			System.IO.File.WriteAllText(diagnosticsFilePath, diagnosticsJson);

			return balancedGroups;
		}

        private static List<List<Rgb24>> Rebalance(List<List<Rgb24>> groupings, out bool aPixelShifted) // TODO: determine how to clean up the data from each iteration. The large test case starts at 2.4GB & increases as much with each iterations, suggesting the data is duplicated in memory without any cleanup...
		{
			var centroids = groupings.Select(g => g.GetCentroid());
			var pixels = groupings.SelectMany(g => g).ToList();
			var correlationMatrix = GetDistanceCorrelationMatrix(centroids.Concat(pixels).ToList());
			var newGroups = GetInitializeGroupList(groupings.Count);
			aPixelShifted = false;

			for (var pixelIndex = 0; pixelIndex < pixels.Count; pixelIndex++)
			{
				var currentLeastDistance = double.MaxValue;
				var leastDistanceGroupIndex = -1;
				for (var groupIndex = 0; groupIndex < groupings.Count; groupIndex++)
				{
					var rowIndex = pixelIndex + groupings.Count;
					var distance = Math.Abs(correlationMatrix[groupIndex, rowIndex]);
					if (distance < currentLeastDistance)
					{
						currentLeastDistance = distance;
						leastDistanceGroupIndex = groupIndex;
					}
				}
				newGroups[leastDistanceGroupIndex].Add(pixels[pixelIndex]);

				if (!groupings[leastDistanceGroupIndex].Contains(pixels[pixelIndex]))
					aPixelShifted = true;
			}

			return newGroups.ToList();
		}

		private static List<Rgb24>[] GetInitializeGroupList(int groupCount)
		{
			var groups = new List<Rgb24>[groupCount];
			for (var i = 0; i < groupCount; i++)
				groups[i] = new List<Rgb24>();

			return groups;
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

		/// <summary>
		/// Technically this isn't a correlation matrix (https://cmci.colorado.edu/classes/INFO-1301/files/borgatti.htm) but just euclidian distance
		/// </summary>
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
