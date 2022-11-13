using MTBjorn.CrossStitch.Business.Image;
using NUnit.Framework;

namespace MTBjorn.CrossStitch.Business.Test.Image
{
	[TestFixture]
	internal class ImageParserTest
	{
		[Test]
		public void TEST()
		{
			var maxWidth = 8.5d;
			var maxHeight = 11.0d;
			var pointsPerInch = 14;

			//var sut = new ImageParser(maxWidth, maxHeight, pointsPerInch);
			//sut.DO(Global.TestPngFilePath, "D:\\chris\\downloads\\crossStitchExample.png", 4);

			var sut = new ImageParser(14.4, 11.8, 14); // TODO: add support for no resizing of image itself. Should we trim if it won't fit, or throw?
			sut.DO(@"D:\Chris\Projects\MTBjorn.CrossStitch\MTBjorn.CrossStitch.Business.Test\Resources\contrast-test\contrast-test-image.png", @"D:\Chris\Projects\MTBjorn.CrossStitch\MTBjorn.CrossStitch.Business.Test\Resources\contrast-test\contrast-test-image_3colors-weighted.png", 3);
			//sut.DO(@"D:\Chris\Downloads\CS_TEST_SHARPEN.png", @"D:\Chris\Downloads\CS_TEST_SHARPEN_OUT.png", 15);

		}
	}
}
