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
                var imageProcessor = new ImageProcessor();
                var basePath = @"C:\Users\water\Downloads\KINGDOM HEARTS Union χ [CROSS][20-02-2020]\misc\map";//$"{Environment.CurrentDirectory}/ImagesToConvert/map";
                var finalPath = $"{Environment.CurrentDirectory}/FinalImage";
                var directories = Directory.GetDirectories(basePath);

                foreach (var directory in directories)
                {
                    var saveName = directory.Split('\\')[^1];
                    Directory.CreateDirectory($"{finalPath}/{saveName}");

                    if (saveName == "BC_0012_00_00")
                        Console.WriteLine();

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
                                 layerType == LayerType.Sky || layerType == LayerType.Sky1 || layerType == LayerType.Sky2 || layerType == LayerType.Bg1)
                        {
                            imageLayers.TryGetValue(LayerType.Base1, out var t);
                            
                            if(t == null)
                            {
                                continue; // Remove this after you figure out how to accurately get the row count of things that are weird
                            }
                            else if (files.Length == imageLayers[LayerType.Base1].NumberOfFiles)
                            {
                                rowCount = imageLayers[LayerType.Base1].RowCount;
                            }
                            else
                            {
                                continue; // Remove this after you figure out how to accurately get the row count of things that are weird
                            }
                        }

                        var imageLayer = imageProcessor.Process(files, rowCount);

                        imageLayer.LayerType = layerType;
                        imageLayer.FileName = subDirectory.Split('\\')[^1];
                        imageLayer.NumberOfFiles = files.Length;

                        imageLayers.Add(layerType, imageLayer);
                    }

                    // Save to AllLayers folder
                    foreach (var layer in imageLayers)
                    {
                        var img = layer.Value;
                        var layerType = layer.Key;

                        if (!Directory.Exists($"{finalPath}/{saveName}/AllLayers"))
                            Directory.CreateDirectory($"{finalPath}/{saveName}/AllLayers");

                        img.Image.Save($"{finalPath}/{saveName}/AllLayers/{layer.Value.FileName}.png");


                        if (layerType == LayerType.Over || layerType == LayerType.Over1 || layerType == LayerType.Over2 ||
                            layerType == LayerType.Sky || layerType == LayerType.Sky1 || layerType == LayerType.Sky2)
                        {
                            if(!Directory.Exists($"{finalPath}/{saveName}/OverSkyLayers"))
                                Directory.CreateDirectory($"{finalPath}/{saveName}/OverSkyLayers");

                            img.Image.Save($"{finalPath}/{saveName}/OverSkyLayers/{layer.Value.FileName}.png");
                        }
                    }

                    var image = new Image<Rgba32>(1, 1);

                    // Stitch layer together + Save
                    if (imageLayers.ContainsKey(LayerType.Base1))
                    {
                        image = imageLayers[LayerType.Base1].Image;

                        if(imageLayers.ContainsKey(LayerType.Shadow))
                        {
                            image.Mutate(o => o.DrawImage(image, new Point(0, 0), 1f));
                        }

                        image.Save($"{finalPath}/{saveName}/{saveName}.png");
                    }

                    // Save Assets
                    foreach(var file in Directory.GetFiles(directory))
                    {
                        if (file.Substring(file.Length - 3).Equals("png"))
                        {
                            if (!Directory.Exists($"{finalPath}/{saveName}/Assets"))
                                Directory.CreateDirectory($"{finalPath}/{saveName}/Assets");

                            File.Copy(file, $"{finalPath}/{saveName}/Assets/{file.Split('\\')[^1]}", true);
                        }
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
