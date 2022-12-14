using MTBjorn.CrossStitch.Business.Image;
using NUnit.Framework;
using System;

namespace MTBjorn.CrossStitch.Business.Test.Helpers
{
	[TestFixture]
	public class AidaClothHelperTest
	{
		[Test]
		public void GetPixelDimensions_WithMaxWidthAndHeight()
		{
			var maxWidth = 8.5d;
			var maxHeight = 11.0d;
			var pointsPerInch = 14;

			var (width, height) = AidaClothHelper.GetPixelDimensions(maxWidth, maxHeight, pointsPerInch);

			var expectedWidth = (int)(maxWidth * pointsPerInch);
			var expectedHeight = (int)(maxHeight * pointsPerInch);
			Assert.AreEqual(expectedWidth, width);
			Assert.AreEqual(expectedHeight, height);
		}

		[TestCase(-1.0, 1.0, 1)]
		[TestCase(1.0, -1.0, 1)]
		[TestCase(1.0, 1.0, 0)]
		[TestCase(1.0, 1.0, -1)]
		public void GetPixelDimensions_WithInvalidValues_Throws(double maxWidth, double maxHeight, int pointsPerInch)
		{
			Assert.Throws<ArgumentException>(() => AidaClothHelper.GetPixelDimensions(maxWidth, maxHeight, pointsPerInch));
		}

		[Test]
		public void GetPixelDimensions_WithNoMax_Throws()
		{
			Assert.Throws<ArgumentException>(() => AidaClothHelper.GetPixelDimensions(0d, 0d, 14));
		}

		[Test]
		public void GetPixelDimensions_WithMaxWidthOnly()
		{
			var maxWidth = 8.5d;
			var pointsPerInch = 14;

			var (width, height) = AidaClothHelper.GetPixelDimensions(maxWidth, 0d, pointsPerInch);

			var expectedWidth = (int)(maxWidth * pointsPerInch);
			Assert.AreEqual(expectedWidth, width);
			Assert.AreEqual(0, height);
		}

		[Test]
		public void GetPixelDimensions_WithMaxHeightOnly()
		{
			var maxHeight = 11.0d;
			var pointsPerInch = 14;

			var (width, height) = AidaClothHelper.GetPixelDimensions(0d, maxHeight, pointsPerInch);

			var expectedHeight = (int)(maxHeight * pointsPerInch);
			Assert.AreEqual(expectedHeight, height);
			Assert.AreEqual(0, width);
		}

		[Test]
		public void GetPixelDimensions_WithAspectRatioSkewedHorizontal_ReturnsFromMaxWidth()
		{
			var originalWidth = 100;
			var originalHeight = 80;
			var maxWidth = 8.0d;
			var maxHeight = 0d; // TODO: find way to detect which dimension should be zero...
			var pointsPerInch = 14;

			var (width, height) = AidaClothHelper.GetPixelDimensions(originalWidth, originalHeight, maxWidth, maxHeight, pointsPerInch);

			var expectedWidth = (int)(maxWidth * pointsPerInch);
			Assert.Greater(originalWidth, originalHeight, "Test setup does not have horizontally skewed aspect ratio");
			Assert.AreEqual(expectedWidth, width);
			Assert.AreEqual(0, height);
		}

		[Test]
		public void GetPixelDimensions_WithAspectRatioSkewedVertical_ReturnsFromMaxHeight()
		{
			var originalWidth = 80;
			var originalHeight = 100;
			var maxWidth = 0d; // TODO: find way to detect which dimension should be zero...
			var maxHeight = 8.0d;
			var pointsPerInch = 14;

			var (width, height) = AidaClothHelper.GetPixelDimensions(originalWidth, originalHeight, maxWidth, maxHeight, pointsPerInch);

			var expectedHeight = (int)(maxHeight * pointsPerInch);
			Assert.Greater(originalHeight, originalWidth, "Test setup does not have horizontally skewed aspect ratio");
			Assert.AreEqual(expectedHeight, height);
			Assert.AreEqual(0, width);
		}
	}
}
