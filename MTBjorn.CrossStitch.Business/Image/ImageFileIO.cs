using System;
using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Image
{
	public static class ImageFileIO
	{
		private static readonly IImageEncoder defaultImageEncoder = new PngEncoder();

		public static Stream Load(string filePath)
		{
			if (!File.Exists(filePath))
				throw new FileNotFoundException($"File does not exist: '{filePath}'");

			return File.OpenRead(filePath);
		}

		public static IS.Image LoadImage(string filePath)
		{
			var fileStream = Load(filePath);
			return IS.Image.Load(fileStream);
		}

		public static void Save(IS.Image image, string filePath)
		{
			if (image is null)
				throw new ArgumentNullException(nameof(image));
			if (string.IsNullOrWhiteSpace(filePath))
				throw new ArgumentNullException(nameof(filePath));

			using var outputFileStream = File.Create(filePath);
			image.Save(outputFileStream, defaultImageEncoder);
		}
	}
}
