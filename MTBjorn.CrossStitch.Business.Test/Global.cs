﻿using System;
using System.IO;
using System.Reflection;

namespace MTBjorn.CrossStitch.Business.Test
{
	internal static class Global
	{
		private const string resourcesDirectoryName = "Resources";
		private const string testPngFileName = "egypticon-128x128.png";
		private const string testSolidRedPngFileName = "solid-red-10x10.png";
		private const string testHalfRedBluePngFileName = "half-red-blue-10x10.png";

		public static string TestPngFilePath => GetResourcePath(testPngFileName);
		public static string TestSolidRedPngFilePath => GetResourcePath(testSolidRedPngFileName);
		public static string TestHalfRedBluePngFilePath => GetResourcePath(testHalfRedBluePngFileName);

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
