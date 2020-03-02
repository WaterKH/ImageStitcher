using System.Collections.Generic;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageStitcher
{
    public class ImageProcessor
    {
        public ImageLayer Process(string[] files, int suppliedRowCount = 0)
        {

            var (rowCount, images) = this.FindRowSize(files);

            if (suppliedRowCount > 0)
                rowCount = suppliedRowCount;

            var firstImage = images.FirstOrDefault().Value;
            var yOffset = 0;

            if (rowCount == 0)
                return null;
                
            var stitchedImage = new Image<Rgba32>(rowCount * firstImage.Width, (files.Length / rowCount) * firstImage.Height);

            foreach(var kv in images.OrderBy(x => x.Key))
            {
                var image = kv.Value;
                var index = kv.Key;

                if (index % rowCount == 0 && index != 0)
                        yOffset++;

                stitchedImage.Mutate(o => o.DrawImage(image, new Point((index % rowCount) * image.Width, yOffset * image.Height), 1f));
            }

            //foreach (var file in files)
            //{
            //    File.Delete(file);
            //}

            return new ImageLayer
            {
                Image = stitchedImage,
                RowCount = rowCount
            };
        }

        public (int RowCount, Dictionary<int, Image> Images) FindRowSize(string[] files)
        {
            // Cases - 
            // 1. Right side of the screen is empty, next image is not - Make sure current rowCount is divisible by total number of images
            // 2. Entire image is empty, next image is not - Make sure current rowCount is divisible by total number of images
            // 3. Entire image is empty, previous image is not - Make sure current rowCount is divisible by total number of images
            
            var rowCount = 0;
            var images = new Dictionary<int, Image>();
            Image<Rgba32> nextImage = null;

            var index = 0;
            for(int i = files.Length - 1; i >= 0; --i)
            {
                Image<Rgba32> image;

                if (nextImage != null)
                {
                    image = nextImage;
                    nextImage = null;
                }
                else
                {
                    image = Image.Load<Rgba32>(files[i]);
                }

                images.Add(i, image);

                if (rowCount != 0 || i == files.Length - 1)
                    continue;
                
                ++index;

                if(Utilities.CheckImageForPixels(image, 255))
                {
                    if(files.Length % (index) == 0)
                    {
                        rowCount = index;
                    }
                }

                //if(Utilities.CheckImageForPixels(image, 255))
                //{
                //    if (files.Length % (i + 1) == 0)
                //    {
                //        rowCount = i + 1;
                //        continue;
                //    }
                //}
                //else if(Utilities.CheckImageForPixels(image, 0))
                //{
                //    images.TryGetValue(i - 1, out var prevImage);

                //    if(prevImage != null)
                //    {
                //        if (Utilities.CheckImageForPixels(image, 255))
                //        {
                //            if (files.Length % (i + 1) == 0)
                //            {
                //                rowCount = i + 1;
                //                continue;
                //            }
                //        }
                //    }

                //    if(i + 1 < files.Length)
                //    {
                //        nextImage = Image.Load<Rgba32>(files[i + 1]);

                //        if (Utilities.CheckImageForPixels(image, 255))
                //        {
                //            if (files.Length % (i + 1) == 0)
                //            {
                //                rowCount = i + 1;
                //                continue;
                //            }
                //        }
                //    }
                //}
            }

            return (rowCount, images);
        }
    }
}