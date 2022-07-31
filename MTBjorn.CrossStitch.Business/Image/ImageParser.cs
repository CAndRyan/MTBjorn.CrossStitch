using MTBjorn.CrossStitch.Business.Helpers;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;
using System.Linq;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Image
{
	/// <summary>
	/// aaa
	/// 1. Resize image to match pixel density of desired aida cloth size (i.e. height, width, pointsPerInch)
	/// 2. Identify all pixel colors in image
	/// 3. Build filter that reduces each pixel to the closest color (in a given color set)
	/// 3. Normalize pixels to reduced color set
	/// 4. ...
	/// </summary>
	public class ImageParser
	{
		private readonly decimal maxWidth;
		private readonly decimal maxHeight;
		private readonly int pointsPerInch;

		/// <summary>
		// A helper to load an image & resize according to desired Aida cloth dimensions
		/// </summary>
		/// <param name="maxHeight">Max canvas height in inches</param>
		/// <param name="maxWidth">Max canvas width in inches</param>
		/// <param name="pointsPerInch">Points/pixels per inch of fabric</param>
		public ImageParser(decimal maxHeight, decimal maxWidth, int pointsPerInch)
		{
			this.maxHeight = maxHeight;
			this.maxWidth = maxWidth;
			this.pointsPerInch = pointsPerInch;
		}

		public void DO(string inputFilePath)
		{
			var resizedImage = ResizeToClothDimensions<Rgb24>(inputFilePath);

			// TODO: implement
			var allColors = ImageColorHelper.GetAllColors(resizedImage);
			var reducedColorSet = ImageColorHelper.GetReducedColorSet(allColors, 100);
			AdjustColors(resizedImage, reducedColorSet);

			ImageFileIO.Save(resizedImage, "D:\\chris\\downloads\\crossStitchExample.png");
		}

		private void AdjustColors<TPixel>(IS.Image<TPixel> image, List<ColorGroup<TPixel>> colorSet) where TPixel : unmanaged, IPixel<TPixel>
		{
			for (var rowIndex = 0; rowIndex < image.Height; rowIndex++)
			{
				for (var columnIndex = 0; columnIndex < image.Width; columnIndex++)
				{
					var currentColor = image[columnIndex, rowIndex];
					var colorGroup = colorSet.Single(g => g.Contains(currentColor));
					image[columnIndex, rowIndex] = colorGroup.Centroid;
				}
			}
		}

		private IS.Image<TPixel> ResizeToClothDimensions<TPixel>(string inputFilePath) where TPixel : unmanaged, IPixel<TPixel>
		{
			var image = ImageFileIO.LoadImage<TPixel>(inputFilePath);
			var (width, height) = AidaClothHelper.GetPixelDimensions(image, maxWidth, maxHeight, pointsPerInch);

			ImageResizer.Resize(image, width, height);

			return image;
		}
	}
}
