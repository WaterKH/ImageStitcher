using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using SixLabors.Primitives;
using System;
using System.Collections.Generic;
using System.IO;

namespace ImageStitcher
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                Console.Write("Automated? (y/n):");
                var automate = Console.ReadLine();
                
                if (automate == "y")
                    ProcessMapFolder($"{Environment.CurrentDirectory}/ImagesToConvert/map");
                else
                    ProcessLayerFolder($"{Environment.CurrentDirectory}/ImagesToConvert/Layer");

            }
            catch (Exception e)
            {
                File.WriteAllText(Environment.CurrentDirectory + "/ErrorLog.txt", e.Message);
                throw;
            }
        }

        private static void ProcessLayerFolder(string basePath)
        {
            Console.Write("Row count: ");
            int.TryParse(Console.ReadLine(), out var rowCount);

            var imageProcessor = new ImageProcessor();
            //var basePath = @"C:\Users\water\Downloads\KINGDOM HEARTS Union χ [CROSS][20-02-2020]\misc\map";//;
            var finalPath = $"{Environment.CurrentDirectory}/FinalImage";

            var files = Directory.GetFiles(basePath);

            var imageLayer = imageProcessor.Process(files, rowCount);

            var saveName = files[0].Split('\\')[^1][0.. ^8];

            if (!Directory.Exists($"{finalPath}/{saveName}"))
                Directory.CreateDirectory($"{finalPath}/{saveName}");

            if (!Directory.Exists($"{finalPath}/{saveName}/AllLayers"))
                Directory.CreateDirectory($"{finalPath}/{saveName}/AllLayers");

            imageLayer.Image.Save($"{finalPath}/{saveName}/AllLayers/{saveName}.png");
            imageLayer.Image.Save($"{finalPath}/{saveName}/{saveName}.png");

            foreach (var file in files)
                File.Delete(file);
        }


        private static void ProcessMapFolder(string basePath)
        {
            var imageProcessor = new ImageProcessor();
            //var basePath = @"C:\Users\water\Downloads\KINGDOM HEARTS Union χ [CROSS][20-02-2020]\misc\map";//;
            var finalPath = $"{Environment.CurrentDirectory}/FinalImage";
            var directories = Directory.GetDirectories(basePath);

            foreach (var directory in directories)
            {
                var saveName = directory.Split('\\')[^1];

                Console.Write($"Accessing {saveName}...... ");

                var imageLayers = new Dictionary<LayerType, ImageLayer>();

                var subDirectories = Directory.GetDirectories(directory);

                foreach (var subDirectory in subDirectories)
                {
                    var layerType = (LayerType)Enum.Parse(typeof(LayerType), subDirectory.Split('_')[^1], true);
                    var rowCount = 0;

                    var files = Directory.GetFiles(subDirectory);

                    if (layerType == LayerType.Shadow)
                    {
                        rowCount = imageLayers[LayerType.Base1].RowCount;
                    }
                    else if (layerType == LayerType.Over || layerType == LayerType.Over1 || layerType == LayerType.Over2 ||
                             layerType == LayerType.A || layerType == LayerType.B ||
                             layerType == LayerType.TreeOver || layerType == LayerType.Effect ||
                             layerType == LayerType.Sky || layerType == LayerType.Sky1 || layerType == LayerType.Sky2 ||
                             layerType == LayerType.Bg1 || layerType == LayerType.Obj04)
                    {
                        imageLayers.TryGetValue(LayerType.Base1, out var layer);

                        if (layer != null && files.Length == layer.NumberOfFiles)
                            rowCount = layer.RowCount;
                    }

                    var imageLayer = imageProcessor.Process(files, rowCount);

                    if (imageLayer == null)
                    {
                        //if (!Directory.Exists($"{finalPath}/{saveName}"))
                        //    Directory.CreateDirectory($"{finalPath}/{saveName}");

                        //File.WriteAllText($"{finalPath}/{saveName}/ErrorLog.txt", "This image was unable to be stitched automatically.");

                        //continue;
                        imageLayer = new ImageLayer();
                    }

                    imageLayer.LayerType = layerType;
                    imageLayer.FileName = subDirectory.Split('\\')[^1];
                    imageLayer.NumberOfFiles = files.Length;

                    if (imageLayers.ContainsKey(layerType))
                    {
                        var type = subDirectory.Split('_')[^2] + layerType.ToString();
                        layerType = (LayerType)Enum.Parse(typeof(LayerType), type, true);
                    }

                    imageLayers.Add(layerType, imageLayer);
                }

                // Save to AllLayers folder
                foreach (var layer in imageLayers)
                {
                    var img = layer.Value;
                    var layerType = layer.Key;

                    if (!Directory.Exists($"{finalPath}/{saveName}"))
                        Directory.CreateDirectory($"{finalPath}/{saveName}");

                    if (!Directory.Exists($"{finalPath}/{saveName}/AllLayers"))
                        Directory.CreateDirectory($"{finalPath}/{saveName}/AllLayers");

                    if (img.Image != null)
                        img.Image.Save($"{finalPath}/{saveName}/AllLayers/{layer.Value.FileName}.png");
                    else
                        File.WriteAllText($"{finalPath}/{saveName}/AllLayers/{layer.Value.FileName}.txt", "Unable to stitch image automatically.");

                    if (layerType == LayerType.Over || layerType == LayerType.Over1 || layerType == LayerType.Over2 ||
                        layerType == LayerType.Sky || layerType == LayerType.Sky1 || layerType == LayerType.Sky2)
                    {
                        if (!Directory.Exists($"{finalPath}/{saveName}/OverSkyLayers"))
                            Directory.CreateDirectory($"{finalPath}/{saveName}/OverSkyLayers");

                        if (img.Image != null)
                            img.Image.Save($"{finalPath}/{saveName}/OverSkyLayers/{layer.Value.FileName}.png");
                        else
                            File.WriteAllText($"{finalPath}/{saveName}/OverSkyLayers/{layer.Value.FileName}.txt", "Unable to stitch image automatically.");

                    }
                }

                var image = new Image<Rgba32>(1, 1);

                // Stitch layer together + Save
                if (imageLayers.ContainsKey(LayerType.Base1))
                {
                    image = imageLayers[LayerType.Base1].Image;

                    if (image != null)
                    {
                        if (imageLayers.ContainsKey(LayerType.Shadow))
                            image.Mutate(o => o.DrawImage(image, new Point(0, 0), 1f));

                        image.Save($"{finalPath}/{saveName}/{saveName}.png");
                    }
                }

                // Save Assets
                foreach (var file in Directory.GetFiles(directory))
                {
                    if (file.Substring(file.Length - 3).Equals("png"))
                    {
                        if (!Directory.Exists($"{finalPath}/{saveName}"))
                            Directory.CreateDirectory($"{finalPath}/{saveName}");

                        if (!Directory.Exists($"{finalPath}/{saveName}/Assets"))
                            Directory.CreateDirectory($"{finalPath}/{saveName}/Assets");

                        File.Copy(file, $"{finalPath}/{saveName}/Assets/{file.Split('\\')[^1]}", true);
                    }
                }

                Console.WriteLine($"Finished.");
            }
        }

    }
}
