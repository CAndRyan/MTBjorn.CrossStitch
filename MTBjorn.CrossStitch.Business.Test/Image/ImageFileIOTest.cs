using MTBjorn.CrossStitch.Business.Image;
using NUnit.Framework;
using System.IO;

namespace MTBjorn.CrossStitch.Business.Test.Image
{
	[TestFixture]
	internal class ImageFileIOTest
	{
		[TestCase(null)]
		[TestCase("nonExistentFile.txt")]
		public void Load_FileDoesNotExist_ShouldThrow(string resourceName)
		{
			Assert.Throws<FileNotFoundException>(() => ImageFileIO.Load(resourceName));
		}

		[Test]
		public void Load_FileExists()
		{
			using var result = ImageFileIO.Load(Global.TestPngFilePath);

			Assert.IsNotNull(result);
		}

		[Test]
		public void LoadImage_FileExists()
		{
			using var result = ImageFileIO.Load(Global.TestPngFilePath);

			Assert.IsNotNull(result);
		}

		//[Test]
		//public void Save_TEST()
		//{
		//	using var resizedImage = ImageResizer.Resize(Global.TestPngFilePath, 64, 64);
		//	ImageFileIO.Save(resizedImage, "D:\\chris\\downloads\\filePathTest.png");
		//}
	}
}
