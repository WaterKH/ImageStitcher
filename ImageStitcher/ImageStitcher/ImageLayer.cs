using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageStitcher
{
    public class ImageLayer
    {
        public string FileName { get; set; }
        public Image<Rgba32> Image { get; set; }
        public LayerType LayerType { get; set; }
        public int RowCount { get; set; }
        public int NumberOfFiles { get; set; }
    }
}
