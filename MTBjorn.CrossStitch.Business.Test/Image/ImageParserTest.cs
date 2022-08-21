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

			var sut = new ImageParser(14.4, 11.8, 14);
			sut.DO(@"D:\Chris\Downloads\CS_test_INPUT.png", @"D:\Chris\Downloads\CS_TEST.png", 15);
			//sut.DO(@"D:\Chris\Downloads\CS_TEST_SHARPEN.png", @"D:\Chris\Downloads\CS_TEST_SHARPEN_OUT.png", 15);
		}
	}
}
