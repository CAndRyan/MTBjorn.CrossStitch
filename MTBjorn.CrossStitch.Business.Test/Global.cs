using System;
using System.IO;
using System.Reflection;

namespace MTBjorn.CrossStitch.Business.Test
{
	internal static class Global
	{
		private const string resourcesDirectoryName = "Resources";
		private const string testPngFileName = "egypticon-128x128-rgba.png";
		private const string testSolidRedPngFileName = "solid-red-10x10.png";
		private const string testHalfRedBluePngFileName = "half-red-blue-10x10.png";
		private const string testQuadColorPngFileName = "quarter-red-orange-blue-green-20x10.png";
		private const string testQuadColorReduced2PngFileName = "quarter-red-orange-blue-green-20x10-reduced2.png";
		private const string testQuadColorNonSequentialPngFileName = "quarter-red-green-orange-blue-20x10.png";
		private const string testQuadColorNonSequentialReduced2PngFileName = "quarter-red-green-orange-blue-20x10-reduced2.png";

		public static string TestPngFilePath => GetResourcePath(testPngFileName);
		public static string TestSolidRedPngFilePath => GetResourcePath(testSolidRedPngFileName);
		public static string TestHalfRedBluePngFilePath => GetResourcePath(testHalfRedBluePngFileName);
		public static string TestQuadColorPngFilePath => GetResourcePath(testQuadColorPngFileName);
		public static string TestQuadColorReduced2PngFilePath => GetResourcePath(testQuadColorReduced2PngFileName);
		public static string TestQuadColorNonSequentialPngFilePath => GetResourcePath(testQuadColorNonSequentialPngFileName);
		public static string TestQuadColorNonSequentialReduced2PngFilePath => GetResourcePath(testQuadColorNonSequentialReduced2PngFileName);

		public static string GetResourcePath(string resourceName)
		{
			var resourcePath = Path.Join(GetAssemblyDirectory(), resourcesDirectoryName, resourceName);
			if (!File.Exists(resourcePath))
				throw new FileNotFoundException($"File does not exist: '{resourcePath}'");

			return resourcePath;
		}

		public static string GetAssemblyDirectory() {
			var codeBase = Assembly.GetExecutingAssembly().CodeBase;
			var uri = new UriBuilder(codeBase);
			var path = Uri.UnescapeDataString(uri.Path);

			return Path.GetDirectoryName(path);
		}
	}
}
