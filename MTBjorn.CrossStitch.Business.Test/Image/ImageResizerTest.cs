using MTBjorn.CrossStitch.Business.Image;
using NUnit.Framework;
using System.IO;

namespace MTBjorn.CrossStitch.Business.Test
{
	[TestFixture]
	internal class ImageResizerTest
	{
		[TestCase(64, 64)]
		public void Resize_ExplicitWidthAndHeight_SetsOutputStreamWithModifiedData(int width, int height)
		{
			var inputStream = File.OpenRead(Global.TestPngFilePath);

			var result = ImageResizer.Resize(inputStream, width, height);

			Assert.AreEqual(width, result.Width);
			Assert.AreEqual(height, result.Height);
		}
	}
}