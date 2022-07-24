using MTBjorn.CrossStitch.Business.Helpers;
using MTBjorn.CrossStitch.Business.Image;
using NUnit.Framework;
using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace MTBjorn.CrossStitch.Business.Test.Helpers
{
	[TestFixture]
	internal class ImageColorHelperTest
	{
		private static readonly List<object[]> colorTestResources = new List<object[]>
		{
			new object[]
			{
				Global.TestSolidRedPngFilePath,
				new Rgb24[]
				{
					new Rgb24(255, 0, 0)
				}
			},
			new object[]
			{
				Global.TestHalfRedBluePngFilePath,
				new Rgb24[]
				{
					new Rgb24(255, 0, 0),
					new Rgb24(0, 0, 255)
				}
			}
		};

		[TestCaseSource(nameof(colorTestResources))]
		public void GetAllColors_ReturnsUniqueColors(string filePath, Rgb24[] colors)
		{
			using var image = ImageFileIO.LoadImage<Rgb24>(filePath);

			var result = ImageColorHelper.GetAllColors(image);

			CollectionAssert.AreEquivalent(colors, result);
		}
	}
}
