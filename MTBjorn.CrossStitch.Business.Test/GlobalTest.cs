using NUnit.Framework;
using System.IO;

namespace MTBjorn.CrossStitch.Business.Test
{
	[TestFixture]
	internal class GlobalTest
	{
		private const string resourcesDirectoryName = "Resources";
		private const string testResourceFileName = "egypticon-128x128.png";

		private static string AssemblyDirectory => Global.GetAssemblyDirectory();

		[TestCase(null)]
		[TestCase("nonExistentFile.txt")]
		public void GetResourcePath_FileDoesNotExist_ShouldThrow(string resourceName)
		{
			Assert.Throws<FileNotFoundException>(() => Global.GetResourcePath(resourceName));
		}

		[Test]
		public void GetResourcePath_FileExists_Returns_FullFilePath()
		{
			var result = Global.GetResourcePath(testResourceFileName);
			var expectedResult = Path.Join(AssemblyDirectory, resourcesDirectoryName, testResourceFileName);

			Assert.AreEqual(expectedResult, result);
		}
	}
}
