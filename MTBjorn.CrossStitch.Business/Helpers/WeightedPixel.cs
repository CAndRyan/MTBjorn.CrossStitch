using SixLabors.ImageSharp.PixelFormats;

namespace MTBjorn.CrossStitch.Business.Helpers
{
	public class WeightedPixel<TPixel> where TPixel : unmanaged, IPixel<TPixel>
	{
		public WeightedPixel(TPixel val, int weight)
		{
			Value = val;
			Weight = weight;
		}

		public TPixel Value { get; }
		public int Weight { get; }
	}
}
