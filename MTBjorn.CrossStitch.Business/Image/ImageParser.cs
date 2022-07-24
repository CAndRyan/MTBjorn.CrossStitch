using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Processing;
using System;
using System.IO;
using IS = SixLabors.ImageSharp;

namespace MTBjorn.CrossStitch.Business.Image
{
	/// <summary>
	/// aaa
	/// 1. Resize image to match pixel density of desired aida cloth size (i.e. height, width, pointsPerInch)
	/// 2. Identify all pixel colors in image
	/// 3. Normalize pixels to reduced color set
	/// 4. ...
	/// </summary>
	public class ImageParser
	{
		private readonly int maxWidth;
		private readonly int maxHeight;
		private readonly int pointsPerInch;

		public ImageParser(int maxHeight, int maxWidth, int pointsPerInch)
		{
			this.maxHeight = maxHeight;
			this.maxWidth = maxWidth;
			this.pointsPerInch = pointsPerInch;
		}

		public void DO(string inputFilePath)
		{
			var resizedImage = Resize(inputFilePath);

			// TODO: implement
		}

		private IS.Image Resize(string inputFilePath)
		{
			var image = ImageFileIO.LoadImage(inputFilePath);
			var (width, height) = GetResizeDimensions(image.Width, image.Height);

			return ImageResizer.Resize(image, width, height);
		}

		private (int width, int height) GetResizeDimensions(int originalWidth, int originalHeight)
		{
			return (0, 0);
		}
	}
}
