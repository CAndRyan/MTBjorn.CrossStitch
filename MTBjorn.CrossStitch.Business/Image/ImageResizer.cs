using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Image
{
	public static class ImageResizer
	{
		/// <summary>
		/// Resize an image stream using the default, <see href="https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.KnownResamplers.html#SixLabors_ImageSharp_Processing_KnownResamplers_Bicubic">Bicubic</see>, sampling algorithm.
		/// If either height or width is 0, the other dimension will be calculated to preserve the input aspect ratio.
		/// </summary>
		public static IS.Image Resize(Stream inputStream, int width = 0, int height = 0)
		{
			if (inputStream is null)
				throw new ArgumentNullException(nameof(inputStream));

			var image = IS.Image.Load(inputStream);

			return Resize(image, width, height);
		}

		/// <summary>
		/// Resize an image file using the default, <see href="https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.KnownResamplers.html#SixLabors_ImageSharp_Processing_KnownResamplers_Bicubic">Bicubic</see>, sampling algorithm.
		/// If either height or width is 0, the other dimension will be calculated to preserve the input aspect ratio.
		/// </summary>
		public static IS.Image Resize(string inputFilePath, int width = 0, int height = 0)
		{
			if (inputFilePath is null)
				throw new ArgumentNullException(nameof(inputFilePath));

			var image = ImageFileIO.LoadImage(inputFilePath);

			return Resize(image, width, height);
		}

		private static IS.Image Resize(IS.Image image, int width, int height)
		{
			if (width == 0 && height == 0)
				return image;

			image.Mutate(x => x.Resize(width, height));

			return image;
		}
	}
}
