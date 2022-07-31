using System;
using System.IO;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
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
			using var fileStream = Load(filePath);
			return IS.Image.Load(fileStream);
		}

		/// <summary>
		/// Load an image using a specific pixel type. e.g. <see cref="Rgba32"/>
		/// </summary>
		public static IS.Image<TPixel> LoadImage<TPixel>(string filePath) where TPixel : unmanaged, IPixel<TPixel>
		{
			using var fileStream = Load(filePath);
			return IS.Image.Load<TPixel>(fileStream);
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
