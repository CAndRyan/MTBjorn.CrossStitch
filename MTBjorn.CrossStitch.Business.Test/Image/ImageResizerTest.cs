using MTBjorn.CrossStitch.Business.Image;
using NUnit.Framework;
using System.IO;

namespace MTBjorn.CrossStitch.Business.Test
{
	[TestFixture]
	internal class ImageResizerTest
	{
		[TestCase(64, 64)]
		[TestCase(512, 512)]
		[TestCase(100, 50)]
		public void Resize_WithStream_ExplicitWidthAndHeight_SetsOutputStreamWithModifiedData(int width, int height)
		{
			using var inputStream = File.OpenRead(Global.TestPngFilePath);

			var result = ImageResizer.Resize(inputStream, width, height);

			Assert.AreEqual(width, result.Width);
			Assert.AreEqual(height, result.Height);
		}

		[TestCase(64, 64)]
		[TestCase(512, 512)]
		[TestCase(100, 50)]
		public void Resize_WithFilePath_ExplicitWidthAndHeight_SetsOutputStreamWithModifiedData(int width, int height)
		{
			using var result = ImageResizer.Resize(Global.TestPngFilePath, width, height);

			Assert.AreEqual(width, result.Width);
			Assert.AreEqual(height, result.Height);
		}

		[Test]
		public void Resize_ZeroWidth_PreservesAspectRatio()
		{
			var height = 64;
			var expectedAspectRatio = 1.0m;

			using var result = ImageResizer.Resize(Global.TestPngFilePath, 0, height);

			Assert.AreEqual((int)(height * expectedAspectRatio), result.Width);
			Assert.AreEqual(height, result.Height);
		}

		[Test]
		public void Resize_ZeroHeight_PreservesAspectRatio()
		{
			var width = 64;
			var expectedAspectRatio = 1.0m;

			using var result = ImageResizer.Resize(Global.TestPngFilePath, width, 0);

			Assert.AreEqual((int)(width * expectedAspectRatio), result.Height);
			Assert.AreEqual(width, result.Width);
		}
	}
}