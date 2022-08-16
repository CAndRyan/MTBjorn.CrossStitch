using SixLabors.ImageSharp.PixelFormats;
using System.Collections.Generic;

namespace MTBjorn.CrossStitch.Business.Helpers
{
    public class PixelHistory
    {
        public Rgb24 Pixel { get; set; }
        public List<int> Groups { get; set; }
    }
}
