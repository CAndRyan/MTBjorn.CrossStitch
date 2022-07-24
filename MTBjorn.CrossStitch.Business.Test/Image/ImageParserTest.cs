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
			var maxWidth = 8.5m;
			var maxHeight = 11.0m;
			var pointsPerInch = 14;
			var sut = new ImageParser(maxWidth, maxHeight, pointsPerInch);

			sut.DO(Global.TestPngFilePath);
		}
	}
}
