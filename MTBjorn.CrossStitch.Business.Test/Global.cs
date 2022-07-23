using System;
using System.IO;
using System.Reflection;

namespace MTBjorn.CrossStitch.Business.Test
{
	internal static class Global
	{
		private const string resourcesDirectoryName = "Resources";

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
