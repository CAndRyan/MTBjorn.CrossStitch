using SixLabors.ImageSharp.PixelFormats;

namespace MTBjorn.CrossStitch.Business.Helpers
{
    public class RebalanceHistory
    {
        public PixelHistory[] Pixels { get; set; }
        public Rgb24[][] Centroids { get; set; }
        public int NumberOfIterations { get; set; }
        public bool Completed { get; set; }
        public string ElapsedTime { get; set; }
    }
}
