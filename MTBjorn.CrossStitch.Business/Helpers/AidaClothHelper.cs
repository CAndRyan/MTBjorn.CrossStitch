using System;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Image
{
	public static class AidaClothHelper
	{
		public static (int width, int height) GetPixelDimensions(double maxWidth, double maxHeight, int pointsPerInch)
		{
			if (maxWidth < 0 || maxHeight < 0 || pointsPerInch <= 0)
				throw new ArgumentException($"Invalid argument(s): {nameof(maxWidth)}={maxWidth}, {nameof(maxHeight)}={maxHeight}, {nameof(pointsPerInch)}={pointsPerInch}");
			if (maxWidth <= 0 && maxHeight <= 0)
				throw new ArgumentException($"Either {nameof(maxWidth)} or {nameof(maxHeight)} required");

			var roundedWidth = (int)(maxWidth * pointsPerInch);
			var roundedHeight = (int)(maxHeight * pointsPerInch);

			return (roundedWidth, roundedHeight);
		}

		public static (int width, int height) GetPixelDimensions(int originalWidth, int originalHeight, double maxWidth, double maxHeight, int pointsPerInch)
		{
			if (maxWidth < 0 || maxHeight < 0 || pointsPerInch <= 0)
				throw new ArgumentException($"Invalid argument(s): {nameof(maxWidth)}={maxWidth}, {nameof(maxHeight)}={maxHeight}, {nameof(pointsPerInch)}={pointsPerInch}");

			if (originalWidth > originalHeight)
				return ((int)(maxWidth * pointsPerInch), 0);

			return (0, (int)(maxHeight * pointsPerInch));
		}

		public static (int width, int height) GetPixelDimensions(IS.Image image, double maxWidth, double maxHeight, int pointsPerInch)
		{
			return GetPixelDimensions(image.Width, image.Height, maxWidth, maxHeight, pointsPerInch);
		}
	}
}
