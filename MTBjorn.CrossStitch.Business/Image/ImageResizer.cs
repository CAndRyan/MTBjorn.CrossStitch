using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Image
{
	public static class ImageResizer
	{
		/// <summary>
		/// Resize an image using the default, <see href="https://docs.sixlabors.com/api/ImageSharp/SixLabors.ImageSharp.Processing.KnownResamplers.html#SixLabors_ImageSharp_Processing_KnownResamplers_Bicubic">Bicubic</see>, sampling algorithm.
		/// If either height or width is 0, the other dimension will be calculated to preserve the input aspect ratio.
		/// </summary>
		public static void Resize(Stream inputStream, Func<Stream> getOutputStream, int width = 0, int height = 0)
		{
			if (inputStream is null)
				throw new ArgumentNullException(nameof(inputStream));
			if (getOutputStream is null)
				throw new ArgumentNullException(nameof(getOutputStream));
			if (width == 0 && height == 0)
				return;

			IImageEncoder outputEncoder = null;

			using (var image = IS.Image.Load(inputStream))
			{
				image.Mutate(x => x.Resize(width, height));

				var outputStream = getOutputStream();
				image.Save(outputStream, outputEncoder);
			}
		}
	}
}
