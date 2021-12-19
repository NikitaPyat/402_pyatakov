using Microsoft.ML;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using YOLOv4MLNet.DataStructures;
using static Microsoft.ML.Transforms.Image.ImageResizingEstimator;

namespace YOLOv4MLNet
{
    //https://towardsdatascience.com/yolo-v4-optimal-speed-accuracy-for-object-detection-79896ed47b50
    public class Recognition
    {
        // model is available here:
        // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4
        const string modelPath = @"D:\Prak4\models\yolov4.onnx";

        const string imageOutputFolder = @"Assets\Output";

        public ConcurrentQueue<PictureInfo> queue = new ConcurrentQueue<PictureInfo>();

        static readonly string[] classesNames = new string[] { "person", "bicycle", "car", "motorbike", "aeroplane", "bus", "train", "truck", "boat", "traffic light", "fire hydrant", "stop sign", "parking meter", "bench", "bird", "cat", "dog", "horse", "sheep", "cow", "elephant", "bear", "zebra", "giraffe", "backpack", "umbrella", "handbag", "tie", "suitcase", "frisbee", "skis", "snowboard", "sports ball", "kite", "baseball bat", "baseball glove", "skateboard", "surfboard", "tennis racket", "bottle", "wine glass", "cup", "fork", "knife", "spoon", "bowl", "banana", "apple", "sandwich", "orange", "broccoli", "carrot", "hot dog", "pizza", "donut", "cake", "chair", "sofa", "pottedplant", "bed", "diningtable", "toilet", "tvmonitor", "laptop", "mouse", "remote", "keyboard", "cell phone", "microwave", "oven", "toaster", "sink", "refrigerator", "book", "clock", "vase", "scissors", "teddy bear", "hair drier", "toothbrush" };

        public void recognize(Bitmap bitmap, CancellationTokenSource cancel)
        {
            if (cancel.IsCancellationRequested) return;
            Directory.CreateDirectory(imageOutputFolder);
            MLContext mlContext = new MLContext();

            // model is available here:
            // https://github.com/onnx/models/tree/master/vision/object_detection_segmentation/yolov4

            // Define scoring pipeline
            var pipeline = mlContext.Transforms.ResizeImages(inputColumnName: "bitmap", outputColumnName: "input_1:0", imageWidth: 416, imageHeight: 416, resizing: ResizingKind.IsoPad)
                .Append(mlContext.Transforms.ExtractPixels(outputColumnName: "input_1:0", scaleImage: 1f / 255f, interleavePixelColors: true))
                .Append(mlContext.Transforms.ApplyOnnxModel(
                    shapeDictionary: new Dictionary<string, int[]>()
                    {
                        { "input_1:0", new[] { 1, 416, 416, 3 } },
                        { "Identity:0", new[] { 1, 52, 52, 3, 85 } },
                        { "Identity_1:0", new[] { 1, 26, 26, 3, 85 } },
                        { "Identity_2:0", new[] { 1, 13, 13, 3, 85 } },
                    },
                    inputColumnNames: new[]
                    {
                        "input_1:0"
                    },
                    outputColumnNames: new[]
                    {
                        "Identity:0",
                        "Identity_1:0",
                        "Identity_2:0"
                    },
                    modelFile: modelPath, recursionLimit: 100));

            // Fit on empty list to obtain input data schema
            var model = pipeline.Fit(mlContext.Data.LoadFromEnumerable(new List<YoloV4BitmapData>()));

            // Create prediction engine
            //var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);

            // save model
            //mlContext.Model.Save(model, predictionEngine.OutputSchema, Path.ChangeExtension(modelPath, "zip"));
            var sw = new Stopwatch();
            sw.Start();

            var imageName = new string[] { "kite.jpg", "dog_cat.jpg", "cars road.jpg", "ski.jpg", "ski2.jpg" };

            try
            {
                var predictionEngine = mlContext.Model.CreatePredictionEngine<YoloV4BitmapData, YoloV4Prediction>(model);
                var predict = predictionEngine.Predict(new YoloV4BitmapData() { Image = bitmap });
                var results = predict.GetResults(classesNames, 0.3f, 0.7f);
                //Console.WriteLine("Image " + imageName[i] + " has " + results.Count + " objects");

                foreach (var res in results)
                {
                    // draw predictions
                    int x1 = (int)res.BBox[0];
                    int y1 = (int)res.BBox[1];
                    int x2 = (int)res.BBox[2];
                    int y2 = (int)res.BBox[3];
                    Coordinate coord = new Coordinate(x1, y1, x2, y2);
                    PictureInfo info = new PictureInfo(bitmap, res.Label, coord);
                    queue.Enqueue(info);
                }
            }
            catch (OperationCanceledException e)
            {
                Console.WriteLine(e.Message);
            }
            finally
            {
                cancel.Dispose();
            }

            queue.Enqueue(new PictureInfo(bitmap, " ", new Coordinate(0, 0, 0, 0)));

            sw.Stop();
            Console.WriteLine($"Done in {sw.ElapsedMilliseconds}ms.");
        }
    }
}
