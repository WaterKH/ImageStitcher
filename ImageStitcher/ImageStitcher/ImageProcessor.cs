using System;
using System.IO;
using System.Linq;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;

namespace ImageStitcher
{
    public class ImageProcessor
    {
        public string Process(int number, string location, string saveNamePrev = "", int width = 256, int height = 256)
        {
            var files = Directory.GetFiles(location);

            var saveName = string.IsNullOrEmpty(saveNamePrev) ? files.FirstOrDefault().Split('\\')[^1] : saveNamePrev;

            using var stitchedImage = string.IsNullOrEmpty(saveNamePrev) ? new Image<Rgba32>(number * width, (files.Length / number) * height) : Image.Load(Path.Combine(Environment.CurrentDirectory, @"FinalImage\") + saveNamePrev);

            var yOffset = 0;

            for (int i = 0; i < files.Length; ++i)
            { 
                using var image = Image.Load(files[i]);

                if (i % number == 0 && i != 0)
                    yOffset++;

                stitchedImage.Mutate(o => o.DrawImage(image, new Point((i % number) * width, yOffset * height), 1f));
            }

            foreach (var file in files)
            {
                File.Delete(file);
            }

            stitchedImage.Save(Path.Combine(Environment.CurrentDirectory, @"FinalImage\") + saveName);

            return saveName;
        }
    }
}
