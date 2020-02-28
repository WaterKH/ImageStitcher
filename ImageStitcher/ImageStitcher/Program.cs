using System;
using System.IO;

namespace ImageStitcher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("How many images per row: ");
                var input = Console.ReadLine();

                int.TryParse(input, out var imagesPerRow);

                Console.Write("How many layers: ");
                input = Console.ReadLine();

                int.TryParse(input, out var imageLayers);

                var saveName = "";
                if (imagesPerRow > 0)
                {
                    var imageProcessor = new ImageProcessor();

                    for (int i = 1; i <= imageLayers; ++i)
                    {
                        var location = Environment.CurrentDirectory + "/ImagesToConvert/Layer" + i;
                        saveName = imageProcessor.Process(imagesPerRow, location, saveName);
                    }
                }
            }
            catch (Exception e)
            {
                File.WriteAllText(Environment.CurrentDirectory + "/ErrorLog.txt", e.Message);
                throw;
            }
        }
    }
}
