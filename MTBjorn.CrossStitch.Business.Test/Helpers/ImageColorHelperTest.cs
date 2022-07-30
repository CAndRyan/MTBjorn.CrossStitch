using MTBjorn.CrossStitch.Business.Helpers;
using MTBjorn.CrossStitch.Business.Image;
using NUnit.Framework;
using SixLabors.ImageSharp.PixelFormats;
using System;
using System.Collections.Generic;

namespace MTBjorn.CrossStitch.Business.Test.Helpers
{
	[TestFixture]
	internal class ImageColorHelperTest
	{
		private static readonly List<object[]> colorTestResources = new List<object[]>
		{
			new object[]
			{
				Global.TestSolidRedPngFilePath,
				new Rgb24[]
				{
					new Rgb24(255, 0, 0)
				}
			},
			new object[]
			{
				Global.TestHalfRedBluePngFilePath,
				new Rgb24[]
				{
					new Rgb24(255, 0, 0),
					new Rgb24(0, 0, 255)
				}
			},
			new object[]
			{
				Global.TestQuadColorPngFilePath,
				new Rgb24[]
				{
					new Rgb24(255, 0, 0),
					new Rgb24(255, 128, 0),
					new Rgb24(0, 0, 255),
					new Rgb24(0, 255, 0)
				}
			}
		};

		[TestCaseSource(nameof(colorTestResources))]
		public void GetAllColors_ReturnsUniqueColors(string filePath, Rgb24[] colors)
		{
			using var image = ImageFileIO.LoadImage<Rgb24>(filePath);

			var result = ImageColorHelper.GetAllColors(image);

			CollectionAssert.AreEquivalent(colors, result);
		}

		[Test]
		public void GetReducedColorSet_OneColorInputAndOutput_ReturnsInputColor()
		{
			var singleColor = new Rgb24(255, 0, 0);
			var input = new List<Rgb24>
			{
				singleColor
			};

			var result = ImageColorHelper.GetReducedColorSet(input, 1);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(singleColor, result[0]);
		}

		[TestCase(2)]
		[TestCase(3)]
		public void GetReducedColorSet_ReducedCountGreaterOrEqualToInputColors_ReturnsAllInputColors(int reducedColorCount)
		{
			var firstColor = new Rgb24(255, 100, 0);
			var secondColor = new Rgb24(20, 0, 255);
			var input = new List<Rgb24>
			{
				firstColor,
				secondColor
			};

			var result = ImageColorHelper.GetReducedColorSet(input, reducedColorCount);

			Assert.AreEqual(input.Count, result.Count);
			CollectionAssert.AreEquivalent(input, result);
		}

		[Test]
		public void GetReducedColorSet_OneColor_ReturnsCentroid()
		{
			var firstColor = new Rgb24(200, 100, 0);
			var secondColor = new Rgb24(100, 0, 255);
			var input = new List<Rgb24>
			{
				firstColor,
				secondColor
			};

			var result = ImageColorHelper.GetReducedColorSet(input, 1);
			var expectedResult = new Rgb24(150, 50, 128);

			Assert.AreEqual(1, result.Count);
			Assert.AreEqual(expectedResult, result[0]);
		}

		[Test]
		public void GetReducedColorSet_TEST()
		{
			using var image = ImageFileIO.LoadImage<Rgb24>(Global.TestQuadColorPngFilePath);
			var colors = ImageColorHelper.GetAllColors(image);

			var result = ImageColorHelper.GetReducedColorSet(colors, 2);
			var expectedResult = new List<Rgb24>
			{
				new Rgb24(255, 64, 0),
				new Rgb24(0, 128, 128)
			};

			CollectionAssert.AreEquivalent(expectedResult, result);
		}
	}
}
