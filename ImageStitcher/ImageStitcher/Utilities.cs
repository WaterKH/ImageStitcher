using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace ImageStitcher
{
    public static class Utilities
    {
        public static bool CheckImageForPixels(Image<Rgba32> image, int value, bool extraCheck = false)
        {
            var returnVal = ((image[0, 0].A == value && image[image.Width - 1, 0].A == 0) &&
                (image[0, image.Height - 1].A == value && image[image.Width - 1, image.Height - 1].A == 0));

            // Check half instead of full dimensions - Really only for DB as of right now
            if(!returnVal)
                returnVal = ((image[0, 0].A == value && image[image.Width - 1, 0].A == 0) &&
                            (image[0, image.Height / 2].A == value && image[image.Width - 1, image.Height / 2].A == 0));

            // Check for AG white background they seemed to use only once?
            if (!returnVal)
                returnVal = ((image[0, 0].A == value && (image[image.Width - 1, 0].A == 255 && image[image.Width - 1, 0].R == 255 && image[image.Width - 1, 0].G == 255 && image[image.Width - 1, 0].B == 255)) &&
                (image[0, image.Height / 2].A == value && (image[image.Width - 1, image.Height / 2].A == 255 && image[image.Width - 1, image.Height / 2].R == 255 && image[image.Width - 1, image.Height / 2].G == 255 && image[image.Width - 1, image.Height / 2].B == 255)));

            //if (extraCheck)
            //    returnVal = (image[0, image.Height - 1].A == value && image[image.Width - 1, image.Height - 1].A == 0);

            return returnVal;
        }
    }
}
